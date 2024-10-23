import React, { useState, useEffect } from 'react';
import { Typography, Box, CircularProgress, Button } from '@mui/material';
import axios from 'axios';
//import DriverPointsList from '../components/PMSDriverList'; for sponsors, but not "implemented" yet

interface UserData { // not the full user data list, but can be edited in preference (duh)
  id: number;
  name: string;
  email: string;
  role: string;
  permissions: string;
}

const Dashboard: React.FC = () => {
  const [loading, setLoading] = useState<boolean>(true);
  const [data, setData] = useState<any[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [userData, setUserData] = useState<UserData | null>(null); // added user data
  const [role, setRole] = useState<string>("Guest"); // defaults to guest /// Just keeping this here in case we add Guest role as a default still

  // implement RBAC data here
  useEffect(() => {
    const fetchData = async () => {
      try {
        // API call for dashboard data not implemented yet
        const response = await axios.get(`/api/dashboard`);
        const fetchedData = Array.isArray(response.data) ? response.data : [];
        setData(fetchedData);

        // need a api for getting user here 
        const userResponse = await axios.get(`/api/User/currentuser`);
        setUserData(userResponse.data);
        //setRole(roleResponse.data.role); 

        setLoading(false);
      } 
      catch (err) {
        setError('Failed to load dashboard data.');
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  //Function to get the specified permission related to specific user/user data
  //Right now, only 4 basic ones in database: "VIEW_POINTS", "MANAGE_DRIVERS", "MANAGE_USERS", "VIEW_LOGS"
  const hasPermission = (permission: string) => {
    return userData?.permissions.includes(permission)
  };

  // starter rbac logic
  const renderDriverSection = () => {
    if (userData?.role.includes("Driver")) {
      return (
        <Box sx={{ mt: 4 }}>
          <Typography variant="h6">Driver Settings</Typography>
          {(//hasPermission("VIEW_POINTS") && ( // <-- hasPermission helper here to check if a permission "VIEW_POINTS" both exisits and only assigns the button/component to drivers when dashboard page renders
            <Button variant="contained" color="primary">
              View My Points
            </Button>
          )}
          <Button variant="contained" color="primary" sx={{ ml: 2 }}>
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
