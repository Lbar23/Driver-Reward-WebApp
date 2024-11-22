import React from 'react';
import {
  Box,
  CssBaseline,
  Typography,
  Avatar,
  Grid2,
  Stack,
} from '@mui/material';
import { useAuth } from '../service/authContext';

const ProfilePage: React.FC = () => {
  const { user } = useAuth();

  if (!user) {
    return (
      <Box sx={{ p: 2 }}>
        <Typography variant="body1" aria-label="User not authenticated">
          User not authenticated. Please log in to view the profile.
        </Typography>
      </Box>
    );
  }

  const renderAdditionalInfo = () => {
    if (user.userType === 'Driver') {
      return (
        <Box>
          <Typography variant="body1" sx={{ mb: 1 }}>
            <strong>Driver Information:</strong>
          </Typography>
          <Typography variant="body2">
            <strong>License Number:</strong> {'G1TGUD'}
          </Typography>
          <Typography variant="body2">
            <strong>Member Since:</strong> {user.createdAt || 'Just now!'}
          </Typography>
        </Box>
      );
    }

    if (user.userType === 'Sponsor') {
      return (
        <Box>
          <Typography variant="body1" sx={{ mb: 1 }}>
            <strong>Sponsor Information:</strong>
          </Typography>
          <Typography variant="body2">
            <strong>Company:</strong> {user.sponsorDetails?.companyName || 'N/A'}
          </Typography>
          <Typography variant="body2">
            <strong>Company Sponsor Since:</strong> {user.sponsorDetails?.joinDate || 0}
          </Typography>
          <Typography variant="body2">
            <strong>Member Since:</strong> {user.createdAt || 'Just now!'}
          </Typography>
        </Box>
      );
    }

    if (user.userType === 'Admin') {
      return (
        <Box>
          <Typography variant="body1" sx={{ mb: 1 }}>
            <strong>Admin Information:</strong>
          </Typography>
          <Typography variant="body2">
            <strong>Managed Roles:</strong> { 'Sponsors, Drivers, Guests'}
          </Typography>
          <Typography variant="body2">
            <strong>Member Since:</strong> {user.createdAt || 'Just now!'}
          </Typography>
        </Box>
      );
    }

    return (
      <Box>
        <Typography variant="body1">
          <strong>Guest Information:</strong>
        </Typography>
        <Typography variant="body2">
          Limited access. Please log in to view more details.
        </Typography>
      </Box>
    );
  };

  return (
    <Box sx={{ display: 'flex', width: '100%', minHeight: '100vh' }}>
      <CssBaseline />
      <a
        href="#profile-main"
        style={{
          position: 'absolute',
          left: '-9999px',
          zIndex: 999,
          padding: '1rem',
          background: '#fff',
          textDecoration: 'none',
        }}
        onFocus={(e) => {
          e.currentTarget.style.left = '1rem';
        }}
        onBlur={(e) => {
          e.currentTarget.style.left = '-9999px';
        }}
      >
        Skip to profile content
      </a>
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          display: 'flex',
          flexDirection: 'column',
          ml: '15px',
          p: 2,
        }}
        id="profile-main"
      >
        <Stack direction="row" spacing={2} alignItems="center">
          <Typography
            variant="h6"
            component="h1"
            tabIndex={0}
            aria-label={`Profile Page - Current user: ${user.userName}`}
          >
            Profile Page
          </Typography>
        </Stack>
        <Box sx={{ mt: 4 }}>
          <Grid2 container spacing={2}>
            <Grid2 size={{ xs: 12, sm: 6, md: 4 }} sx={{ display: 'flex', justifyContent: 'center' }}>
              <Avatar
                src={user.email || ''}
                alt={`${user.userName}'s profile picture`}
                sx={{ width: 150, height: 150 }}
              >
                {user.userName[0]}
              </Avatar>
            </Grid2>
            <Grid2 size={{ xs: 12, md: 8 }}>
              <Stack spacing={2}>
                <Typography variant="h5">{user.userName}</Typography>
                <Typography variant="body1" color="textSecondary">
                  <strong>Email:</strong> {user.email}
                </Typography>
                <Typography variant="body1" color="textSecondary">
                  <strong>Role:</strong> {user.userType}
                </Typography>
                {renderAdditionalInfo()}
              </Stack>
            </Grid2>
          </Grid2>
        </Box>
      </Box>
    </Box>
  );
};

export default ProfilePage;
