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
  Typography,
  LinearProgress,
  Select,
  MenuItem,
  FormControl,
  InputLabel
} from '@mui/material';
import axios from 'axios';

interface SponsorPoints {
  sponsorId: number;
  sponsorName: string;
  totalPoints: number;
  pointDollarValue: number;
}

const DriverPointsList: React.FC = () => {
  const [sponsorPoints, setSponsorPoints] = useState<SponsorPoints[]>([]);
  const [selectedSponsorId, setSelectedSponsorId] = useState<number | ''>('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchSponsorPoints();
  }, []);

  const fetchSponsorPoints = async () => {
    try {
      setLoading(true);
      const response = await axios.get('/api/driver/my-sponsors');
      setSponsorPoints(response.data);
      // Set the first sponsor as default if none selected
      if (response.data.length > 0 && !selectedSponsorId) {
        setSelectedSponsorId(response.data[0].sponsorId);
      }
      setError('');
    } catch (err) {
      setError('Failed to load sponsor points data');
      console.error('Error fetching sponsor points:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSponsorChange = (event: any) => {
    setSelectedSponsorId(event.target.value);
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

  if (sponsorPoints.length === 0) {
    return (
      <Typography sx={{ mt: 2 }}>
        You are not currently registered with any sponsors.
      </Typography>
    );
  }

  const selectedSponsor = sponsorPoints.find(sp => sp.sponsorId === selectedSponsorId);

  return (
    <Box sx={{ width: '100%', mb: 2, mt: 2 }}>
      <FormControl fullWidth sx={{ mb: 2 }}>
        <InputLabel>Select Sponsor</InputLabel>
        <Select
          value={selectedSponsorId}
          label="Select Sponsor"
          onChange={handleSponsorChange}
        >
          {sponsorPoints.map((sp) => (
            <MenuItem key={sp.sponsorId} value={sp.sponsorId}>
              {sp.sponsorName}
            </MenuItem>
          ))}
        </Select>
      </FormControl>

      {selectedSponsor && (
        <Paper>
          <TableContainer>
            <Table aria-label="sponsor points details">
              <TableHead>
                <TableRow>
                  <TableCell>Sponsor</TableCell>
                  <TableCell align="right">Total Points</TableCell>
                  <TableCell align="right">Point Value ($)</TableCell>
                  <TableCell align="right">Total Value ($)</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                <TableRow>
                  <TableCell>{selectedSponsor.sponsorName}</TableCell>
                  <TableCell align="right">
                    {selectedSponsor.totalPoints.toLocaleString()}
                  </TableCell>
                  <TableCell align="right">
                    ${selectedSponsor.pointDollarValue.toFixed(2)}
                  </TableCell>
                  <TableCell align="right">
                    ${(selectedSponsor.totalPoints * selectedSponsor.pointDollarValue).toFixed(2)}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>
      )}
    </Box>
  );
};

export default DriverPointsList;