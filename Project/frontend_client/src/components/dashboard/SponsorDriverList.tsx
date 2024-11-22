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
import MonetizationOnIcon from '@mui/icons-material/MonetizationOn';
import EditIcon from '@mui/icons-material/Edit';
import SaveIcon from '@mui/icons-material/Save';
import CancelIcon from '@mui/icons-material/Cancel';
import useMediaQuery from '@mui/material/useMediaQuery';
import { useTheme } from '@mui/material/styles';
import { PieChart } from '@mui/x-charts/PieChart';
import TextField from '@mui/material/TextField';
import IconButton from '@mui/material/IconButton';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import Snackbar from '@mui/material/Snackbar';

interface Driver {
  userID: number;
  email: string;
  name: string;
  totalPoints: number;
}

interface PointRatio {
  pointDollarValue: number;
  sponsorID: number;
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
  description: string | React.ReactNode; 
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
  const [sponsorInfo, setSponsorInfo] = React.useState<PointRatio | null>(null);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);
  const [editingPoints, setEditingPoints] = React.useState<{ [key: number]: number }>({});
  const [showPointValueDialog, setShowPointValueDialog] = React.useState(false);
  const [newPointValue, setNewPointValue] = React.useState('');
  const [snackbar, setSnackbar] = React.useState({ 
    open: false, 
    message: '', 
    severity: 'success' as 'success' | 'error' 
  });

  // Fetch both sponsor info and drivers data
  React.useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        
        // Fetch drivers
        const driversResponse = await fetch('/api/sponsor/drivers', {
          credentials: 'include',
        });
        
        if (!driversResponse.ok) {
          throw new Error('Failed to fetch drivers');
        }
        
        const driversData = await driversResponse.json();
        setDrivers(driversData);

        // Fetch sponsor's point ratio using new endpoint
        const pointRatioResponse = await fetch('/api/sponsor/point-ratio', {
          credentials: 'include',
        });

        if (!pointRatioResponse.ok) {
          throw new Error('Failed to fetch point ratio');
        }

        const pointRatioData: PointRatio = await pointRatioResponse.json();
        setSponsorInfo(pointRatioData);


      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred while fetching data');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  const handleUpdatePointValue = async () => {
    if (!sponsorInfo) return;
    
    try {
      const response = await fetch(`/api/sponsor/point-value`, {
        method: 'PUT',
        headers: { 
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(parseFloat(newPointValue)),
      });

      if (!response.ok) throw new Error('Failed to update point value');

      const updatedValue = await response.json();
      setSponsorInfo(prev => prev ? { ...prev, pointDollarValue: parseFloat(newPointValue) } : null);
      setShowPointValueDialog(false);
      setSnackbar({ open: true, message: 'Point value updated successfully', severity: 'success' });
    } catch (err) {
      setSnackbar({ 
        open: true, 
        message: err instanceof Error ? err.message : 'Failed to update point value',
        severity: 'error'
      });
    }
  };

  const handleUpdateDriverPoints = async (driverID: number, points: number) => {
    if (!sponsorInfo) return;

    try {
      const response = await fetch(
        `/api/sponsor/driver/${driverID}/points`,
        {
          method: 'PUT',
          headers: { 
            'Content-Type': 'application/json',
          },
          credentials: 'include',
          body: JSON.stringify(points),
        }
      );

      if (!response.ok) throw new Error('Failed to update driver points');

      const updatedPoints = await response.json();
      setDrivers(prev => 
        prev.map(driver => 
          driver.userID === driverID ? { ...driver, totalPoints: points } : driver
        )
      );
      setEditingPoints(prev => {
        const newState = { ...prev };
        delete newState[driverID];
        return newState;
      });
      setSnackbar({ open: true, message: 'Points updated successfully', severity: 'success' });
    } catch (err) {
      setSnackbar({ 
        open: true, 
        message: err instanceof Error ? err.message : 'Failed to update points',
        severity: 'error'
      });
    }
  };
  const formattedPointValue = sponsorInfo?.pointDollarValue !== undefined 
  ? `$${sponsorInfo.pointDollarValue.toFixed(2)}` 
  : '$0.00';

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
  const sortedDrivers = [...drivers].sort((a, b) => b.totalPoints - a.totalPoints);
  const maxPoints = Math.max(...drivers.map(d => d.totalPoints));

    // Data for charts
    const pieChartData = drivers.map(driver => ({
      id: driver.userID,
      value: driver.totalPoints,
      label: driver.email
    }));

  return (
    <Box sx={{ width: '100%' }}>
      <Typography component="h2" variant="h6" sx={{ mb: 2 }}>
        Sponsored Drivers Overview
      </Typography>
      
      <Grid container spacing={2} columns={12}>
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
            <StatsCard 
              icon={MonetizationOnIcon}
              title="Point Value"
              value={formattedPointValue}
              description={
                <Button
                  size="small"
                  startIcon={<EditIcon />}
                  onClick={() => {
                    setNewPointValue(sponsorInfo?.pointDollarValue.toString() || '0.00');
                    setShowPointValueDialog(true);
                  }}
                >
                  Update Value
                </Button>
              }
            />
          </Stack>
        </Grid>

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
                        <TableCell align="right">Points</TableCell>
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
                            {editingPoints[driver.userID] !== undefined ? (
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, justifyContent: 'flex-end' }}>
                                <TextField
                                  size="small"
                                  type="number"
                                  value={editingPoints[driver.userID]}
                                  onChange={(e) => setEditingPoints(prev => ({
                                    ...prev,
                                    [driver.userID]: parseInt(e.target.value) || 0
                                  }))}
                                  inputProps={{ min: 0 }}
                                />
                                <IconButton
                                  size="small"
                                  color="primary"
                                  onClick={() => handleUpdateDriverPoints(driver.userID, editingPoints[driver.userID])}
                                >
                                  <SaveIcon />
                                </IconButton>
                                <IconButton
                                  size="small"
                                  color="error"
                                  onClick={() => setEditingPoints(prev => {
                                    const newState = { ...prev };
                                    delete newState[driver.userID];
                                    return newState;
                                  })}
                                >
                                  <CancelIcon />
                                </IconButton>
                              </Box>
                            ) : (
                              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end', gap: 1 }}>
                                {driver.totalPoints.toLocaleString()}
                                <IconButton
                                  size="small"
                                  onClick={() => setEditingPoints(prev => ({
                                    ...prev,
                                    [driver.userID]: driver.totalPoints
                                  }))}
                                >
                                  <EditIcon />
                                </IconButton>
                              </Box>
                            )}
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
            {/* Point Value Dialog */}
            <Dialog open={showPointValueDialog} onClose={() => setShowPointValueDialog(false)}>
        <DialogTitle>Update Point Dollar Value</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label="Dollar Value per Point"
            type="number"
            fullWidth
            value={newPointValue}
            onChange={(e) => setNewPointValue(e.target.value)}
            inputProps={{ step: "0.01", min: "0" }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowPointValueDialog(false)}>Cancel</Button>
          <Button onClick={handleUpdatePointValue} variant="contained">
            Update
          </Button>
        </DialogActions>
      </Dialog>

      {/* Snackbar for notifications */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={() => setSnackbar(prev => ({ ...prev, open: false }))}
        message={snackbar.message}
      />
    </Box>
  );
}