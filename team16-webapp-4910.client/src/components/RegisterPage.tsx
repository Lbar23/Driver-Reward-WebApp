import React, { useState, useEffect, FormEvent, ChangeEvent } from 'react';
import { TextField, Button, Typography, Box, Link as MuiLink, LinearProgress } from '@mui/material';
import { Link } from 'react-router-dom';
import axios from 'axios';

const API_BASE_URL = 'https://localhost:7284';

const usePasswordStrength = (password: string): number => {
  const [strength, setStrength] = useState<number>(0);

  useEffect(() => {
    const calculateStrength = (pwd: string): number => {
      let score = 0;
      if (pwd.length > 6) score += 1;
      if (pwd.length > 10) score += 1;
      if (/[A-Z]/.test(pwd)) score += 1;
      if (/[0-9]/.test(pwd)) score += 1;
      if (/[^A-Za-z0-9]/.test(pwd)) score += 1;
      return (score / 5) * 100;
    };

    setStrength(calculateStrength(password));
  }, [password]);

  return strength;
};

const RegisterPage: React.FC = () => {
  const [username, setUsername] = useState<string>('');
  const [email, setEmail] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const passwordStrength = usePasswordStrength(password);

    const handleRegister = async (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        try {
            const response = await axios.post(`${API_BASE_URL}/api/user/register`, {
                username,
                email,
                password,
                registrationCode: "DRIVER" // or whatever default you want
            });
            console.log('Registration successful:', response.data);
        } catch (error) {
          if (axios.isAxiosError(error)){ //check for axios issues
            console.error('Registration failed:', error.response?.data);
          }
          else if (error instanceof Error){ //should catch js
            console.error('Registration failed:', error.message);
          }
          else{ 
            console.error('An unknown error occurred:', JSON.stringify(error));
          }
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
      >
        Register
      </Button>
      <MuiLink component={Link} to="/login" variant="body2">
        Already have an account? Sign in
      </MuiLink>
    </Box>
  );
};

export default RegisterPage;