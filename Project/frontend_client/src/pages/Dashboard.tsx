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

export default function Dashboard(props: { disableCustomTheme?: boolean }) {
  console.log("Dashboard component has started rendering");

  const [loading, setLoading] = React.useState<boolean>(true);
  const [data, setData] = React.useState<any[]>([]);
  const [error, setError] = React.useState<string | null>(null);
  const [userData, setUserData] = React.useState<UserData | null>(null);
  const [userRole, setUserRole] = React.useState<string>('Guest');


  //Constantly fetched data, even if on a different page from dashboard...mounted check to essentially "clean up" fetches
  React.useEffect(() => {
    let isMounted = true;

    const fetchData = async () => {
      try {
        console.log("Fetching user data from API...");
        const userResponse = await axios.get('/api/user/currentuser', {
          withCredentials: true 
        });

        if (isMounted) {
          console.log("User data fetched:", userResponse.data);
          setUserData(userResponse.data);
          setLoading(false);
        }
      } 
      catch (err: any) {
        if (isMounted) {
          console.error("Error fetching user data:", err);
          if (err.name !== 'CanceledError') {
            setError(err.response?.status === 401 ? 'You are not logged in.' : 'Failed to load dashboard data.');
          }
          setLoading(false);
        }
      }
    };      

    fetchData();

    //Cleanup function; fetches data back only when redirected to page instead of constantly reloading to/from dashboard
    return () => {
      isMounted = false;
    };
  }, []);

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
