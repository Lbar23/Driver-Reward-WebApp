import React, { useState, useEffect } from "react";
import {
  Container,
  Box,
  Avatar,
  Typography,
  TextField,
  Button,
  Grid,
  MenuItem,
} from "@mui/material";
import { AccountCircle } from "@mui/icons-material";

interface UserProfile {
  firstName: string;
  lastName: string;
  phone: string;
  address: string;
  gender: string;
  nationality: string;
  preferredPronouns: string;
  dateOfBirth: string;
  profilePicture: string | File;
}

interface ProfileProps {
  profileData?: UserProfile;
  onSave?: (profile: UserProfile) => void;
  onUpdate?: (profile: UserProfile) => void;
}

const Profile: React.FC<ProfileProps> = ({
  profileData,
  onSave = (profile) => console.log("Default save function", profile),
  onUpdate = (profile) => console.log("Default update function", profile),
}) => {
  const initialProfile = profileData || {
    firstName: "",
    lastName: "",
    phone: "",
    address: "",
    gender: "",
    nationality: "",
    preferredPronouns: "",
    dateOfBirth: "",
    profilePicture: "",
  };

  const [profile, setProfile] = useState<UserProfile>(initialProfile);
  const [imagePreview, setImagePreview] = useState<string>("");

  // Load profile from localStorage on component mount
  useEffect(() => {
    const savedProfile = localStorage.getItem("userProfile");
    if (savedProfile) {
      const parsedProfile = JSON.parse(savedProfile);
      setProfile(parsedProfile);
      if (typeof parsedProfile.profilePicture === "string") {
        setImagePreview(parsedProfile.profilePicture);
      }
    }
  }, []);

  // Save profile data to localStorage on profile change
  useEffect(() => {
    localStorage.setItem("userProfile", JSON.stringify(profile));
  }, [profile]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setProfile({ ...profile, [name]: value });
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files ? e.target.files[0] : null;
    if (file) {
      const imageUrl = URL.createObjectURL(file);
      setImagePreview(imageUrl);
      setProfile({ ...profile, profilePicture: file });
    }
  };

  const handleSave = () => {
    onSave(profile);
    localStorage.setItem("userProfile", JSON.stringify(profile));
  };

  const handleUpdate = () => {
    onUpdate(profile);
    localStorage.setItem("userProfile", JSON.stringify(profile));
  };

  return (
    <Container maxWidth="xs">
      <Box
        sx={{
          mt: 2,
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
        }}
      >
        <Avatar sx={{ m: 1, bgcolor: "primary.light" }}>
          <AccountCircle />
        </Avatar>
        <Typography variant="h5">User Profile</Typography>
        <Box sx={{ mt: 3 }}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                margin="normal"
                required
                fullWidth
                label="First Name"
                name="firstName"
                value={profile.firstName}
                onChange={handleInputChange}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                margin="normal"
                required
                fullWidth
                label="Last Name"
                name="lastName"
                value={profile.lastName}
                onChange={handleInputChange}
              />
            </Grid>
          </Grid>
          <TextField
            margin="normal"
            required
            fullWidth
            label="Phone Number"
            name="phone"
            value={profile.phone}
            onChange={handleInputChange}
          />
          <TextField
            margin="normal"
            required
            fullWidth
            label="Address"
            name="address"
            value={profile.address}
            onChange={handleInputChange}
          />
          <TextField
            select
            margin="normal"
            fullWidth
            label="Gender"
            name="gender"
            value={profile.gender}
            onChange={handleInputChange}
          >
            <MenuItem value="Male">Male</MenuItem>
            <MenuItem value="Female">Female</MenuItem>
            <MenuItem value="Non-Binary">Non-Binary</MenuItem>
            <MenuItem value="Other">Other</MenuItem>
          </TextField>
          <TextField
            margin="normal"
            required
            fullWidth
            label="Nationality"
            name="nationality"
            value={profile.nationality}
            onChange={handleInputChange}
          />
          <TextField
            margin="normal"
            fullWidth
            label="Preferred Pronouns"
            name="preferredPronouns"
            value={profile.preferredPronouns}
            onChange={handleInputChange}
          />
          <TextField
            margin="normal"
            fullWidth
            label="Date of Birth"
            name="dateOfBirth"
            type="date"
            InputLabelProps={{
              shrink: true,
            }}
            value={profile.dateOfBirth}
            onChange={handleInputChange}
          />

          {imagePreview && (
            <Box
              component="img"
              src={imagePreview}
              alt="Profile Preview"
              sx={{
                width: "100%",
                height: "auto",
                maxHeight: "200px",
                mt: 2,
                mb: 2,
              }}
            />
          )}

          <Button
            variant="contained"
            component="label"
            fullWidth
            sx={{ mt: 2, mb: 2 }}
          >
            Upload Profile Picture
            <input
              type="file"
              hidden
              name="profilePicture"
              accept="image/*"
              onChange={handleFileChange}
            />
          </Button>

          <Button
            fullWidth
            variant="contained"
            sx={{ mt: 3, mb: 2 }}
            onClick={handleSave}
          >
            Save
          </Button>

          <Button
            fullWidth
            variant="outlined"
            sx={{ mt: 1, mb: 2 }}
            onClick={handleUpdate}
          >
            Update Profile
          </Button>
        </Box>
      </Box>
    </Container>
  );
};

export default Profile;
