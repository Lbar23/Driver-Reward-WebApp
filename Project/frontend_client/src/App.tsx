import { Route, Routes, Navigate, Outlet } from 'react-router-dom';
import { Toolbar, Box } from '@mui/material';
import AppNavbar from './components/layout/AppNavbar';
import AppTheme from './theme/AppTheme';
import { AuthProvider, useAuth } from './service/authContext';
import { ViewProvider } from './service/viewContext';
import ProfilePage from './pages/ProfileView'
import SideMenu from './components/layout/SideMenu';
//Accessibility service
import { AccessibilityProvider } from './service/accessibilityContext';
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
import ResetPasswordPage from './pages/ResetPassword';
// Other pages for authenticated routes
import PasswordChangeForm from './pages/PasswordChangeForm';
import Settings from './pages/Settings'

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
      <AccessibilityProvider> {/* Added Accessibility Provider for cascading to pages */}
    <AppTheme>
      {/* Skip link for keyboard users */}
      <a
        href="#main-content"
        style={{
          position: 'absolute',
          left: '-9999px',
          zIndex: 999,
          padding: '1rem',
          background: '#fff',
          textDecoration: 'none',
        }}
        onFocus={(e) => {
          e.currentTarget.style.left = '1rem';
        }}
        onBlur={(e) => {
          e.currentTarget.style.left = '-9999px';
        }}
      >
        Skip to main content
      </a>
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
          <Route path="/reset-password" element={<ResetPasswordPage />} />

          

          {/* Protected Routes */}
          <Route element={<ProtectedRoute />}>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/catalog" element={<ProductCatalog />} />
            <Route path="/change-password" element={<PasswordChangeForm />} />
            <Route path="/settings" element={<Settings/>}/>
            <Route path="/order" element={<Order />} />
            <Route path="/profile" element={<ProfilePage />} />

          </Route>
        </Routes>
      </Box>
    </AppTheme>
    </AccessibilityProvider> {/* Added Accessibility Provider for cascading to pages */}
    </ViewProvider>
  </AuthProvider>
);

export default App;

