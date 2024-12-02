import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  TextField,
  Typography,
  Modal,
  FormControlLabel,
  Checkbox,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
} from '@mui/material';
import { UsaStates } from 'usa-states';
import axios from 'axios';

const Roles = [
  { id: 1, name: 'Admin', normalizedName: 'ADMIN' },
  { id: 2, name: 'Sponsor', normalizedName: 'SPONSOR' },
  { id: 3, name: 'Driver', normalizedName: 'DRIVER' },
  { id: 4, name: 'Guest', normalizedName: 'GUEST' },
];

interface Sponsor {
  sponsorID: number;
  companyName: string;
}

interface Props {
  open: boolean;
  onClose: () => void;
}

const CreateUserModal: React.FC<Props> = ({ open, onClose }) => {
  const [formData, setFormData] = useState({
    username: '',
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    state: '',
    role: Roles[0].normalizedName,
    enable2FA: false,
    sponsorID: '',
    driverPointValue: '', // Hidden for now
  });

  const [sponsors, setSponsors] = useState<Sponsor[]>([]);
  const [loadingSponsors, setLoadingSponsors] = useState(false);
  const [pointRatio, setPointRatio] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  const usStates = new UsaStates().states;

  // Fetch sponsors and point ratio on open
  useEffect(() => {
    if (open) {
      fetchSponsors();
      fetchPointRatio();
    }
  }, [open]);

  const fetchSponsors = async () => {
    setLoadingSponsors(true);
    try {
      const response = await axios.get<Sponsor[]>('/api/sponsor/list-all');
      setSponsors(response.data);
      setError(null);
    } catch (err) {
      console.error('Error fetching sponsors:', err);
      setError('Failed to load sponsors.');
    } finally {
      setLoadingSponsors(false);
    }
  };

  const fetchPointRatio = async () => {
    try {
      const response = await axios.get<{ pointDollarValue: number }>('/api/sponsor/point-ratio');
      setPointRatio(response.data.pointDollarValue);
      setFormData((prev) => ({ ...prev, driverPointValue: response.data.pointDollarValue.toString() }));
      setError(null);
    } catch (err) {
      console.error('Error fetching point ratio:', err);
      setError('Failed to load point ratio.');
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
  };

  const handleSelectChange = (e: React.ChangeEvent<{ name?: string; value: unknown }>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name || '']: value as string,
    }));
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const payload = {
      username: formData.username,
      firstName: formData.firstName,
      lastName: formData.lastName,
      email: formData.email,
      password: formData.password,
      state: formData.state,
      role: formData.role,
      enable2FA: formData.enable2FA,
      sponsorID: formData.role === 'SPONSOR' ? formData.sponsorID : null,
      driverPointValue: formData.role === 'DRIVER' ? formData.driverPointValue : null,
    };

    try {
      const response = await axios.post('/api/admin/create-user', payload, {
        headers: { 'Content-Type': 'application/json' },
      });

      if (response.status === 200) {
        alert(`${formData.role} created successfully`);
        onClose();
        setFormData({
          username: '',
          firstName: '',
          lastName: '',
          email: '',
          password: '',
          state: '',
          role: Roles[0].normalizedName,
          enable2FA: false,
          sponsorID: '',
          driverPointValue: '',
        });
      } else {
        alert('Failed to create user.');
      }
    } catch (error) {
      console.error('Error creating user:', error);
      const errorMessage =
        error.response?.data?.message || 'Failed to create user.';
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
        component="form"
        onSubmit={handleSubmit}
        sx={{
          backgroundColor: 'white',
          padding: 4,
          borderRadius: 2,
          width: '100%',
          maxWidth: 600,
        }}
      >
        <Typography id="create-user-modal-title" variant="h6" component="h2" sx={{ mb: 2 }}>
          Create New User
        </Typography>
        {error && (
          <Typography color="error" sx={{ mb: 2 }}>
            {error}
          </Typography>
        )}
        <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
          <TextField
            label="First Name"
            name="firstName"
            value={formData.firstName}
            onChange={handleInputChange}
            required
            fullWidth
          />
          <TextField
            label="Last Name"
            name="lastName"
            value={formData.lastName}
            onChange={handleInputChange}
            required
            fullWidth
          />
        </Box>
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
        <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
          <TextField
            label="Email"
            name="email"
            type="email"
            value={formData.email}
            onChange={handleInputChange}
            required
            fullWidth
          />
          <FormControl fullWidth>
            <InputLabel id="state-select-label">State</InputLabel>
            <Select
              labelId="state-select-label"
              id="state-select"
              name="state"
              value={formData.state}
              onChange={handleSelectChange}
              required
            >
              {usStates.map((state) => (
                <MenuItem key={state.abbreviation} value={state.abbreviation}>
                  {state.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
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
            <InputLabel id="role-select-label">Role</InputLabel>
            <Select
              labelId="role-select-label"
              id="role-select"
              name="role"
              value={formData.role}
              onChange={handleSelectChange}
              required
            >
              {Roles.map((role) => (
                <MenuItem key={role.id} value={role.normalizedName}>
                  {role.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>
        {formData.role === 'SPONSOR' && (
          <Box sx={{ mb: 2 }}>
            <FormControl fullWidth>
              <InputLabel id="sponsor-select-label">Sponsor</InputLabel>
              <Select
                labelId="sponsor-select-label"
                id="sponsor-select"
                name="sponsorID"
                value={formData.sponsorID}
                onChange={handleSelectChange}
                required
              >
                {loadingSponsors ? (
                  <MenuItem disabled>
                    <CircularProgress size={20} />
                  </MenuItem>
                ) : (
                  sponsors.map((sponsor) => (
                    <MenuItem key={sponsor.sponsorID} value={sponsor.sponsorID}>
                      {sponsor.companyName}
                    </MenuItem>
                  ))
                )}
              </Select>
            </FormControl>
          </Box>
        )}
        <FormControlLabel
          control={
            <Checkbox
              name="enable2FA"
              checked={formData.enable2FA}
              onChange={handleInputChange}
            />
          }
          label="Enable Two-Factor Authentication (2FA)"
        />
        <Button type="submit" variant="contained" color="primary" fullWidth sx={{ mt: 2 }}>
          Create User
        </Button>
      </Box>
    </Modal>
  );
};

export default CreateUserModal;
