import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Button,
  CircularProgress,
  Alert,
  Typography,
  Snackbar,
  Box,
} from '@mui/material';
import axios from 'axios';

const CatalogOverlay: React.FC<{ sponsorId: number; onClose: () => void }> = ({
  sponsorId,
  onClose,
}) => {
  const [categories, setCategories] = useState<number[]>([]);
  const [numberOfProducts, setNumberOfProducts] = useState(10);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [snackbarMessage, setSnackbarMessage] = useState('');

  useEffect(() => {
    checkExistingCatalog();
  }, []);

  const checkExistingCatalog = async () => {
    setLoading(true);
    try {
      const response = await axios.get(`/api/catalog/${sponsorId}`);
      if (response.data.length > 0) {
        setSnackbarMessage('Catalog already exists.');
        setSuccess(true);
        onClose(); // Automatically close if catalog exists
      }
    } catch (err) {
      console.error('Error checking catalog:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateCatalog = async () => {
    if (categories.length === 0) {
      setError('Please select at least one category.');
      return;
    }
  
    setLoading(true);
    try {
      const payload = {
        sponsorId,
        categories,
        numberOfProducts,
        pointValue: 0.1, // Example point value, could be dynamic
      };
  
      const response = await axios.post(`/api/catalog/create`, payload);
      if (response.status === 200) {
        setSnackbarMessage('Catalog created successfully.');
        setSuccess(true);
        onClose();
      } else {
        setError('Failed to create catalog.');
      }
    } catch (err: any) {
      console.error('Error creating catalog:', err);
      setError(err.response?.data?.message || 'Failed to create catalog.');
    } finally {
      setLoading(false);
    }
  };
  

  return (
    <Dialog open={true} onClose={onClose}>
      <DialogTitle>Create Catalog</DialogTitle>
      <DialogContent>
        {loading ? (
          <CircularProgress />
        ) : (
          <>
            {error && <Alert severity="error">{error}</Alert>}
            <Typography gutterBottom>
              Select categories and the number of products to include in your catalog.
            </Typography>
            <FormControl fullWidth margin="normal">
              <InputLabel>Select Categories</InputLabel>
              <Select
                multiple
                value={categories}
                onChange={(e) => setCategories(e.target.value as number[])}
                renderValue={(selected) =>
                  selected.map((id) => `Category ${id}`).join(', ')
                }
              >
                <MenuItem value={6030}>Accessories</MenuItem>
                <MenuItem value={293}>Electronics</MenuItem>
                <MenuItem value={11450}>Clothing</MenuItem>
                <MenuItem value={11700}>Home & Garden</MenuItem>
              </Select>
            </FormControl>
            <FormControl fullWidth margin="normal">
              <InputLabel>Number of Products</InputLabel>
              <Select
                value={numberOfProducts}
                onChange={(e) => setNumberOfProducts(Number(e.target.value))}
              >
                {[5, 10, 15, 20].map((count) => (
                  <MenuItem key={count} value={count}>
                    {count}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={loading}>
          Cancel
        </Button>
        <Button onClick={handleCreateCatalog} disabled={loading} variant="contained">
          Create Catalog
        </Button>
      </DialogActions>
      <Snackbar
        open={!!snackbarMessage}
        autoHideDuration={3000}
        onClose={() => setSnackbarMessage('')}
        message={snackbarMessage}
      />
    </Dialog>
  );
};

export default CatalogOverlay;
