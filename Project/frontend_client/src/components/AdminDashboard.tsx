import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Typography, Button, List, ListItem, ListItemText, Select, MenuItem } from '@mui/material';
const API_BASE_URL = 'https://localhost:7284';


const AdminDashboard = () => {
  const [users, setUsers] = useState([]);
  const [selectedUser, setSelectedUser] = useState(null);

  useEffect(() => {
    fetchUsers();
  }, []);

  const fetchUsers = async () => {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/user/all`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      });
      setUsers(response.data);
    } catch (error) {
      console.error('Error fetching users:', error);
      // Handle error (e.g., show error message to user)
    }
  };

  const handleUserAccess = async (userId, action, permission) => {
    try {
      await axios.post('/api/user/manage-access', 
        { userId, action, permission },
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        }
      );
      fetchUsers();
    } catch (error) {
      console.error('Error managing user access:', error);
      // Handle error (e.g., show error message to user)
    }
  };

  const handleChangeUserType = async (userId, newUserType) => {
    try {
      await axios.post(`${API_BASE_URL}/api/user/change-user-type`, 
        { userId, newUserType },
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        }
      );
      fetchUsers();
    } catch (error) {
      console.error('Error changing user type:', error);
      // Handle error (e.g., show error message to user)
    }
  };

  const handleImpersonateUser = async (userId) => {
    try {
      const response = await axios.post(`${API_BASE_URL}/api/user/impersonate`, 
        { userId },
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        }
      );
      if (response.data.success) {
        // Handle successful impersonation (e.g., redirect to user's dashboard)
        console.log('Impersonation successful');
      }
    } catch (error) {
      console.error('Error impersonating user:', error);
      // Handle error (e.g., show error message to user)
    }
  };

  const handleRemoveUser = async (userId) => {
    try {
      await axios.delete(`${API_BASE_URL}/api/user/remove-user/${userId}`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      });
      fetchUsers();
    } catch (error) {
      console.error('Error removing user:', error);
      // Handle error (e.g., show error message to user)
    }
  };

  return (
    <div>
      <Typography variant="h4">Admin Dashboard</Typography>
      <Typography variant="h6">User Management</Typography>
      <List>
        {users.map((user) => (
          <ListItem key={user.id}>
            <ListItemText primary={user.userName} secondary={`Type: ${user.userType}`} />
            <Select
              value={user.userType}
              onChange={(e) => handleChangeUserType(user.id, e.target.value)}
            >
              <MenuItem value="Driver">Driver</MenuItem>
              <MenuItem value="Sponsor">Sponsor</MenuItem>
              <MenuItem value="Admin">Admin</MenuItem>
            </Select>
            <Button onClick={() => handleUserAccess(user.id, 'grant', 'FullAccess')}>Grant Full Access</Button>
            <Button onClick={() => handleUserAccess(user.id, 'revoke', 'FullAccess')}>Revoke Full Access</Button>
            <Button onClick={() => handleImpersonateUser(user.id)}>Impersonate User</Button>
            <Button onClick={() => handleRemoveUser(user.id)}>Remove User</Button>
          </ListItem>
        ))}
      </List>
    </div>
  );
};

export default AdminDashboard;