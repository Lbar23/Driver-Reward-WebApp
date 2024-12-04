import React, { useEffect } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import DriverApplication from './DriverApplication';
import ApplicationManager from './ApplicationManager';
import PasswordChangeForm from '../../pages/PasswordChangeForm';
import DriverPointsList from '../dashboard/PMSDriverList';
import DriverActivity from '../dashboard/DriverActivity';
import SponsorDrivers from '../dashboard/SponsorDriverList';
import DriverPointsHistory from '../dashboard/DriverPointHistory';
import SponsorReports from './SponsorReports';
import AdminReports from './AdminReports';
import AuditLogDashboard from '../../pages/AuditLogDashboard';
import { useView } from '../../service/viewContext';
import { useAuth } from '../../service/authContext';
import AdminConsole from './AdminConsole';
import Profile from './Profile';
import UserManagementDashboard from './ManagementUsers';
import SponsorAuditLogs from './SponsorAudit';

const DashboardHome: React.FC = () => {
  const { user } = useAuth();

  if (user?.roles.includes('Admin')) {
    return (
      <Box>
        <Typography variant="h6">Admin Dashboard</Typography>
        <UserManagementDashboard />
      </Box>
    );
  }

  if (user?.roles.includes('Sponsor')) {
    return (
      <Box>
        <Typography variant="h6">Sponsor Dashboard</Typography>
        <SponsorDrivers />
      </Box>
    );
  }

  // For other roles (like Driver) or if no specific role matches
  return (
    <Box>
      <Typography variant="h6">Dashboard</Typography>
      <Typography>Welcome to your dashboard</Typography>
      <DriverPointsList />
    </Box>
  );
};

// Link the current view to the corresponding component
const viewComponents: Record<string, JSX.Element> = {
  MAIN: <DashboardHome />,
  // General Components
  CHANGE_PASSWORD: <PasswordChangeForm />,
  // Admin Components
  ADMIN_REPORTS: <AdminReports />,
  ADMIN_AUDIT_REPORTS: <AuditLogDashboard/>,
  ADMIN_CONSOLE: <AdminConsole />,
  // Driver Components
  DRIVER_APPLICATION: <DriverApplication />,
  DRIVER_ACTIVITY: <DriverActivity />,
  DRIVER_POINTS_HISTORY: <DriverPointsHistory/>,
  PROFILE: <Profile/>,
  // Sponsor Components
  APPLICATION_MANAGER: <ApplicationManager />,
  SPONSOR_REPORTS: <SponsorReports />,
  SPONSOR_AUDIT_REPORTS: <SponsorAuditLogs />,
};

export default function MainGrid() {
  const { currentView } = useView();
  const { user } = useAuth();

  useEffect(() => {
    console.log("Rendering MainGrid with current view:", currentView);
    console.log("Current user roles:", user?.roles);
  }, [currentView, user]);

  return (
    <Box sx={{ width: '100%' }}>
      {viewComponents[currentView] || <Typography>Select an option to view content</Typography>}
    </Box>
  );
}