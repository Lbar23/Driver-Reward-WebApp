import React, { useState, useEffect, createContext } from 'react';
import { Route, Routes, Link, Navigate, useNavigate } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Button, Container, Menu, MenuItem } from '@mui/material';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import FAQ from './pages/FAQ';
import FeedbackForm from './pages/Feedback';
import Dashboard from './pages/Dashboard';
import About from './pages/About';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import PasswordChangeForm from './pages/PasswordChangeForm';
import DriverPointsList from './components/PMSDriverList';
import DriverActivity from './components/DriverActivity';

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
            <Typography variant="h6" component={Link} to="/" sx={{ flexGrow: 1, textDecoration: 'none', color: 'inherit' }}>
                Incentive Application
            </Typography>
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
    return (
        <ThemeProvider theme={theme}>
            <NavBar />
            <Container maxWidth="md" sx={{ mt: 4 }}>
                <Routes>
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="/register" element={<RegisterPage />} />
                    <Route path="/faq" element={<FAQ />} />
                    <Route path="/feedback" element={<FeedbackForm />} />
                    <Route path="/" element={<Navigate to="/login" replace />} />
                    <Route path="/test" element={<TestPage /> } />
                </Routes>
            </Container>
        </ThemeProvider>
    );
};

export default App;