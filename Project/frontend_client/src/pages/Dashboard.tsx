import React, { useState, useEffect } from 'react';
import axios from 'axios';
import {
  Typography,
  Box,
  Alert,
  AccessibleSelect,
  LoadingSpinner,
  Section,
  OverviewItem
} from '../components/MuiComponents';
import type { SelectChangeEvent } from '@mui/material';
import { useNavigate } from 'react-router-dom';

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

const Dashboard: React.FC = () => {
  const [loading, setLoading] = useState<boolean>(true);
  const [data, setData] = useState<any[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [userData, setUserData] = useState<UserData | null>(null);
  const [userRole, setUserRole] = useState<string>('Guest');
  const [announcement, setAnnouncement] = useState<string>('');
  const navigate = useNavigate();

  // Manage focus when loading state changes
  useEffect(() => {
    if (!loading) {
      const mainContent = document.getElementById('main-content');
      if (mainContent) {
        mainContent.focus();
      }
    }
  }, [loading]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const userResponse = await axios.get(`/api/User/currentuser`);
        setUserData(userResponse.data);
        // Announce when data is loaded
        setAnnouncement('Dashboard data has been loaded');
        setLoading(false);
      } catch (err: any) {
        const errorMessage = err.response?.status === 401 
          ? 'You are not logged in.' 
          : 'Failed to load dashboard data.';
        setError(errorMessage);
        setAnnouncement(`Error: ${errorMessage}`);
        setLoading(false);
      }
    };
    fetchData();

    // Cleanup function
    return () => {
      setAnnouncement('');
    };
  }, []);

  const handleRoleChange = (event: SelectChangeEvent<string>) => {
    const newRole = event.target.value as string;
    setUserRole(newRole);
    // Announce role change
    setAnnouncement(`Dashboard view changed to ${newRole} role`);
  };

  const roleOptions = [
    { value: 'Guest', label: 'Guest User' },
    { value: 'Driver', label: 'Driver Account' },
    { value: 'Sponsor', label: 'Sponsor Account' },
    { value: 'Admin', label: 'Administrator' }
  ];

  const handleKeyboardNavigation = (event: React.KeyboardEvent, path: string) => {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      navigate(path);
    }
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
      description="Manage your account settings and preferences"
      ariaLabel="General account settings section"
    />
  );

  const renderRoleSpecificSections = () => {
    const sections = [];
    
    if (userRole === 'Guest') {
      sections.push(
        <Section 
          key="guest" 
          title="Applicant Options" 
          buttons={[{ label: 'Apply for Driver Program', path: '/apply' }]}
          description="Start your application process for the driver program"
          ariaLabel="Guest user application options"
        />
      );
    }
    
    if (userRole === 'Driver') {
      sections.push(
        <Section
          key="driver"
          title="Driver Settings"
          buttons={[
            { label: 'View Points History', path: '/points-history:12' },
            { label: 'Manage My Orders', path: '/orders' },
          ]}
          description="Access your driver account features and history"
          ariaLabel="Driver account settings and features"
        />
      );
    }
    
    if (userRole === 'Sponsor') {
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
          description="Manage your sponsor account and view driver activities"
          ariaLabel="Sponsor management options"
        />
      );
    }
    
    if (userRole === 'Admin') {
      sections.push(
        <Section
          key="admin"
          title="Admin Controls"
          buttons={[
            { label: 'Manage Users', path: '/manage-users' },
            { label: 'View System Logs', path: '/logs' },
          ]}
          color="secondary"
          description="Access administrative controls and system management"
          ariaLabel="Administrative controls section"
        />
      );
    }
    return sections;
  };

  return (
    <main>
      {/* Live region for announcements */}
      <div 
        aria-live="polite" 
        aria-atomic="true" 
        className="sr-only"
        style={{ position: 'absolute', height: 1, width: 1, padding: 0, margin: -1, overflow: 'hidden', clip: 'rect(0, 0, 0, 0)', whiteSpace: 'nowrap', border: 0 }}
      >
        {announcement}
      </div>

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
          component="header"
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            width: '100%',
            alignItems: 'center',
            mb: 3,
          }}
        >
          <Typography 
            variant="h1" 
            sx={{ 
              fontSize: 'h4.fontSize',
              color: 'text.primary',
              fontWeight: 'bold',
            }}
          >
            Dashboard
          </Typography>

          <AccessibleSelect
            id="role-selector"
            label="View as Role"
            value={userRole}
            onChange={handleRoleChange}
            options={roleOptions}
            helpText="Change the dashboard view based on user role"
          />
        </Box>

        {loading ? (
          <LoadingSpinner />
        ) : error ? (
          <Alert 
            severity="error" 
            role="alert"
            sx={{
              width: '100%',
              '& .MuiAlert-message': {
                color: '#5f2120', // Ensuring sufficient contrast
              }
            }}
          >
            {error}
          </Alert>
        ) : !userData ? (
          <Alert 
            severity="warning" 
            sx={{ 
              mt: 4,
              width: '100%',
              '& .MuiAlert-message': {
                color: '#663c00', // Ensuring sufficient contrast
              }
            }} 
            role="alert"
          >
            <span>You are not logged in. Please </span>
            <a 
              href="/login"
              style={{ 
                color: '#2b4bd3',
                textDecoration: 'underline',
                fontWeight: 'bold'
              }}
              onKeyDown={(e) => handleKeyboardNavigation(e, '/login')}
            >
              log in
            </a>
            <span> to access your dashboard.</span>
          </Alert>
        ) : (
          <Box 
            sx={{ width: '100%', mt: 2 }} 
            component="section" 
            aria-label="Dashboard content"
            id="main-content"
            tabIndex={-1}
          >
            <Typography 
              variant="h2" 
              sx={{ 
                fontSize: 'h6.fontSize',
                color: 'text.primary',
                mb: 2
              }}
            >
              Overview
            </Typography>

            {data.length > 0 ? (
              <div role="feed" aria-label="Dashboard overview items">
                {data.map((item, index) => (
                  <OverviewItem 
                    key={index} 
                    title={item.title} 
                    value={item.value}
                  />
                ))}
              </div>
            ) : (
              <Typography 
                role="status"
                sx={{
                  color: 'text.secondary',
                  fontSize: '1rem',
                  textAlign: 'center',
                  py: 2
                }}
              >
                No dashboard data available
              </Typography>
            )}

            {renderGeneralSection()}
            {renderRoleSpecificSections()}
          </Box>
        )}
      </Box>
    </main>
  );
};

export default Dashboard;