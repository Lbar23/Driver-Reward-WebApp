import React, { useState, useEffect, createContext } from 'react';
import { Route, Routes, Link, Navigate, useNavigate } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Button, Container, Menu, MenuItem } from '@mui/material';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import FAQ from './components/FAQ';
import FeedbackForm from './components/Feedback';
import LoginPage from './components/LoginPage';
import RegisterPage from './components/RegisterPage';
import TestPage from './components/TestPage';
import Home from './pages/Home';
import DriverPoints from './components/DriverPoints';
import Application from './components/Application';
import AdminDashboard from './components/AdminDashboard';
import SponsorDashboard from './components/SponsorDashboard';
import UserManagement from './components/UserManagement';

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
  },
});

export const UserContext = createContext({
  userType: '',
  setUserType: (type: string) => {},
});

const NavBar = ({ isLoggedIn, handleLogout, userType }) => {
  const [anchorEl, setAnchorEl] = useState(null);

  const handleMenuOpen = (event) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  return (
    <AppBar position="static">
      <Toolbar>
        <Typography variant="h6" style={{ flexGrow: 1 }}>
          Incentive Application
        </Typography>
        {isLoggedIn ? (
          <>
            <Button color="inherit" component={Link} to="/">Home</Button>
            <Button color="inherit" component={Link} to="/driver-points">Driver Points</Button>
            {userType === 'admin' && (
              <>
                <Button color="inherit" onClick={handleMenuOpen}>Admin</Button>
                <Menu
                  anchorEl={anchorEl}
                  open={Boolean(anchorEl)}
                  onClose={handleMenuClose}
                >
                  <MenuItem component={Link} to="/admin/dashboard" onClick={handleMenuClose}>Dashboard</MenuItem>
                  <MenuItem component={Link} to="/admin/user-management" onClick={handleMenuClose}>User Management</MenuItem>
                  <MenuItem component={Link} to="/driver-view" onClick={handleMenuClose}>Driver View</MenuItem>
                </Menu>
              </>
            )}
            {userType === 'sponsor' && (
              <Button color="inherit" component={Link} to="/sponsor/dashboard">Sponsor Dashboard</Button>
            )}
            <Button color="inherit" onClick={handleLogout}>Logout</Button>
          </>
        ) : (
          <>
            <Button color="inherit" component={Link} to="/login">Login</Button>
            <Button color="inherit" component={Link} to="/register">Register</Button>
          </>
        )}
        <Button color="inherit" component={Link} to="/faq">FAQ</Button>
        <Button color="inherit" component={Link} to="/feedback">Feedback</Button>
      </Toolbar>
    </AppBar>
  );
};

const App = () => {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [userType, setUserType] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('userToken');
    const storedUserType = localStorage.getItem('userType');
    if (token === 'loggedIn' && storedUserType) {
      setIsLoggedIn(true);
      setUserType(storedUserType);
    }
  }, []);

  const handleLogout = () => {
    localStorage.removeItem('userToken');
    localStorage.removeItem('userType');
    setIsLoggedIn(false);
    setUserType('');
    navigate('/login');
  };

  const handleLogin = (type) => {
    localStorage.setItem('userToken', 'loggedIn');
    localStorage.setItem('userType', type);
    setIsLoggedIn(true);
    setUserType(type);
  };

  return (
    <ThemeProvider theme={theme}>
      <UserContext.Provider value={{ userType, setUserType }}>
        <NavBar isLoggedIn={isLoggedIn} handleLogout={handleLogout} userType={userType} />
        <Container>
          <Routes>
            <Route path="/" element={isLoggedIn ? <Home /> : <Navigate to="/login" />} />
            <Route path="/login" element={<LoginPage onLogin={handleLogin} />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/faq" element={<FAQ />} />
            <Route path="/feedback" element={<FeedbackForm />} />
            <Route path="/driver-points" element={isLoggedIn ? <DriverPoints /> : <Navigate to="/login" />} />
            <Route path="/application" element={isLoggedIn ? <Application /> : <Navigate to="/login" />} />
            <Route path="/admin/dashboard" element={userType === 'admin' ? <AdminDashboard /> : <Navigate to="/" />} />
            <Route path="/admin/user-management" element={userType === 'admin' ? <UserManagement /> : <Navigate to="/" />} />
            <Route path="/sponsor/dashboard" element={userType === 'sponsor' ? <SponsorDashboard /> : <Navigate to="/" />} />
            <Route path="/driver-view" element={(userType === 'admin' || userType === 'sponsor') ? <DriverPoints isDriverView={true} /> : <Navigate to="/" />} />
            <Route path="/test" element={<TestPage />} />
          </Routes>
        </Container>
      </UserContext.Provider>
    </ThemeProvider>
  );
};

export default App;