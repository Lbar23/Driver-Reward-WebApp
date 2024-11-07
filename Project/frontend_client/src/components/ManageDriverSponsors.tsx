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
import InputAdornment from '@mui/material/InputAdornment';
import SearchRoundedIcon from '@mui/icons-material/SearchRounded';

interface Driver {
  userId: number;
  name: string;
  email: string;
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
      const response = await axios.get('/api/admin/drivers/details');
      setDrivers(response.data);
      setError('');
    } catch (err) {
      setError('Failed to load driver data');
      console.error('Error fetching drivers:', err);
    } finally {
      setLoading(false);
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

      <TextField
        fullWidth
        variant="outlined"
        placeholder="Search by driver name, email, or sponsor..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        sx={{ mb: 3 }}
        InputProps={{
          startAdornment: (
            <InputAdornment position="start">
              <SearchRoundedIcon fontSize="small" />
            </InputAdornment>
          ),
        }}
      />

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Driver Name</TableCell>
              <TableCell>Email</TableCell>
              <TableCell>Sponsors</TableCell>
              <TableCell align="center">Total Sponsors</TableCell>
              <TableCell>Points Distribution</TableCell>
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
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};

export default ManageDriverSponsors;
