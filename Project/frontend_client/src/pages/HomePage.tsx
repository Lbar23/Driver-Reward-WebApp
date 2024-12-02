import React from 'react';
import { 
  Box, 
  Typography, 
  Button, 
  Container, 
  Grid2, 
  Paper, 
  Avatar
} from '@mui/material';
import { 
  DirectionsCar as CarIcon, 
  EmojiEvents as RewardsIcon, 
  PlaylistAddCheck as StepsIcon 
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';

const FeatureCard: React.FC<{
  icon: React.ReactNode;
  title: string;
  description: string;
}> = ({ icon, title, description }) => (
  <Paper
    elevation={3}
    sx={{
      p: 3,
      height: '100%',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      textAlign: 'center',
      transition: theme => theme.transitions.create('transform', {
        duration: theme.transitions.duration.shortest
      }),
      '&:hover': {
        transform: 'translateY(-8px)',
        boxShadow: theme => theme.shadows[6]
      }
    }}
  >
    <Avatar 
      sx={{ 
        mb: 2, 
        width: 72, 
        height: 72, 
        bgcolor: 'primary.main', 
        color: 'primary.contrastText' 
      }}
    >
      {icon}
    </Avatar>
    <Typography variant="h6" gutterBottom>
      {title}
    </Typography>
    <Typography variant="body2" color="text.secondary">
      {description}
    </Typography>
  </Paper>
);

const HomePage: React.FC = () => {
  const navigate = useNavigate();

  return (
    <Box 
      sx={{ 
        background: theme => `linear-gradient(135deg, ${theme.palette.background.default}, ${theme.palette.background.paper})`,
        minHeight: '100vh',
        pt: 8,
        pb: 8
      }}
    >
      <Container maxWidth="lg">
        {/* Hero Section */}
        <Box 
          sx={{ 
            textAlign: 'center', 
            mb: 8,
            p: 6,
            borderRadius: 3,
            backgroundColor: theme => theme.palette.background.paper,
            boxShadow: theme => theme.shadows[3],
            animation: theme => theme.transitions.create('opacity', { duration: theme.transitions.duration.standard }),
          }}
        >
          <Typography 
            variant="h2" 
            component="h1" 
            sx={{ 
              mb: 3,
              fontWeight: 700,
              background: theme => `linear-gradient(45deg, ${theme.palette.primary.dark}, ${theme.palette.primary.light})`,
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent'
            }}
          >
            GitGud Drivers
          </Typography>
          <Typography 
            variant="h6" 
            color="text.primary" 
            sx={{ mb: 4, maxWidth: 700, mx: 'auto' }}
          >
            Join a rewards program that helps professional drivers achieve excellence and unlock valuable rewards.
          </Typography>
          <Box>
            <Button 
              variant="contained" 
              size="large"
              onClick={() => navigate('/register')}
              sx={{ 
                mr: 2,
                px: 5,
                py: 2,
                textTransform: 'none'
              }}
            >
              Join Now
            </Button>
            <Button 
              variant="outlined" 
              size="large"
              onClick={() => navigate('/login')}
              sx={{
                px: 5,
                py: 2,
                textTransform: 'none'
              }}
            >
              Sign In
            </Button>
          </Box>
        </Box>

        {/* Features Section */}
        <Typography 
          variant="h4" 
          align="center" 
          gutterBottom
          sx={{ 
            mb: 4,
            fontWeight: 600,
            color: 'primary.main'
          }}
        >
          Why Join GitGud Drivers
        </Typography>
        <Grid2 container spacing={4} sx={{ mb: 6 }}>
          <Grid2 size={{ xs: 12, sm: 6, md: 4 }}>
            <FeatureCard 
              icon={<CarIcon fontSize="large" />}
              title="Professional Drivers"
              description="Join a community of drivers committed to excellence."
            />
          </Grid2>
          <Grid2 size={{ xs: 12, sm: 6, md: 4 }}>
            <FeatureCard 
              icon={<RewardsIcon fontSize="large" />}
              title="Reward System"
              description="Earn points for safety, customer satisfaction, and professionalism."
            />
          </Grid2>
          <Grid2 size={{ xs: 12, sm: 6, md: 4 }}>
            <FeatureCard 
              icon={<StepsIcon fontSize="large" />}
              title="Easy Process"
              description="Simple onboarding and transparent tracking."
            />
          </Grid2>
        </Grid2>

        {/* Steps Section */}
        <Box 
          sx={{ 
            bgcolor: 'background.paper', 
            p: 4, 
            borderRadius: 3, 
            boxShadow: theme => theme.shadows[2]
          }}
        >
          <Typography 
            variant="h5" 
            align="center" 
            gutterBottom
            sx={{ 
              mb: 4,
              fontWeight: 600,
              color: 'primary.main'
            }}
          >
            Your Journey in 4 Steps
          </Typography>
          <Grid2 container spacing={3}>
            {['Sign Up', 'Get Verified', 'Start Driving', 'Earn Rewards'].map((step, index) => (
              <Grid2 size={{ xs: 12, sm: 6, md: 3 }} key={index}>
                <Box 
                  sx={{ 
                    display: 'flex', 
                    flexDirection: 'column', 
                    alignItems: 'center',
                    textAlign: 'center'
                  }}
                >
                  <Avatar 
                    sx={{ 
                      mb: 2, 
                      bgcolor: 'primary.light', 
                      color: 'primary.contrastText',
                      width: 56,
                      height: 56 
                    }}
                  >
                    {index + 1}
                  </Avatar>
                  <Typography variant="body1">{step}</Typography>
                </Box>
              </Grid2>
            ))}
          </Grid2>
        </Box>
      </Container>
    </Box>
  );
};

export default HomePage;
