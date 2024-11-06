import React, { useState, useEffect } from 'react';
import {
  Dialog, DialogTitle, DialogContent, List, ListItem, ListItemText, Container,
  Card, CardMedia, CardContent, CircularProgress, Alert, Box, Button, Typography,
} from '@mui/material';import axios, { AxiosResponse } from 'axios';
import OverviewItem from '../components/layout/OverviewItem';

interface Listing {
  name: string;
  price: string;
  imageUrl: string;
}

const CACHE_KEY = 'productCatalog';
const CACHE_EXPIRY = 10 * 60 * 1000; // 10 minutes in milliseconds

const ProductCatalog: React.FC = () => {
  const [listings, setListings] = useState<Listing[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [cart, setCart] = useState<Listing[]>([]);
  const [isCartOpen, setIsCartOpen] = useState<boolean>(false);

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

  const addToCart = (item: Listing) => {
    setCart((prevCart) => [...prevCart, item]);
  };

  const openCart = () => setIsCartOpen(true);
  const closeCart = () => setIsCartOpen(false);
  const clearCart = () => setCart([]);

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

    // Fetch fresh data if no valid cache is available
    fetchListings();
  }, []);

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;

  return (
    <>
      <Container>
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
                <OverviewItem title="Price" value={`$${parseFloat(listing.price).toFixed(2)}`} />
                <Button
                  variant="contained"
                  color="primary"
                  fullWidth
                  onClick={() => addToCart(listing)}
                  sx={{ marginTop: 1 }}
                >
                  Add to Cart
                </Button>
              </CardContent>
            </Card>
          ))}
        </Box>

        {/* Cart and Clear Cart Buttons Below Cards */}
        <Box display="flex" justifyContent="center" flexDirection="column" alignItems="center" mt={4} gap={2}>
          <Button variant="contained" color="secondary" onClick={openCart}>
            View Cart
          </Button>
          <Button variant="outlined" color="error" onClick={clearCart}>
            Clear Cart
          </Button>
        </Box>
      </Container>

      <Dialog open={isCartOpen} onClose={closeCart}>
        <DialogTitle>Your Cart</DialogTitle>
        <DialogContent>
          {cart.length > 0 ? (
            <List>
              {cart.map((item, index) => (
                <ListItem key={index}>
                  <ListItemText primary={item.name} secondary={`Price: $${parseFloat(item.price).toFixed(2)}`} />
                </ListItem>
              ))}
            </List>
          ) : (
            <Typography variant="body1">Your cart is empty.</Typography>
          )}
        </DialogContent>
      </Dialog>
    </>
  );
};

export default ProductCatalog;
