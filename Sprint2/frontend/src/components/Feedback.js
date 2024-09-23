import React, { useState } from 'react';
import { TextField, Button, Typography, Box, Snackbar, Alert } from '@mui/material';

const FeedbackForm = () => {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = (e) => {
    e.preventDefault();
    // Basic form validation
    if (!name || !email || !message) {
      setError('Please fill in all fields');
      return;
    }
    if (!/\S+@\S+\.\S+/.test(email)) {
      setError('Please enter a valid email address');
      return;
    }
    // Here you would typically send the feedback to your server
    console.log('Feedback submitted:', { name, email, message });
    setSubmitted(true);
    // Reset form
    setName('');
    setEmail('');
    setMessage('');
    setError('');
  };

  const handleCloseSnackbar = (event, reason) => {
    if (reason === 'clickaway') {
      return;
    }
    setSubmitted(false);
  };

  return (
    <Box
      component="form"
      onSubmit={handleSubmit}
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        maxWidth: 600,
        margin: 'auto',
        padding: 3,
        boxShadow: 3,
        borderRadius: 2,
        backgroundColor: 'background.paper',
      }}
    >
      <Typography variant="h4" component="h1" gutterBottom>
        Provide Feedback
      </Typography>
      <TextField
        label="Your Name"
        variant="outlined"
        margin="normal"
        fullWidth
        value={name}
        onChange={(e) => setName(e.target.value)}
        required
      />
      <TextField
        label="Your Email"
        type="email"
        variant="outlined"
        margin="normal"
        fullWidth
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        required
      />
      <TextField
        label="Your Feedback"
        variant="outlined"
        margin="normal"
        fullWidth
        multiline
        rows={4}
        value={message}
        onChange={(e) => setMessage(e.target.value)}
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
        Submit Feedback
      </Button>
      {error && (
        <Typography color="error" sx={{ mt: 2 }}>
          {error}
        </Typography>
      )}
      <Snackbar open={submitted} autoHideDuration={6000} onClose={handleCloseSnackbar}>
        <Alert onClose={handleCloseSnackbar} severity="success" sx={{ width: '100%' }}>
          Thank you for your feedback!
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default FeedbackForm;