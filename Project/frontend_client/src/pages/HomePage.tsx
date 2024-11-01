import React from 'react';
import { Box, Typography, Button, Container, Grid } from '@mui/material';
import { useNavigate } from 'react-router-dom';

const HomePage: React.FC = () => {
  const navigate = useNavigate();

  return (
    <Container 
      maxWidth="lg"
      component="main"
      role="main"
      aria-label="Git Gud Drivers Homepage"
    >
      {/* Hero Section */}
      <Box 
        component="section"
        aria-labelledby="hero-title"
        sx={{ 
          textAlign: 'center', 
          py: 8,
          backgroundColor: 'background.paper',
          borderRadius: 2,
          mb: 4 
        }}
      >
        <Typography 
          variant="h2" 
          component="h1" 
          gutterBottom
          id="hero-title"
        >
          Git Gud Drivers
        </Typography>
        <Typography 
          variant="h5" 
          color="text.secondary" 
          paragraph
          component="p"
        >
          A rewards program for professional drivers committed to excellence
        </Typography>
        <Box role="navigation" aria-label="Account access">
          <Button 
            variant="contained" 
            size="large"
            onClick={() => navigate('/register')}
            sx={{ 
              mr: 2,
              // Ensure sufficient touch target size
              minHeight: '44px',
              minWidth: '88px'
            }}
            aria-label="Sign up for Git Gud Drivers"
          >
            Sign Up
          </Button>
          <Button 
            variant="outlined" 
            size="large"
            onClick={() => navigate('/login')}
            sx={{
              // Ensure sufficient touch target size
              minHeight: '44px',
              minWidth: '88px'
            }}
            aria-label="Sign in to your account"
          >
            Sign In
          </Button>
        </Box>
      </Box>

      {/* Features Grid */}
      <Grid 
        container 
        spacing={4} 
        sx={{ mb: 6 }} 
        justifyContent="center" 
        alignItems="center"
        component="section"
        aria-labelledby="how-it-works-title"
      >
        <Grid 
          item 
          xs={12} 
          md={4}
          component="article"
          role="article"
        >
          <Typography 
            variant="h5" 
            gutterBottom
            component="h2"
            id="how-it-works-title"
          >
            How It Works
          </Typography>
          <Box 
            component="ol" 
            sx={{ 
              listStyleType: 'decimal',
              pl: 3, // Add padding for list items
              mb: 2
            }}
            aria-label="Steps to join Git Gud Drivers"
          >
            <li>Apply as a driver</li>
            <li>Get approved by a sponsor</li>
            <li>Start earning points</li>
            <li>Redeem rewards</li>
          </Box>
          <Typography 
            variant="h6" 
            gutterBottom
            component="p"
            sx={{ mt: 2 }}
          >
            We hope to see you soon!
          </Typography>
        </Grid>
      </Grid>
    </Container>
  );
};

export default HomePage;