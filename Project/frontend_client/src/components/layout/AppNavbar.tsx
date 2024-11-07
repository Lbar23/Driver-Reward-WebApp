import * as React from 'react';
import { styled } from '@mui/material/styles';
import AppBar from '@mui/material/AppBar';
import Box from '@mui/material/Box';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import MenuRoundedIcon from '@mui/icons-material/MenuRounded';
import LocalShippingTwoToneIcon from "@mui/icons-material/LocalShippingTwoTone";
import SideMenuMobile from './SideMenuMobile';
import { useAuth } from '../../service/authContext';
import MenuButton from './MenuButton';
import ColorModeIconDropdown from './ColorModeIconDropdown';
import { Link } from 'react-router-dom';

export default function AppNavbar() {
  const [open, setOpen] = React.useState(false);
  const { isAuthenticated } = useAuth();

  const toggleDrawer = (newOpen: boolean) => () => {
    setOpen(newOpen);
  };
  
  return (
    <AppBar
      position="fixed"
      sx={{
        zIndex: (theme) => theme.zIndex.drawer + 1,
        boxShadow: 0,
        bgcolor: 'background.paper',
        borderBottom: '1px solid',
        borderColor: 'divider',
        maxWidth: '100vw', // Keep navbar within viewport width
        overflow: 'hidden',
      }}
    >
      <Toolbar
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          flexWrap: 'wrap',
          gap: 1,
          maxWidth: '100%',
          overflow: 'hidden',
        }}
      >
        {/* Logo and Title aligned to the left */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexShrink: 0 }}>
          <CustomIcon />
          <Typography
            variant="h5"
            component="a"
            href="/"
            sx={{
              color: 'text.primary',
              textDecoration: 'none',
              whiteSpace: 'nowrap',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
            }}
          >
            GitGud Drivers
          </Typography>
        </Box>

        {/* Right-aligned links and controls */}
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            gap: { xs: 1, md: 2 },
            flexWrap: 'wrap',
            justifyContent: 'flex-end',
            width: { xs: '100%', md: 'auto' }, // Allows items to stack on small screens
          }}
        >
          <Typography
            component={Link}
            to="/about"
            sx={{
              color: 'text.primary',
              textDecoration: 'none',
              fontSize: { xs: '0.9rem', md: '1rem' },
              whiteSpace: 'nowrap',
            }}
          >
            About
          </Typography>
          <Typography
            component={Link}
            to="/faq"
            sx={{
              color: 'text.primary',
              textDecoration: 'none',
              fontSize: { xs: '0.9rem', md: '1rem' },
              whiteSpace: 'nowrap',
            }}
          >
            FAQ
          </Typography>

          {/* Color mode and menu icon */}
          <ColorModeIconDropdown />
          <Box sx={{ display: { xs: 'block', md: 'none' } }}>
            <MenuButton aria-label="menu" onClick={toggleDrawer(true)}>
              <MenuRoundedIcon />
            </MenuButton>
            <SideMenuMobile open={open} toggleDrawer={toggleDrawer} />
          </Box>
        </Box>
      </Toolbar>
    </AppBar>
  );
}

function CustomIcon() {
  return (
    <Box
      sx={{
        width: '1.5rem',
        height: '1.5rem',
        bgcolor: 'black',
        borderRadius: '999px',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        backgroundImage:
          'linear-gradient(135deg, hsl(210, 98%, 60%) 0%, hsl(210, 100%, 35%) 100%)',
        color: 'hsla(210, 100%, 95%, 0.9)',
        border: '1px solid',
        borderColor: 'hsl(210, 100%, 55%)',
        boxShadow: 'inset 0 2px 5px rgba(255, 255, 255, 0.3)',
      }}
    >
      <LocalShippingTwoToneIcon color="inherit" sx={{ fontSize: '1rem' }} />
    </Box>
  );
}
