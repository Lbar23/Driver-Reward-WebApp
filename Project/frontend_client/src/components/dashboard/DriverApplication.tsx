import React, { useState, useEffect, useMemo } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  FormGroup,
  FormControlLabel,
  Checkbox,
  Button,
  Alert,
  CircularProgress,
  Divider,
  Stack,
  Chip,
  Tooltip,
  IconButton,
} from '@mui/material';
import MonetizationOnIcon from '@mui/icons-material/MonetizationOn';
import BusinessIcon from '@mui/icons-material/Business';
import HelpOutlineIcon from '@mui/icons-material/HelpOutline';
import axios from 'axios';

interface Sponsor {
  sponsorID: number;
  companyName: string;
  pointDollarValue: number;
}

const DriverApplication: React.FC = () => {
  const [sponsors, setSponsors] = useState<Sponsor[]>([]);
  const [selectedSponsors, setSelectedSponsors] = useState<number[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    fetchSponsors();
  }, []);

  const fetchSponsors = async () => {
    try {
      const response = await axios.get<Sponsor[]>('/api/driver/available-sponsors', {
        withCredentials: true
      });
      setSponsors(Array.isArray(response.data) ? response.data : []);
      setError(null);
    } catch (err) {
      if (axios.isAxiosError(err)) {
        const errorMessage = err.response?.data || 'Failed to load sponsors. Please try again later.';
        setError(typeof errorMessage === 'string' ? errorMessage : 'An error occurred.');
      } else {
        setError('Failed to load sponsors. Please try again later.');
      }
      setSponsors([]);
    } finally {
      setLoading(false);
    }
  };

  const handleSponsorToggle = (sponsorId: number) => {
    setSelectedSponsors(prev => 
      prev.includes(sponsorId) 
        ? prev.filter(id => id !== sponsorId)
        : [...prev, sponsorId]
    );
  };

  const handleSubmit = async () => {
    if (selectedSponsors.length === 0) {
      setError('Please select at least one sponsor');
      return;
    }

    setSubmitting(true);
    setError(null);
    setSuccess(null);

    try {
      // Send just the array of sponsor IDs
      const response = await axios.post('/api/driver/apply-sponsors', selectedSponsors, {
        withCredentials: true,
        headers: {
          'Content-Type': 'application/json',
        }
      });

      setSuccess(response.data.message || 'Application submitted successfully!');
      setSelectedSponsors([]);
    } catch (err) {
      if (axios.isAxiosError(err)) {
        const errorMessage = err.response?.data?.message || 'Failed to submit application. Please try again.';
        setError(typeof errorMessage === 'string' ? errorMessage : 'Submission failed.');
      } else {
        setError('Failed to submit application. Please try again.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  const sponsorsList = useMemo(() => (
    sponsors.map((sponsor) => (
      <Box key={sponsor.sponsorID} sx={{ mb: 2 }}>
        <FormControlLabel
          control={
            <Checkbox
              checked={selectedSponsors.includes(sponsor.sponsorID)}
              onChange={() => handleSponsorToggle(sponsor.sponsorID)}
              color="primary"
            />
          }
          label={
            <Stack direction="row" spacing={2} alignItems="center">
              <BusinessIcon color="action" />
              <Typography>{sponsor.companyName}</Typography>
              <Chip
                icon={<MonetizationOnIcon />}
                label={`$${sponsor.pointDollarValue.toFixed(2)} per point`}
                size="small"
                color="primary"
                variant="outlined"
              />
            </Stack>
          }
        />
        <Divider sx={{ ml: 7, my: 1 }} />
      </Box>
    ))
  ), [sponsors, selectedSponsors]);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ maxWidth: 800, mx: 'auto', p: 2 }}>
      <Typography variant="h5" component="h1" gutterBottom>
        Driver Application
      </Typography>
      
      <Card sx={{ mb: 2, bgcolor: 'background.paper' }}>
        <CardContent>
          <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
            <Typography variant="subtitle1">Available Reward Programs</Typography>
            <Tooltip title="Select multiple sponsors to join their reward programs">
              <IconButton size="small">
                <HelpOutlineIcon color="action" fontSize="small" />
              </IconButton>
            </Tooltip>
          </Stack>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          
          {success && (
            <Alert severity="success" sx={{ mb: 2 }}>
              {success}
            </Alert>
          )}

          <FormGroup>
            {sponsors.length === 0 ? (
              <Typography color="text.secondary">
                No available sponsors found.
              </Typography>
            ) : sponsorsList}
          </FormGroup>

          <Button
            variant="contained"
            color="primary"
            onClick={handleSubmit}
            disabled={selectedSponsors.length === 0 || submitting}
            sx={{ mt: 2 }}
          >
            {submitting ? (
              <CircularProgress size={24} color="inherit" />
            ) : (
              `Submit Application (${selectedSponsors.length} Sponsor${selectedSponsors.length !== 1 ? 's' : ''})`
            )}
          </Button>
        </CardContent>
      </Card>
    </Box>
  );
};

export default DriverApplication;