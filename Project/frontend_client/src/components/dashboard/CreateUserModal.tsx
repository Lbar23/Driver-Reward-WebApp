import React, { useState } from 'react';
import axios from 'axios';
import {
  Box,
  Button,
  Chip,
  FormControl,
  InputLabel,
  MenuItem,
  Modal,
  Select,
  TextField,
  Typography,
} from '@mui/material';

const CreateUserModal = ({ open, onClose }) => {
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    userType: 'Admin',
    companyName: '',
    sponsorType: '',
    pointDollarValue: 0.01,
    sponsorId: null,
  });

  const handleInputChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      await axios.post('/api/admin/create-user', formData, {
        headers: {
          'Content-Type': 'application/json',
        },
      });
      alert('User created successfully');
      onClose();
      setFormData({
        username: '',
        email: '',
        password: '',
        userType: 'Admin',
        companyName: '',
        sponsorType: '',
        pointDollarValue: 0.01,
        sponsorId: null,
      });
    } catch (error) {
      console.error('Error creating user:', error);
      alert('Failed to create user');
    }
  };

  return (
    <Modal
      open={open}
      onClose={onClose}
      aria-labelledby="create-user-modal-title"
      aria-describedby="create-user-modal-description"
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: 'rgba(0, 0, 0, 0.5)',
      }}
    >
      <Box
        sx={{
          backgroundColor: 'white',
          padding: 4,
          borderRadius: 2,
          width: '100%',
          maxWidth: 400,
        }}
      >
        <Typography id="create-user-modal-title" variant="h6" component="h2">
          Create New User
        </Typography>
        <form onSubmit={handleSubmit}>
        <Box sx={{ mb: 2 }}>
          <TextField
            label="Username"
            name="username"
            value={formData.username}
            onChange={handleInputChange}
            required
            fullWidth
          />
        </Box>
        <Box sx={{ mb: 2 }}>
          <TextField
            label="Email"
            name="email"
            type="email"
            value={formData.email}
            onChange={handleInputChange}
            required
            fullWidth
          />
        </Box>
        <Box sx={{ mb: 2 }}>
          <TextField
            label="Password"
            name="password"
            type="password"
            value={formData.password}
            onChange={handleInputChange}
            required
            fullWidth
          />
        </Box>
        <Box sx={{ mb: 2 }}>
          <FormControl fullWidth>
            <InputLabel id="user-type-label">User Type</InputLabel>
            <Select
              labelId="user-type-label"
              name="userType"
              value={formData.userType}
              onChange={handleInputChange}
              required
            >
              <MenuItem value="Admin">Admin</MenuItem>
              <MenuItem value="Sponsor">Sponsor</MenuItem>
              <MenuItem value="Driver">Driver</MenuItem>
            </Select>
          </FormControl>
        </Box>
        {formData.userType === 'Sponsor' && (
          <>
            <Box sx={{ mb: 2 }}>
              <TextField
                label="Company Name"
                name="companyName"
                value={formData.companyName}
                onChange={handleInputChange}
                required
                fullWidth
              />
            </Box>
            <Box sx={{ mb: 2 }}>
              <TextField
                label="Sponsor Type"
                name="sponsorType"
                value={formData.sponsorType}
                onChange={handleInputChange}
                required
                fullWidth
              />
            </Box>
            <Box sx={{ mb: 2 }}>
              <TextField
                label="Point Dollar Value"
                name="pointDollarValue"
                type="number"
                value={formData.pointDollarValue}
                onChange={handleInputChange}
                required
                fullWidth
              />
            </Box>
          </>
        )}
        {formData.userType === 'Driver' && (
          <Box sx={{ mb: 2 }}>
            <TextField
              label="Sponsor ID"
              name="sponsorId"
              type="number"
              value={formData.sponsorId}
              onChange={handleInputChange}
              required
              fullWidth
            />
          </Box>
        )}
        <Button type="submit" variant="contained" color="primary" fullWidth>
          Create User
        </Button>
        </form>
      </Box>
    </Modal>
  );
};

export default CreateUserModal;