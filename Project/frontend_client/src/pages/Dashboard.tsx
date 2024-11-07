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


const Dashboard: React.FC = () => {
  const { user, viewRole, setViewRole } = useAuth();
  const activeRole = viewRole || user?.userType;

  const handleViewRoleChange = (event: SelectChangeEvent<string>) => {
    setViewRole(event.target.value as string);
  };

  const resetViewRole = () => {
    setViewRole(null);
  };

  return (
      <Box sx={{ display: 'flex', width: '100%', minHeight: '100vh'}}>
        <CssBaseline />
        <Box component="main" sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column', ml: '15px' }}>
          <Box sx={{ p: 2 }}>
            <Stack direction="row" spacing={2} alignItems="center">
              <Typography variant="h6">Dashboard</Typography>
              {user && (user.userType === 'Admin' || user.userType === 'Sponsor') && (
                <Select
                  value={viewRole || ''}
                  onChange={handleViewRoleChange}
                  displayEmpty
                  sx={{ minWidth: 180 }}
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
              {viewRole && <Button onClick={resetViewRole}>Exit "View As"</Button>}
            </Stack>
          </Box>
          <MainGrid />
        </Box>
      </Box>
  );
};

export default Dashboard;
