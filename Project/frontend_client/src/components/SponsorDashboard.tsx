import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Typography, Button, List, ListItem, ListItemText } from '@mui/material';

const API_BASE_URL = 'https://localhost:7284';

const SponsorDashboard = () => {
  const [drivers, setDrivers] = useState([]);

  useEffect(() => {
    fetchDrivers();
  }, []);

  const fetchDrivers = async () => {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/user/drivers`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
        },
      });
      setDrivers(response.data);
    } catch (error) {
      console.error('Error fetching drivers:', error);
      // Handle error (e.g., show error message to user)
    }
  };

  const handleRemoveDriver = async (driverId) => {
    try {
      await axios.delete(`${API_BASE_URL}/api/drivers/remove/${driverId}`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
        },
      });
      fetchDrivers(); // Refresh the list of drivers
    } catch (error) {
      console.error('Error removing driver:', error);
      // Handle error (e.g., show error message to user)
    }
  };

  return (
    <div>
      <Typography variant="h4">Sponsor Dashboard</Typography>
      <Typography variant="h6">Driver Management</Typography>
      <List>
        {drivers.map((driver) => (
          <ListItem key={driver.id}>
            <ListItemText primary={driver.userName} secondary={`Type: ${driver.userType}`} />
            <Button onClick={() => handleRemoveDriver(driver.id)}>Remove Driver</Button>
          </ListItem>
        ))}
      </List>
    </div>
  );
};

export default SponsorDashboard;
