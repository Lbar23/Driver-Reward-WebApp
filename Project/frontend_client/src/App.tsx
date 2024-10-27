import { Route, Routes, Link, Navigate } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Button, Container } from '@mui/material';
import { ThemeProvider, createTheme } from '@mui/material/styles';
// import FAQ from './pages/FAQ';
import FeedbackForm from './pages/Feedback';
import Dashboard from './pages/Dashboard';
import About from './pages/About';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import PasswordChangeForm from './pages/PasswordChangeForm';
import DriverPointsList from './components/PMSDriverList';
import DriverPointsHistory from './components/DriverPointHistory';
import DriverActivity from './components/DriverActivity';
import HomePage from './pages/HomePage';

const theme = createTheme({
    palette: {
        primary: {
            main: '#1976d2', // Facebook-like blue
        },
    },
});

const NavBar = () => (
    <AppBar position="static">
        <Toolbar>
            <Typography variant="h6" component={Link} to="/" sx={{ flexGrow: 1, textDecoration: 'none', color: 'inherit' }}>
                Git Gud Drivers
            </Typography>
            <Button color="inherit" component={Link} to="/about">About</Button>
            <Button color="inherit" component={Link} to="/login">Login</Button>
            <Button color="inherit" component={Link} to="/register">Register</Button>
            <Button color="inherit" component={Link} to="/faq">FAQ</Button>
            <Button color="inherit" component={Link} to="/feedback">Feedback</Button>
        </Toolbar>
    </AppBar>
);

const App = () => {
    return (
        <ThemeProvider theme={theme}>
            <NavBar />
            <Container maxWidth="md" sx={{ mt: 4 }}>
                <Routes>
                    <Route path="/about" element={<About />} />
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="/register" element={<RegisterPage />} />
                    <Route path="/home" element={<HomePage />} /> {/* This is just here when dashboard has this and database has some test data; just kind of here as a test that it loads */}
                    {/* <Route path="/faq" element={<FAQ />} /> */}
                    <Route path="/feedback" element={<FeedbackForm />} />
                    <Route path="/dashboard" element={<Dashboard/>} />
                    <Route path="/change-password" element={<PasswordChangeForm/>} /> {/* This is just here when profile pages are well and done */}
                    <Route path="/driver-points" element={<DriverPointsList />} /> {/* Again; remove once dashboard has this and database has some test data; just kind of here as a test that it loads */}
                    <Route path="/driver-activity" element={<DriverActivity />} /> {/* Same thing as above; make this end */}
                    {/* Stub for auth rework, this will be logged in driver and taken out of main*/}
                    <Route path="/points-history/:driverId" element={<DriverPointsHistory />} />
                    <Route path="/" element={<Navigate to="/home" replace />} />
                </Routes>
            </Container>
        </ThemeProvider>
    );
};

export default App;