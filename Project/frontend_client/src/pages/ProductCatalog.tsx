import React, { useState, useEffect } from 'react';
import { Container, Typography, Card, CardMedia, CardContent, TextField, Checkbox, FormControlLabel, CircularProgress, Alert, Box, Button, Snackbar
} from '@mui/material';
import axios, { AxiosResponse } from 'axios';
import OverviewItem from '../components/layout/OverviewItem';

interface Listing {
  name: string;
  price: string;
  imageUrl: string;
  isOutOfStock?: boolean;
}

const CACHE_KEY = 'productCatalog';
const CACHE_EXPIRY = 10 * 60 * 1000; // 10 minutes in milliseconds

const ProductCatalog: React.FC = () => {
  const [listings, setListings] = useState<Listing[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [saveSuccess, setSaveSuccess] = useState<boolean>(false); // State to handle save success feedback

  const fetchListings = async () => {
    try {
      const response: AxiosResponse<Listing[]> = await axios.get(`/api/catalog/products`);
      setListings(response.data);
      setLoading(false);

      // Cache data and timestamp
      localStorage.setItem(CACHE_KEY, JSON.stringify({ listings: response.data, timestamp: Date.now() }));
    } catch (error: any) {
      setError(error.response?.data || 'Failed to load products.');
      setLoading(false);
    }
  };

  useEffect(() => {
    const cachedData = localStorage.getItem(CACHE_KEY);
    if (cachedData) {
      const { listings, timestamp } = JSON.parse(cachedData);
      if (Date.now() - timestamp < CACHE_EXPIRY) {
        setListings(listings);
        setLoading(false);
        return;
      }
    }

    fetchListings();
  }, []);

  const handlePriceChange = (index: number, newPrice: string) => {
    const updatedListings = [...listings];
    updatedListings[index].price = newPrice;
    setListings(updatedListings);
  };

  const handleStockStatusChange = (index: number, isOutOfStock: boolean) => {
    const updatedListings = [...listings];
    updatedListings[index].isOutOfStock = isOutOfStock;
    setListings(updatedListings);
  };

  const updateProducts = async () => {
    try {
      await axios.put(`/api/catalog/products`, listings);
      setSaveSuccess(true); // Show success message on successful save
    } catch (error: any) {
      setError(error.response?.data || 'Failed to save product changes.');
    }
  };

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;

  return (
    <Container>
      <Typography variant="h4" component="h1" gutterBottom>
        Product Catalog
      </Typography>
      <Box
        sx={{
          display: 'flex',
          flexWrap: 'wrap',
          gap: 2,
          justifyContent: 'flex-start',
        }}
      >
        {listings.map((listing, index) => (
          <Card
            key={index}
            sx={{
              width: 'calc(33.333% - 16px)',
              maxWidth: 'calc(33.333% - 16px)',
              marginBottom: 2,
            }}
          >
            <CardMedia
              component="img"
              height="200"
              image={listing.imageUrl}
              alt={listing.name}
              sx={{ objectFit: 'cover' }}
            />
            <CardContent>
              <Typography
                variant="h6"
                component="p"
                sx={{
                  whiteSpace: 'nowrap',
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                }}
              >
                {listing.name}
              </Typography>
              <TextField
                label="Price"
                variant="outlined"
                value={listing.price}
                onChange={(e) => handlePriceChange(index, e.target.value)}
                fullWidth
                margin="dense"
              />
              <FormControlLabel
                control={
                  <Checkbox
                    checked={listing.isOutOfStock || false}
                    onChange={(e) => handleStockStatusChange(index, e.target.checked)}
                  />
                }
                label="Out of Stock"
              />
              <OverviewItem title="Price" value={listing.price} />
            </CardContent>
          </Card>
        ))}
      </Box>
      <Button variant="contained" color="primary" onClick={updateProducts}>
        Save Changes
      </Button>

      {/* Snackbar to show save success message */}
      <Snackbar
        open={saveSuccess}
        autoHideDuration={3000}
        onClose={() => setSaveSuccess(false)}
        message="Changes saved successfully!"
      />
    </Container>
  );
};

export default ProductCatalog;
