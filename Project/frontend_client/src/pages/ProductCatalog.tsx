import React, { useState, useEffect } from 'react';
import {
  Dialog, DialogTitle, DialogContent, DialogActions, List, ListItem, ListItemText,
  Container,
  Typography,
  Card,
  CardMedia,
  CardContent,
  CircularProgress,
  Alert,
  Box,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Button,
  Snackbar
} from '@mui/material';
import axios, { AxiosResponse } from 'axios';
import { useNavigate } from 'react-router-dom';
import OverviewItem from '../components/layout/OverviewItem';

interface Listing {
  name: string;
  price: string;
  imageUrl: string;
  pointCost?: number;
}

interface SponsorPoints {
  sponsorId: number;
  sponsorName: string;
  totalPoints: number;
  pointDollarValue: number;
}

const CACHE_KEY = 'productCatalog';
const CACHE_EXPIRY = 10 * 60 * 1000; // 10 minutes in milliseconds

const ProductCatalog: React.FC = () => {
  const [listings, setListings] = useState<Listing[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [carts, setCarts] = useState<{ [sponsorId: number]: Listing[] }>({});
  const [isCartOpen, setIsCartOpen] = useState<boolean>(false);
  const [sponsorPoints, setSponsorPoints] = useState<SponsorPoints[]>([]);
  const [selectedSponsorId, setSelectedSponsorId] = useState<number | ''>('');
  const [snackbarMessage, setSnackbarMessage] = useState('');
  const [showSnackbar, setShowSnackbar] = useState(false);
  const history = useNavigate();

  useEffect(() => {
    fetchSponsorPoints();
  }, []);

  useEffect(() => {
    const cachedData = localStorage.getItem(CACHE_KEY);
    if (cachedData) {
      const { listings: cachedListings, timestamp } = JSON.parse(cachedData);
      if (Date.now() - timestamp < CACHE_EXPIRY) {
        setListings(cachedListings);
        setLoading(false);
        return;
      }
    }
    fetchListings();
  }, []);

  const fetchSponsorPoints = async () => {
    try {
      const response = await axios.get<SponsorPoints[]>('/api/driver/my-sponsors');
      setSponsorPoints(response.data);
      if (response.data.length > 0) {
        setSelectedSponsorId(response.data[0].sponsorId);
      }
    } catch (err) {
      setError('Failed to load sponsor points data');
      console.error('Error fetching sponsor points:', err);
    }
  };

  const fetchListings = async () => {
    try {
      const response: AxiosResponse<Listing[]> = await axios.get('/api/catalog/products');
      const listingsWithPoints = response.data.map(listing => ({
        ...listing,
        pointCost: calculatePointCost(listing.price)
      }));
      setListings(listingsWithPoints);
      localStorage.setItem(CACHE_KEY, JSON.stringify({ 
        listings: listingsWithPoints, 
        timestamp: Date.now() 
      }));
    } catch (error: any) {
      setError(error.response?.data || 'Failed to load products.');
    } finally {
      setLoading(false);
    }
  };

  const calculatePointCost = (price: string): number => {
    const selectedSponsor = sponsorPoints.find(s => s.sponsorId === selectedSponsorId);
    if (!selectedSponsor) return 0;
    
    const numericPrice = parseFloat(price.split(' ')[0]);
    return Math.ceil(numericPrice / selectedSponsor.pointDollarValue);
  };

  const handlePurchase = (listing: Listing) => {
    const selectedSponsor = sponsorPoints.find(s => s.sponsorId === selectedSponsorId);
    if (!selectedSponsor || !listing.pointCost) return;

    if (selectedSponsor.totalPoints < listing.pointCost) {
      setSnackbarMessage(`Not enough points! You need ${listing.pointCost} points but have ${selectedSponsor.totalPoints}`);
    } 
    setShowSnackbar(true);
  };

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;

  const selectedSponsor = sponsorPoints.find(s => s.sponsorId === selectedSponsorId);
  const getCurrentCart = () => carts[selectedSponsorId] || [];

  const addToCart = (item: Listing) => {
    setCarts((prevCarts) => ({
      ...prevCarts,
      [selectedSponsorId]: [...(prevCarts[selectedSponsorId] || []), item],
    }));
  };
  const openCart = () => setIsCartOpen(true);
  const closeCart = () => setIsCartOpen(false);
  const clearCart = () => {
    setCarts((prevCarts) => ({
      ...prevCarts,
      [selectedSponsorId]: [],
    }));
  };

  const goToOrderPage = () => {
    history('/order', {
      state: { cartItems: getCurrentCart(), sponsorId: selectedSponsorId, points: selectedSponsor?.totalPoints }  // pass cart and sponsor data to Order page
    });
    closeCart();
  };

  

  return (
    <>
      <Container>
        <Typography variant="h4" component="h1" gutterBottom>
          Product Catalog
        </Typography>

        <FormControl fullWidth sx={{ mb: 4 }}>
          <InputLabel>Select Sponsor</InputLabel>
          <Select
            value={selectedSponsorId}
            label="Select Sponsor"
            onChange={(e) => setSelectedSponsorId(e.target.value as number)}
          >
            {sponsorPoints.map((sp) => (
              <MenuItem key={sp.sponsorId} value={sp.sponsorId}>
                {sp.sponsorName} - {sp.totalPoints.toLocaleString()} points available
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        {selectedSponsor && (
          <Alert severity="info" sx={{ mb: 2 }}>
            You have {selectedSponsor.totalPoints.toLocaleString()} points available with {selectedSponsor.sponsorName}.
            Point Value: ${selectedSponsor.pointDollarValue.toFixed(2)} per point
          </Alert>
        )}

        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, justifyContent: 'flex-start' }}>
          {listings.map((listing, index) => (
            <Card
              key={index}
              sx={{
                width: 'calc(33.333% - 16px)',
                maxWidth: 'calc(33.333% - 16px)',
                marginBottom: 2,
              }}>
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
                <OverviewItem title="Price" value={listing.price} />
                <OverviewItem 
                  title="Point Cost" 
                  value={ calculatePointCost(listing.price).toLocaleString() } 
                />
                <Button
                  fullWidth
                  variant="contained"
                  color="primary"
                  onClick={() => {
                    handlePurchase(listing);
                    if (!(!selectedSponsor || (listing.pointCost || 0) > selectedSponsor.totalPoints))
                      addToCart(listing);
                  }}
                  sx={{ mt: 2 }}
                >
                  Add to Cart
                </Button>
              </CardContent>
            </Card>
          ))}
        </Box>

        <Snackbar
          open={showSnackbar}
          autoHideDuration={3000}
          onClose={() => setShowSnackbar(false)}
          message={snackbarMessage}
        />

        {/* Cart and Clear Cart Buttons Below Cards */}
        <Box
          sx={{
            position: 'fixed',
            top: 80,
            right: 16,
            display: 'flex',
            flexDirection: 'column',
            gap: 2,
          }}
        >
          <Button variant="contained" color="secondary" onClick={openCart}>
            View Cart
          </Button>
        </Box>
      </Container>

      <Dialog open={isCartOpen} onClose={closeCart}>
        <DialogTitle>Your Cart</DialogTitle>
        <DialogContent>
          {getCurrentCart().length > 0 ? (
            <List>
              {getCurrentCart().map((item, index) => (
                <ListItem key={index}>
                  <ListItemText primary={item.name} secondary={`Price: ${parseFloat(item.pointCost).toFixed(2)}`} />
                </ListItem>
              ))}
            </List>
          ) : (
            <Typography variant="body1">Your cart is empty.</Typography>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={clearCart} color="error">
            Clear Cart
          </Button>
          <Button 
            onClick={goToOrderPage} 
            color="primary" 
            variant="contained" 
            disabled={getCurrentCart().length === 0}
          >
            Proceed to Checkout
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default ProductCatalog;