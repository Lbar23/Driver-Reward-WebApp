import React from 'react';
import { BrowserRouter as Router, Route, Routes, Link, Navigate } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Button, Container } from '@mui/material';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import FAQ from './components/FAQ';
import FeedbackForm from './components/Feedback';
import LoginPage from './components/LoginPage';
import RegisterPage from './components/RegisterPage';

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
        MyApp
      </Typography>
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
      <Router>
        <NavBar />
        <Container maxWidth="md" sx={{ mt: 4 }}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/faq" element={<FAQ />} />
            <Route path="/feedback" element={<FeedbackForm />} />
            <Route path="/" element={<Navigate to="/login" replace />} />
          </Routes>
        </Container>
      </Router>
    </ThemeProvider>
  );
};

export default App;