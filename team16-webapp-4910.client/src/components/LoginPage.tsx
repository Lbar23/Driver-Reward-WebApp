import React, { useState, FormEvent, ChangeEvent } from 'react';
import { TextField, Button, Typography, Box, Link as MuiLink } from '@mui/material';
import { Link } from 'react-router-dom';
import axios from 'axios';

const LoginPage: React.FC = () => {
  const [username, setUsername] = useState<string>('');
  const [password, setPassword] = useState<string>('');

  const handleLogin = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    // Here you would typically handle the login logic
    console.log('Login attempted with:', username, password);
  };

  return (
    <Box
      component="form"
      onSubmit={handleLogin}
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
        Welcome Back
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
        label="Password"
        type="password"
        variant="outlined"
        margin="normal"
        fullWidth
        value={password}
        onChange={(e: ChangeEvent<HTMLInputElement>) => setPassword(e.target.value)}
        required
      />
      <Button
        type="submit"
        variant="contained"
        color="primary"
        size="large"
        fullWidth
        sx={{ mt: 3, mb: 2 }}
      >
        Log In
      </Button>
      <MuiLink component={Link} to="/register" variant="body2">
        Don't have an account? Sign Up
      </MuiLink>
    </Box>
  );
};

export default LoginPage;