import React, { useEffect, useState } from 'react';
import { Container, Typography, Card, CardContent, CircularProgress, Alert, Button, Divider, Box } from '@mui/material';
import Grid from '@mui/material/Grid2';
import axios from 'axios';

interface Application {
  applicationID: number;
  userID: number;
  applyDate: string;
  status: number; // 0 for Submitted, 1 for Approved, 2 for Rejected
  reason?: string;
}

// Status mapping
const statusLabels: { [key: number]: string } = {
  0: "Submitted",
  1: "Approved",
  2: "Rejected"
};

const ApplicationManager: React.FC = () => {
  const [applications, setApplications] = useState<Application[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchApplications = async () => {
    try {
      const response = await axios.get('/api/sponsor/applications');
      setApplications(response.data);
    } catch (error: any) {
      setError(error.response?.data || 'Failed to load applications.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchApplications();
  }, []);

  const handleProcessApplication = async (applicationID: number, action: "approve" | "reject") => {
    try {
      await axios.post(`/api/sponsor/applications/${applicationID}/process?action=${action}`);
      setApplications((prevApps) =>
        prevApps.map((app) =>
          app.applicationID === applicationID ? { ...app, status: action === "approve" ? 1 : 2 } : app
        )
      );
    } catch (error: any) {
      setError(error.response?.data || `Failed to ${action} application.`);
    }
  };

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;

  // Separate applications by status
  const submittedApplications = applications.filter(app => app.status === 0);
  const approvedApplications = applications.filter(app => app.status === 1);
  const rejectedApplications = applications.filter(app => app.status === 2);

  const renderApplications = (apps: Application[], message: string) => (
    apps.length > 0 ? (
      apps.map((app) => (
        <Grid size={{xs:12, sm:6, md:4}} key={app.applicationID}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1">Application ID: {app.applicationID}</Typography>
              <Typography>User ID: {app.userID}</Typography>
              <Typography>Apply Date: {app.applyDate}</Typography>
              <Typography>Status: {statusLabels[app.status]}</Typography>
              {app.status === 0 && (
                <>
                  <Button color="primary" variant="contained" onClick={() => handleProcessApplication(app.applicationID, "approve")}>
                    Approve
                  </Button>
                  <Button color="secondary" variant="contained" onClick={() => handleProcessApplication(app.applicationID, "reject")} sx={{ ml: 1 }}>
                    Reject
                  </Button>
                </>
              )}
            </CardContent>
          </Card>
        </Grid>
      ))
    ) : (
      <Typography variant="body2" color="textSecondary" sx={{ mt: 2 }}>{message}</Typography>
    )
  );

  return (
    <Container>
      <Typography variant="h4" component="h1" gutterBottom>
        Driver Applications
      </Typography>

      <Box>
        <Typography variant="h6">Submitted Applications</Typography>
        <Divider sx={{ mb: 2 }} />
        <Grid container spacing={2} style={{ maxHeight: '300px', overflowY: 'auto' }}>
          {renderApplications(submittedApplications, "No pending applications")}
        </Grid>
      </Box>

      <Box sx={{ mt: 4 }}>
        <Typography variant="h6">Approved Applications</Typography>
        <Divider sx={{ mb: 2 }} />
        <Grid container spacing={2} style={{ maxHeight: '300px', overflowY: 'auto' }}>
          {renderApplications(approvedApplications, "No approved applications")}
        </Grid>
      </Box>

      <Box sx={{ mt: 4 }}>
        <Typography variant="h6">Rejected Applications</Typography>
        <Divider sx={{ mb: 2 }} />
        <Grid container spacing={2} style={{ maxHeight: '300px', overflowY: 'auto' }}>
          {renderApplications(rejectedApplications, "No rejected applications")}
        </Grid>
      </Box>
    </Container>
  );
};

export default ApplicationManager;
