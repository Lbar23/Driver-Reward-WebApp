import React, { useEffect, useState } from 'react';
import {
  Container,
  Typography,
  Card,
  CardContent,
  CircularProgress,
  Alert,
  Button,
  Divider,
  Box,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
} from '@mui/material';
import Grid from '@mui/material/Grid2';
import axios from 'axios';

// Interfaces and Enums
interface Application {
  applicationID: number;
  driverName: string;
  userID: number;
  applyDate: string;
  status: AppStatus;
  processReason?: ProcessedReason;
  comments?: string;
}

enum AppStatus {
  Submitted = 0,
  Approved = 1,
  Rejected = 2,
}

enum ProcessedReason {
  MeetsCriteria = 0,
  Referral = 1,
  InvalidApplication = 2,
  Ineligible = 3,
  FullCapacity = 4,
}

const ProcessedReasonLabels: Record<ProcessedReason, string> = {
  [ProcessedReason.MeetsCriteria]: 'Meets Criteria',
  [ProcessedReason.Referral]: 'Referral',
  [ProcessedReason.InvalidApplication]: 'Invalid Application',
  [ProcessedReason.Ineligible]: 'Ineligible',
  [ProcessedReason.FullCapacity]: 'Full Capacity',
};

const statusLabels: { [key: number]: string } = {
  0: 'Submitted',
  1: 'Approved',
  2: 'Rejected',
};

interface ApplicationCardProps {
  app: Application;
  onProcess: (applicationID: number, status: AppStatus, reason: ProcessedReason, comments: string) => Promise<void>;
}

const ApplicationCard: React.FC<ApplicationCardProps> = ({ app, onProcess }) => {
  const [selectedStatus, setSelectedStatus] = useState<AppStatus | ''>('');
  const [selectedReason, setSelectedReason] = useState<ProcessedReason | ''>('');
  const [comments, setComments] = useState<string>('');
  const [processing, setProcessing] = useState(false);

  const handleProcess = async () => {
    if (selectedStatus === '' || selectedReason === '') return;

    setProcessing(true);
    try {
      await onProcess(app.applicationID, selectedStatus, selectedReason, comments);
      setSelectedStatus('');
      setSelectedReason('');
      setComments('');
    } finally {
      setProcessing(false);
    }
  };

  const getAvailableReasons = () => {
    if (selectedStatus === AppStatus.Approved) {
      return [
        { value: ProcessedReason.MeetsCriteria, label: 'Meets Criteria' },
        { value: ProcessedReason.Referral, label: 'Referral' },
      ];
    }
    if (selectedStatus === AppStatus.Rejected) {
      return [
        { value: ProcessedReason.InvalidApplication, label: 'Invalid Application' },
        { value: ProcessedReason.Ineligible, label: 'Ineligible' },
        { value: ProcessedReason.FullCapacity, label: 'Full Capacity' },
      ];
    }
    return [];
  };

  return (
    <Card>
      <CardContent>
        <Typography variant="subtitle1">Application ID: {app.applicationID}</Typography>
        <Typography>Driver: {app.driverName}</Typography>
        <Typography>User ID: {app.userID}</Typography>
        <Typography>Apply Date: {app.applyDate}</Typography>
        <Typography>Status: {statusLabels[app.status]}</Typography>

        {app.status === AppStatus.Submitted && (
          <>
            <FormControl fullWidth sx={{ mt: 2, mb: 2 }}>
              <InputLabel>Decision</InputLabel>
              <Select
                value={selectedStatus}
                label="Decision"
                onChange={(e) => {
                  setSelectedStatus(e.target.value as AppStatus);
                  setSelectedReason('');
                }}
              >
                <MenuItem value={AppStatus.Approved}>Approve</MenuItem>
                <MenuItem value={AppStatus.Rejected}>Reject</MenuItem>
              </Select>
            </FormControl>

            {selectedStatus !== '' && (
              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel>Reason</InputLabel>
                <Select
                  value={selectedReason}
                  label="Reason"
                  onChange={(e) => setSelectedReason(e.target.value as ProcessedReason)}
                >
                  {getAvailableReasons().map(({ value, label }) => (
                    <MenuItem key={value} value={value}>
                      {label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            )}

            <TextField
              label="Comments"
              multiline
              rows={3}
              value={comments}
              onChange={(e) => setComments(e.target.value)}
              fullWidth
              sx={{ mb: 2 }}
            />

            <Button
              fullWidth
              variant="contained"
              color="primary"
              disabled={selectedStatus === '' || selectedReason === '' || processing}
              onClick={handleProcess}
            >
              {processing ? 'Processing...' : 'Process Application'}
            </Button>
          </>
        )}

        {app.processReason !== undefined && (
          <Typography sx={{ mt: 1 }}>
            Reason: {ProcessedReasonLabels[app.processReason]}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
};

const ApplicationManager: React.FC = () => {
  const [applications, setApplications] = useState<Application[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const fetchApplications = async () => {
    try {
      const response = await axios.get('/api/sponsor/applications');
      setApplications(response.data);
      setError(null);
    } catch (error: any) {
      setError(error.response?.data || 'Failed to load applications.');
    } finally {
      setLoading(false);
    }
  };

  const handleProcessApplication = async (
    applicationID: number,
    status: AppStatus,
    reason: ProcessedReason,
    comments: string
  ) => {
    try {
      const updatedApplication = {
        status,
        processReason: reason,
        processedDate: new Date().toISOString().split('T')[0],
        comments,
      };

      await axios.post(`/api/sponsor/applications/${applicationID}`, updatedApplication);

      setApplications((prevApps) =>
        prevApps.map((app) =>
          app.applicationID === applicationID ? { ...app, status, processReason: reason } : app
        )
      );

      setSuccessMessage('Application processed successfully');
      setTimeout(() => setSuccessMessage(null), 3000);
      setError(null);
    } catch (error: any) {
      setError(error.response?.data || `Failed to process application.`);
      throw error;
    }
  };

  useEffect(() => {
    fetchApplications();
  }, []);

  const renderApplications = (apps: Application[], message: string) => (
    apps.length > 0 ? (
      apps.map((app) => (
        <Grid size={{ xs: 12, sm: 6, md: 4 }} key={app.applicationID}>
          <ApplicationCard app={app} onProcess={handleProcessApplication} />
        </Grid>
      ))
    ) : (
      <Typography variant="body2" color="textSecondary" sx={{ mt: 2 }}>
        {message}
      </Typography>
    )
  );

  return (
    <Container>
      <Typography variant="h4" component="h1" gutterBottom>
        Driver Applications
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {successMessage && (
        <Alert severity="success" sx={{ mb: 2 }}>
          {successMessage}
        </Alert>
      )}

      <Box>
        <Typography variant="h6">Pending Applications</Typography>
        <Divider sx={{ mb: 2 }} />
        <Grid container spacing={2}>
          {renderApplications(
            applications.filter((app) => app.status === AppStatus.Submitted),
            'No pending applications'
          )}
        </Grid>
      </Box>

      <Box sx={{ mt: 4 }}>
        <Typography variant="h6">Approved Applications</Typography>
        <Divider sx={{ mb: 2 }} />
        <Grid container spacing={2}>
          {renderApplications(
            applications.filter((app) => app.status === AppStatus.Approved),
            'No approved applications'
          )}
        </Grid>
      </Box>

      <Box sx={{ mt: 4 }}>
        <Typography variant="h6">Rejected Applications</Typography>
        <Divider sx={{ mb: 2 }} />
        <Grid container spacing={2}>
          {renderApplications(
            applications.filter((app) => app.status === AppStatus.Rejected),
            'No rejected applications'
          )}
        </Grid>
      </Box>
    </Container>
  );
};

export default ApplicationManager;
