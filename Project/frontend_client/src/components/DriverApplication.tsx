import React, { useState, useEffect, useCallback } from 'react';
import { Button, TextField, Typography, Alert, MenuItem, Select, InputLabel, FormControl } from '@mui/material';
import { SelectChangeEvent } from '@mui/material';
import axios from 'axios';

interface ApplicationFormData {
  sponsorId: number;
  reason: string;
}

// Updated Sponsor interface to match API response
interface Sponsor {
  sponsorID: number;
  companyName: string;
}

const DriverApplication: React.FC = () => {
  const [applicationData, setApplicationData] = useState<ApplicationFormData>({ sponsorId: 0, reason: '' });
  const [sponsors, setSponsors] = useState<Sponsor[]>([]);
  const [status, setStatus] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const fetchSponsors = useCallback(async () => {
    try {
      const response = await axios.get('/api/driver/available-sponsors');
      setSponsors(response.data);
      setError(null); // Clear any error if data loads successfully
    } catch (error) {
      setError('Failed to load data. Please try again.');
    }
  }, []);

  useEffect(() => {
    fetchSponsors();
  }, [fetchSponsors]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setApplicationData({ ...applicationData, [name]: value });
  };

  const handleSponsorChange = (event: SelectChangeEvent<number>) => {
    setApplicationData({ ...applicationData, sponsorId: parseInt(event.target.value as string) });
  };

  const submitApplication = async () => {
    try {
      const response = await axios.post('/api/driverapp/apply', applicationData);
      setStatus(response.data.message || 'Application submitted successfully!');
      setError(null); // Clear any existing errors on success
    } catch (error: any) {
      setError('Failed to submit application.');
    }
  };

  return (
    <div>
      <Typography variant="h5">Driver Application</Typography>

      {error && <Alert severity="error">{error}</Alert>}
      {status && <Alert severity="success">{status}</Alert>}

      <FormControl fullWidth margin="normal">
        <InputLabel id="sponsor-label">Sponsor</InputLabel>
        <Select
          labelId="sponsor-label"
          value={applicationData.sponsorId}
          onChange={handleSponsorChange}
          fullWidth
        >
          {sponsors.map((sponsor) => (
            <MenuItem key={sponsor.sponsorID} value={sponsor.sponsorID}>
              {sponsor.companyName}
            </MenuItem>
          ))}
        </Select>
      </FormControl>

      <TextField
        label="Reason for Application"
        name="reason"
        onChange={handleInputChange}
        fullWidth
        multiline
        margin="normal"
      />

      <Button variant="contained" color="primary" onClick={submitApplication}>
        Submit Application
      </Button>
    </div>
  );
};

export default DriverApplication;
