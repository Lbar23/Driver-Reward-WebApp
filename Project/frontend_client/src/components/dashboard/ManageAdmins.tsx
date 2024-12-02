import React, { useState, useEffect } from 'react';
import axios from 'axios';
import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Typography from '@mui/material/Typography';
import LinearProgress from '@mui/material/LinearProgress';
import Alert from '@mui/material/Alert';
import TextField from '@mui/material/TextField';
import SearchIcon from '@mui/icons-material/SearchRounded';
import IconButton from '@mui/material/IconButton';
import Select from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';
import DeleteIcon from '@mui/icons-material/Delete';
import Button from '@mui/material/Button';
import CreateUserModal from './CreateUserModal';

interface Admin {
  userId: number;
  userName: string;
  firstName: string;
  lastName: string;
  email: string;
  createdAt: string;
  lastLogin: string;
}

const ManageAdmins: React.FC = () => {
  const [admins, setAdmins] = useState<Admin[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);

  useEffect(() => {
    fetchAdmins();
  }, []);

  const fetchAdmins = async () => {
    try {
      setLoading(true);
      const response = await axios.get('/api/admin/view-users/admin');
      setAdmins(response.data);
      setError('');
    } catch (err) {
      setError('Failed to load admin data');
      console.error('Error fetching admins:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleChangeUserType = async (userId: string, newUserType: string) => {
    try {
      await axios.post('/api/admin/change-user-type', { userId, newUserType });
      await fetchAdmins();
    } catch (error: any) {
      setError(error.message);
    }
  };

  const handleRemoveUser = async (userId: string) => {
    try {
      await axios.delete(`/api/admin/remove-user/${userId}`);
      await fetchAdmins();
    } catch (error: any) {
      setError(error.message);
    }
  };

  const filteredAdmins = admins.filter(admin =>
    admin.userName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    admin.email.toLowerCase().includes(searchTerm.toLowerCase())
  );

  if (loading) {
    return (
      <Box sx={{ width: '100%', mt: 2 }}>
        <LinearProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mt: 2 }}>
        {error}
      </Alert>
    );
  }

  return (
    <Box sx={{ width: '100%', p: 3 }}>
      <Typography variant="h5" gutterBottom>
        Manage Administrators
      </Typography>

      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <TextField
          fullWidth
          variant="outlined"
          placeholder="Search admins by username or email..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          InputProps={{
            endAdornment: (
              <IconButton>
                <SearchIcon />
              </IconButton>
            ),
          }}
        />
        <Button
          variant="contained"
          color="primary"
          onClick={() => setShowCreateModal(true)}
          sx={{ ml: 2 }}
        >
          Create User
        </Button>
      </Box>

      {showCreateModal && (
        <CreateUserModal open={showCreateModal} onClose={() => setShowCreateModal(false)} />
      )}

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>UserName</TableCell>
              <TableCell>Email</TableCell>
              <TableCell>Created At</TableCell>
              <TableCell>Last Login</TableCell>
              <TableCell>User Type</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredAdmins.map((admin) => (
              <TableRow key={admin.userId}>
                <TableCell>{admin.userName}</TableCell>
                <TableCell>{admin.email}</TableCell>
                <TableCell>{new Date(admin.createdAt).toLocaleDateString()}</TableCell>
                <TableCell>{new Date(admin.lastLogin).toLocaleDateString()}</TableCell>
                <TableCell>
                  <Select
                    size="small"
                    value={'admin'}
                    onChange={(e) => handleChangeUserType(admin.userId.toString(), e.target.value)}
                  >
                    <MenuItem value="admin">Admin</MenuItem>
                    <MenuItem value="sponsor">Sponsor</MenuItem>
                    <MenuItem value="driver">Driver</MenuItem>
                  </Select>
                </TableCell>
                <TableCell>
                  <IconButton
                    onClick={() => handleRemoveUser(admin.userId.toString())}
                    color="error"
                    size="small"
                  >
                    <DeleteIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};

export default ManageAdmins;