import React, { useState, useEffect, FormEvent, ChangeEvent } from 'react';
import {
  TextField,
  Button,
  Typography,
  Box,
  Link as MuiLink,
  LinearProgress,
  Alert,
  CircularProgress,
  Checkbox,
  FormControlLabel,
  FormControl,
  InputLabel,
  Select,
  MenuItem
} from '@mui/material';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { UsaStates } from 'usa-states';
import axios from 'axios';

// Custom hook to calculate password strength
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

// Password validation function
const validatePassword = (password: string): string | null => {
  if (password.length < 8) return 'Password must be at least 8 characters long.';
  if (!/[A-Z]/.test(password)) return 'Password must contain at least one uppercase letter.';
  if (!/[0-9]/.test(password)) return 'Password must contain at least one number.';
  if (!/[^A-Za-z0-9]/.test(password)) return 'Password must contain at least one special character.';
  return null;
};

// Main Register Page Component
const RegisterPage: React.FC = () => {
  // Form state
  const [formData, setFormData] = useState({
    username: '',
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    state: '',
  });
  const [enable2FA, setEnable2FA] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [passwordError, setPasswordError] = useState<string | null>(null);
  
  // Hooks
  const navigate = useNavigate();
  const location = useLocation();
  const passwordStrength = usePasswordStrength(formData.password);
  const usStates = new UsaStates().states;

  // Handle form input changes
  const handleInputChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  // Handle state select changes
  const handleStateChange = (e: any) => {
    setFormData(prev => ({
      ...prev,
      state: e.target.value
    }));
  };

  // Get color for password strength indicator
  const getStrengthColor = (strength: number): 'error' | 'warning' | 'success' => {
    if (strength < 33) return 'error';
    if (strength < 66) return 'warning';
    return 'success';
  };

  // Handle form submission
  const handleRegister = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);
    setPasswordError(null);

    // Validate password
    const passwordValidationError = validatePassword(formData.password);
    if (passwordValidationError) {
      setPasswordError(passwordValidationError);
      return;
    }

    setLoading(true);

    try {
      const response = await axios.post('/api/system/register', {
        ...formData,
        enable2FA,
        role: 'Guest'
      });

      if (response.data.requiresTwoFactor && response.data.userId) {
        // Redirect to 2FA setup with proper URL parameters
        const searchParams = new URLSearchParams();
        searchParams.set('step', '2fa');
        searchParams.set('userId', response.data.userId.toString());
        
        navigate({
          pathname: '/login',
          search: searchParams.toString()
        });
      } else {
        // Registration successful without 2FA
        navigate('/login');
      }
    } catch (error: any) {
      setError(error.response?.data?.message || 'Registration failed. Please try again.');
      console.error('Registration error:', error);
    } finally {
      setLoading(false);
    }
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

      {error && <Alert severity="error" sx={{ width: '100%', mb: 2 }}>{error}</Alert>}
      {passwordError && <Alert severity="warning" sx={{ width: '100%', mb: 2 }}>{passwordError}</Alert>}

      <Box sx={{ display: 'flex', gap: 2, width: '100%' }}>
        <TextField
          label="First Name"
          name="firstName"
          variant="outlined"
          margin="normal"
          fullWidth
          value={formData.firstName}
          onChange={handleInputChange}
          required
          disabled={loading}
        />
        <TextField
          label="Last Name"
          name="lastName"
          variant="outlined"
          margin="normal"
          fullWidth
          value={formData.lastName}
          onChange={handleInputChange}
          required
          disabled={loading}
        />
      </Box>

      <TextField
        label="Username"
        name="username"
        variant="outlined"
        margin="normal"
        fullWidth
        value={formData.username}
        onChange={handleInputChange}
        required
        disabled={loading}
      />

      <Box sx={{ display: 'flex', gap: 2, width: '100%' }}>
        <TextField
          label="Email"
          name="email"
          type="email"
          variant="outlined"
          margin="normal"
          fullWidth
          value={formData.email}
          onChange={handleInputChange}
          required
          disabled={loading}
        />

        <FormControl fullWidth margin="normal">
          <InputLabel id="state-select-label">State</InputLabel>
          <Select
            labelId="state-select-label"
            id="state-select"
            value={formData.state}
            label="State"
            onChange={handleStateChange}
            required
            disabled={loading}
          >
            {usStates.map((state) => (
              <MenuItem key={state.abbreviation} value={state.abbreviation}>
                {state.name}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      <TextField
        label="Password"
        name="password"
        type="password"
        variant="outlined"
        margin="normal"
        fullWidth
        value={formData.password}
        onChange={handleInputChange}
        required
        disabled={loading}
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
            disabled={loading}
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