import React, { useState, FormEvent, ChangeEvent } from 'react';
import { TextField, Button, Typography, Box, Link as MuiLink, Alert, CircularProgress } from '@mui/material';
import { Link } from 'react-router-dom';
import axios from 'axios';

const LoginPage: React.FC = () => {
  const [step, setStep] = useState<'login' | '2fa'>('login');  
  const [userId, setUserId] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(false); // Loading state
  const [username, setUsername] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [twoFactorCode, setTwoFactorCode] = useState<string>(''); // 2FA code input
  const [error, setError] = useState<string | null>(null); 

  const handleLogin = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setLoading(true);
    try {
      const response = await axios.post('/api/user/login', { 
        username, 
        password 
      });
      // If 2FA is required, move to the 2FA step
      if (response.data.message === '2FA required') {
        setStep('2fa');
        setUserId(response.data.userId); // Save the user ID for 2FA verification
      } 
      else {
        // If login is successful without 2FA
        alert('Login successful!');
      }
    } 
    catch (err: any) {
      setError(err.response?.data?.message || 'Login failed. Please try again.');
    } 
    finally {
      setLoading(false);
    }
   
  };

// Handle 2FA form submission
const handle2FA = async (e: FormEvent<HTMLFormElement>) => {
  e.preventDefault();
  setLoading(true);

  try {
    const response = await axios.post('/api/user/verify-2fa', {
        userId: String(userId),
        code: twoFactorCode
    });

    // If 2FA verification is successful
    if (response.data.message === '2FA successful') {
      alert('Login successful!');
    }
  } 
  catch (err: any) {
    setError(err.response?.data?.message || 'Invalid 2FA code. Please try again.');
  } 
  finally {
    setLoading(false);
  }
};
  return (
    <Box
      component="form"
      onSubmit={step === 'login' ? handleLogin : handle2FA} // Switch form submit handler based on step
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
      <MuiLink component={Link} to="/register" variant="body2">
        Don't have an account? Sign Up
      </MuiLink>
      <MuiLink component={Link} to="/reset-password" variant="body2">
        Forgot Password? Click here to Reset it
      </MuiLink>
    </Box>
  );
};

export default LoginPage;