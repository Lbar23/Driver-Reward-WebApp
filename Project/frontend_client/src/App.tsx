import { Route, Routes, Link, Navigate } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Button, Container } from '@mui/material';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import AppNavbar from './components/layout/AppNavbar';
import AppTheme from './components/layout/AppTheme';
import FAQ from './pages/FAQ';
import FeedbackForm from './pages/Feedback';
import Dashboard from './pages/Dashboard';
import About from './pages/About';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import PasswordChangeForm from './pages/PasswordChangeForm';
import DriverPointsList from './components/dashboard/PMSDriverList';
import DriverPointsHistory from './components/dashboard/DriverPointHistory';
import DriverActivity from './components/dashboard/DriverActivity';

// const theme = createTheme({
//     palette: {
//         primary: {
//             main: '#1976d2', // Facebook-like blue
//         },
//     },
// });

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
        </AppTheme>
    );
};

export default App;