import React from 'react';
import {
  Box,
  Button,
  Typography,
  MenuItem,
  Select,
  CssBaseline,
  Stack
} from '@mui/material';
import MainGrid from '../components/dashboard/MainGrid';
import { SelectChangeEvent } from '@mui/material';
import { useAuth } from '../service/authContext';
import { useAccessibility } from '../service/accessibilityContext';


const Dashboard: React.FC = () => {
  const { user, viewRole, setViewRole } = useAuth();
  const activeRole = viewRole || user?.userType;
  const { announce } = useAccessibility();

  const handleViewRoleChange = (event: SelectChangeEvent<string>) => {
    const newRole = event.target.value as string;
    setViewRole(newRole);
    announce(`View changed to ${newRole}`);
  };

  const resetViewRole = () => {
    setViewRole(null);
    announce('View reset back to original role');
  };

  return (
      <Box sx={{ display: 'flex', width: '100%', minHeight: '100vh'}}>
        <CssBaseline />
        <a
        href="#dashboard-main"
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
        Skip to dashboard content
      </a>
        <Box component="main" sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column', ml: '15px' }}>
          <Box sx={{ p: 2 }}>
            <Stack direction="row" spacing={2} alignItems="center">
              <Typography variant="h6"
              component="h1"
              tabIndex={0}
              aria-label={`Dashboard - Current role: ${activeRole}`}
              >
                Dashboard
              </Typography>
              {user && (user.userType === 'Admin' || user.userType === 'Sponsor') && (
                <Select
                  value={viewRole || ''}
                  onChange={handleViewRoleChange}
                  displayEmpty
                  sx={{ minWidth: 180 }}
                  aria-label="Select role to view as"
                  renderValue={() => 'Select a role to view'}
                >
                  <MenuItem disabled value="">
                    <em>Select a role to view</em>
                  </MenuItem>
                  {user.userType === 'Admin' && [
                    <MenuItem key="Driver" value="Driver">Driver</MenuItem>,
                    <MenuItem key="Sponsor" value="Sponsor">Sponsor</MenuItem>,
                    <MenuItem key="Guest" value="Guest">Guest</MenuItem>
                  ]}
                  {user.userType === 'Sponsor' && (
                    <MenuItem key="Driver" value="Driver">Driver</MenuItem>
                  )}
                </Select>
              )}
              {viewRole && <Button onClick={resetViewRole}
              aria-label='Exited view as mode'
              >
                Exit 
              "View As"</Button>}
            </Stack>
          </Box>
          <MainGrid />
        </Box>
      </Box>
  );
};

export default Dashboard;
