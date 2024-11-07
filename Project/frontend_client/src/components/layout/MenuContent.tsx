// MenuContent.tsx
import * as React from 'react';
import { useState } from 'react';
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
import { useView, ViewProvider } from '../../service/viewContext';

type ListItemType = {
  text: string;
  icon: React.ReactNode;
  view?: string;
  nestedItems?: ListItemType[];
};

// Menu configuration
const menuConfig: Record<string, ListItemType[]> = {
  base: [{ text: 'Dashboard', icon: <DashboardIcon />, view: 'MAIN' }],
  Guest:[
    { text: 'Applications', icon: <ApprovalIcon />, view: 'DRIVER_APPLICATION' },
  ],
  Driver: [
    { text: 'Applications', icon: <ApprovalIcon />, view: 'DRIVER_APPLICATION' },
    { text: 'Register With Sponsors', icon: <ApprovalIcon />, view: 'DRIVER_REGISTRATION' },
    { text: 'Points', icon: <InfoRoundedIcon />, view: 'DRIVER_POINTS' },
    { text: 'Activity', icon: <InfoRoundedIcon />, view: 'DRIVER_ACTIVITY' },
    // { text: 'Points History', icon: <InfoRoundedIcon />, view: 'DRIVER_POINTS_HISTORY' }, <-- this is outdated
  ],
  Admin: [
    { text: 'Manage Users', icon: <PeopleIcon />, view: 'MANAGE_USERS' },
    {
      text: 'Reports Views',
      icon: <HelpRoundedIcon />,
      nestedItems: [
        { text: 'Sales', icon: <InfoRoundedIcon />, view: 'ADMIN_SALES_REPORTS' },
        { text: 'Invoices', icon: <HelpRoundedIcon />, view: 'ADMIN_INVOICE_REPORTS' },
        { text: 'Audits', icon: <HelpRoundedIcon />, view: 'ADMIN_AUDIT_REPORTS' },
      ],
    },
  ],
  Sponsor: [
    { text: 'Manage Drivers', icon: <PeopleIcon />, view: 'SPONSOR_DRIVERS' },
    { text: 'Approve/Reject Applications', icon: <ApprovalIcon />, view: 'APPLICATION_MANAGER' },
    { text: 'Reports Views',
      icon: <HelpRoundedIcon />,
      nestedItems: [
        { text: 'Points', icon: <InfoRoundedIcon />, view: 'SPONSOR_POINTS_REPORTS' },
        { text: 'Audits', icon: <HelpRoundedIcon />, view: 'SPONSOR_AUDIT_REPORTS' },
      ],
    },
    
  ],
};

export default function MenuContent() {
  const { setCurrentView } = useView();
  const { user, viewRole } = useAuth();
  const currentRole = viewRole || user?.userType || 'Guest';
  const [openItems, setOpenItems] = useState<{ [key: string]: boolean }>({});

  // Combine base items with role-specific items
  const menuItems = [...menuConfig.base, ...(menuConfig[currentRole] || [])];

  const toggleOpen = (text: string) => {
    setOpenItems((prev) => ({ ...prev, [text]: !prev[text] }));
  };
  
  const handleItemClick = (view?: string) => {
    if (view) {
      console.log(`Menu item clicked to set view: "${view}"`); // Log the view to be set
      setCurrentView(view);
    }
  };

    return (
      <ViewProvider>
      <List dense>
        {menuItems.map((item, index) => (
          <React.Fragment key={index}>
            <ListItem disablePadding>
              <ListItemButton
                onClick={() =>
                  item.nestedItems ? toggleOpen(item.text) : handleItemClick(item.view)
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
                      <ListItemButton onClick={() => handleItemClick(nestedItem.view)}>
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
      </ViewProvider>
    );
  }