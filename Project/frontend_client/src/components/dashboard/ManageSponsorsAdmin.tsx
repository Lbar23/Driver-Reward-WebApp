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
} from '@mui/material';
import { PieChart, BarChart } from '@mui/x-charts';

interface DriverDetail {
  userId: number;
  name: string;
  email: string;
  sponsorRelationships: {
    sponsorId: number;
    sponsorName: string;
    points: number;
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
  const [driverDetails, setDriverDetails] = useState<DriverDetail[]>([]);
  const [sponsorMetrics, setSponsorMetrics] = useState<SponsorMetrics[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const response = await fetch('/api/admin/drivers/details', {
          credentials: 'include',
        });

        if (!response.ok) {
          throw new Error('Failed to fetch data');
        }

        const data: DriverDetail[] = await response.json();
        setDriverDetails(data);

        // Process data to get sponsor metrics
        const sponsorMap = new Map<number, SponsorMetrics>();
        
        data.forEach(driver => {
          driver.sponsorRelationships.forEach(rel => {
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
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight={400}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        {error}
      </Alert>
    );
  }

  // Prepare chart data
  const driverDistributionData = sponsorMetrics.map(sponsor => ({
    id: sponsor.sponsorId,
    value: sponsor.totalDrivers,
    label: sponsor.sponsorName
  }));

  const pointsData = sponsorMetrics.map(sponsor => ({
    id: sponsor.sponsorId,
    value: sponsor.totalPoints,
    label: sponsor.sponsorName
  }));

  return (
    <Box sx={{ width: '100%' }}>
      <Typography variant="h5" gutterBottom>
        Sponsor Management Overview
      </Typography>

      <Grid container spacing={2}>
        {/* Summary Statistics */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Grid container spacing={3}>
                <Grid item xs={12} md={4}>
                  <Typography variant="h6" gutterBottom>
                    Total Sponsors
                  </Typography>
                  <Typography variant="h3">
                    {sponsorMetrics.length}
                  </Typography>
                </Grid>
                <Grid item xs={12} md={4}>
                  <Typography variant="h6" gutterBottom>
                    Total Drivers
                  </Typography>
                  <Typography variant="h3">
                    {driverDetails.length}
                  </Typography>
                </Grid>
                <Grid item xs={12} md={4}>
                  <Typography variant="h6" gutterBottom>
                    Total Points
                  </Typography>
                  <Typography variant="h3">
                    {sponsorMetrics.reduce((sum, s) => sum + s.totalPoints, 0).toLocaleString()}
                  </Typography>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        </Grid>

        {/* Charts */}
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
                    label: 'Total Points'
                  }]}
                  height={350}
                />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Detailed Table */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Sponsor Details
              </Typography>
              <TableContainer component={Paper}>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Sponsor Name</TableCell>
                      <TableCell align="right">Total Drivers</TableCell>
                      <TableCell align="right">Total Points</TableCell>
                      <TableCell align="right">Average Points per Driver</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {sponsorMetrics.map((sponsor) => (
                      <TableRow key={sponsor.sponsorId}>
                        <TableCell>{sponsor.sponsorName}</TableCell>
                        <TableCell align="right">{sponsor.totalDrivers}</TableCell>
                        <TableCell align="right">{sponsor.totalPoints.toLocaleString()}</TableCell>
                        <TableCell align="right">
                          {Math.round(sponsor.averagePoints).toLocaleString()}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default ManageSponsors;