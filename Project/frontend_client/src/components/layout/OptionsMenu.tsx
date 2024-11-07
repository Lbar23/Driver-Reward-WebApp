import * as React from 'react';
import { styled } from '@mui/material/styles';
import Divider from '@mui/material/Divider';
import Menu from '@mui/material/Menu';
import MuiMenuItem from '@mui/material/MenuItem';
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded';
import MoreVertRoundedIcon from '@mui/icons-material/MoreVertRounded';
import MenuButton from './MenuButton';
import { useAuth } from '../../service/authContext';
import ListItemText from '@mui/material/ListItemText';
import { Link as RouterLink } from 'react-router-dom';

const MenuItem = styled(MuiMenuItem)({
  margin: '2px',
});

// Utility component to use RouterLink within MenuItem
const MenuItemLink = React.forwardRef<HTMLAnchorElement, { to: string; children: React.ReactNode; onClick?: () => void }>(
  ({ to, children, onClick }, ref) => (
    <MuiMenuItem component={RouterLink} to={to} ref={ref} onClick={onClick}>
      {children}
    </MuiMenuItem>
  )
);

export default function OptionsMenu() {
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const { logout } = useAuth();
  const open = Boolean(anchorEl);

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleLogoutClick = () => {
    logout();
    handleClose();
  };

  return (
    <>
      <MenuButton aria-label="Open menu" onClick={handleClick} sx={{ borderColor: 'transparent' }}>
        <MoreVertRoundedIcon />
      </MenuButton>
      <Menu
        anchorEl={anchorEl}
        id="menu"
        open={open}
        onClose={handleClose}
        transformOrigin={{ horizontal: 'right', vertical: 'top' }}
        anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
        sx={{
          [`& .MuiList-root`]: { padding: '4px' },
          [`& .MuiPaper-root`]: { padding: 0 },
        }}
      >
        <MenuItemLink to="/profile" onClick={handleClose}>
          Profile
        </MenuItemLink>
        <Divider />
        <MenuItemLink to="/settings" onClick={handleClose}>
          Settings
        </MenuItemLink>
        <MenuItemLink to="/feedback" onClick={handleClose}>
          Feedback
        </MenuItemLink>
        <Divider />
        <MenuItem onClick={handleLogoutClick}>
          <LogoutRoundedIcon fontSize="small" />
          <ListItemText>Logout</ListItemText>
        </MenuItem>
      </Menu>
    </>
  );
}
