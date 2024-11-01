import { Route, Routes, Link, Navigate } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Button, Container, createTheme } from '@mui/material';
import AppNavbar from './components/layout/AppNavbar';
import AppTheme from './components/layout/AppTheme';
// Pages
import HomePage from './pages/HomePage';
import About from './pages/About';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import Dashboard from './pages/Dashboard';
import ProductCatalog from './pages/ProductCatalog';
import FAQ from './pages/FAQ';
import FeedbackForm from './pages/Feedback';
// to be removed bc component
import PasswordChangeForm from './pages/PasswordChangeForm';
import DriverPointsList from './components/dashboard/PMSDriverList';
import DriverPointsHistory from './components/dashboard/DriverPointHistory';
import DriverActivity from './components/dashboard/DriverActivity';
import ManageUsers from './pages/ManageUsers';

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
                    <Route path="/change-password" element={<PasswordChangeForm/>} /> {/* This is just here when profile pages are well and done */}
                    <Route path="/driver-points" element={<DriverPointsList />} /> {/* Again; remove once dashboard has this and database has some test data; just kind of here as a test that it loads */}
                    <Route path="/driver-activity" element={<DriverActivity />} /> {/* Same thing as above; make this end */}
                    {/* Stub for auth rework, this will be logged in driver and taken out of main*/}
                    <Route path="/points-history/:driverId" element={<DriverPointsHistory />} />
                    <Route path="/" element={<Navigate to="/home" replace />} />
                    <Route path="/manage-users" element={<ManageUsers />}/>
                </Routes>
            </Container>
        </AppTheme>
    );
};

export default App;