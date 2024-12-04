import React, { useEffect } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import DriverApplication from './DriverApplication';
import ApplicationManager from './ApplicationManager';
import PasswordChangeForm from '../../pages/PasswordChangeForm';
import DriverPointsList from '../dashboard/PMSDriverList';
import DriverActivity from '../dashboard/DriverActivity';
import SponsorDrivers from '../dashboard/SponsorDriverList';
import SponsorRegistrationPage from '../dashboard/SponsorRegistrationForDriver';
import DriverPointsHistory from '../dashboard/DriverPointHistory';
import SponsorReports from './SponsorReports';
import AdminReports from './AdminReports';
import ManageDriverSponsors from './ManageDriverSponsors';
import AuditLogDashboard from '../../pages/AuditLogDashboard';
import { useView } from '../../service/viewContext';
import AdminConsole from './AdminConsole';
import ManageSponsors from './ManageSponsorsAdmin';
import ManageAdmins from './ManageAdmins';
import Profile from './Profile';

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
  MANAGE_DRIVERS: <ManageDriverSponsors />,
  MANAGE_SPONSORS: <ManageSponsors/>,
  MANAGE_ADMINS: <ManageAdmins/>,
  ADMIN_REPORTS: <AdminReports />,
  ADMIN_AUDIT_REPORTS: <AuditLogDashboard/>,
  ADMIN_CONSOLE: <AdminConsole />,
  // Driver Components
  DRIVER_APPLICATION: <DriverApplication />,
  // DRIVER_REGISTRATION: <SponsorRegistrationPage />, // this probably needs to be consolidated with app <-- Done
  DRIVER_POINTS: <DriverPointsList />,
  DRIVER_ACTIVITY: <DriverActivity />,
  DRIVER_POINTS_HISTORY: <DriverPointsHistory/>,
  PROFILE: <Profile/>,
  // Sponsor Components
  APPLICATION_MANAGER: <ApplicationManager />,
  SPONSOR_REPORTS: <SponsorReports />,
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
