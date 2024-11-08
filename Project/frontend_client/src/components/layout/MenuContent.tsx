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
import PeopleIcon from '@mui/icons-material/People';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { useAuth } from '../../service/authContext';
import { useView } from '../../service/viewContext';
import { Person } from '@mui/icons-material';

type ListItemType = {
  text: string;
  icon: React.ReactNode;
  view?: string;
  path?: string;
  nestedItems?: ListItemType[];
};

// Menu configuration
const menuConfig: Record<string, ListItemType[]> = {
  base: [{ text: 'Dashboard', icon: <DashboardIcon />, view: 'MAIN' }],
  Guest: [{ text: 'Applications', icon: <ApprovalIcon />, view: 'DRIVER_APPLICATION' }],
  Driver: [
    { text: 'Applications', icon: <ApprovalIcon />, view: 'DRIVER_APPLICATION' },
    { text: 'Product Catalog', icon: <ShoppingBagIcon />, path: '/catalog' },
    { text: 'Register With Sponsors', icon: <ApprovalIcon />, view: 'DRIVER_REGISTRATION' },
    { text: 'Points', icon: <InfoRoundedIcon />, view: 'DRIVER_POINTS' },
    { text: 'Activity', icon: <InfoRoundedIcon />, view: 'DRIVER_ACTIVITY' },
  ],
  Admin: [
    { 
      text: 'Manage Users', 
      icon: <PeopleIcon />, 
      nestedItems: [
        { text: "Drivers", icon: <Person />, view: 'MANAGE_DRIVERS' },
        { text: "Sponsors", icon: <Person />, view: 'MANAGE_USERS' },
        { text: "Admins", icon: <Person />, view: 'MANAGE_USERS' },

      ],
    },
    {
      text: 'Reports Views',
      icon: <HelpRoundedIcon />,
      nestedItems: [
        { text: 'General', icon: <HelpRoundedIcon />, view: 'ADMIN_REPORTS' },
        { text: 'Audits', icon: <HelpRoundedIcon />, view: 'ADMIN_AUDIT_REPORTS' },
      ],
    },
  ],
  Sponsor: [
    { text: 'Manage Drivers', icon: <PeopleIcon />, view: 'SPONSOR_DRIVERS' },
    { text: 'Approve/Reject Applications', icon: <ApprovalIcon />, view: 'APPLICATION_MANAGER' },
    { text: 'Product Catalog', icon: <ShoppingBagIcon />, path: '/catalog' },
    {
      text: 'Reports Views',
      icon: <HelpRoundedIcon />,
      nestedItems: [
        { text: 'General', icon: <InfoRoundedIcon />, view: 'SPONSOR_REPORTS' },
        { text: 'Audits', icon: <HelpRoundedIcon />, view: 'SPONSOR_AUDIT_REPORTS' },
      ],
    },
  ],
};

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
