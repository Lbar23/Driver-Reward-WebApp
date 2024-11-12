import * as React from 'react';
import Box from '@mui/material/Box';
import Grid from '@mui/material/Grid2';
import Stack from '@mui/material/Stack';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import CircularProgress from '@mui/material/CircularProgress';
import Alert from '@mui/material/Alert';
import LinearProgress from '@mui/material/LinearProgress';
import ChevronRightRoundedIcon from '@mui/icons-material/ChevronRightRounded';
import GroupRoundedIcon from '@mui/icons-material/GroupRounded';
import WorkspacePremiumRoundedIcon from '@mui/icons-material/WorkspacePremiumRounded';
import useMediaQuery from '@mui/material/useMediaQuery';
import { useTheme } from '@mui/material/styles';
import { BarChart, LineChart, PieChart } from '@mui/x-charts';

interface Driver {
  userID: number;
  email: string;
  name: string;
  totalPoints: number;
}

function StatsCard({ 
  icon: Icon, 
  title, 
  value, 
  description 
}: { 
  icon: React.ElementType; 
  title: string; 
  value: string; 
  description: string; 
}) {
  return (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Icon sx={{ color: 'primary.main', mb: 1 }} />
        <Typography
          component="h2"
          variant="subtitle2"
          sx={{ fontWeight: '600', mb: 1 }}
        >
          {title}
        </Typography>
        <Typography variant="h4" sx={{ mb: 1 }}>
          {value}
        </Typography>
        <Typography sx={{ color: 'text.secondary', mb: 1 }}>
          {description}
        </Typography>
      </CardContent>
    </Card>
  );
}

export default function SponsorDrivers() {
  const theme = useTheme();
  const isSmallScreen = useMediaQuery(theme.breakpoints.down('sm'));
  const [drivers, setDrivers] = React.useState<Driver[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    const fetchDrivers = async () => {
      try {
        const response = await fetch('/api/sponsor/drivers', {
          credentials: 'include',
        });
        
        if (!response.ok) {
          throw new Error('Failed to fetch drivers');
        }
        
        const data = await response.json();
        setDrivers(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred while fetching drivers');
      } finally {
        setLoading(false);
      }
    };

    fetchDrivers();
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

  const totalPoints = drivers.reduce((sum, driver) => sum + driver.totalPoints, 0);
  
  // Data for charts
  const pieChartData = drivers.map(driver => ({
    id: driver.userID,
    value: driver.totalPoints,
    label: driver.email
  }));

  // Sort drivers by points for leaderboard
  const sortedDrivers = [...drivers].sort((a, b) => b.totalPoints - a.totalPoints);
  const maxPoints = Math.max(...drivers.map(d => d.totalPoints));

  return (
    <Box sx={{ width: '100%' }}>
      <Typography component="h2" variant="h6" sx={{ mb: 2 }}>
        Sponsored Drivers Overview
      </Typography>
      
      <Grid container spacing={2} columns={12}>
        {/* Drivers Table */}
        <Grid size={{ xs: 12, lg: 9 }}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              {drivers.length === 0 ? (
                <Box display="flex" justifyContent="center" alignItems="center" minHeight={200}>
                  <Typography color="text.secondary">
                    No drivers found under your sponsorship.
                  </Typography>
                </Box>
              ) : (
                <TableContainer>
                  <Table sx={{ minWidth: 650 }}>
                    <TableHead>
                      <TableRow>
                        <TableCell>Driver ID</TableCell>
                        <TableCell>Email</TableCell>
                        <TableCell align="right">Total Points</TableCell>
                        <TableCell align="right">Progress</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {drivers.map((driver) => (
                        <TableRow key={driver.userID}>
                          <TableCell component="th" scope="row">
                            {driver.userID}
                          </TableCell>
                          <TableCell>{driver.email}</TableCell>
                          <TableCell align="right">
                            {driver.totalPoints.toLocaleString()}
                          </TableCell>
                          <TableCell align="right" sx={{ width: '30%' }}>
                            <Box sx={{ display: 'flex', alignItems: 'center' }}>
                              <Box sx={{ width: '100%', mr: 1 }}>
                                <LinearProgress
                                  variant="determinate"
                                  value={(driver.totalPoints / maxPoints) * 100}
                                />
                              </Box>
                              <Typography variant="body2" color="text.secondary">
                                {Math.round((driver.totalPoints / maxPoints) * 100)}%
                              </Typography>
                            </Box>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              )}
            </CardContent>
          </Card>
        </Grid>
        
        {/* Stats Cards */}
        <Grid size={{ xs: 12, lg: 3 }}>
          <Stack gap={2} direction={{ xs: 'column', sm: 'row', lg: 'column' }}>
            <StatsCard 
              icon={GroupRoundedIcon}
              title="Active Drivers"
              value={drivers.length.toString()}
              description="Currently sponsored drivers"
            />
            <StatsCard 
              icon={WorkspacePremiumRoundedIcon}
              title="Total Points"
              value={totalPoints.toLocaleString()}
              description="Accumulated driver points"
            />
          </Stack>
        </Grid>

        {/* Points Distribution Chart */}
        {drivers.length > 0 && (
          <Grid size={12}>
            <Card sx={{ mt: 2 }}>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Points Distribution
                </Typography>
                <Box sx={{ height: 400 }}>
                  <PieChart
                    series={[{
                      data: pieChartData,
                      highlightScope: { faded: 'global', highlighted: 'item' },
                    }]}
                    height={350}
                  />
                </Box>
              </CardContent>
            </Card>
          </Grid>
        )}

        {/* Driver Leaderboard */}
        {drivers.length > 0 && (
          <Grid size={12}>
            <Card sx={{ mt: 2 }}>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Driver Leaderboard
                </Typography>
                <TableContainer>
                  <Table>
                    <TableHead>
                      <TableRow>
                        <TableCell>Rank</TableCell>
                        <TableCell>Driver</TableCell>
                        <TableCell align="right">Points</TableCell>
                        <TableCell align="right">Progress</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {sortedDrivers.map((driver, index) => (
                        <TableRow key={driver.userID}>
                          <TableCell>{index + 1}</TableCell>
                          <TableCell>{driver.email}</TableCell>
                          <TableCell align="right">
                            {driver.totalPoints.toLocaleString()}
                          </TableCell>
                          <TableCell align="right" sx={{ width: '30%' }}>
                            <Box sx={{ display: 'flex', alignItems: 'center' }}>
                              <Box sx={{ width: '100%', mr: 1 }}>
                                <LinearProgress
                                  variant="determinate"
                                  value={(driver.totalPoints / maxPoints) * 100}
                                />
                              </Box>
                              <Typography variant="body2" color="text.secondary">
                                {Math.round((driver.totalPoints / maxPoints) * 100)}%
                              </Typography>
                            </Box>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </CardContent>
            </Card>
          </Grid>
        )}
      </Grid>

      {/* Driver Management Section */}
      <Typography component="h2" variant="h6" sx={{ mb: 2, mt: 4 }}>
        Driver Management
      </Typography>
      <Grid container spacing={2} columns={12}>
        <Grid size={{ xs: 12, sm: 6, lg: 4 }}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <GroupRoundedIcon sx={{ color: 'primary.main', mb: 1 }} />
              <Typography
                component="h2"
                variant="subtitle2"
                sx={{ fontWeight: '600' }}
              >
                Manage Drivers
              </Typography>
              <Typography sx={{ color: 'text.secondary', mb: 2 }}>
                Add or remove drivers from your sponsorship.
              </Typography>
              <Button
                variant="contained"
                size="small"
                color="primary"
                endIcon={<ChevronRightRoundedIcon />}
                fullWidth={isSmallScreen}
              >
                Manage Drivers
              </Button>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}