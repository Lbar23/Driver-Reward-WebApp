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
import Order from './pages/Order';
// Other pages for authenticated routes
import PasswordChangeForm from './pages/PasswordChangeForm';
import Settings from './pages/Settings';
import AuditLogDashboard from './pages/AuditLogDashboard';

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
            <Route path="/settings" element={<Settings/>}/>
            <Route path="/order" element={<Order />} />
            
          </Route>
        </Routes>
      </Box>
    </AppTheme>
    </ViewProvider>
  </AuthProvider>
);

export default App;

