import React, { useState, useEffect } from 'react';
import { Typography, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Select, MenuItem } from '@mui/material';

const UserManagement = () => {
  const [users, setUsers] = useState([]);

  useEffect(() => {
    // Fetch users from your API
    // This is a placeholder for demonstration
    setUsers([
      { id: 1, name: 'John Doe', type: 'driver', permissions: 'read-only' },
      { id: 2, name: 'Jane Smith', type: 'sponsor', permissions: 'full-access' },
      { id: 3, name: 'Bob Johnson', type: 'admin', permissions: 'full-access' },
    ]);
  }, []);

  const handlePermissionChange = (userId, newPermission) => {
    // Implement permission change logic here
    console.log(`Changing permission for user ${userId} to ${newPermission}`);
    // Update the users state with the new permission
    setUsers(users.map(user => 
      user.id === userId ? { ...user, permissions: newPermission } : user
    ));
  };

  return (
    <div>
      <Typography variant="h4">User Management</Typography>
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>Permissions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {users.map((user) => (
              <TableRow key={user.id}>
                <TableCell>{user.name}</TableCell>
                <TableCell>{user.type}</TableCell>
                <TableCell>
                  <Select
                    value={user.permissions}
                    onChange={(e) => handlePermissionChange(user.id, e.target.value)}
                  >
                    <MenuItem value="read-only">Read Only</MenuItem>
                    <MenuItem value="full-access">Full Access</MenuItem>
                  </Select>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </div>
  );
};

export default UserManagement;