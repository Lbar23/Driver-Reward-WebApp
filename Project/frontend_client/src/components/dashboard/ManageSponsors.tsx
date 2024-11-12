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
import TextField from '@mui/material/TextField';
import SearchIcon from '@mui/icons-material/SearchRounded';
import IconButton from '@mui/material/IconButton';
import Select from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';
import DeleteIcon from '@mui/icons-material/Delete';

interface Sponsor {
  userId: number;
  name: string;
  email: string;
  companyName: string;
  sponsorType: string;
  pointDollarValue: number;
  driverRelationships: {
    driverId: number;
    driverName: string;
  }[];
  userType?: string;
}

const ManageSponsors: React.FC = () => {
  const [sponsors, setSponsors] = useState<Sponsor[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    fetchSponsors();
  }, []);

  const fetchSponsors = async () => {
    try {
      setLoading(true);
      const response = await axios.get('/api/admin/sponsors/details');
      setSponsors(response.data);
      setError('');
    } catch (err) {
      setError('Failed to load sponsor data');
      console.error('Error fetching sponsors:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleChangeUserType = async (userId: string, newUserType: string) => {
    try {
      await axios.post('/api/admin/change-user-type', { userId, newUserType });
      await fetchSponsors();
    } catch (error: any) {
      setError(error.message);
    }
  };

  const handleRemoveUser = async (userId: string) => {
    try {
      await axios.delete(`/api/admin/remove-user/${userId}`);
      await fetchSponsors();
    } catch (error: any) {
      setError(error.message);
    }
  };

  const filteredSponsors = sponsors.filter(sponsor =>
    sponsor.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    sponsor.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
    sponsor.companyName.toLowerCase().includes(searchTerm.toLowerCase())
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
        Manage Sponsors
      </Typography>

      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <TextField
          fullWidth
          variant="outlined"
          placeholder="Search sponsors by name, email, or company..."
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
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Email</TableCell>
              <TableCell>Company</TableCell>
              <TableCell>Sponsor Type</TableCell>
              <TableCell>Point Value ($)</TableCell>
              <TableCell>User Type</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredSponsors.map((sponsor) => (
              <TableRow key={sponsor.userId}>
                <TableCell>{sponsor.name}</TableCell>
                <TableCell>{sponsor.email}</TableCell>
                <TableCell>{sponsor.companyName}</TableCell>
                <TableCell>
                  <Chip
                    label={sponsor.sponsorType}
                    color="primary"
                    size="small"
                  />
                </TableCell>
                <TableCell>${sponsor.pointDollarValue.toFixed(2)}</TableCell>
                <TableCell>
                  <Select
                    size="small"
                    value={sponsor.userType || 'sponsor'}
                    onChange={(e) => handleChangeUserType(sponsor.userId.toString(), e.target.value)}
                  >
                    <MenuItem value="sponsor">Sponsor</MenuItem>
                    <MenuItem value="admin">Admin</MenuItem>
                    <MenuItem value="driver">Driver</MenuItem>
                  </Select>
                </TableCell>
                <TableCell>
                  <IconButton
                    onClick={() => handleRemoveUser(sponsor.userId.toString())}
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

export default ManageSponsors;