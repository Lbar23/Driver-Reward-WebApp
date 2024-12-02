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
import Chip from '@mui/material/Chip';
import Tooltip from '@mui/material/Tooltip';
import TextField from '@mui/material/TextField';
import SearchIcon from '@mui/icons-material/SearchRounded';
import IconButton from '@mui/material/IconButton';
import Select from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';
import DeleteIcon from '@mui/icons-material/Delete';

interface Driver {
  userId: number;
  name: string;
  email: string;
  userType?: string;
  sponsorRelationships: {
    sponsorId: number;
    sponsorName: string;
    points: number;
  }[];
}

const ManageDriverSponsors: React.FC = () => {
  const [drivers, setDrivers] = useState<Driver[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    fetchDrivers();
  }, []);

  const fetchDrivers = async () => {
    try {
      setLoading(true);
      const response = await axios.get('/api/admin/view-users/driver');
      setDrivers(response.data);
      setError('');
    } catch (err) {
      setError('Failed to load driver data');
      console.error('Error fetching drivers:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleChangeUserType = async (userId: string, newUserType: string) => {
    try {
      await axios.post('/api/admin/change-user-type', { userId, newUserType });
      await fetchDrivers(); // Reuse existing fetch function
    } catch (error: any) {
      setError(error.message);
    }
  };

  const handleRemoveUser = async (userId: string) => {
    try {
      await axios.delete(`/api/admin/remove-user/${userId}`);
      await fetchDrivers(); // Reuse existing fetch function
    } catch (error: any) {
      setError(error.message);
    }
  };

  const filteredDrivers = Array.isArray(drivers)
    ? drivers.filter(driver =>
        driver.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        driver.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
        driver.sponsorRelationships.some(sr =>
          sr.sponsorName.toLowerCase().includes(searchTerm.toLowerCase())
        )
      )
    : [];

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
        Driver-Sponsor Associations
      </Typography>

      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <TextField
          fullWidth
          variant="outlined"
          placeholder="Search by driver name, email, or sponsor..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
        <IconButton sx={{ ml: -4 }}>
          <SearchIcon />
        </IconButton>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Driver Name</TableCell>
              <TableCell>Email</TableCell>
              <TableCell>Sponsors</TableCell>
              <TableCell align="center">Total Sponsors</TableCell>
              <TableCell>Points Distribution</TableCell>
              <TableCell>User Type</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredDrivers.map((driver) => (
              <TableRow key={driver.userId}>
                <TableCell>{driver.name}</TableCell>
                <TableCell>{driver.email}</TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                    {driver.sponsorRelationships.map((sr) => (
                      <Chip
                        key={sr.sponsorId}
                        label={sr.sponsorName}
                        size="small"
                        color="primary"
                      />
                    ))}
                  </Box>
                </TableCell>
                <TableCell align="center">
                  {driver.sponsorRelationships.length}
                </TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    {driver.sponsorRelationships.map((sr) => (
                      <Tooltip
                        key={sr.sponsorId}
                        title={`${sr.sponsorName}: ${sr.points} points`}
                      >
                        <Box sx={{ display: 'flex', alignItems: 'center' }}>
                          <Typography variant="body2">
                            {sr.points}
                          </Typography>
                        </Box>
                      </Tooltip>
                    ))}
                  </Box>
                </TableCell>
                <TableCell>
                  <Select
                    size="small"
                    value={driver.userType || ''}
                    onChange={(e) => handleChangeUserType(driver.userId.toString(), e.target.value)}
                  >
                    <MenuItem value="driver">Driver</MenuItem>
                    <MenuItem value="sponsor">Sponsor</MenuItem>
                    <MenuItem value="admin">Admin</MenuItem>
                  </Select>
                </TableCell>
                <TableCell>
                  <IconButton
                    onClick={() => handleRemoveUser(driver.userId.toString())}
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

export default ManageDriverSponsors;