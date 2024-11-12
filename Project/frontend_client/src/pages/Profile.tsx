import React, { useState, useEffect } from "react";
import axios from "axios";
import { Container, CssBaseline, Box, Avatar, Typography, TextField, Button, Grid, MenuItem } from "@mui/material";
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

const Profile: React.FC = () => {
  const [profile, setProfile] = useState<UserProfile>({
    firstName: "",
    lastName: "",
    phone: "",
    address: "",
    gender: "",
    nationality: "",
    preferredPronouns: "",
    dateOfBirth: "",
    profilePicture: "",
  });

  const [imagePreview, setImagePreview] = useState<string>("");

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const response = await axios.get("/profile");
        const profileData = response.data;
        setProfile(profileData);
        if (profileData.profilePicture) {
          setImagePreview(profileData.profilePicture);
        }
      } catch (error) {
        console.error("Error fetching profile:", error);
      }
    };
    fetchProfile();
  }, []);

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

  const handleSave = async () => {
    try {
      const formData = new FormData();
      formData.append("firstName", profile.firstName);
      formData.append("lastName", profile.lastName);
      formData.append("phone", profile.phone);
      formData.append("address", profile.address);
      formData.append("gender", profile.gender);
      formData.append("nationality", profile.nationality);
      formData.append("preferredPronouns", profile.preferredPronouns);
      formData.append("dateOfBirth", profile.dateOfBirth);
      if (profile.profilePicture) {
        formData.append("profilePicture", profile.profilePicture);
      }

      await axios.post("/profile", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      console.log("Profile saved:", profile);
    } catch (error) {
      console.error("Error saving profile:", error);
    }
  };

  const handleUpdate = async () => {
    try {
      const formData = new FormData();
      formData.append("firstName", profile.firstName);
      formData.append("lastName", profile.lastName);
      formData.append("phone", profile.phone);
      formData.append("address", profile.address);
      formData.append("gender", profile.gender);
      formData.append("nationality", profile.nationality);
      formData.append("preferredPronouns", profile.preferredPronouns);
      formData.append("dateOfBirth", profile.dateOfBirth);
      if (profile.profilePicture) {
        formData.append("profilePicture", profile.profilePicture);
      }

      await axios.put("/profile", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      console.log("Profile updated:", profile);
    } catch (error) {
      console.error("Error updating profile:", error);
    }
  };

  return (
    <Container maxWidth="xs">
      <CssBaseline />
      <Box
        display="flex"
        flexDirection="column"
        alignItems="center"
        mt={2}
      >
        <Avatar sx={{ bgcolor: "primary.main", mb: 1 }}>
          <AccountCircle />
        </Avatar>
        <Typography variant="h5" mb={3}>
          User Profile
        </Typography>

        {/* Form fields */}
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              label="First Name"
              name="firstName"
              value={profile.firstName}
              onChange={handleInputChange}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              label="Last Name"
              name="lastName"
              value={profile.lastName}
              onChange={handleInputChange}
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              fullWidth
              label="Phone Number"
              name="phone"
              value={profile.phone}
              onChange={handleInputChange}
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              fullWidth
              label="Address"
              name="address"
              value={profile.address}
              onChange={handleInputChange}
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              select
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
          </Grid>
          <Grid item xs={12}>
            <TextField
              fullWidth
              label="Nationality"
              name="nationality"
              value={profile.nationality}
              onChange={handleInputChange}
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              fullWidth
              label="Preferred Pronouns"
              name="preferredPronouns"
              value={profile.preferredPronouns}
              onChange={handleInputChange}
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              fullWidth
              label="Date of Birth"
              name="dateOfBirth"
              type="date"
              InputLabelProps={{ shrink: true }}
              value={profile.dateOfBirth}
              onChange={handleInputChange}
            />
          </Grid>
          
          {/* Image preview */}
          {imagePreview && (
            <Grid item xs={12}>
              <Box
                component="img"
                src={imagePreview}
                alt="Profile Preview"
                sx={{ width: "100%", height: "auto", maxHeight: "200px" }}
              />
            </Grid>
          )}
          
          <Grid item xs={12}>
            <Button
              variant="contained"
              component="label"
              fullWidth
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
          </Grid>

          {/* Save and Update Buttons */}
          <Grid item xs={6}>
            <Button fullWidth variant="contained" onClick={handleSave}>
              Save
            </Button>
          </Grid>
          <Grid item xs={6}>
            <Button fullWidth variant="outlined" onClick={handleUpdate}>
              Update Profile
            </Button>
          </Grid>
        </Grid>
      </Box>
    </Container>
  );
};

export default Profile;
