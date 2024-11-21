import React, { useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Slider,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  SelectChangeEvent,
  FormControlLabel,
  Tooltip,
  Button,
  Collapse,
  Switch,
} from '@mui/material';
import Grid2 from '@mui/material/Grid2';
import { useAuth } from '../service/authContext';
import { useAppTheme } from '../components/layout/AppTheme';
import { getAccessibilityTokens } from '../theme/themePrimitives';
import Section from '../components/form-elements/Section';

const Settings: React.FC = () => {
  const { notifySettings, updateNotifySetting, user } = useAuth();
  const [showPreferences, setShowPreferences] = useState(false);
  const { fontSize, setFontSize, accessibleTokens, setAccessibleTokens } = useAppTheme();
  const [selectedProfile, setSelectedProfile] = useState<string>('default'); // Default accessibility token



  const handleTogglePreferences = () => {
    setShowPreferences((prev) => !prev);
  };

  const handleFontSizeChange = (_: Event, newValue: number | number[]) => {
    if (typeof newValue === 'number') {
      setFontSize(newValue);
    }
  };

  const handleProfileChange = (event: React.ChangeEvent<{ value: unknown }>) => {
    const profileKey = event.target.value as string;
    setSelectedProfile(profileKey);
    setAccessibleTokens(getAccessibilityTokens(profileKey)); // Fetch and apply the token dynamically
  };



  return (
    <Box sx={{ width: '100%', maxWidth: 600, margin: 'auto' }}>
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
                  <Grid2 size={{ xs: 12, sm: 6, md: 4 }} key={key}>
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

       {/* Accessibility Settings */}
       <Typography variant="h5" component="h2" gutterBottom sx={{ mt: 3 }}>
        Accessibility
      </Typography>
      <Card variant="outlined" sx={{ mt: 2 }}>
        <CardContent>
          {/* Font Size */}
          <Typography variant="body1" gutterBottom>
            Font Size
          </Typography>
          <Slider
            value={fontSize}
            min={12}
            max={20}
            step={1}
            valueLabelDisplay="auto"
            onChange={handleFontSizeChange}
            sx={{ width: '100%' }}
          />

          {/* Accessibility Profiles */}
          <Typography variant="body1" gutterBottom sx={{ mt: 3 }}>
            Color Palette
          </Typography>
          <FormControl fullWidth>
            <InputLabel id="accessibility-profile-select-label">Accessibility Profile</InputLabel>
            <Select
              labelId="accessibility-profile-select-label"
              value={selectedProfile}
              onChange={handleProfileChange}
            >
              {['default', 'highContrast', 'colorBlind', 'monochrome'].map((profileKey) => (
                <MenuItem value={profileKey} key={profileKey}>
                  {profileKey.replace(/([A-Z])/g, ' $1').replace(/^./, (str) => str.toUpperCase())}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </CardContent>
      </Card>
    </Box>
  );
};

export default Settings;
