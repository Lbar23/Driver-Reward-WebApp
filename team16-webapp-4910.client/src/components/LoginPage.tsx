import React, { useState, FormEvent, ChangeEvent, useEffect } from 'react';
import { TextField, Button, Typography, Box, Stepper, Step, StepLabel, Checkbox, FormControlLabel } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

const API_BASE_URL = 'https://localhost:7284';

declare global {
  interface Window {
    google: any;
  }
}

const LoginPage: React.FC<{ setIsLoggedIn: (isLoggedIn: boolean) => void }> = ({ setIsLoggedIn }) => {
  const [username, setUsername] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [twoFactorCode, setTwoFactorCode] = useState<string>('');
  const [email, setEmail] = useState<string>('');
  const [rememberMe, setRememberMe] = useState<boolean>(false);
  const [error, setError] = useState<string>('');
  const [activeStep, setActiveStep] = useState<number>(0);
  const [enableTwoFactor, setEnableTwoFactor] = useState<boolean>(false);
  const [userId, setUserId] = useState<string>('');
  const navigate = useNavigate();

  const steps = ['Login', 'Two-Factor Authentication'];

  useEffect(() => {
    axios.defaults.withCredentials = true;
  }, []);

  const handleLogin = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError('');

    try {
      const response = await axios.post(`${API_BASE_URL}/api/user/login`, { username, password, rememberMe });
      console.log('Login response:', response.data);

      if (response.data.requiresTwoFactor || enableTwoFactor) {
        setActiveStep(1);
        setEmail(response.data.email);
        setUserId(response.data.userId);
        await sendTwoFactorCode(response.data.userId);
      } else {
        handleLoginSuccess(response.data);
      }
    } catch (error) {
      handleLoginError(error);
    }
  };

  const sendTwoFactorCode = async (userId) => {
    try {
      const response = await axios.post(`${API_BASE_URL}/api/user/send-2fa-code`, { userId });
      console.log('2FA code sent:', response.data);
      if (response.data.success) {
        setError('A 2FA code has been sent to your email.');
      } else {
        setError('Failed to send 2FA code. Please try again.');
      }
    } catch (error) {
      console.error('Error sending 2FA code:', error);
      setError('An error occurred while sending the 2FA code.');
    }
  };

  const handleTwoFactorSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError('');

    try {
      const response = await axios.post(`${API_BASE_URL}/api/user/verify-2fa`, { token: twoFactorCode, rememberMe });
      console.log('2FA verification response:', response.data);

      if (response.data && response.data.success) {
        handleLoginSuccess(response.data);
      } else {
        setError('Invalid 2FA code');
      }
    } catch (error) {
      handleLoginError(error);
    }
  };

  const handleLoginSuccess = (data: any) => {
    console.log('Login successful, attempting to redirect...');
    setIsLoggedIn(true);
    navigate('/home');
  };

  const handleLoginError = (error: any) => {
    console.error('Login error:', error);
    if (axios.isAxiosError(error) && error.response) {
      setError(error.response.data.message || 'An error occurred. Please try again.');
      console.error('Login failed:', error.response.data);
      console.error('Status code:', error.response.status);
      console.error('Headers:', error.response.headers);
    } else if (error.response) {
      console.error('Login failed:', error.response.data);
      console.error('Status code:', error.response.status);
      console.error('Headers:', error.response.headers);
    } else {
      setError('An unexpected error occurred. Please try again.');
    }
  };

  useEffect(() => {
    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    document.body.appendChild(script);

    script.onload = () => {
      window.google.accounts.id.initialize({
        client_id: '446234699257-5k91m28pvfpk6mov3gr9pi190p261d8r.apps.googleusercontent.com',
        callback: handleGoogleSignIn
      });
      window.google.accounts.id.renderButton(
        document.getElementById('googleSignInButton'),
        { theme: 'outline', size: 'large' }
      );
    };

    return () => {
      document.body.removeChild(script);
    };
  }, []);

  const handleGoogleSignIn = async (response: any) => {
    try {
      const backendResponse = await axios.post(`${API_BASE_URL}/api/user/google-sign-in`, {
        token: response.credential
      });

      if (backendResponse.data && backendResponse.data.success) {
        handleLoginSuccess(backendResponse.data);
      } else {
        setError('Google sign-in failed');
      }
    } catch (error) {
      handleLoginError(error);
    }
  };

  const getStepContent = (step: number) => {
    switch (step) {
      case 0:
        return (
          <Box component="form" onSubmit={handleLogin} sx={{ mt: 1 }}>
            <TextField
              margin="normal"
              required
              fullWidth
              id="username"
              label="Username or Email"
              name="username"
              autoComplete="username"
              autoFocus
              value={username}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setUsername(e.target.value)}
            />
            <TextField
              margin="normal"
              required
              fullWidth
              name="password"
              label="Password"
              type="password"
              id="password"
              autoComplete="current-password"
              value={password}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setPassword(e.target.value)}
            />
            <FormControlLabel
              control={<Checkbox value="remember" color="primary" />}
              label="Remember me"
              checked={rememberMe}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setRememberMe(e.target.checked)}
            />
            <FormControlLabel
              control={<Checkbox value="enableTwoFactor" color="primary" />}
              label="Enable Two-Factor Authentication (for testing)"
              checked={enableTwoFactor}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setEnableTwoFactor(e.target.checked)}
            />
            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
            >
              Sign In
            </Button>

            <Typography variant="body1" sx={{ mt: 2, mb: 1 }}>Or sign in with:</Typography>
            <div id="googleSignInButton"></div>
          </Box>
        );
      case 1:
        return (
          <Box component="form" onSubmit={handleTwoFactorSubmit} sx={{ mt: 1 }}>
            <TextField
              margin="normal"
              required
              fullWidth
              id="twoFactorCode"
              label="Two-Factor Authentication Code"
              name="twoFactorCode"
              autoComplete="off"
              autoFocus
              value={twoFactorCode}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setTwoFactorCode(e.target.value)}
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
    </Box>
  );
};

export default LoginPage;