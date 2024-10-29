import React, { useState } from 'react';
import { Button, TextField, Typography, Alert } from '@mui/material';
import axios from 'axios';

interface ApplicationFormData {
  userId: number;
  sponsorId: number;
  reason: string;
}

const DriverApplication: React.FC = () => {
  const [applicationData, setApplicationData] = useState<ApplicationFormData>({ userId: 0, sponsorId: 0, reason: '' });
  const [status, setStatus] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setApplicationData({ ...applicationData, [name]: value });
  };

  const submitApplication = async () => {
    try {
      const response = await axios.post('/api/driverapp/apply', applicationData);
      setStatus(response.data);
    } catch (error: any) {
      setError('Failed to submit application.');
    }
  };

  return (
    <div>
      <Typography variant="h5">Driver Application</Typography>
      <TextField
        label="User ID"
        name="userId"
        type="number"
        onChange={handleInputChange}
        fullWidth
        margin="normal"
      />
      <TextField
        label="Sponsor ID"
        name="sponsorId"
        type="number"
        onChange={handleInputChange}
        fullWidth
        margin="normal"
      />
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
      {status && <Alert severity="success">{status}</Alert>}
      {error && <Alert severity="error">{error}</Alert>}
    </div>
  );
};

export default DriverApplication;
