import React, { useState, useEffect } from 'react';
import { Typography, Box, CircularProgress, Button } from '@mui/material';
import axios from 'axios';

interface UserData {
  id: number;
  userName: string;
  email: string;
  userType: string;
  createdAt: string;
  lastLogin: string;
  roles: string[];
  permissions: string[];
}

const Dashboard: React.FC = () => {
  const [loading, setLoading] = useState<boolean>(true);
  const [data, setData] = useState<any[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [userData, setUserData] = useState<UserData | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        // Fetch dashboard data
        // const response = await axios.get(`/api/dashboard`);
        // setData(Array.isArray(response.data) ? response.data : []);

        // Fetch current user data
        const userResponse = await axios.get(`/api/User/currentuser`);
        setUserData(userResponse.data);

        setLoading(false);
      } 
      catch (err) {
        setError('Failed to load dashboard data.');
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  const renderGuestSection = () => {
    if (userData?.roles.includes("Guest")) {
      return (
        <Box sx={{ mt: 4 }}>
          <Typography variant="h6">Guest Settings</Typography>
          <Button variant="contained" color="primary" sx={{ ml: 2 }}>
            Apply for Driver Program
          </Button>
          <Button variant="contained" color="primary" sx={{ ml: 2 }}>
            View Profile
          </Button>
        </Box>
      );
    }
    return null;
  };

  const renderDriverSection = () => {
    if (userData?.roles.includes("Driver")) {
      return (
        <Box sx={{ mt: 4 }}>
          <Typography variant="h6">Driver Settings</Typography>
          <Button variant="contained" color="primary">
            View My Points
          </Button>
          <Button variant="contained" color="primary" sx={{ ml: 2 }}>
            Manage My Data
          </Button>
        </Box>
      );
    }
    return null;
  };

  const renderSponsorSection = () => {
    if (userData?.roles.includes("Sponsor")) {
      return (
        <Box sx={{ mt: 4 }}>
          <Typography variant="h6">Sponsor Settings</Typography>
          <Button variant="contained" color="primary">
            Manage Sponsor Company
          </Button>
        </Box>
      );
    }
    return null;
  };

  const renderAdminSection = () => {
    if (userData?.roles.includes("Admin")) {
      return (
        <Box sx={{ mt: 4 }}>
          <Typography variant="h6">Admin Controls</Typography>
          <Button variant="contained" color="secondary">
            Manage Users
          </Button>
          <Button variant="contained" color="secondary" sx={{ ml: 2 }}>
            View Logs
          </Button>
        </Box>
      );
    }
    return null;
  };

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        maxWidth: 600,
        margin: 'auto',
        padding: 3,
        boxShadow: 3,
        borderRadius: 2,
        backgroundColor: 'background.paper',
      }}
    >
      <Typography variant="h4" component="h1" gutterBottom>
        Dashboard
      </Typography>

      {loading ? (
        <CircularProgress />
      ) : error ? (
        <Typography color="error">{error}</Typography>
      ) : (
        <Box sx={{ width: '100%', mt: 2 }}>
          <Typography variant="h6" component="h2" gutterBottom>
            Overview
          </Typography>

          {/* Display this data only if available */}
          {Array.isArray(data) && data.length > 0 ? (
            data.map((item, index) => (
              <Box
                key={index}
                sx={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  p: 2,
                  mb: 2,
                  border: '1px solid #ddd',
                  borderRadius: 2,
                }}
              >
                <Typography>{item.title}</Typography>
                <Typography>{item.value}</Typography>
              </Box>
            ))
          ) : (
            <Typography>No data available</Typography>
          )}

          {/* Render sections based on user roles */}
          {renderGuestSection()}
          {renderDriverSection()}
          {renderSponsorSection()}
          {renderAdminSection()}
        </Box>
      )}
    </Box>
  );
};

export default Dashboard;
