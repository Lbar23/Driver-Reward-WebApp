import React, { useState, useEffect, useCallback } from 'react';
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
  Alert,
  TextField,
  IconButton,
  Select,
  MenuItem,
  Chip,
  LinearProgress,
  Button,
  Tab,
  Tabs,
  Tooltip,
} from '@mui/material';
import { PieChart, BarChart } from '@mui/x-charts';
import SearchIcon from '@mui/icons-material/SearchRounded';
import DeleteIcon from '@mui/icons-material/Delete';
import axios from 'axios';
import CreateUserModal from './CreateUserModal';

// Types
interface CurrentUser {
  role: 'Admin' | 'Sponsor';
  userId: number;
  sponsorId?: number;
}

interface Admin {
  userId: number;
  userName: string;
  firstName: string;
  lastName: string;
  email: string;
  createdAt: string;
  lastLogin: string;
}

interface Sponsor {
  userId: number;
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  sponsorId: number;
  companyName: string;
  sponsorType: string;
  pointDollarValue: number;
  isPrimary: boolean;
  joinDate: string;
}

interface SponsorRelationship {
  sponsorID: number;
  sponsorName: string;
  driverPoints: number;
  milestoneLevel: number;
}

interface Driver {
  userId: number;
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  sponsorRelationships: SponsorRelationship[];
}

interface SponsorMetrics {
  sponsorId: number;
  sponsorName: string;
  totalDrivers: number;
  totalPoints: number;
  averagePoints: number;
}

type UserType = 'driver' | 'sponsor' | 'admin';

const UserManagementDashboard: React.FC = () => {
  const [selectedTab, setSelectedTab] = useState(0);
  const [admins, setAdmins] = useState<Admin[]>([]);
  const [sponsors, setSponsors] = useState<Sponsor[]>([]);
  const [drivers, setDrivers] = useState<Driver[]>([]);
  const [sponsorMetrics, setSponsorMetrics] = useState<SponsorMetrics[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      if (selectedTab === 0) {
        const response = await axios.get('/api/admin/view-users/admin');
        setAdmins(response.data);
      } else if (selectedTab === 1) {
        const sponsorsResponse = await axios.get<Sponsor[]>('/api/admin/view-users/sponsor');
        setSponsors(sponsorsResponse.data);
        
        // Calculate sponsor metrics
        const sponsorMap = new Map<number, SponsorMetrics>();
        sponsorsResponse.data.forEach(sponsor => {
          sponsorMap.set(sponsor.sponsorId, {
            sponsorId: sponsor.sponsorId,
            sponsorName: sponsor.companyName,
            totalDrivers: 0,
            totalPoints: 0,
            averagePoints: 0
          });
        });

        // Update metrics with driver data
        const driversResponse = await axios.get('/api/admin/view-users/driver');
        if (driversResponse.data) {
          driversResponse.data.forEach((driver: Driver) => {
            driver.sponsorRelationships.forEach(rel => {
              const metrics = sponsorMap.get(rel.sponsorID);
              if (metrics) {
                metrics.totalDrivers++;
                metrics.totalPoints += rel.driverPoints;
                metrics.averagePoints = metrics.totalPoints / metrics.totalDrivers;
              }
            });
          });
        }
        setSponsorMetrics(Array.from(sponsorMap.values()));
      } else {
        const response = await axios.get<Driver[]>('/api/admin/view-users/driver');
        setDrivers(response.data);
      }
    } catch (err: any) {
      console.error('Error fetching data:', err);
      setError(err.response?.data?.error || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  }, [selectedTab]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleChangeUserType = async (userId: number, newUserType: UserType) => {
    try {
      await axios.post('/api/admin/change-user-type', { userId, newUserType });
      await fetchData();
    } catch (error: any) {
      setError(error.response?.data?.error || 'Failed to change user type');
    }
  };

  const handleRemoveUser = async (userId: number) => {
    try {
      await axios.delete(`/api/admin/remove-user/${userId}`);
      await fetchData();
    } catch (error: any) {
      setError(error.response?.data?.error || 'Failed to remove user');
    }
  };

  // Filtering functions
  const filteredAdmins = admins.filter(admin =>
    admin.userName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    admin.email.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const filteredSponsors = sponsors.filter(sponsor =>
    sponsor.userName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    sponsor.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
    sponsor.companyName.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const filteredDrivers = drivers.filter(driver =>
    driver.firstName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    driver.lastName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    driver.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
    driver.sponsorRelationships.some(sr =>
      sr.sponsorName.toLowerCase().includes(searchTerm.toLowerCase())
    )
  );

  // Render functions for different tables
  const renderAdminTable = () => (
    <TableContainer component={Paper}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>UserName</TableCell>
            <TableCell>Email</TableCell>
            <TableCell>Created At</TableCell>
            <TableCell>Last Login</TableCell>
            <TableCell>User Type</TableCell>
            <TableCell>Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {filteredAdmins.map((admin) => (
            <TableRow key={admin.userId}>
              <TableCell>{admin.userName}</TableCell>
              <TableCell>{admin.email}</TableCell>
              <TableCell>{new Date(admin.createdAt).toLocaleDateString()}</TableCell>
              <TableCell>{new Date(admin.lastLogin).toLocaleDateString()}</TableCell>
              <TableCell>
                <Select
                  size="small"
                  value="admin"
                  onChange={(e) => handleChangeUserType(admin.userId, e.target.value as UserType)}
                >
                  <MenuItem value="admin">Admin</MenuItem>
                  <MenuItem value="sponsor">Sponsor</MenuItem>
                  <MenuItem value="driver">Driver</MenuItem>
                </Select>
              </TableCell>
              <TableCell>
                <IconButton
                  onClick={() => handleRemoveUser(admin.userId)}
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
  );

  const renderDriverTable = () => (
    <TableContainer component={Paper}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>Driver Name</TableCell>
            <TableCell>Email</TableCell>
            <TableCell>Sponsors</TableCell>
            <TableCell align="center">Total Sponsors</TableCell>
            <TableCell>Points & Milestones</TableCell>
            <TableCell>User Type</TableCell>
            <TableCell>Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {filteredDrivers.map((driver) => (
            <TableRow key={driver.userId}>
              <TableCell>
                {driver.firstName} {driver.lastName}
              </TableCell>
              <TableCell>{driver.email}</TableCell>
              <TableCell>
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                  {driver.sponsorRelationships.map((sr) => (
                    <Chip
                      key={sr.sponsorID}
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
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                  {driver.sponsorRelationships.map((sr) => (
                    <Tooltip
                      key={sr.sponsorID}
                      title={`${sr.sponsorName}: Level ${sr.milestoneLevel}`}
                    >
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Chip
                          size="small"
                          label={`${sr.driverPoints} pts`}
                          color="secondary"
                        />
                        <Typography variant="caption">
                          Level {sr.milestoneLevel}
                        </Typography>
                      </Box>
                    </Tooltip>
                  ))}
                </Box>
              </TableCell>
              <TableCell>
                <Select
                  size="small"
                  value="driver"
                  onChange={(e) => handleChangeUserType(driver.userId, e.target.value as UserType)}
                >
                  <MenuItem value="driver">Driver</MenuItem>
                  <MenuItem value="sponsor">Sponsor</MenuItem>
                  <MenuItem value="admin">Admin</MenuItem>
                </Select>
              </TableCell>
              <TableCell>
                <IconButton
                  onClick={() => handleRemoveUser(driver.userId)}
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
  );

  const renderSponsorTable = () => (
    <TableBody>
      {filteredSponsors.map((sponsor) => {
        const metrics = sponsorMetrics.find(m => m.sponsorId === sponsor.sponsorId);
        return (
          <TableRow key={sponsor.userId}>
            <TableCell>
              {sponsor.firstName} {sponsor.lastName}
            </TableCell>
            <TableCell>{sponsor.email}</TableCell>
            <TableCell>{sponsor.companyName}</TableCell>
            <TableCell>
              <Chip
                label={sponsor.sponsorType}
                color="primary"
                size="small"
              />
            </TableCell>
            <TableCell>
              ${sponsor.pointDollarValue.toFixed(2)}
            </TableCell>
            <TableCell>{metrics?.totalDrivers || 0}</TableCell>
            <TableCell>
              {metrics?.totalPoints.toLocaleString() || 0}
            </TableCell>
            <TableCell>
              <Select
                size="small"
                value="sponsor"
                onChange={(e) =>
                  handleChangeUserType(
                    sponsor.userId,
                    e.target.value as UserType
                  )
                }
              >
                <MenuItem value="sponsor">Sponsor</MenuItem>
                <MenuItem value="admin">Admin</MenuItem>
                <MenuItem value="driver">Driver</MenuItem>
              </Select>
            </TableCell>
            <TableCell>
              <IconButton
                onClick={() => handleRemoveUser(sponsor.userId)}
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
  );

  const renderSponsorDashboard = () => {
    const driverDistributionData = sponsorMetrics.map((sponsor) => ({
      id: sponsor.sponsorId,
      value: sponsor.totalDrivers,
      label: sponsor.sponsorName,
    }));

    return (
      <>
        <Grid container spacing={3} sx={{ mb: 3 }}>
          <Grid item xs={12} md={4}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Total Sponsors
                </Typography>
                <Typography variant="h3">{sponsors.length}</Typography>
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
                  {sponsorMetrics
                    .reduce((sum, s) => sum + s.totalPoints, 0)
                    .toLocaleString()}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>

        <Grid container spacing={3} sx={{ mb: 3 }}>
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Driver Distribution by Sponsor
                </Typography>
                <Box sx={{ height: 400 }}>
                  <PieChart
                    series={[
                      {
                        data: driverDistributionData,
                        highlightScope: { faded: 'global', highlighted: 'item' },
                      },
                    ]}
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
                    xAxis={[
                      {
                        scaleType: 'band',
                        data: sponsorMetrics.map((s) => s.sponsorName),
                      },
                    ]}
                    series={[
                      {
                        data: sponsorMetrics.map((s) => s.totalPoints),
                      },
                    ]}
                    height={350}
                  />
                </Box>
              </CardContent>
            </Card>
          </Grid>
        </Grid>

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
            {renderSponsorTable()}
          </Table>
        </TableContainer>
      </>
    );
  };

  if (loading) {
    return (
      <Box sx={{ width: '100%', mt: 2 }}>
        <LinearProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ width: '100%', p: 3 }}>
      <Typography variant="h5" gutterBottom>
        User Management Dashboard
      </Typography>

      <Tabs 
        value={selectedTab} 
        onChange={(_, newValue) => setSelectedTab(newValue)} 
        sx={{ mb: 3 }}
      >
        <Tab label="Administrators" />
        <Tab label="Sponsors" />
        <Tab label="Drivers" />
      </Tabs>

      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <TextField
          fullWidth
          variant="outlined"
          placeholder={
            selectedTab === 0
              ? "Search admins..."
              : selectedTab === 1
              ? "Search sponsors..."
              : "Search drivers..."
          }
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
        <Button
          variant="contained"
          color="primary"
          onClick={() => setShowCreateModal(true)}
          sx={{ ml: 2 }}
        >
          Create User
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {showCreateModal && (
        <CreateUserModal 
          open={showCreateModal} 
          onClose={() => setShowCreateModal(false)}
          onSuccess={() => {
            setShowCreateModal(false);
            fetchData();
          }} 
        />
      )}

      {selectedTab === 0 && renderAdminTable()}
      {selectedTab === 1 && renderSponsorDashboard()}
      {selectedTab === 2 && renderDriverTable()}
    </Box>
  );
};

export default UserManagementDashboard;