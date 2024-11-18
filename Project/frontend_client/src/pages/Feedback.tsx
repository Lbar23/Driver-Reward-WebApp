import React, { useState, FormEvent, ChangeEvent } from 'react';
import { TextField, Button, Typography, Box, Snackbar, Alert } from '@mui/material';
import axios from 'axios';

const FeedbackForm: React.FC = () => {
  const [name, setName] = useState<string>('');
  const [email, setEmail] = useState<string>('');
  const [message, setMessage] = useState<string>('');
  const [submitted, setSubmitted] = useState<boolean>(false);
  const [error, setError] = useState<string>('');

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (!name || !email || !message) {
      setError('Please fill in all fields');
      return;
    }
    if (!/\S+@\S+\.\S+/.test(email)) {
      setError('Please enter a valid email address');
      return;
    }

    try {
      await axios.post('/api/feedback', {
        Name: name,
        Email: email,
        Description: message,
        SubmissionDate: new Date().toISOString(),
      });
      console.log('Feedback submitted:', { name, email, message });
      setSubmitted(true);
      setName('');
      setEmail('');
      setMessage('');
      setError('');
    } catch (err) {
      console.error('Error submitting feedback:', err);
      setError('An error occurred while submitting your feedback. Please try again later.');
    }
  };

  const handleCloseSnackbar = (_event: React.SyntheticEvent | Event, reason?: string) => {
    if (reason === 'clickaway') return;
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
        p: 3,
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
        fullWidth
        margin="normal"
        value={name}
        onChange={(e: ChangeEvent<HTMLInputElement>) => setName(e.target.value)}
        required
      />
      <TextField
        label="Your Email"
        type="email"
        fullWidth
        margin="normal"
        value={email}
        onChange={(e: ChangeEvent<HTMLInputElement>) => setEmail(e.target.value)}
        required
      />
      <TextField
        label="Your Feedback"
        fullWidth
        margin="normal"
        multiline
        rows={4}
        value={message}
        onChange={(e: ChangeEvent<HTMLInputElement>) => setMessage(e.target.value)}
        required
      />
      <Button type="submit" variant="contained" size="large" fullWidth sx={{ mt: 3, mb: 2 }}>
        Submit Feedback
      </Button>
      {error && <Typography color="error" sx={{ mt: 2 }}>{error}</Typography>}
      <Snackbar open={submitted} autoHideDuration={6000} onClose={handleCloseSnackbar}>
        <Alert onClose={handleCloseSnackbar} severity="success" sx={{ width: '100%' }}>
          Thank you for your feedback!
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default FeedbackForm;