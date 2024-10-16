import React, { useState, useEffect } from 'react';
import { Typography, Button, List, ListItem, ListItemText } from '@mui/material';

const AdminDashboard = () => {
  const [users, setUsers] = useState([]);

  useEffect(() => {
    // Fetch users from your API
    // This is a placeholder for demonstration
    setUsers([
      { id: 1, name: 'John Doe', type: 'driver' },
      { id: 2, name: 'Jane Smith', type: 'sponsor' },
      { id: 3, name: 'Bob Johnson', type: 'admin' },
    ]);
  }, []);

  const handleUserAccess = (userId, action) => {
    // Implement user access management logic here
    console.log(`${action} access for user ${userId}`);
  };

  return (
    <div>
      <Typography variant="h4">Admin Dashboard</Typography>
      <Typography variant="h6">User Management</Typography>
      <List>
        {users.map((user) => (
          <ListItem key={user.id}>
            <ListItemText primary={user.name} secondary={`Type: ${user.type}`} />
            <Button onClick={() => handleUserAccess(user.id, 'grant')}>Grant Access</Button>
            <Button onClick={() => handleUserAccess(user.id, 'revoke')}>Revoke Access</Button>
          </ListItem>
        ))}
      </List>
    </div>
  );
};

export default AdminDashboard;