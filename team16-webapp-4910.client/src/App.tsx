import React, { useState, useEffect } from 'react';
import { Route, Routes, Link, Navigate, useNavigate } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Button, Container } from '@mui/material';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import FAQ from './components/FAQ';
import FeedbackForm from './components/Feedback';
import LoginPage from './components/LoginPage';
import RegisterPage from './components/RegisterPage';
import TestPage from './components/TestPage';
import Home from './pages/Home';
import DriverPoints from './components/DriverPoints';
import Application from './components/Application'

const theme = createTheme({
    palette: {
        primary: {
            main: '#1976d2', // Facebook-like blue
        },
    },
});

const NavBar = ({ isLoggedIn, handleLogout }) => (
    <AppBar position="static">
        <Toolbar>
            <Typography variant="h6" component={Link} to="/" sx={{ flexGrow: 1, textDecoration: 'none', color: 'inherit' }}>
                Incentive Application
            </Typography>
            {isLoggedIn ? (
                <>
                    <Button color="inherit" component={Link} to="/home">Home</Button>
                    <Button color="inherit" component={Link} to="/driver-points">Driver Points</Button>
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

const App = () => {
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        // Check if user is logged in (e.g., by checking for a token in localStorage)
        const token = localStorage.getItem('userToken');
        if(token == 'loggedIn')
            setIsLoggedIn(true);
    }, []);

    const handleLogout = () => {
        // Clear user token and update state
        localStorage.removeItem('userToken');
        setIsLoggedIn(false);
        navigate('/login');
    };

    return (
        <ThemeProvider theme={theme}>
            <NavBar isLoggedIn={isLoggedIn} handleLogout={handleLogout} />
            <Container maxWidth="md" sx={{ mt: 4 }}>
                <Routes>
                    <Route path="/home" element={isLoggedIn ? <Home /> : <Navigate to="/login" replace />} />
                    <Route path="/login" element={<LoginPage setIsLoggedIn={setIsLoggedIn} />} />
                    <Route path="/register" element={<RegisterPage />} />
                    <Route path="/faq" element={<FAQ />} />
                    <Route path="/feedback" element={<FeedbackForm />} />
                    <Route path="/driver-points" element={isLoggedIn ? <DriverPoints /> : <Navigate to="/login" replace />} />
                    <Route path="/" element={isLoggedIn ? <Navigate to="/home" replace /> : <Navigate to="/login" replace />} />
                    <Route path="/test" element={<TestPage />} />
                    <Route path= "/apply" element={<Application />} />
                </Routes>
            </Container>
        </ThemeProvider>
    );
};

export default App;