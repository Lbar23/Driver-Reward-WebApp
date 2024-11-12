import React from 'react';
import { Box, Typography } from '@mui/material';
import { Link } from 'react-router-dom';
import Section from '../components/form-elements/Section';

const Settings: React.FC = () => {
  return (
    <Box sx={{ width: '100%', maxWidth: 600, margin: 'auto', mt: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Settings
      </Typography>

      <Section
        title="Account Settings"
        buttons={[
          { label: 'Update Email', path: '/update-email' },
          { label: 'Change Password', path: '/change-password' },
          { label: 'View Profile', path: '/profile' },
        ]}
      />

      <Section
        title="Notification Settings"
        buttons={[
          { label: 'Manage Notifications', path: '/notifications' },
          { label: 'Set Preferences', path: '/preferences' },
        ]}
      />

      {/* Removed the extra Change Password button */}
    </Box>
  );
};

export default Settings;
