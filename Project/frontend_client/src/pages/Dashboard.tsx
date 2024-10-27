import React, { useState, useEffect } from 'react';
import { Typography, Box, CircularProgress, Button, Alert, Paper, MenuItem, Select, FormControl, InputLabel } from '@mui/material';
import { Link } from 'react-router-dom';
import axios from 'axios';
import { SelectChangeEvent } from '@mui/material';

interface UserData {
  id: number;
  userName: string;
  email: string;
  userType: string;
  createdAt: string;
  lastLogin: string;
  roles: string[];
  permissions: string[];
}

interface ButtonInfo {
  label: string;
  path: string;
}

const Dashboard: React.FC = () => {
  const [loading, setLoading] = useState<boolean>(true);
  const [data, setData] = useState<any[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [userData, setUserData] = useState<UserData | null>(null);
  const [userRole, setUserRole] = useState<string>('Guest'); // Role switching state

  useEffect(() => {
    const fetchData = async () => {
      try {
        const userResponse = await axios.get(`/api/user/currentuser`);
        setUserData(userResponse.data);
        setLoading(false);
      } catch (err: any) {
        setError(err.response?.status === 401 ? 'You are not logged in.' : 'Failed to load dashboard data.');
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  const handleRoleChange = (event: SelectChangeEvent<string>) => {
    setUserRole(event.target.value as string);
  };

  const renderGeneralSection = () => (
    <Section
      title="General Settings"
      buttons={[
        { label: 'Change My Password', path: '/change-password' },
        { label: 'Update Profile', path: '/update-profile' },
        { label: 'View Profile', path: '/profile' },
        { label: 'Change Settings', path: '/settings' },
      ]}
    />
  );

  const renderRoleSpecificSections = () => {
    const role = userRole; // Use the selected role
    const sections = [];
    if (role === 'Guest') {
      sections.push(<Section key="guest" title="Applicant" buttons={[{ label: 'Apply for Driver Program', path: '/apply' }]} />);
    }
    if (role === 'Driver') {
      sections.push(
        <Section
          key="driver"
          title="Driver Settings"
          buttons={[
            { label: 'View Points History', path: '/points-history:12' },
            { label: 'Manage My Orders', path: '/orders' },
          ]}
        />
      );
    }
    if (role === 'Sponsor') {
      sections.push(
        <Section
          key="sponsor"
          title="Sponsor Settings"
          buttons={[
            { label: 'Manage Sponsor Company', path: '/manage-sponsor' },
            { label: 'Driver Activity', path: '/driver-activity' },
            { label: 'Manage Incentives', path: '/incentives' },
            { label: 'Generate Custom Reports', path: '/reports' },
          ]}
        />
      );
    }
    if (role === 'Admin') {
      sections.push(
        <Section
          key="admin"
          title="Admin Controls"
          buttons={[
            { label: 'Manage Users', path: '/manage-users' },
            { label: 'View System Logs', path: '/logs' },
          ]}
          color="secondary"
        />
      );
    }
    return sections;
  };

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        maxWidth: 800,
        margin: 'auto',
        padding: 4,
        boxShadow: 3,
        borderRadius: 2,
        backgroundColor: 'background.paper',
      }}
    >
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          width: '100%',
          alignItems: 'center',
        }}
      >
        <Typography variant="h4" component="h1" gutterBottom>
          Dashboard
        </Typography>

        {/* Role Switcher aligned to the right */}
        <FormControl sx={{ minWidth: 200 }}>
          <InputLabel>View as Role</InputLabel>
          <Select value={userRole} onChange={handleRoleChange} label="View as Role">
            <MenuItem value="Guest">Guest</MenuItem>
            <MenuItem value="Driver">Driver</MenuItem>
            <MenuItem value="Sponsor">Sponsor</MenuItem>
            <MenuItem value="Admin">Admin</MenuItem>
          </Select>
        </FormControl>
      </Box>

      {loading ? (
        <CircularProgress />
      ) : error ? (
        <Alert severity="error">{error}</Alert>
      ) : !userData ? (
        <Alert severity="warning" sx={{ mt: 4 }}>
          You are not logged in. Please <Link to="/login">log in</Link> to access your dashboard.
        </Alert>
      ) : (
        <Box sx={{ width: '100%', mt: 2 }}>
          <Typography variant="h6" component="h2" gutterBottom>
            Overview
          </Typography>

          {data.length ? (
            data.map((item, index) => (
              <OverviewItem key={index} title={item.title} value={item.value} />
            ))
          ) : (
            <Typography>No data available</Typography>
          )}

          {renderGeneralSection()}
          {renderRoleSpecificSections()}
        </Box>
      )}
    </Box>
  );
};

// Helper components- these will be refactored soon so they don't need to be seperated now
const Section: React.FC<{ title: string; buttons: ButtonInfo[]; color?: 'primary' | 'secondary' }> = ({
  title,
  buttons,
  color = 'primary',
}) => (
  <Box sx={{ mt: 4 }}>
    <Typography variant="h6">{title}</Typography>
    <ButtonGroup buttons={buttons} color={color} />
  </Box>
);

const ButtonGroup: React.FC<{ buttons: ButtonInfo[]; color?: 'primary' | 'secondary' }> = ({ buttons, color = 'primary' }) => (
  <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', mt: 1 }}>
    {buttons.map((button, index) => (
      <Button
        key={index}
        component={Link}
        to={button.path}
        variant="contained"
        color={color}
        sx={{ textTransform: 'none' }}
      >
        {button.label}
      </Button>
    ))}
  </Box>
);

const OverviewItem: React.FC<{ title: string; value: string }> = ({ title, value }) => (
  <Paper sx={{ p: 2, mb: 2, display: 'flex', justifyContent: 'space-between', borderRadius: 2 }}>
    <Typography variant="body1">{title}</Typography>
    <Typography variant="body2" color="text.secondary">
      {value}
    </Typography>
  </Paper>
);

export default Dashboard;
