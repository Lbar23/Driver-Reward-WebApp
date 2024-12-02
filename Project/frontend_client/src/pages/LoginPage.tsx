import React, { useState, FormEvent, ChangeEvent, useEffect } from 'react';
import {
  TextField,
  Button,
  Typography,
  Box,
  Link as MuiLink,
  Alert,
  CircularProgress
} from '@mui/material';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import axios from 'axios';
import { useAuth } from '../service/authContext';

const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { checkAuth, isAuthenticated, isLoading } = useAuth();

  // Setup for login steps (2FA or standard login) based on URL parameters
  const queryParams = new URLSearchParams(location.search);
  const initialStep = queryParams.get('step') === '2fa' ? '2fa' : 'login';
  const initialUserId = queryParams.get('userId');

  const [step, setStep] = useState<'login' | '2fa'>(initialStep);
  const [userId, setUserId] = useState<string | null>(initialUserId);
  const [loading, setLoading] = useState(false);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [twoFactorCode, setTwoFactorCode] = useState('');
  const [error, setError] = useState<string | null>(null);

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated && !isLoading) {
      navigate('/dashboard');
    }
  }, [isAuthenticated, isLoading, navigate]);

  // Handle login submission
  const handleLogin = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const response = await axios.post('/api/system/login', {
        username: username.trim(),
        password
      }, { withCredentials: true });

      if (response.data.requiresTwoFactor) {
        // Update URL for 2FA step
        const searchParams = new URLSearchParams();
        searchParams.set('step', '2fa');
        searchParams.set('userId', response.data.userId.toString());
        
        // Update local state
        setStep('2fa');
        setUserId(response.data.userId.toString());
        
        // Update URL without reloading
        navigate({
          pathname: '/login',
          search: searchParams.toString()
        }, { replace: true });
      } else {
        // Standard login successful
        await checkAuth(); // Update auth context
        navigate('/dashboard');
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Login failed. Please try again.');
      console.error('Login error:', err);
    } finally {
      setLoading(false);
    }
  };

  // Handle 2FA verification
  const handle2FA = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!userId) {
      setError('Invalid session. Please try logging in again.');
      navigate('/login', { replace: true });
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const response = await axios.post(
        '/api/system/verify-2fa',
        {
          userId: userId.toString(),
          code: twoFactorCode.trim()
        },
        { withCredentials: true }
      );

      if (response.data.succeeded) {
        await checkAuth(); // Update auth context with new session
        navigate('/dashboard', { replace: true });
      } else {
        throw new Error(response.data.message || '2FA verification failed');
      }
    } catch (err: any) {
      console.error('2FA error:', err);
      setError(err.response?.data?.message || 'Verification failed. Please try again.');
      
      if (err.response?.status === 401) {
        setTimeout(() => {
          navigate('/login', { replace: true });
        }, 2000);
      }
    } finally {
      setLoading(false);
    }
  };

  // Reset form and go back to login
  const handleReset = () => {
    setError(null);
    setStep('login');
    setUserId(null);
    setTwoFactorCode('');
    navigate('/login', { replace: true });
  };

  // Don't render if already authenticated
  if (isAuthenticated && !isLoading) {
    return null;
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

      {error && (
        <Alert 
          severity="error" 
          sx={{ mb: 2, width: '100%' }}
          action={
            <Button color="inherit" size="small" onClick={handleReset}>
              Start Over
            </Button>
          }
        >
          {error}
        </Alert>
      )}

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
            disabled={loading}
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
            disabled={loading}
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
          disabled={loading}
          autoFocus
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

      {step === '2fa' && (
        <Button
          variant="text"
          color="primary"
          onClick={handleReset}
          disabled={loading}
        >
          Back to Login
        </Button>
      )}

      {step === 'login' && (
        <Box sx={{ textAlign: 'center', mt: 2 }}>
          <MuiLink component={Link} to="/register" variant="body2" sx={{ display: 'block', mb: 1 }}>
            Don't have an account? Sign Up
          </MuiLink>
          <MuiLink component={Link} to="/reset-password" variant="body2">
            Forgot Password? Click here to Reset it
          </MuiLink>
        </Box>
      )}
    </Box>
  );
};

export default LoginPage;