import * as React from 'react';
import { alpha } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import Box from '@mui/material/Box';
import Stack from '@mui/material/Stack';
import AppNavbar from '../components/layout/AppNavbar';
import Header from '../components/layout/Header';
import MainGrid from '../components/dashboard/MainGrid';
import SideMenu from '../components/layout/SideMenu';
import AppTheme from '../components/layout/AppTheme';
import Section from '../components/form-elements/Section';
import OverviewItem from '../components/layout/OverviewItem';
import { Typography, Alert, CircularProgress } from '@mui/material';
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
// {loading ? (
//   <CircularProgress />
// ) : error ? (
//   <Alert severity="error">{error}</Alert>
// ) : !userData ? (
//   <Alert severity="warning">
//     You are not logged in. Please <Link to="/login">log in</Link> to access your dashboard.
//   </Alert>
// ) : (
//   <Box sx={{ width: '100%'}}>

//     {/* {data.length ? (
//       data.map((item, index) => (
//         <OverviewItem key={index} title={item.title} value={item.value} />
//       ))
//     ) : (
//       <Typography>No data available</Typography>
//     )} */}

//     {/* {renderGeneralSection()} */}
//     {/* {renderRoleSpecificSections()} */}
//   </Box>
// )}

export default function Dashboard(props: { disableCustomTheme?: boolean }) {
  console.log("Dashboard component has started rendering");

  const [loading, setLoading] = React.useState<boolean>(true);
  const [data, setData] = React.useState<any[]>([]);
  const [error, setError] = React.useState<string | null>(null);
  const [userData, setUserData] = React.useState<UserData | null>(null);
  const [userRole, setUserRole] = React.useState<string>('Guest');

  React.useEffect(() => {
    console.log("Component mounted, starting data fetch...");
      const fetchData = async () => {
        try {
          console.log("Fetching user data from API...");
          const userResponse = await axios.get(`/api/User/currentuser`, { timeout: 8000 });
          console.log("User data fetched:", userResponse.data);
          setUserData(userResponse.data);
          setLoading(false);
        } 
        catch (err: any) {
          console.error("Error fetching user data:", err);
          setError(err.response?.status === 401 ? 'You are not logged in.' : 'Failed to load dashboard data.');
          setLoading(false);
        }
      };      
      fetchData();
  }, []);

  // const handleRoleChange = (event: React.ChangeEvent<{ value: unknown }>) => {
  //   const newRole = event.target.value as string;
  //   console.log("Role changed to:", newRole);
  //   setUserRole(newRole);
  // };

  const renderGeneralSection = React.useCallback(() => {
    console.log("Rendering general section");
    return (
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
  }, []); 

  // const renderRoleSpecificSections = () => {
  //   console.log("Rendering role-specific sections for role:", userRole);
  //   const sections = [];
  //   if (userRole === 'Driver') {
  //     sections.push(
  //       <Section
  //         key="driver"
  //         title="Driver Settings"
  //         buttons={[
  //           { label: 'View Points History', path: '/points-history:3'},
  //         ]}
  //       />
  //     );
  //   }
  //   return sections;
  // };

  console.log("Rendering Dashboard component, loading:", loading);

  return (
    <AppTheme disableCustomTheme={props.disableCustomTheme}>
      <CssBaseline enableColorScheme />
      <SideMenu />
      <Box sx={{ display: 'flex-start',ml:'80px', width: '100%' }}>
        <AppNavbar padding={240}/>
        <Box
          component="main"
          sx={(theme) => ({
            flexGrow: 1,
            display: 'flex',
            backgroundColor: alpha(theme.palette.background.default, 1), // Fixed color for background
            // mt: 8,
            // p:3
          })}>
          <Stack
            spacing={1}
            sx={{
              alignItems: 'flex-start',
              width: '100%'
            }}>
            <Header />
            <MainGrid />
          </Stack>
        </Box>
      </Box>
    </AppTheme>
  );
}
