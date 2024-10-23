import React, { useState, useEffect } from "react";
import axios from "axios";
import {
  Container,
  CssBaseline,
  Box,
  Avatar,
  Typography,
  TextField,
  Button,
  Grid,
  MenuItem,
} from "@mui/material";
import { AccountCircle } from "@mui/icons-material";
import { Link } from "react-router-dom";

interface UserProfile {
  firstName: string;
  lastName: string;
  phone: string;
  address: string;
  gender: string;
  nationality: string;
  preferredLanguage: string;
  preferredPronouns: string;
  dateOfBirth: string;
  profilePicture: string | File;
}

const Profile = () => {
  const [profile, setProfile] = useState<UserProfile>({
    firstName: "",
    lastName: "",
    phone: "",
    address: "",
    gender: "",
    nationality: "",
    preferredLanguage: "",
    preferredPronouns: "",
    dateOfBirth: "",
    profilePicture: "",
  });

  const [imagePreview, setImagePreview] = useState<string>(""); // To store the image preview URL

  useEffect(() => {
    // Fetch profile data from the API when the component mounts
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
      const imageUrl = URL.createObjectURL(file); // Create a URL for the file
      setImagePreview(imageUrl); // Set the preview URL
      setProfile({ ...profile, profilePicture: file }); // Save the file in state
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
      formData.append("preferredLanguage", profile.preferredLanguage);
      formData.append("preferredPronouns", profile.preferredPronouns);
      formData.append("dateOfBirth", profile.dateOfBirth);
      if (profile.profilePicture) {
        formData.append("profilePicture", profile.profilePicture);
      }

      // POST request to save the profile
      await axios.post("/profile", formData, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
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
      formData.append("preferredLanguage", profile.preferredLanguage);
      formData.append("preferredPronouns", profile.preferredPronouns);
      formData.append("dateOfBirth", profile.dateOfBirth);
      if (profile.profilePicture) {
        formData.append("profilePicture", profile.profilePicture);
      }

      // PUT request to update the profile
      await axios.put("/profile", formData, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
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
        <Typography variant="h5">Sponsor User Profile</Typography>
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
            select
            margin="normal"
            fullWidth
            label="Preferred Language"
            name="preferredLanguage"
            value={profile.preferredLanguage}
            onChange={handleInputChange}
          >
            <MenuItem value="English">English</MenuItem>
            <MenuItem value="Spanish">Spanish</MenuItem>
          </TextField>
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

          {/* Display the image preview if available */}
          {imagePreview && (
            <Box
              component="img"
              src={imagePreview}
              alt="Profile Preview"
              sx={{
                width: "100%",
                height: "auto",
                maxHeight: "200px", // Limit the max height of the image
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

          {/* Save Button */}
          <Button
            fullWidth
            variant="contained"
            sx={{ mt: 3, mb: 2 }}
            onClick={handleSave}
          >
            Save
          </Button>

          {/* Update Button */}
          <Button
            fullWidth
            variant="outlined"
            sx={{ mt: 1, mb: 2 }}
            onClick={handleUpdate}
          >
            Update Profile
          </Button>

          {/* Update Password Link */}
          <Grid container justifyContent="flex-end">
            <Grid item>
              <Link to="/updatepassword">Change Password?</Link>
            </Grid>
          </Grid>
        </Box>
      </Box>
    </Container>
  );
};

export default Profile;
