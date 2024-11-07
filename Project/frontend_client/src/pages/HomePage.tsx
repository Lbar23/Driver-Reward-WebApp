import React from 'react';
import { Box, Typography, Button, Container, Grid } from '@mui/material';
import { useNavigate } from 'react-router-dom';

const HomePage: React.FC = () => {
  const navigate = useNavigate();

  return (
    <Container maxWidth="md">
      {/* Hero Section */}
      <Box sx={{ 
        textAlign: 'center', 
        py: 8,
        backgroundColor: 'background.paper',
        borderRadius: 2,
        mb: 4 
      }}>
        <Typography variant="h2" component="h1" gutterBottom>
          Git Gud Drivers
        </Typography>
        <Typography variant="h5" color="text.secondary" paragraph>
          A rewards program for professional drivers committed to excellence
        </Typography>
        <Button 
          variant="contained" 
          size="large"
          onClick={() => navigate('/register')}
          sx={{ mr: 2 }}
        >
          Sign Up
        </Button>
        <Button 
          variant="outlined" 
          size="large"
          onClick={() => navigate('/login')}
        >
          Sign In
        </Button>
      </Box>

      {/* Features Grid */}
      <Grid container spacing={4} sx={{ mb: 6 }} justifyContent="center" alignItems="center">
        <Grid item xs={12} md={4}>
            <Typography variant="h5" gutterBottom>
              How It Works
            </Typography>
            <Typography>
              1. Apply as a driver<br/>
              2. Get approved by a sponsor<br/>
              3. Start earning points<br/>
              4. Redeem rewards
            </Typography>
            <Typography variant="h6" gutterBottom>
                <br/>
                We hope to see you soon!
            </Typography>
        </Grid>
      </Grid>
    </Container>
  );
};

export default HomePage;