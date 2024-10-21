import React, { useState, FormEvent } from 'react';
import { TextField, Button, Typography, Box, LinearProgress, Stepper, Step, StepLabel } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

const API_BASE_URL = 'https://localhost:7284';

const passwordRequirements = [
  { check: (pw: string) => pw.length >= 6, message: "At least 6 characters long" },
  { check: (pw: string) => /[0-9]/.test(pw), message: "Contains at least one digit" },
  { check: (pw: string) => /[a-z]/.test(pw), message: "Contains at least one lowercase letter" },
  { check: (pw: string) => /[A-Z]/.test(pw), message: "Contains at least one uppercase letter" },
  { check: (pw: string) => /[^A-Za-z0-9]/.test(pw), message: "Contains at least one special character" },
  { check: (pw: string) => new Set(pw).size >= 1, message: "Contains at least 1 unique character" },
];

const RegisterPage: React.FC<{ setIsLoggedIn: (isLoggedIn: boolean) => void }> = ({ setIsLoggedIn }) => {
  const [username, setUsername] = useState<string>('');
  const [email, setEmail] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [confirmPassword, setConfirmPassword] = useState<string>('');
  const [twoFactorCode, setTwoFactorCode] = useState<string>('');
  const [activeStep, setActiveStep] = useState<number>(0);
  const [error, setError] = useState<string>('');
  const [registrationCode, setRegistrationCode] = useState<string>('');
  const navigate = useNavigate();

  const steps = ['Register', 'Verify 2FA'];

  const handleRegister = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError('');

    if (password !== confirmPassword) {
      setError("Passwords do not match");
      return;
    }

    if (!passwordRequirements.every(req => req.check(password))) {
      setError("Password does not meet all requirements");
      return;
    }

    try {
      const response = await axios.post(`${API_BASE_URL}/api/user/register`, {
        username,
        email,
        password,
        registrationCode      
      });

      if (response.data && response.data.success) {
        setActiveStep(1);
      } else {
        setError('Registration failed. Please try again.');
      }
    } catch (error) {
      console.error('Registration failed:', error);
      if (axios.isAxiosError(error) && error.response) {
        setError(error.response.data.message || 'An error occurred during registration');
      } else {
        setError('An unexpected error occurred');
      }
    }
  };

  const handleVerify2FA = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError('');

    try {
      const response = await axios.post(`${API_BASE_URL}/api/user/verify2fa`, {
        email,
        twoFactorCode
      });

      if (response.data && response.data.success) {
        localStorage.setItem('userToken', response.data.token);
        setIsLoggedIn(true);
        navigate('/home');
      } else {
        setError('2FA verification failed. Please try again.');
      }
    } catch (error) {
      console.error('2FA verification failed:', error);
      if (axios.isAxiosError(error) && error.response) {
        setError(error.response.data.message || 'An error occurred during 2FA verification');
      } else {
        setError('An unexpected error occurred');
      }
    }
  };

  const getStepContent = (step: number) => {
    switch (step) {
      case 0:
        return (
          <Box component="form" onSubmit={handleRegister} noValidate sx={{ mt: 1 }}>
            <TextField
              margin="normal"
              required
              fullWidth
              id="username"
              label="Username"
              name="username"
              autoComplete="username"
              autoFocus
              value={username}
              onChange={(e) => setUsername(e.target.value)}
            />
            <TextField
              margin="normal"
              required
              fullWidth
              id="email"
              label="Email Address"
              name="email"
              autoComplete="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
            <TextField
              margin="normal"
              required
              fullWidth
              name="password"
              label="Password"
              type="password"
              id="password"
              autoComplete="new-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
            <TextField
              margin="normal"
              required
              fullWidth
              name="confirmPassword"
              label="Confirm Password"
              type="password"
              id="confirmPassword"
              autoComplete="new-password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
            />
            <TextField
              margin="normal"
              required
              fullWidth
              id="registerCode"
              label="Register Code"
              name="Register Code"
              autoComplete="Register Code"
              value={registrationCode}
              onChange={(e) => setRegistrationCode(e.target.value)}
            />
            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
            >
              Register
            </Button>
          </Box>
        );
      case 1:
        return (
          <Box component="form" onSubmit={handleVerify2FA} noValidate sx={{ mt: 1 }}>
            <TextField
              margin="normal"
              required
              fullWidth
              id="twoFactorCode"
              label="2FA Code"
              name="twoFactorCode"
              autoComplete="off"
              autoFocus
              value={twoFactorCode}
              onChange={(e) => setTwoFactorCode(e.target.value)}
            />
            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
            >
              Verify
            </Button>
          </Box>
        );
      default:
        return 'Unknown step';
    }
  };

  return (
    <Box sx={{ width: '100%', maxWidth: 400, margin: 'auto', mt: 4 }}>
      <Stepper activeStep={activeStep}>
        {steps.map((label) => (
          <Step key={label}>
            <StepLabel>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>
      <Box sx={{ mt: 4 }}>
        {getStepContent(activeStep)}
        {error && (
          <Typography color="error" sx={{ mt: 2 }}>
            {error}
          </Typography>
        )}
      </Box>
      <Box sx={{ mt: 4 }}>
        <Typography variant="h6">Password Requirements:</Typography>
        {passwordRequirements.map((req, index) => (
          <Box key={index} sx={{ display: 'flex', alignItems: 'center', mt: 1 }}>
            <Box sx={{ width: '100%', mr: 1 }}>
              <LinearProgress
                variant="determinate"
                value={req.check(password) ? 100 : 0}
                color={req.check(password) ? 'success' : 'error'}
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
    </Box>
  );
};

export default RegisterPage;