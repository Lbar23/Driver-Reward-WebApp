import React, { useState, useEffect, FormEvent, ChangeEvent } from 'react';
import { TextField, Button, Typography, Box, Link as MuiLink, LinearProgress, Alert, CircularProgress, Checkbox, FormControlLabel } from '@mui/material';
import { Link, useNavigate } from 'react-router-dom'; 
import axios from 'axios';

const usePasswordStrength = (password: string): number => {
  const [strength, setStrength] = useState<number>(0);

  useEffect(() => {
    const calculateStrength = (pwd: string): number => {
      let score = 0;
      if (pwd.length >= 8) score += 1;
      if (/[A-Z]/.test(pwd)) score += 1;
      if (/[0-9]/.test(pwd)) score += 1;
      if (/[^A-Za-z0-9]/.test(pwd)) score += 1;
      return (score / 4) * 100;
    };

    setStrength(calculateStrength(password));
  }, [password]);

  return strength;
};

const validatePassword = (password: string): string | null => {
  if (password.length < 8) return 'Password must be at least 8 characters long.';
  if (!/[A-Z]/.test(password)) return 'Password must contain at least one uppercase letter.';
  if (!/[0-9]/.test(password)) return 'Password must contain at least one number.';
  if (!/[^A-Za-z0-9]/.test(password)) return 'Password must contain at least one special character.';
  return null;
};

const RegisterPage: React.FC = () => {
  const [username, setUsername] = useState<string>('');
  const [email, setEmail] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [enable2FA, setEnable2FA] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const passwordStrength = usePasswordStrength(password);

  const navigate = useNavigate(); // Use the useNavigate hook for programmatic navigation

  const handleRegister = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null); 
    setPasswordError(null); 

    const passwordValidationError = validatePassword(password);
    if (passwordValidationError) {
      setPasswordError(passwordValidationError);
      return;
    }

    setLoading(true);
    try {
      const response = await axios.post('/api/system/register', {
        username,
        email,
        password,
        enable2FA // Send the 2FA preference during registration
      });

      const loginResponse = await axios.post('/api/system/login', {
        username,
        password
      });

      if (response.data.requiresTwoFactor) {
        // Redirect to the login page with 2FA
        navigate(`/login?step=2fa&userId=${response.data.userId}`);
      } else {
        // Redirect directly to the dashboard
        navigate('/dashboard');
      }
    } 
    catch (error: any) {
      setError(error.response?.data?.message || 'Registration failed. Please try again.');
    } 
    finally {
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
      {passwordError && <Alert severity="warning">{passwordError}</Alert>}

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

      <FormControlLabel
        control={
          <Checkbox
            checked={enable2FA}
            onChange={(e) => setEnable2FA(e.target.checked)}
            color="primary"
          />
        }
        label="Enable Two-Factor Authentication (2FA)"
      />

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
