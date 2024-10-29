import * as React from 'react';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Stack from '@mui/material/Stack';
import HomeRoundedIcon from '@mui/icons-material/HomeRounded';
import AnalyticsRoundedIcon from '@mui/icons-material/AnalyticsRounded';
import PeopleRoundedIcon from '@mui/icons-material/PeopleRounded';
import AssignmentRoundedIcon from '@mui/icons-material/AssignmentRounded';
import SettingsRoundedIcon from '@mui/icons-material/SettingsRounded';
import InfoRoundedIcon from '@mui/icons-material/InfoRounded';
import HelpRoundedIcon from '@mui/icons-material/HelpRounded';
import FeedbackIcon from '@mui/icons-material/Feedback';
import DashboardIcon from '@mui/icons-material/Dashboard';
import ShoppingBagIcon from '@mui/icons-material/ShoppingBag';
import ApprovalIcon from '@mui/icons-material/Approval';
import { useLocation, useNavigate } from 'react-router-dom';

const mainListItems = [
  { text: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard'}, //added navigation pathing
  // { text: 'Analytics', icon: <AnalyticsRoundedIcon /> },
  // { text: 'Clients', icon: <PeopleRoundedIcon /> },
  // { text: 'Tasks', icon: <AssignmentRoundedIcon /> },
];

//Methods below, except for settings, probably, should have a "back to dashboard" button redirect...
//Unless make them components instead of individual pages, bit unideal...
// will add to nav bar ^
const secondaryListItems = [
  { text: 'Settings', icon: <SettingsRoundedIcon /> },
  { text: 'About', icon: <InfoRoundedIcon />, path: '/about' }, //same
  { text: 'FAQ', icon: <HelpRoundedIcon />, path: '/faq' }, //same
  { text: 'Catalog', icon: <ShoppingBagIcon />, path: '/catalog' }, //same
  { text: 'Driver Application', icon: <ApprovalIcon />, path: '/application' }, //same
  { text: 'Manage Applications', icon: <ApprovalIcon />, path: '/application-manager' }, //same
  { text: 'Feedback', icon: <FeedbackIcon />, path: '/feedback' }, //same
];

export default function MenuContent() {
  const navigate = useNavigate();
  const location = useLocation();

  const handleNavigation = (path: string) => {
    navigate(path);
  };

  const isCurrentPath = (path: string) => location.pathname === path;

  return (
    <Stack sx={{ flexGrow: 1, p: 1, justifyContent: 'space-between' }}>
      <List dense>
        {mainListItems.map((item, index) => (
          <ListItem key={index} disablePadding sx={{ display: 'block' }}>
            <ListItemButton 
              selected={isCurrentPath(item.path)}
              onClick={() => handleNavigation(item.path)}
            >
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.text} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>

      <List dense>
        {secondaryListItems.map((item, index) => (
          <ListItem key={index} disablePadding sx={{ display: 'block' }}>
            <ListItemButton
              selected={item.path !== undefined && isCurrentPath(item.path)} //added null check for navigation purposes
              onClick={() => handleNavigation(item.path!)}
            >
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.text} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
    </Stack>
  );
}

