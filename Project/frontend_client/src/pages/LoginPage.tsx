import React, { useState, FormEvent, ChangeEvent, useEffect } from 'react';
import { TextField, Button, Typography, Box, Link as MuiLink, Alert, CircularProgress } from '@mui/material';
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom';
import axios from 'axios';

const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  
  const queryParams = new URLSearchParams(location.search);
  const initialStep = queryParams.get('step') === '2fa' ? '2fa' : 'login';
  const initialUserId = queryParams.get('userId');

  const [step, setStep] = useState<'login' | '2fa'>(initialStep);
  const [userId, setUserId] = useState<string | null>(initialUserId || null);
  const [loading, setLoading] = useState<boolean>(false);
  const [username, setUsername] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [twoFactorCode, setTwoFactorCode] = useState<string>(''); 
  const [error, setError] = useState<string | null>(null);
  const [redirectToDashboard, setRedirectToDashboard] = useState<boolean>(false);

  // Ensure 2FA form shows if redirected from login with query params.
  useEffect(() => {
    if (initialStep === '2fa' && initialUserId) {
      setStep('2fa');
      setUserId(initialUserId);
    }
  }, [initialStep, initialUserId]);

  const handleLogin = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    
    try {
      const response = await axios.post('/api/user/login', { username, password });

      // Identity returns requiresTwoFactor when 2FA is needed
      if (response.data.requiresTwoFactor) {
        setStep('2fa');
        setUserId(response.data.userId.toString()); // Ensure userId is a string
        navigate(`/login?step=2fa&userId=${response.data.userId}`, { replace: true }); // Update URL with 2FA step
      } 
      else if (response.data.succeeded) {
        setRedirectToDashboard(true); // User logged in successfully without 2FA
      } 
      else {
        setError(response.data.message || 'Login failed. Please try again.');
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Login failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handle2FA = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setLoading(true);

    try {
      const response = await axios.post('/api/user/verify-2fa', {
        userId: String(userId),
        code: twoFactorCode
      },{
        withCredentials: true, // Make sure cookies are included in the request
      });

      if (response.data.succeeded) {
        setRedirectToDashboard(true); // Redirect after successful 2FA verification
      } else {
        setError(response.data.message || 'Invalid 2FA code. Please try again.');
      }
    } catch (err: any) {
      setError(err.response?.data?.message || '2FA verification failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  if (redirectToDashboard) {
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <Box
      component="form"
      onSubmit={step === 'login' ? handleLogin : handle2FA}
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
        {step === 'login' ? 'Login' : '2FA Verification'}
      </Typography>

      {error && <Alert severity="error">{error}</Alert>}

      {step === 'login' ? (
        <>
          <TextField
            label="Username"
            variant="outlined"
            margin="normal"
            fullWidth
            value={username}
            onChange={(e: ChangeEvent<HTMLInputElement>) => setUsername(e.target.value)}
            required
          />
          <TextField
            label="Password"
            type="password"
            variant="outlined"
            margin="normal"
            fullWidth
            value={password}
            onChange={(e: ChangeEvent<HTMLInputElement>) => setPassword(e.target.value)}
            required
          />
        </>
      ) : (
        <TextField
          label="2FA Code"
          variant="outlined"
          margin="normal"
          fullWidth
          value={twoFactorCode}
          onChange={(e: ChangeEvent<HTMLInputElement>) => setTwoFactorCode(e.target.value)}
          required
        />
      )}

      <Button
        type="submit"
        variant="contained"
        color="primary"
        size="large"
        fullWidth
        sx={{ mt: 3, mb: 2 }}
        disabled={loading}
      >
        {loading ? <CircularProgress size={24} /> : step === 'login' ? 'Log In' : 'Verify 2FA'}
      </Button>

      {step === 'login' && (
        <>
          <MuiLink component={Link} to="/register" variant="body2">
            Don't have an account? Sign Up
          </MuiLink>
          <MuiLink component={Link} to="/reset-password" variant="body2">
            Forgot Password? Click here to Reset it
          </MuiLink>
        </>
      )}
    </Box>
  );
};

export default LoginPage;
