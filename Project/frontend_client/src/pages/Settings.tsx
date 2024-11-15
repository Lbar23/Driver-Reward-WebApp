import React, { useState } from 'react';
import { Box, Typography, Card, CardContent, FormControlLabel, Button, Collapse, Switch, Tooltip } from '@mui/material';
import Section from '../components/form-elements/Section';
import Grid2 from '@mui/material/Grid2'; // Import Grid2
import { useAuth } from '../service/authContext';

const Settings: React.FC = () => {
  const { notifySettings, updateNotifySetting, user } = useAuth();
  const [showPreferences, setShowPreferences] = useState(false);

  const handleTogglePreferences = () => {
    setShowPreferences((prev) => !prev);
  };

  return (
    <Box sx={{ width: '100%', maxWidth: 600, margin: 'auto'}}>
      <Typography variant="h4" component="h2" gutterBottom>
        Settings
      </Typography>

      {/* Account Settings Section */}
      <Typography variant="h5" component="h2" gutterBottom sx={{ mt: 3 }}>
        Account Settings
      </Typography>
      <Section
        title=""
        buttons={[
          { label: 'Update Email', path: '/update-email' },
          { label: 'Change Password', path: '/change-password' },
        ]}
      />

      {/* Notification Settings Section */}
      <Typography variant="h5" component="h2" gutterBottom sx={{ mt: 3 }}>
        Notification Settings
      </Typography>
      
      <Button variant="outlined" onClick={handleTogglePreferences}>
        {showPreferences ? 'Hide Preferences' : 'Change Preferences'}
      </Button>

      <Collapse in={showPreferences} timeout="auto" unmountOnExit>
        <Card variant="outlined" sx={{ mt: 2 }}>
          <CardContent>
            <Grid2 container spacing={2}>
              {Object.keys(notifySettings).map((key) => {
                // Display "applicationApproved" and "orderIssue" only for Driver role
                if (
                  (key === 'applicationApproved' || key === 'orderIssue') &&
                  user?.userType !== 'Driver'
                ) {
                  return null;
                }

                return (
                  <Grid2 size={{xs:12, sm:6, md:4}} key={key}>
                    <Tooltip
                      title={
                        key === 'systemDrop' || key === 'applicationApproved'
                          ? 'This setting cannot be disabled.'
                          : ''
                      }
                      placement="top"
                    >
                      <FormControlLabel
                        control={
                          <Switch
                            checked={notifySettings[key as keyof typeof notifySettings]}
                            onChange={() =>
                              updateNotifySetting(
                                key as keyof typeof notifySettings,
                                !notifySettings[key as keyof typeof notifySettings]
                              )
                            }
                            disabled={key === 'systemDrop' || key === 'applicationApproved'}
                            color="primary"
                          />
                        }
                        label={
                          <Typography variant="body2">
                            {key.replace(/([A-Z])/g, ' $1').replace(/^./, (str) => str.toUpperCase())}
                          </Typography>
                        }
                      />
                    </Tooltip>
                  </Grid2>
                );
              })}
            </Grid2>
          </CardContent>
        </Card>
      </Collapse>
    </Box>
  );
};

export default Settings;
