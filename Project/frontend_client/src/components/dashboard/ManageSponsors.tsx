import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Grid,
  CircularProgress,
  Alert,
  TextField,
  IconButton,
  Select,
  MenuItem,
  Chip,
  LinearProgress
} from '@mui/material';
import { PieChart, BarChart } from '@mui/x-charts';
import SearchIcon from '@mui/icons-material/SearchRounded';
import DeleteIcon from '@mui/icons-material/Delete';
import axios from 'axios';

interface Sponsor {
  userId: number;
  name: string;
  email: string;
  companyName: string;
  sponsorType: string;
  pointDollarValue: number;
  userType?: string;
  driverRelationships: {
    driverId: number;
    driverName: string;
  }[];
}

interface SponsorMetrics {
  sponsorId: number;
  sponsorName: string;
  totalDrivers: number;
  totalPoints: number;
  averagePoints: number;
}

const ManageSponsors: React.FC = () => {
  const [sponsors, setSponsors] = useState<Sponsor[]>([]);
  const [sponsorMetrics, setSponsorMetrics] = useState<SponsorMetrics[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [sponsorsResponse, metricsResponse] = await Promise.all([
        axios.get('/api/admin/sponsors/details'),
        axios.get('/api/admin/drivers/details')
      ]);

      setSponsors(sponsorsResponse.data);

      // Process metrics data
      const sponsorMap = new Map<number, SponsorMetrics>();
      metricsResponse.data.forEach((driver: any) => {
        driver.sponsorRelationships.forEach((rel: any) => {
          const existing = sponsorMap.get(rel.sponsorId) || {
            sponsorId: rel.sponsorId,
            sponsorName: rel.sponsorName,
            totalDrivers: 0,
            totalPoints: 0,
            averagePoints: 0,
          };

          sponsorMap.set(rel.sponsorId, {
            ...existing,
            totalDrivers: existing.totalDrivers + 1,
            totalPoints: existing.totalPoints + rel.points,
            averagePoints: (existing.totalPoints + rel.points) / (existing.totalDrivers + 1)
          });
        });
      });

      setSponsorMetrics(Array.from(sponsorMap.values()));
      setError(null);
    } catch (err) {
      setError('Failed to load sponsor data');
      console.error('Error fetching data:', err);
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

  // Chart data preparation
  const driverDistributionData = sponsorMetrics.map(sponsor => ({
    id: sponsor.sponsorId,
    value: sponsor.totalDrivers,
    label: sponsor.sponsorName
  }));

  const pointsData = sponsorMetrics.map(sponsor => ({
    value: sponsor.totalPoints,
    label: sponsor.sponsorName
  }));

  return (
    <Box sx={{ width: '100%', p: 3 }}>
      <Typography variant="h5" gutterBottom>
        Sponsor Management Dashboard
      </Typography>

      {/* Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Total Sponsors
              </Typography>
              <Typography variant="h3">
                {sponsors.length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Total Drivers
              </Typography>
              <Typography variant="h3">
                {sponsorMetrics.reduce((sum, s) => sum + s.totalDrivers, 0)}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Total Points
              </Typography>
              <Typography variant="h3">
                {sponsorMetrics.reduce((sum, s) => sum + s.totalPoints, 0).toLocaleString()}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Charts */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Driver Distribution by Sponsor
              </Typography>
              <Box sx={{ height: 400 }}>
                <PieChart
                  series={[{
                    data: driverDistributionData,
                    highlightScope: { faded: 'global', highlighted: 'item' },
                  }]}
                  height={350}
                />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Total Points by Sponsor
              </Typography>
              <Box sx={{ height: 400 }}>
                <BarChart
                  xAxis={[{
                    scaleType: 'band',
                    data: sponsorMetrics.map(s => s.sponsorName)
                  }]}
                  series={[{
                    data: sponsorMetrics.map(s => s.totalPoints),
                  }]}
                  height={350}
                />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Sponsor Management Table */}
      <Card>
        <CardContent>
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
                  <TableCell>Total Drivers</TableCell>
                  <TableCell>Total Points</TableCell>
                  <TableCell>User Type</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredSponsors.map((sponsor) => {
                  const metrics = sponsorMetrics.find(m => m.sponsorId === sponsor.userId);
                  return (
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
                      <TableCell>{metrics?.totalDrivers || 0}</TableCell>
                      <TableCell>{metrics?.totalPoints.toLocaleString() || 0}</TableCell>
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
                  );
                })}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>
    </Box>
  );
};

export default ManageSponsors;