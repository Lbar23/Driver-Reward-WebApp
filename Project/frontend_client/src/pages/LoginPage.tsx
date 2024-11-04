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

      if (response.data.requiresTwoFactor) {
        setStep('2fa');
        setUserId(response.data.userId.toString());
        navigate(`/login?step=2fa&userId=${response.data.userId}`, { replace: true });
      } 
      else if (response.data.succeeded) {
        setRedirectToDashboard(true);
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
        withCredentials: true,
      });

      if (response.data.succeeded) {
        setRedirectToDashboard(true);
      } 
      else {
        setError(response.data.message || 'Invalid 2FA code. Please try again.');
      }
    } 
    catch (err: any) {
      setError(err.response?.data?.message || '2FA verification failed. Please try again.');
    } 
    finally {
      setLoading(false);
    }
  };

  if (redirectToDashboard) {
    //return <Navigate to="/dashboard" replace />;
    navigate('/dashboard');
  }

  const formAriaLabel = step === 'login' 
  ? 'Login Form' 
  : 'Two-Factor Authentication Form';

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
      role = "form"
      aria-label={formAriaLabel}
    >
      <Typography variant="h1"
      sx={{fontSize: 'h4.fontSize', mb: 2, color: 'text.primary'}}>
        {step === 'login' ? 'Login' : '2FA Verification'}
      </Typography>

      {error && <Alert severity="error" sx={{ mb: 2 }} role="alert" aria-live="assertive">{error}</Alert>}

      {step === 'login' ? (
        <>
          <TextField
            label="Username"
            variant="outlined"
            margin="normal"
            fullWidth
            autoComplete="username"
            value={username}
            onChange={(e: ChangeEvent<HTMLInputElement>) => setUsername(e.target.value)}
            required
            inputProps={{ 'aria-label': 'Username', 'aria-required': 'true'}}
            helperText="Enter your username"
          />
          <TextField
            label="Password"
            type="password"
            variant="outlined"
            margin="normal"
            fullWidth
            autoComplete="current-password"
            value={password}
            onChange={(e: ChangeEvent<HTMLInputElement>) => setPassword(e.target.value)}
            required
            inputProps={{'aria-label': 'Password', 'aria-required': 'true'}}
            helperText="Enter your password"
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
          inputProps={{'aria-label': 'Password', 'aria-required': 'true', maxLength: 6, inputMode: 'numeric', pattern: '[0-9]*'}}
          helperText="Enter the 6-digit code from your authenticator app"
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
        aria-busy={loading}
      >
        {loading ?( <>
        <CircularProgress size={24} aria-label="Loading"/>
        <span className ="sr-only">Processing...</span>
        </>
        ) 
         :( step === 'login' ? 'Log In' : 'Verify 2FA')}
      </Button>

      {step === 'login' && (
        <Box sx={{ textAlign: 'center', mt: 2 }}
             role = "navigation"
             aria-label = "Additional Account Options">
          <MuiLink component={Link} to="/register" variant="body2" sx={{ display: 'block', mb: 1, textDecoration: 'underline' }}
          aria-label="Create new account"
          >
            Don't have an account? Sign Up
          </MuiLink>
          <MuiLink component={Link} to="/reset-password" variant="body2" sx={{ textDecoration: 'underline' }}
          aria-label="Reset password"
          >
            Forgot Password? Click here to Reset it
          </MuiLink>
        </Box>
      )}
    </Box>
  );
};

export default LoginPage;
