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
  InputLabel,
  Grid
} from '@mui/material';
import { PieChart, BarChart } from '@mui/x-charts';
import axios from 'axios';

interface SponsorPoints {
  sponsorId: number;
  sponsorName: string;
  totalPoints: number;
  pointDollarValue: number;
}

interface PointHistory {
  date: string;
  points: number;
  reason: string;
}

interface SponsorPointDetails {
  sponsorId: number;
  sponsorName: string;
  totalPoints: number;
  pointDollarValue: number;
  pointsHistory: PointHistory[];
}

const DriverPointsList: React.FC = () => {
  const [sponsorPoints, setSponsorPoints] = useState<SponsorPoints[]>([]);
  const [selectedSponsorDetails, setSelectedSponsorDetails] = useState<SponsorPointDetails | null>(null);
  const [selectedSponsorId, setSelectedSponsorId] = useState<number | ''>('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchSponsorPoints();
  }, []);

  useEffect(() => {
    if (selectedSponsorId) {
      fetchSponsorDetails(selectedSponsorId);
    }
  }, [selectedSponsorId]);

  const fetchSponsorPoints = async () => {
    try {
      setLoading(true);
      const response = await axios.get('/api/driver/my-sponsors');
      setSponsorPoints(response.data);
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

  const fetchSponsorDetails = async (sponsorId: number) => {
    try {
      const response = await axios.get(`/api/driver/sponsor-points/${sponsorId}`);
      setSelectedSponsorDetails(response.data);
    } catch (err) {
      console.error('Error fetching sponsor details:', err);
    }
  };

  const handleSponsorChange = (event: any) => {
    setSelectedSponsorId(event.target.value);
  };

  if (loading) {
    return <Box sx={{ width: '100%', mt: 2 }}><LinearProgress /></Box>;
  }

  if (error) {
    return <Typography color="error" sx={{ mt: 2 }}>{error}</Typography>;
  }

  if (sponsorPoints.length === 0) {
    return (
      <Typography sx={{ mt: 2 }}>
        You are not currently registered with any sponsors.
      </Typography>
    );
  }

  const selectedSponsor = sponsorPoints.find(sp => sp.sponsorId === selectedSponsorId);

  // Format data for pie chart
  const pieChartData = sponsorPoints.map(sp => ({
    id: sp.sponsorId,
    value: sp.totalPoints,
    label: sp.sponsorName
  }));

  // Format data for bar chart
  const barChartData = selectedSponsorDetails?.pointsHistory.map(ph => ({
    date: new Date(ph.date).toLocaleDateString(),
    points: ph.points,
    reason: ph.reason
  })).reverse() || [];

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

      <Grid container spacing={3}>
        {/* Points Table */}
        <Grid item xs={12}>
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
                  {selectedSponsor && (
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
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </Paper>
        </Grid>

        {/* Points Distribution Pie Chart */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>Points Distribution Across Sponsors</Typography>
            <Box sx={{ width: '100%', height: 300 }}>
              <PieChart
                series={[
                  {
                    data: pieChartData,
                    highlightScope: { faded: 'global', highlighted: 'item' },
                    faded: { innerRadius: 30, additionalRadius: -30 }
                  }
                ]}
                height={300}
              />
            </Box>
          </Paper>
        </Grid>

        {/* Points History Bar Chart */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>Recent Points History</Typography>
            {barChartData.length > 0 ? (
              <Box sx={{ width: '100%', height: 300 }}>
                <BarChart
                  xAxis={[{ 
                    scaleType: 'band', 
                    data: barChartData.map(d => d.date),
                    label: 'Date'
                  }]}
                  series={[
                    { 
                      data: barChartData.map(d => d.points),
                      label: 'Points'
                    }
                  ]}
                  height={300}
                  tooltip={{ trigger: 'item' }}
                />
              </Box>
            ) : (
              <Typography>No history data available</Typography>
            )}
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default DriverPointsList;