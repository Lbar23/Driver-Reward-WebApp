import React, { useState } from 'react';
import axios from 'axios';
import {
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Modal,
  Select,
  TextField,
  Typography,
} from '@mui/material';

const UserTypeEnum = {
  Driver: 0,
  Sponsor: 1,
  Admin: 2
};

const CreateUserModal = ({ open, onClose }) => {
  const [formData, setFormData] = useState({
    Username: '',  // Changed to PascalCase to match DTO
    Email: '',
    Password: '',
    UserType: UserTypeEnum.Admin,  // Changed to PascalCase
    CompanyName: '',
    SponsorType: '',
    PointDollarValue: 0.01,
    SponsorID: '',  // Changed to PascalCase and empty string instead of null
  });

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    if (name === 'UserType') {
      setFormData(prev => ({
        ...prev,
        [name]: UserTypeEnum[value] // Convert string to enum value
      }));
    } else {
      setFormData(prev => ({
        ...prev,
        [name]: name === 'SponsorID' ? (value === '' ? '' : parseInt(value)) : value
      }));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    // Transform data before sending
    const payload = {
      ...formData,
      // Convert empty string to null for optional fields
      CompanyName: formData.CompanyName || null,
      SponsorType: formData.SponsorType || null,
      PointDollarValue: formData.PointDollarValue || null,
      SponsorID: formData.SponsorID === '' ? null : parseInt(formData.SponsorID)
    };

    try {
      const response = await axios.post('/api/admin/create-user', payload, {
        headers: {
          'Content-Type': 'application/json',
        },
      });
      alert('User created successfully');
      onClose();
      setFormData({
        Username: '',
        Email: '',
        Password: '',
        UserType: 'Admin',
        CompanyName: '',
        SponsorType: '',
        PointDollarValue: 0.01,
        SponsorID: '',
      });
    } catch (error) {
      console.error('Error creating user:', error);
      const errorMessage = error.response?.data?.details || 'Failed to create user';
      alert(errorMessage);
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
              name="Username"
              value={formData.Username}
              onChange={handleInputChange}
              required
              fullWidth
            />
          </Box>
          <Box sx={{ mb: 2 }}>
            <TextField
              label="Email"
              name="Email"
              type="email"
              value={formData.Email}
              onChange={handleInputChange}
              required
              fullWidth
            />
          </Box>
          <Box sx={{ mb: 2 }}>
            <TextField
              label="Password"
              name="Password"
              type="password"
              value={formData.Password}
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
                name="UserType"
                value={Object.keys(UserTypeEnum).find(key => UserTypeEnum[key] === formData.UserType)}
                onChange={handleInputChange}
                required
              >
                <MenuItem value="Admin">Admin</MenuItem>
                <MenuItem value="Sponsor">Sponsor</MenuItem>
                <MenuItem value="Driver">Driver</MenuItem>
              </Select>
            </FormControl>
          </Box>
          {formData.UserType === UserTypeEnum.Sponsor && (
            <>
              <Box sx={{ mb: 2 }}>
                <TextField
                  label="Company Name"
                  name="CompanyName"
                  value={formData.CompanyName}
                  onChange={handleInputChange}
                  required
                  fullWidth
                />
              </Box>
              <Box sx={{ mb: 2 }}>
                <TextField
                  label="Sponsor Type"
                  name="SponsorType"
                  value={formData.SponsorType}
                  onChange={handleInputChange}
                  required
                  fullWidth
                />
              </Box>
              <Box sx={{ mb: 2 }}>
                <TextField
                  label="Point Dollar Value"
                  name="PointDollarValue"
                  type="number"
                  step="0.01"
                  value={formData.PointDollarValue}
                  onChange={handleInputChange}
                  required
                  fullWidth
                />
              </Box>
            </>
          )}
          {formData.UserType === UserTypeEnum.Driver && (
            <Box sx={{ mb: 2 }}>
              <TextField
                label="Sponsor ID"
                name="SponsorID"
                type="number"
                value={formData.SponsorID}
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