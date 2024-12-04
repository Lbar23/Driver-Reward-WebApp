import React, { useState, useEffect } from 'react';
import {
  TextField,
  Button,
  Typography,
  Box,
  Stepper,
  Step,
  StepLabel,
  Alert,
  CircularProgress
} from '@mui/material';
import { useNavigate, useLocation } from 'react-router-dom';
import axios from 'axios';
import { useAuth } from '../service/authContext';

const ResetPasswordPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { isAuthenticated, user } = useAuth();
  const [activeStep, setActiveStep] = useState(0);
  const [userId, setUserId] = useState('');
  const [resetToken, setResetToken] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const steps = ['Initiate Reset', 'Reset Password'];

  useEffect(() => {
    // If user is authenticated, pre-fill the userId
    if (isAuthenticated && user) {
      setUserId(user.id.toString());
    }

    // Check if there's a userId in the location state (e.g., passed from login page)
    const stateUserId = location.state?.userId;
    if (stateUserId) {
      setUserId(stateUserId);
    }
  }, [isAuthenticated, user, location.state]);

  const handleInitiateReset = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      await axios.post(`/api/user/init-reset-password/${userId}`);
      setActiveStep(1);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to initiate password reset. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleResetPassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (newPassword !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      await axios.post('/api/user/reset-password', {
        userId,
        token: resetToken,
        newPassword
      });
      navigate('/login', { state: { message: 'Password reset successfully. Please log in with your new password.' } });
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reset password. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        maxWidth: 400,
        margin: 'auto',
        padding: 3,
        boxShadow: 3,
        borderRadius: 2,
        backgroundColor: 'background.paper',
      }}
    >
      <Typography variant="h4" component="h1" gutterBottom>
        Reset Password
      </Typography>

      <Stepper activeStep={activeStep} alternativeLabel sx={{ width: '100%', mb: 4 }}>
        {steps.map((label) => (
          <Step key={label}>
            <StepLabel>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>

      {error && (
        <Alert severity="error" sx={{ width: '100%', mb: 2 }}>
          {error}
        </Alert>
      )}

      {activeStep === 0 ? (
        <Box component="form" onSubmit={handleInitiateReset} sx={{ width: '100%' }}>
          <TextField
            label="User ID"
            variant="outlined"
            margin="normal"
            fullWidth
            value={userId}
            onChange={(e) => setUserId(e.target.value)}
            required
            disabled={isAuthenticated}
          />
          <Button
            type="submit"
            variant="contained"
            color="primary"
            fullWidth
            sx={{ mt: 3, mb: 2 }}
            disabled={loading}
          >
            {loading ? <CircularProgress size={24} /> : 'Initiate Reset'}
          </Button>
        </Box>
      ) : (
        <Box component="form" onSubmit={handleResetPassword} sx={{ width: '100%' }}>
          <TextField
            label="Reset Token"
            variant="outlined"
            margin="normal"
            fullWidth
            value={resetToken}
            onChange={(e) => setResetToken(e.target.value)}
            required
          />
          <TextField
            label="New Password"
            type="password"
            variant="outlined"
            margin="normal"
            fullWidth
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            required
          />
          <TextField
            label="Confirm New Password"
            type="password"
            variant="outlined"
            margin="normal"
            fullWidth
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
          />
          <Button
            type="submit"
            variant="contained"
            color="primary"
            fullWidth
            sx={{ mt: 3, mb: 2 }}
            disabled={loading}
          >
            {loading ? <CircularProgress size={24} /> : 'Reset Password'}
          </Button>
        </Box>
      )}

      <Button
        variant="text"
        color="primary"
        onClick={() => navigate('/login')}
        sx={{ mt: 2 }}
      >
        Back to Login
      </Button>
    </Box>
  );
};

export default ResetPasswordPage;