import React, { useState, FormEvent, ChangeEvent, useEffect } from 'react';
import { TextField, Button, Typography, Box } from '@mui/material';
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
  const [otp, setOtp] = useState<string>('');
  const [email, setEmail] = useState<string>('');
  const [error, setError] = useState<string>('');
  const [requiresOTP, setRequiresOTP] = useState<boolean>(false);
  const navigate = useNavigate();
  
  const handleLogin = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError('');

    try {
      const response = await axios.post(`${API_BASE_URL}/api/user/login`, { username, password });
      console.log('Login response:', response.data);

      if (response.data.requiresOTP) {
        setRequiresOTP(true);
        setEmail(response.data.email);
      } else {
        handleLoginSuccess(response.data);
      }
    } catch (error) {
      handleLoginError(error);
    }
  };

  const handleOTPSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError('');

    try {
      const response = await axios.post(`${API_BASE_URL}/api/user/verify-otp`, { email, otp });
      console.log('OTP verification response:', response.data);

      if (response.data && response.data.message === "Login successful") {
        handleLoginSuccess(response.data);
      } else {
        setError('Invalid OTP');
      }
    } catch (error) {
      handleLoginError(error);
    }
  };

  const handleLoginSuccess = (data: any) => {
    console.log('Login successful, attempting to redirect...');
    localStorage.setItem('userToken', data.token);
    setIsLoggedIn(true);
    navigate('/home');
  };

  const handleLoginError = (error: any) => {
    console.error('Login error:', error);
    if (axios.isAxiosError(error) && error.response) {
      setError(error.response.data.message || 'An error occurred. Please try again.');
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

  if (requiresOTP) {
    return (
      <Box component="form" onSubmit={handleOTPSubmit} sx={{ mt: 1 }}>
        <TextField
          margin="normal"
          required
          fullWidth
          id="otp"
          label="One-Time Password"
          name="otp"
          autoComplete="off"
          autoFocus
          value={otp}
          onChange={(e: ChangeEvent<HTMLInputElement>) => setOtp(e.target.value)}
        />
        <Button
          type="submit"
          fullWidth
          variant="contained"
          sx={{ mt: 3, mb: 2 }}
        >
          Verify OTP
        </Button>
        {error && <Typography color="error">{error}</Typography>}
      </Box>
    );
  }

  return (
    <Box component="form" onSubmit={handleLogin} sx={{ mt: 1 }}>
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

      {error && <Typography color="error">{error}</Typography>}
    </Box>
  );
};

export default LoginPage;