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
  id: number;
  productId: number;
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
      const response: AxiosResponse<any[]> = await axios.get('/api/catalog/products');
      const listingsWithPoints = response.data.map(listing => ({
        id: listing.productID,
        productId: listing.productID,
        name: listing.name,
        price: listing.price,
        imageUrl: listing.imageUrl,
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

  const addToCart = (item: Listing) => {
    if (typeof selectedSponsorId !== 'number') return;
    
    setCarts((prevCarts) => ({
      ...prevCarts,
      [selectedSponsorId]: [
        ...(prevCarts[selectedSponsorId] || []),
        {
          ...item,
          productId: item.id
        }
      ],
    }));
  };

  //This kind of just exists, but doesn't really need to be atm
  const handleAddToCart = (listing: Listing) => {
    const selectedSponsor = sponsorPoints.find(s => s.sponsorId === selectedSponsorId);
    if (!selectedSponsor) {
      setSnackbarMessage('Please select a sponsor first');
      setShowSnackbar(true);
      return;
    }

    const pointCost = calculatePointCost(listing.price);
    if (selectedSponsor.totalPoints < pointCost) {
      setSnackbarMessage(`Not enough points! You need ${pointCost} points but have ${selectedSponsor.totalPoints}`);
      setShowSnackbar(true);
      return;
    }

    addToCart(listing);
    setSnackbarMessage('Item added to cart');
    setShowSnackbar(true);
  };

  const openCart = () => setIsCartOpen(true);
  const closeCart = () => setIsCartOpen(false);
  const clearCart = () => {
    setCarts((prevCarts) => ({
      ...prevCarts,
      [selectedSponsorId]: [],
    }));
  };

  const goToOrderPage = async () => {
    const currentCart = getCurrentCart();
    history('/order', {
      state: { 
        cartItems: currentCart,
        sponsorId: selectedSponsorId, 
        points: selectedSponsor?.totalPoints, 
        pointValue: selectedSponsor?.pointDollarValue 
      }
    });
    closeCart();
  };

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;

  const selectedSponsor = sponsorPoints.find(s => s.sponsorId === selectedSponsorId);
  const getCurrentCart = () => carts[selectedSponsorId as number] || [];

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
                alt={`Image of ${listing.name}`} // Added alt text
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
                  value={calculatePointCost(listing.price).toLocaleString()} 
                />
                <Button
                  fullWidth
                  variant="contained"
                  color="primary"
                  onClick={() => {
                    const pointCost = calculatePointCost(listing.price);
                    if (!selectedSponsor) {
                      setSnackbarMessage('Please select a sponsor first');
                      setShowSnackbar(true);
                      return;
                    }
                    if (pointCost > selectedSponsor.totalPoints) {
                      setSnackbarMessage(`Not enough points! You need ${pointCost} points but have ${selectedSponsor.totalPoints}`);
                      setShowSnackbar(true);
                      return;
                    }
                    addToCart(listing);
                    setSnackbarMessage('Item added to cart');
                    setShowSnackbar(true);
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
          <Button 
            variant="contained" 
            color="secondary" 
            onClick={openCart}
          >
            View Cart ({getCurrentCart().length})
          </Button>
        </Box>
      </Container>

      <Dialog open={isCartOpen} onClose={closeCart}>
        <DialogTitle>Your Cart</DialogTitle>
        <DialogContent>
          {getCurrentCart().length > 0 ? (
            <List>
              {getCurrentCart().map((item: Listing, index: number) => (
                <ListItem key={index}>
                  <ListItemText 
                    primary={item.name} 
                    secondary={`Points: ${calculatePointCost(item.price).toLocaleString()}`} 
                  />
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
