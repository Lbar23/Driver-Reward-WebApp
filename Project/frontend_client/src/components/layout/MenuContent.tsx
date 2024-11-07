// MenuContent.tsx
import * as React from 'react';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Collapse from '@mui/material/Collapse';
import DashboardIcon from '@mui/icons-material/Dashboard';
import ApprovalIcon from '@mui/icons-material/Approval';
import InfoRoundedIcon from '@mui/icons-material/InfoRounded';
import HelpRoundedIcon from '@mui/icons-material/HelpRounded';
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
  const { setCurrentView } = useView();
  const { user, viewRole } = useAuth();
  const navigate = useNavigate();
  const currentRole = viewRole || user?.userType || 'Guest';
  const [openItems, setOpenItems] = useState<{ [key: string]: boolean }>({});

  // Combine base items with role-specific items
  const menuItems = [...menuConfig.base, ...(menuConfig[currentRole] || [])];

  const toggleOpen = (text: string) => {
    setOpenItems((prev) => ({ ...prev, [text]: !prev[text] }));
  };

  const handleItemClick = (view?: string, path?: string) => {
    if (path) {
      navigate(path); // Use navigate for direct paths like '/catalog'
    } else if (view) {
      setCurrentView(view); // Set the view in ViewProvider
    }
  };

  return (
    <List dense>
      {menuItems.map((item, index) => (
        <React.Fragment key={index}>
          <ListItem disablePadding>
            <ListItemButton
              onClick={() =>
                item.nestedItems ? toggleOpen(item.text) : handleItemClick(item.view, item.path)
              }
            >
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.text} />
              {item.nestedItems ? (openItems[item.text] ? <ExpandLessIcon /> : <ExpandMoreIcon />) : null}
            </ListItemButton>
          </ListItem>
          {item.nestedItems && (
            <Collapse in={openItems[item.text]} timeout="auto" unmountOnExit>
              <List component="div" disablePadding>
                {item.nestedItems.map((nestedItem, nestedIndex) => (
                  <ListItem key={nestedIndex} disablePadding>
                    <ListItemButton
                      onClick={() => handleItemClick(nestedItem.view, nestedItem.path)}
                    >
                      <ListItemIcon>{nestedItem.icon}</ListItemIcon>
                      <ListItemText primary={nestedItem.text} />
                    </ListItemButton>
                  </ListItem>
                ))}
              </List>
            </Collapse>
          )}
        </React.Fragment>
      ))}
    </List>
  );
}
