import React, { useEffect } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import DriverApplication from '../DriverApplication';
import ApplicationManager from '../ApplicationManager';
import PasswordChangeForm from '../../pages/PasswordChangeForm';
import DriverPointsList from '../dashboard/PMSDriverList';
import DriverActivity from '../dashboard/DriverActivity';
import SponsorDrivers from '../dashboard/SponsorDriverList';
import SponsorRegistrationPage from '../dashboard/SponsorRegistrationForDriver';
import DriverPointsHistory from '../dashboard/DriverPointHistory';
import { useView } from '../../service/viewContext';

// Link the current view to the corresponding component
const viewComponents: Record<string, JSX.Element> = {
  MAIN: (
    <Box>
      <Typography variant="h6">Dashboard Home</Typography>
      {/* Add other main dashboard content here */}
    </Box>
  ),
  // General Components here
  CHANGE_PASSWORD: <PasswordChangeForm />,
  // Admin Components
  MANAGE_USERS: <Typography>Manage Users</Typography>,
  ADMIN_SALES_REPORTS: <Typography>Sales Reports</Typography>,
  ADMIN_INVOICE_REPORTS: <Typography>Invoice Reports</Typography>,
  ADMIN_AUDIT_REPORTS: <Typography>Audit Reports</Typography>,
  // Driver Components
  DRIVER_APPLICATION: <DriverApplication />,
  DRIVER_REGISTRATION: <SponsorRegistrationPage />, // this probably needs to be consolidated with app
  DRIVER_POINTS: <DriverPointsList />,
  DRIVER_ACTIVITY: <DriverActivity />,
  DRIVER_POINTS_HISTORY: <DriverPointsHistory/>,
  // Sponsor Components
  APPLICATION_MANAGER: <ApplicationManager />,
  SPONSOR_POINTS_REPORTS: <Typography>Points Reports</Typography>,
  SPONSOR_AUDIT_REPORTS: <Typography>Audit Reports</Typography>,
  SPONSOR_DRIVERS: <SponsorDrivers />,
};

export default function MainGrid() {
  const { currentView } = useView();

  useEffect(() => {
    console.log("Rendering MainGrid with current view:", currentView);
  }, [currentView]);

  return (
    <Box sx={{ width: '100%' }}>
      {/* Render the component corresponding to the current view */}
      {viewComponents[currentView] || <Typography>Select an option to view content</Typography>}
    </Box>
  );
}
