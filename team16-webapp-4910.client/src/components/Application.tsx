import React, { useState } from 'react';
import { Box, Typography, TextField, Button, FormControlLabel, Checkbox, MenuItem } from '@mui/material';

const DriverApplicationForm = () => {
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [licenseNumber, setLicenseNumber] = useState('');
  const [experience, setExperience] = useState('');
  const [preferredRoutes, setPreferredRoutes] = useState([]);

  const handleSubmit = (event) => {
    event.preventDefault();
    // Handle form submission logic here
    console.log('Form submitted:', { fullName, email, phoneNumber, licenseNumber, experience, preferredRoutes, hasHazmatEndorsement });
  };

  return (
    <Box
      component="form"
      onSubmit={handleSubmit}
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        maxWidth: 600,
        margin: 'auto',
        padding: 3,
        boxShadow: 3,
        borderRadius: 2,
        backgroundColor: 'background.paper',
      }}
    >
      <Typography variant="h4" component="h1" gutterBottom>
        Truck Driver Application
      </Typography>
      <TextField
        label="Full Name"
        variant="outlined"
        margin="normal"
        fullWidth
        value={fullName}
        onChange={(e) => setFullName(e.target.value)}
        required
      />
      <TextField
        label="Email"
        type="email"
        variant="outlined"
        margin="normal"
        fullWidth
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        required
      />
      <TextField
        label="Phone Number"
        type="tel"
        variant="outlined"
        margin="normal"
        fullWidth
        value={phoneNumber}
        onChange={(e) => setPhoneNumber(e.target.value)}
        required
      />
      <TextField
        label="Driver's License Number"
        variant="outlined"
        margin="normal"
        fullWidth
        value={licenseNumber}
        onChange={(e) => setLicenseNumber(e.target.value)}
        required
      />
      <TextField
        label="Years of Experience"
        type="number"
        variant="outlined"
        margin="normal"
        fullWidth
        value={experience}
        onChange={(e) => setExperience(e.target.value)}
        required
      />
      <TextField
        select
        label="Preferred Routes"
        variant="outlined"
        margin="normal"
        fullWidth
        value={preferredRoutes}
        onChange={(e) => setPreferredRoutes(e.target.value)}
        SelectProps={{
          multiple: true,
        }}
      >
        <MenuItem value="local">Local</MenuItem>
        <MenuItem value="regional">Regional</MenuItem>
        <MenuItem value="long-haul">Long Haul</MenuItem>
        <MenuItem value="interstate">Interstate</MenuItem>
      </TextField>
      <Button
        type="submit"
        variant="contained"
        color="primary"
        size="large"
        fullWidth
        sx={{ mt: 3, mb: 2 }}
      >
        Submit Application
      </Button>
    </Box>
  );
};

export default DriverApplicationForm;