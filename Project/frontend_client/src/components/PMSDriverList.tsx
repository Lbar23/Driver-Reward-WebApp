import React, { useState, useEffect } from 'react';
import {
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TableSortLabel,
  Typography,
  LinearProgress
} from '@mui/material';
import axios from 'axios';

//Will definitely be broken, considering no city, state, and name since db changes (can remove to test with actual data later)
interface Driver {
  id: number;
  name: string;
  totalPoints: number;
  city: string;
  state: string;
}

const DriverPointsList: React.FC = () => {
    const [drivers, setDrivers] = useState<Driver[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [sortBy, setSortBy] = useState<'points' | 'name'>('points');
    const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');
  
    useEffect(() => {
      fetchDrivers();
    }, [sortBy, sortOrder]);
  
    const fetchDrivers = async () => {
      try {
        setLoading(true);
        const response = await axios.get('/api/sponsor/drivers', {
          params: {
            sortBy,
            sortOrder
          }
        });
        setDrivers(response.data);
        setError('');
      } catch (err) {
        setError('Failed to load drivers data');
        console.error('Error fetching drivers:', err);
      } finally {
        setLoading(false);
      }
    };
  
    const handleSort = (field: 'points' | 'name') => {
      if (sortBy === field) {
        setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
      } else {
        setSortBy(field);
        setSortOrder('desc');
      }
    };
  
    if (loading) {
      return (
        <Box sx={{ width: '100%', mt: 2 }}>
          <LinearProgress />
        </Box>
      );
    }
  
    if (error) {
      return (
        <Typography color="error" sx={{ mt: 2 }}>
          {error}
        </Typography>
      );
    }
  
    return (
      <Paper sx={{ width: '100%', mb: 2, mt: 2 }}>
        <TableContainer>
          <Table aria-label="drivers points table">
            <TableHead>
              <TableRow>
                <TableCell>
                  <TableSortLabel
                    active={sortBy === 'name'}
                    direction={sortBy === 'name' ? sortOrder : 'asc'}
                    onClick={() => handleSort('name')}
                  >
                    Driver Name
                  </TableSortLabel>
                </TableCell>
                <TableCell>
                  <TableSortLabel
                    active={sortBy === 'points'}
                    direction={sortBy === 'points' ? sortOrder : 'asc'}
                    onClick={() => handleSort('points')}
                  >
                    Total Points
                  </TableSortLabel>
                </TableCell>
                <TableCell>City</TableCell>
                <TableCell>State</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {drivers.map((driver) => (
                <TableRow key={driver.id}>
                  <TableCell>{driver.name}</TableCell>
                  <TableCell>{driver.totalPoints.toLocaleString()}</TableCell>
                  <TableCell>{driver.city}</TableCell>
                  <TableCell>{driver.state}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>
    );
  };

export default DriverPointsList;