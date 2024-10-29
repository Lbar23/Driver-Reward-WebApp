import React, { useState, useEffect } from 'react';
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
  Tooltip
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

interface ApiResponse {
  data: Sponsor[];
  succeeded: boolean;
  message?: string;
}

const SponsorRegistrationForDriver = () => {
  const [sponsors, setSponsors] = useState<Sponsor[]>([]);
  const [selectedSponsors, setSelectedSponsors] = useState<number[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    fetchAvailableSponsors();
  }, []);

  const fetchAvailableSponsors = async () => {
    try {
      const response = await axios.get<Sponsor[]>('/api/driver/available-sponsors', {
        withCredentials: true
      });
      
      // Ensure we're setting an array
      setSponsors(Array.isArray(response.data) ? response.data : []);
      
      // Log the response for debugging
      console.log('Sponsors response:', response.data);
      
    } catch (err) {
      if (axios.isAxiosError(err)) {
        const errorMessage = err.response?.data || 'Error loading sponsors. Please try again later.';
        setError(typeof errorMessage === 'string' ? errorMessage : 'An error occurred.');
        console.error('Axios error:', err.response?.data);
      } else {
        setError('Error loading sponsors. Please try again later.');
        console.error('Error:', err);
      }
      // Initialize empty array on error
      setSponsors([]);
    } finally {
      setLoading(false);
    }
  };

  const handleSponsorToggle = (sponsorId: number) => {
    setSelectedSponsors(prev => {
      if (prev.includes(sponsorId)) {
        return prev.filter(id => id !== sponsorId);
      } else {
        return [...prev, sponsorId];
      }
    });
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
      const response = await axios.post<string>('/api/driver/register-sponsors', selectedSponsors, {
        withCredentials: true,
        headers: {
          'Content-Type': 'application/json',
        }
      });

      setSuccess(response.data || 'Successfully registered with selected sponsors!');
      setSelectedSponsors([]);
      // Refresh available sponsors list
      fetchAvailableSponsors();
    } catch (err) {
      if (axios.isAxiosError(err)) {
        const errorMessage = err.response?.data || 'Failed to register with sponsors. Please try again.';
        setError(typeof errorMessage === 'string' ? errorMessage : 'Registration failed.');
        console.error('Axios error:', err.response?.data);
      } else {
        setError('Failed to register with sponsors. Please try again.');
        console.error('Error:', err);
      }
    } finally {
      setSubmitting(false);
    }
  };

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
        Select Sponsors
      </Typography>
      
      <Card sx={{ mb: 2, bgcolor: 'background.paper' }}>
        <CardContent>
          <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
            <Typography variant="subtitle1">Available Reward Programs</Typography>
            <Tooltip title="Select multiple sponsors to join their reward programs">
              <HelpOutlineIcon color="action" fontSize="small" />
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
            ) : (
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
                          label={`$${sponsor.pointDollarValue} per point`}
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
            )}
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
              `Register with ${selectedSponsors.length} Sponsor${selectedSponsors.length !== 1 ? 's' : ''}`
            )}
          </Button>
        </CardContent>
      </Card>
    </Box>
  );
};

export default SponsorRegistrationForDriver;