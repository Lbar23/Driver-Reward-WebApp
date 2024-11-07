import * as React from 'react';
import Avatar from '@mui/material/Avatar';
import Button from '@mui/material/Button';
import Divider from '@mui/material/Divider';
import Drawer, { drawerClasses } from '@mui/material/Drawer';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded';
import NotificationsRoundedIcon from '@mui/icons-material/NotificationsRounded';
import MenuButton from '../layout/MenuButton';
import MenuContent from '../layout/MenuContent';
import CardAlert from '../layout/CardAlert';
import { useAuth } from '../../service/authContext';
import { ViewProvider } from '../../service/viewContext';


interface SideMenuMobileProps {
  open: boolean | undefined;
  toggleDrawer: (newOpen: boolean) => () => void;
}

export default function SideMenuMobile({ open, toggleDrawer }: SideMenuMobileProps) {
  const { user, viewRole, logout } = useAuth();

  // Determine user display data
  const displayUser = viewRole
    ? { 
        userName: `Viewing as ${viewRole}`, 
        email: `${viewRole.toLowerCase()}@example.com`, 
        userType: viewRole 
      }
    : user;

  return (
      <Drawer
        anchor="right"
        open={open}
        onClose={toggleDrawer(false)}
        sx={{
          zIndex: (theme) => theme.zIndex.drawer + 1,
          [`& .${drawerClasses.paper}`]: {
            backgroundImage: 'none',
            backgroundColor: 'background.paper',
          },
        }}
      >
        <Stack sx={{ maxWidth: '70dvw', height: '100%' }}>
          {/* User Info and Notification Icon */}
          <Stack direction="row" sx={{ p: 2, pb: 0, gap: 1 }}>
            {displayUser ? (
              <Stack direction="row" sx={{ gap: 1, alignItems: 'center', flexGrow: 1, p: 1 }}>
                <Avatar alt={displayUser.userName} src="/static/images/avatar/7.jpg" sx={{ width: 24, height: 24 }} />
                <Typography component="p" variant="h6">
                  {displayUser.userName}
                </Typography>
              </Stack>
            ) : (
              <Typography variant="body2" sx={{ flexGrow: 1, p: 1 }}>
                Please log in to access more features.
              </Typography>
            )}
            <MenuButton showBadge>
              <NotificationsRoundedIcon />
            </MenuButton>
          </Stack>

          <Divider />

          {/* Menu Content */}
          <Stack sx={{ flexGrow: 1 }}>
            <MenuContent />
            <Divider />
          </Stack>

          {/* Alert Card */}
          <CardAlert />

          {/* Logout Button */}
          {displayUser && (
            <Stack sx={{ p: 2 }}>
              <Button
                variant="outlined"
                fullWidth
                startIcon={<LogoutRoundedIcon />}
                onClick={logout}>
                Logout
              </Button>
            </Stack>
          )}
        </Stack>
      </Drawer>
  );
}
