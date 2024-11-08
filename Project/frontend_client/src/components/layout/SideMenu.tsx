import * as React from 'react';
import Box from '@mui/material/Box';
import Drawer from '@mui/material/Drawer';
import AppBar from '@mui/material/AppBar';
import CssBaseline from '@mui/material/CssBaseline';
import Toolbar from '@mui/material/Toolbar';
import Divider from '@mui/material/Divider';
import Avatar from '@mui/material/Avatar';
import Typography from '@mui/material/Typography';
import { useAuth } from '../../service/authContext';
import MenuContent from './MenuContent';
import OptionsMenu from './OptionsMenu';


export const drawerWidth = 240;

export default function SideMenu() {
  const { user, viewRole } = useAuth();

  // Substitute role-based placeholder data if `viewRole` is active
  const displayUser = viewRole
    ? { 
        userName: `Viewing as ${viewRole}`, 
        email: `${viewRole.toLowerCase()}@example.com`, 
        userType: viewRole 
      }
    : user;

  return (
    // <ViewProvider>
      <Box sx={{ display: 'flex' }}>
        <CssBaseline />
        <AppBar position="fixed" sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}>
        </AppBar>
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', md: 'block' },  // Hide on smaller than md screens
            width: drawerWidth,
            flexShrink: 0,
            [`& .MuiDrawer-paper`]: {
              width: drawerWidth,
              boxSizing: 'border-box',
            },
          }}>
          <Toolbar />
          <Box sx={{ overflow: 'auto' }}>
            {displayUser && (
              <>
                <Box sx={{ display: 'flex', alignItems: 'center', p: 2 }}>
                  <Avatar alt={displayUser.userName} src="/static/images/avatar/7.jpg" sx={{ width: 36, height: 36 }} />
                  <Box sx={{ ml: 2 }}>
                    <Typography variant="body2" sx={{ fontWeight: 500, lineHeight: '16px' }}>
                      {displayUser.userName}
                    </Typography>
                    <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                      {displayUser.email}
                    </Typography>
                  </Box>
                  <OptionsMenu />
                </Box>
                <Divider />
                <MenuContent />
              </>
            )}
          </Box>
        </Drawer>
      </Box>
  );
}
