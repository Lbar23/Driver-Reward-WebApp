import React, { useState, useEffect, FormEvent, ChangeEvent } from 'react';
import { TextField, Button, Typography, Box, Link as MuiLink, LinearProgress, Alert, CircularProgress } from '@mui/material';
import { Link, Navigate } from 'react-router-dom';
import axios from 'axios';

const usePasswordStrength = (password: string): number => {
  const [strength, setStrength] = useState<number>(0);

  useEffect(() => {
    const calculateStrength = (pwd: string): number => {
      let score = 0;
      if (pwd.length >= 8) score += 1; // Enforce minimum 8 characters
      if (/[A-Z]/.test(pwd)) score += 1; // At least one uppercase letter
      if (/[0-9]/.test(pwd)) score += 1; // At least one number
      if (/[^A-Za-z0-9]/.test(pwd)) score += 1; // At least one special character
      return (score / 4) * 100;
    };

    setStrength(calculateStrength(password));
  }, [password]);

  return strength;
};

// Password validation function
const validatePassword = (password: string): string | null => {
  if (password.length < 8) {
    return 'Password must be at least 8 characters long.';
  }
  if (!/[A-Z]/.test(password)) {
    return 'Password must contain at least one uppercase letter.';
  }
  if (!/[0-9]/.test(password)) {
    return 'Password must contain at least one number.';
  }
  if (!/[^A-Za-z0-9]/.test(password)) {
    return 'Password must contain at least one special character.';
  }
  return null; // No validation errors
};

const RegisterPage: React.FC = () => {
  const [username, setUsername] = useState<string>('');
  const [email, setEmail] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null); // Error state
  const [passwordError, setPasswordError] = useState<string | null>(null); // Password validation error
  const passwordStrength = usePasswordStrength(password);

  const handleRegister = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null); // Reset error state before new request
    setPasswordError(null); // Reset password validation error

    // Validate password complexity
    const passwordValidationError = validatePassword(password);
    if (passwordValidationError) {
      setPasswordError(passwordValidationError);
      return; // Stop the form submission if password is invalid
    }

    setLoading(true);
    try {
      const response = await axios.post(`/api/user/register`, {
        username,
        email,
        password,
      });
      // Handle the response (assuming 2FA setup or success response)
      if (response.data.message === 'User registered successfully') {
        alert('Registration successful!');
        {<Navigate to="/dashboard" replace />}
      }
    } catch (error: any) {
      setError(error.response?.data?.message || 'Registration failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const getStrengthColor = (strength: number): 'error' | 'warning' | 'success' => {
    if (strength < 33) return 'error';
    if (strength < 66) return 'warning';
    return 'success';
  };

  return (
    <Box
      component="form"
      onSubmit={handleRegister}
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
        Create Account
      </Typography>

      {error && <Alert severity="error">{error}</Alert>}
      {passwordError && <Alert severity="warning">{passwordError}</Alert>} {/* Display password validation errors */}

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
        label="Email"
        type="email"
        variant="outlined"
        margin="normal"
        fullWidth
        value={email}
        onChange={(e: ChangeEvent<HTMLInputElement>) => setEmail(e.target.value)}
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
      <Box sx={{ width: '100%', mt: 1 }}>
        <LinearProgress
          variant="determinate"
          value={passwordStrength}
          color={getStrengthColor(passwordStrength)}
        />
        <Typography variant="body2" sx={{ mt: 1 }}>
          Password strength: {passwordStrength < 33 ? 'Weak' : passwordStrength < 66 ? 'Medium' : 'Strong'}
        </Typography>
      </Box>

      <Button
        type="submit"
        variant="contained"
        color="primary"
        size="large"
        fullWidth
        sx={{ mt: 3, mb: 2 }}
        disabled={loading}
      >
        {loading ? <CircularProgress size={24} /> : 'Register'}
      </Button>

      <MuiLink component={Link} to="/login" variant="body2">
        Already have an account? Sign in
      </MuiLink>
    </Box>
  );
};

export default RegisterPage;
