import React from 'react';
import { 
  Box, 
  Typography, 
  Button, 
  Container, 
  Grid, 
  Paper, 
  Avatar,
  Divider
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
      transition: 'all 0.3s ease',
      '&:hover': {
        transform: 'translateY(-10px)',
        boxShadow: 6
      }
    }}
  >
    <Avatar 
      sx={{ 
        mb: 2, 
        width: 70, 
        height: 70, 
        bgcolor: 'primary.main' 
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
        background: 'linear-gradient(135deg, #f0f4f8 0%, #ffffff 100%)',
        minHeight: '100vh',
        pt: { xs: 4, md: 8 }
      }}
    >
      <Container maxWidth="lg">
        {/* Hero Section */}
        <Box 
          sx={{ 
            textAlign: 'center', 
            mb: 6,
            p: { xs: 2, md: 6 },
            borderRadius: 3,
            background: 'rgba(255,255,255,0.8)',
            boxShadow: '0 8px 32px 0 rgba(31, 38, 135, 0.1)',
            backdropFilter: 'blur(4px)',
            opacity: 1,
            transition: 'opacity 0.5s ease',
            '@keyframes fadeIn': {
              from: { opacity: 0 },
              to: { opacity: 1 }
            },
            animation: 'fadeIn 0.8s ease-out'
          }}
        >
          <Typography 
            variant="h2" 
            component="h1" 
            sx={{ 
              mb: 3,
              fontWeight: 700,
              background: 'linear-gradient(45deg, #1976d2, #42a5f5)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent'
            }}
          >
            Git Gud Drivers
          </Typography>
          <Typography 
            variant="h5" 
            color="text.primary" 
            sx={{ mb: 4, maxWidth: 600, mx: 'auto' }}
          >
            Empowering professional drivers through a comprehensive rewards program 
            that recognizes excellence and drives continuous improvement.
          </Typography>
          <Box>
            <Button 
              variant="contained" 
              size="large"
              onClick={() => navigate('/register')}
              sx={{ 
                mr: 2,
                px: 4,
                py: 1.5,
                borderRadius: 3,
                textTransform: 'none',
                boxShadow: '0 4px 14px 0 rgba(0, 0, 0, 0.1)',
                transition: 'transform 0.2s ease',
                '&:hover': {
                  transform: 'scale(1.05)'
                }
              }}
            >
              Join Now
            </Button>
            <Button 
              variant="outlined" 
              size="large"
              onClick={() => navigate('/login')}
              sx={{
                px: 4,
                py: 1.5,
                borderRadius: 3,
                textTransform: 'none',
                borderColor: 'primary.main',
                color: 'primary.main',
                transition: 'transform 0.2s ease',
                '&:hover': {
                  transform: 'scale(1.05)'
                }
              }}
            >
              Sign In
            </Button>
          </Box>
        </Box>

        {/* Features Section */}
        <Grid 
          container 
          spacing={4} 
          sx={{ mb: 6 }}
        >
          <Grid item xs={12}>
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
              How Git Gud Drivers Works
            </Typography>
          </Grid>
          
          <Grid item xs={12} md={4}>
            <FeatureCard 
              icon={<CarIcon fontSize="large" />}
              title="Professional Drivers"
              description="A dedicated platform for drivers committed to delivering exceptional service and continuous improvement."
            />
          </Grid>
          
          <Grid item xs={12} md={4}>
            <FeatureCard 
              icon={<RewardsIcon fontSize="large" />}
              title="Reward System"
              description="Earn points for safe driving, customer satisfaction, and professional excellence."
            />
          </Grid>
          
          <Grid item xs={12} md={4}>
            <FeatureCard 
              icon={<StepsIcon fontSize="large" />}
              title="Simple Process"
              description="Easy onboarding, transparent tracking, and meaningful rewards for your hard work."
            />
          </Grid>
        </Grid>

        {/* Steps Section */}
        <Box 
          sx={{ 
            bgcolor: 'background.paper', 
            borderRadius: 3, 
            p: { xs: 2, md: 4 },
            boxShadow: 3
          }}
        >
          <Typography 
            variant="h5" 
            align="center" 
            gutterBottom
            sx={{ 
              mb: 4,
              color: 'primary.main'
            }}
          >
            Your Journey in 4 Simple Steps
          </Typography>
          
          <Grid container spacing={3}>
            {[
              'Apply as a professional driver',
              'Get approved by a sponsor',
              'Start earning performance points',
              'Redeem exciting rewards'
            ].map((step, index) => (
              <Grid item xs={12} md={3} key={step}>
                <Box sx={{ 
                  display: 'flex', 
                  alignItems: 'center',
                  justifyContent: 'center',
                  textAlign: 'center'
                }}>
                  <Avatar 
                    sx={{ 
                      mr: 2, 
                      bgcolor: 'primary.light',
                      color: 'primary.contrastText'
                    }}
                  >
                    {index + 1}
                  </Avatar>
                  <Typography>{step}</Typography>
                </Box>
                {index < 3 && (
                  <Divider 
                    orientation="horizontal" 
                    flexItem 
                    sx={{ 
                      my: 2, 
                      display: { xs: 'none', md: 'flex' } 
                    }} 
                  />
                )}
              </Grid>
            ))}
          </Grid>
        </Box>
      </Container>
    </Box>
  );
};

export default HomePage;