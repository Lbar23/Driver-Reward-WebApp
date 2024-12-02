import React, { useState } from 'react';
import {
  Box,
  Button,
  Typography,
  MenuItem,
  Select,
  CssBaseline,
  Stack,
  Alert,
  Snackbar,
  SelectChangeEvent
} from '@mui/material';
import MainGrid from '../components/dashboard/MainGrid';
import { useAuth } from '../service/authContext';
import { useAccessibility } from '../service/accessibilityContext';
import axios from 'axios';

interface User {
  userId: number;
  userName: string;
  email: string;
}

const Dashboard: React.FC = () => {
  const { user, viewRole, setViewRole } = useAuth();
  const { announce } = useAccessibility();
  const [users, setUsers] = useState<User[]>([]);
  const [selectedUserId, setSelectedUserId] = useState<string>('');
  const [error, setError] = useState<string | null>(null);
  
  const activeRole = viewRole || (user?.roles && user.roles[0]);

  const fetchUsersByRole = async (role: string) => {
    try {
      const response = await axios.get(`/api/admin/view-users/${role.toLowerCase()}`);
      setUsers(response.data);
    } catch (err) {
      setError('Failed to fetch users for selected role');
      console.error('Error fetching users:', err);
    }
  };

  const handleViewRoleChange = async (event: SelectChangeEvent<string>) => {
    const newRole = event.target.value;
    setViewRole(newRole);
    setSelectedUserId(''); // Reset selected user
    if (newRole) {
      await fetchUsersByRole(newRole);
    }
    announce(`Role selected: ${newRole}`);
  };

  const handleUserSelect = async (event: SelectChangeEvent<string>) => {
    const userId = event.target.value;
    setSelectedUserId(userId);
    
    try {
      const response = await axios.post(`/api/system/impersonate/${userId}`, {}, {
        withCredentials: true
      });
      if (response.status !== 200) {
        throw new Error('Failed to impersonate user');
      }
      announce(`Now viewing as user ${users.find(u => u.userId.toString() === userId)?.userName}`);
    } catch (err) {
      setError('Failed to impersonate user. Please try again.');
      console.error('Error impersonating user:', err);
    }
  };

  const resetViewRole = async () => {
    try {
      // Try to stop impersonation on the server
      await axios.post('/api/system/impersonate/stop', {}, {
        withCredentials: true
      });
      
    } catch (err) {
      // Log the error but continue with local state cleanup
      console.error('Error stopping impersonation on server:', err);
      setError('Failed to stop impersonation on server, but view has been reset locally.');
    } finally {
      // Always clean up local state, even if the server request failed
      setViewRole(null);
      setSelectedUserId('');
      setUsers([]);
      announce('View reset back to original role');
    }
  };
  

  const hasRole = (roleToCheck: string) => {
    return user?.roles?.includes(roleToCheck);
  };

  return (
    <Box sx={{ display: 'flex', width: '100%', minHeight: '100vh'}}>
      <CssBaseline />
      <Box
        component="a"
        href="#dashboard-main"
        sx={{
          position: 'absolute',
          left: '-9999px',
          zIndex: 999,
          padding: '1rem',
          background: '#fff',
          textDecoration: 'none',
          '&:focus': {
            left: '1rem',
          }
        }}
      >
        Skip to dashboard content
      </Box>
      <Box component="main" sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column', ml: '15px' }}>
        <Box sx={{ p: 2 }}>
          <Stack direction="row" spacing={2} alignItems="center">
            <Typography 
              variant="h6"
              component="h1"
              tabIndex={0}
              aria-label={`Dashboard - Current role: ${activeRole}`}
            >
              Dashboard
            </Typography>
            {user && (hasRole('Admin') || hasRole('Sponsor')) && (
              <>
                <Select
                  value={viewRole || ''}
                  onChange={handleViewRoleChange}
                  displayEmpty
                  sx={{ minWidth: 180 }}
                  aria-label="Select role to view as"
                >
                  <MenuItem value="">
                    <em>Select a role to view</em>
                  </MenuItem>
                  {hasRole('Admin') && [
                    <MenuItem key="Driver" value="Driver">Driver</MenuItem>,
                    <MenuItem key="Sponsor" value="Sponsor">Sponsor</MenuItem>,
                    <MenuItem key="Guest" value="Guest">Guest</MenuItem>
                  ]}
                  {hasRole('Sponsor') && (
                    <MenuItem key="Driver" value="Driver">Driver</MenuItem>
                  )}
                </Select>

                {viewRole && users.length > 0 && (
                  <Select
                    value={selectedUserId}
                    onChange={handleUserSelect}
                    displayEmpty
                    sx={{ minWidth: 180 }}
                    aria-label="Select user to impersonate"
                  >
                    <MenuItem value="">
                      <em>Select a user</em>
                    </MenuItem>
                    {users.map(user => (
                      <MenuItem key={user.userId} value={user.userId.toString()}>
                        {user.userName} ({user.email})
                      </MenuItem>
                    ))}
                  </Select>
                )}
              </>
            )}
            {(viewRole || selectedUserId) && (
              <Button 
                onClick={resetViewRole}
                variant="outlined"
                color="secondary"
                aria-label='Exit view as mode'
              >
                Exit &quot;View As&quot;
              </Button>
            )}

          </Stack>
        </Box>
        <MainGrid />
      </Box>
      <Snackbar 
        open={!!error} 
        autoHideDuration={6000} 
        onClose={() => setError(null)}
      >
        <Alert severity="error" onClose={() => setError(null)}>
          {error}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default Dashboard;