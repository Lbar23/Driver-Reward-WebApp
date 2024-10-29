import React, { useState, FormEvent } from 'react';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box';
import LinearProgress from '@mui/material/LinearProgress';
import axios from 'axios';

//Reusing the password requirements from RegisterPage
const passwordRequirements = [
  { check: (pw: string) => pw.length >= 8, message: "At least 8 characters long" },
  { check: (pw: string) => /[0-9]/.test(pw), message: "Contains at least one digit" },
  { check: (pw: string) => /[a-z]/.test(pw), message: "Contains at least one lowercase letter" },
  { check: (pw: string) => /[A-Z]/.test(pw), message: "Contains at least one uppercase letter" },
  { check: (pw: string) => /[^A-Za-z0-9]/.test(pw), message: "Contains at least one special character" },
  { check: (pw: string) => new Set(pw).size >= 1, message: "Contains at least 1 unique character" },
];

const PasswordChangeForm: React.FC = () => {
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError('');
    setSuccess(false);

    if (newPassword !== confirmPassword) {
      setError("New passwords do not match");
      return;
    }

    if (!passwordRequirements.every(req => req.check(newPassword))) {
      setError("New password does not meet all requirements");
      return;
    }

    try {
      const response = await axios.post('/api/user/change-password', {
        currentPassword,
        newPassword,
      });

      if (response.data && response.data.success) {
        setSuccess(true);
        setCurrentPassword('');
        setNewPassword('');
        setConfirmPassword('');
      }
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        setError(error.response.data.message || 'Failed to change password');
      } else {
        setError('An unexpected error occurred');
      }
    }
  };

  return (
    <Box sx={{ width: '100%', maxWidth: 400, margin: 'auto', mt: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Change Password
      </Typography>

      {error && (
        <Typography color="error" sx={{ mt: 2, mb: 2 }}>
          {error}
        </Typography>
      )}

      {success && (
        <Typography color="success.main" sx={{ mt: 2, mb: 2 }}>
          Password changed successfully!
        </Typography>
      )}

      <Box component="form" onSubmit={handleSubmit} noValidate sx={{ mt: 1 }}>
        <TextField
          margin="normal"
          required
          fullWidth
          name="currentPassword"
          label="Current Password"
          type="password"
          id="currentPassword"
          autoComplete="current-password"
          value={currentPassword}
          onChange={(e) => setCurrentPassword(e.target.value)}
        />

        <TextField
          margin="normal"
          required
          fullWidth
          name="newPassword"
          label="New Password"
          type="password"
          id="newPassword"
          autoComplete="new-password"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
        />

        <TextField
          margin="normal"
          required
          fullWidth
          name="confirmPassword"
          label="Confirm New Password"
          type="password"
          id="confirmPassword"
          autoComplete="new-password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
        />

        <Box sx={{ mt: 4 }}>
          <Typography variant="h6">Password Requirements:</Typography>
          {passwordRequirements.map((req, index) => (
            <Box key={index} sx={{ display: 'flex', alignItems: 'center', mt: 1 }}>
              <Box sx={{ width: '100%', mr: 1 }}>
                <LinearProgress
                  variant="determinate"
                  value={req.check(newPassword) ? 100 : 0}
                  color={req.check(newPassword) ? 'success' : 'error'}
                />
              </Box>
              <Box sx={{ minWidth: 35 }}>
                <Typography variant="body2" color="text.secondary">
                  {req.message}
                </Typography>
              </Box>
            </Box>
          ))}
        </Box>

        <Button
          type="submit"
          fullWidth
          variant="contained"
          sx={{ mt: 3, mb: 2 }}
        >
          Change Password
        </Button>
      </Box>
    </Box>
  );
};

export default PasswordChangeForm;