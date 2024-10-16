import React, { useState, useEffect } from 'react';
import { Typography, Box, CircularProgress, Button } from '@mui/material';
import axios from 'axios';

const Dashboard: React.FC = () => {
  const [loading, setLoading] = useState<boolean>(true);
  const [data, setData] = useState<any[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [role, setRole] = useState<string>("Guest"); // defaults to guest

  // implement RBAC data here
  useEffect(() => {
    const fetchData = async () => {
      try {
        // API call for dashboard data not implemented yet
        const response = await axios.get(`/api/dashboard`);
        const fetchedData = Array.isArray(response.data) ? response.data : [];
        setData(fetchedData);

        // need a api for getting user here 
        const roleResponse = await axios.get(`/api/User/GetUser`);
        setRole(roleResponse.data.role); 

        setLoading(false);
      } 
      catch (err) {
        setError('Failed to load dashboard data.');
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  // starter rbac logic
  const renderDriverSection = () => {
    if (role === "Driver") {
      return (
        <Box sx={{ mt: 4 }}>
          <Typography variant="h6">Driver Settings</Typography>
          <Button variant="contained" color="primary">
            Manage My Data
          </Button>
        </Box>
      );
    }
    return null;
  };

  const renderSponsorSection = () => {
    if (role === "Sponsor") {
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
    if (role === "Admin") {
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

          {/* render rbac here based on role */}
          {renderDriverSection()}
          {renderSponsorSection()}
          {renderAdminSection()}
        </Box>
      )}
    </Box>
  );
};

export default Dashboard;
