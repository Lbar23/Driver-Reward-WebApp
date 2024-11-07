import { Route, Routes, Navigate, Outlet } from 'react-router-dom';
import { Toolbar, Box } from '@mui/material';
import AppNavbar from './components/layout/AppNavbar';
import AppTheme from './components/layout/AppTheme';
import { AuthProvider, useAuth } from './service/authContext';
import { ViewProvider } from './service/viewContext';
import SideMenu from './components/layout/SideMenu';
// Pages
import HomePage from './pages/HomePage';
import About from './pages/About';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import Dashboard from './pages/Dashboard';
import ProductCatalog from './pages/ProductCatalog';
import FAQ from './pages/FAQ';
import FeedbackForm from './pages/Feedback';
<<<<<<< HEAD
import Settings from './pages/Settings';
// to be removed bc component
=======
// Other pages for authenticated routes
>>>>>>> origin/finalMain
import PasswordChangeForm from './pages/PasswordChangeForm';

<<<<<<< HEAD
const App = () => {
    return (
        <AppTheme>
            <AppNavbar />
            <Toolbar /> {/* Spacer for the AppBar height */}
            <Container maxWidth="md" sx={{ mt: 4 }}>
                <Routes>
                    <Route path="/about" element={<About />} />
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="/register" element={<RegisterPage />} />
                    <Route path="/home" element={<HomePage />} /> {/* Will still be useful to serve for people that are not logged in */}
                    <Route path="/faq" element={<FAQ />} />
                    <Route path="/feedback" element={<FeedbackForm />} />
                    <Route path="/dashboard" element={<Dashboard/>} />
                    <Route path="/catalog" element={<ProductCatalog />} />
                    <Route path="/application" element={<DriverApplication />} />
                    <Route path="/application-manager" element={<ApplicationManager />} />
                    <Route path="/change-password" element={<PasswordChangeForm/>} /> {/* This is just here when profile pages are well and done */}
                    <Route path="/driver-points" element={<DriverPointsList />} /> {/* Again; remove once dashboard has this and database has some test data; just kind of here as a test that it loads */}
                    <Route path="/driver-activity" element={<DriverActivity />} /> {/* Same thing as above; make this end */}
                    <Route path="/sponsor-driver-test" element={<SponsorDrivers />} />
                    <Route path="/sponsor-reg-test" element={<SponsorRegistrationPage/>} />
                    <Route path="/settings" element={<Settings/>} />
                    {/* Stub for auth rework, this will be logged in driver and taken out of main*/}
                    <Route path="/points-history/:driverId" element={<DriverPointsHistory />} />
                    <Route path="/" element={<Navigate to="/home" replace />} />
                </Routes>
            </Container>
        </AppTheme>
=======
const ProtectedRoute = () => {
    const { isAuthenticated } = useAuth();
    return isAuthenticated ? (
        <Box sx={{ display: 'flex' }}>
          {/* Sidebar added here */}
            <SideMenu /> 
            <Box sx={{ flexGrow: 1}}>
                <Outlet />
            </Box>
        </Box>
    ) : (
        <Navigate to="/" replace />
>>>>>>> origin/finalMain
    );
};

// conditional check to change default redirect based on auth status
const ConditionalRedirect = () => {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? <Navigate to="/dashboard" replace /> : <Navigate to="/home" replace />;
};

const App = () => (
  <AuthProvider>
    <ViewProvider>
    <AppTheme>
      <AppNavbar />
      {/* Spacer for AppBar height */}
      <Toolbar /> 
      <Box sx={{ mt: 1, width: '100%' }}> 
        {/* Main container for routes */}
        <Routes>
          {/* Default Route */}
          <Route path="/" element={<ConditionalRedirect />} />

          {/* Public Routes */}
          <Route path="/about" element={<About />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/home" element={<HomePage />} />
          <Route path="/faq" element={<FAQ />} />
          <Route path="/feedback" element={<FeedbackForm />} />

          {/* Protected Routes */}
          <Route element={<ProtectedRoute />}>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/catalog" element={<ProductCatalog />} />
            <Route path="/change-password" element={<PasswordChangeForm />} />
          </Route>
        </Routes>
      </Box>
    </AppTheme>
    </ViewProvider>
  </AuthProvider>
);

export default App;
