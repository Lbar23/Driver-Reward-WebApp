import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  List,
  ListItem,
  ListItemText,
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
  Snackbar,
} from '@mui/material';
import axios, { AxiosResponse } from 'axios';
import OverviewItem from '../components/layout/OverviewItem';

interface Listing {
  name: string;
  price: string;
  imageUrl: string;
  pointCost?: number;
  altText?: string;
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
  const [cart, setCart] = useState<Listing[]>([]);
  const [isCartOpen, setIsCartOpen] = useState<boolean>(false);
  const [sponsorPoints, setSponsorPoints] = useState<SponsorPoints[]>([]);
  const [selectedSponsorId, setSelectedSponsorId] = useState<number | ''>('');
  const [snackbarMessage, setSnackbarMessage] = useState('');
  const [showSnackbar, setShowSnackbar] = useState(false);

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

  const addToCart = (item: Listing) => setCart((prevCart) => [...prevCart, item]);
  const openCart = () => setIsCartOpen(true);
  const closeCart = () => setIsCartOpen(false);
  const clearCart = () => setCart([]);

  const fetchSponsorPoints = async () => {
    try {
      const response = await axios.get<SponsorPoints[]>('/api/driver/my-sponsors');
      setSponsorPoints(response.data);
      if (response.data.length > 0) setSelectedSponsorId(response.data[0].sponsorId);
    } catch (err) {
      setError('Failed to load sponsor points data');
      console.error('Error fetching sponsor points:', err);
    }
  };

  const fetchListings = async () => {
    try {
      const response: AxiosResponse<Listing[]> = await axios.get('/api/catalog/products');
      const listingsWithPoints = response.data.map((listing) => ({
        ...listing,
        pointCost: calculatePointCost(listing.price),
      }));
      setListings(listingsWithPoints);
      localStorage.setItem(
        CACHE_KEY,
        JSON.stringify({ listings: listingsWithPoints, timestamp: Date.now() })
      );
    } catch (error: any) {
      setError(error.response?.data || 'Failed to load products.');
    } finally {
      setLoading(false);
    }
  };

  const calculatePointCost = (price: string): number => {
    const selectedSponsor = sponsorPoints.find((s) => s.sponsorId === selectedSponsorId);
    if (!selectedSponsor) return 0;

    const numericPrice = parseFloat(price.split(' ')[0]);
    return Math.ceil(numericPrice / selectedSponsor.pointDollarValue);
  };

  const handlePurchase = (listing: Listing) => {
    const selectedSponsor = sponsorPoints.find((s) => s.sponsorId === selectedSponsorId);
    if (!selectedSponsor || !listing.pointCost) return;

    if (selectedSponsor.totalPoints < listing.pointCost) {
      setSnackbarMessage(
        `Not enough points! You need ${listing.pointCost} points but have ${selectedSponsor.totalPoints}`
      );
    }
    setShowSnackbar(true);
  };

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;

  const selectedSponsor = sponsorPoints.find((s) => s.sponsorId === selectedSponsorId);

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
            You have {selectedSponsor.totalPoints.toLocaleString()} points available with{' '}
            {selectedSponsor.sponsorName}. Point Value: $
            {selectedSponsor.pointDollarValue.toFixed(2)} per point.
          </Alert>
        )}

        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, justifyContent: 'flex-start' }}>
          {listings.map((listing, index) => (
            <ProductCard
              key={index}
              listing={listing}
              selectedSponsor={selectedSponsor}
              onPurchase={() => {
                handlePurchase(listing);
                addToCart(listing);
              }}
            />
          ))}
        </Box>

        <Snackbar
          open={showSnackbar}
          autoHideDuration={3000}
          onClose={() => setShowSnackbar(false)}
          message={snackbarMessage}
        />

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
                  <ListItemText primary={item.name} secondary={`Price: ${item.price}`} />
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

// Reusable ProductCard Component
const ProductCard: React.FC<{
  listing: Listing;
  selectedSponsor: SponsorPoints | undefined;
  onPurchase: () => void;
}> = ({ listing, selectedSponsor, onPurchase }) => (
  <Card
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
      alt={listing.altText || `Image of ${listing.name}`}
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
      <OverviewItem title="Point Cost" value={`${listing.pointCost?.toLocaleString() || 0} points`} />
      <Button
        fullWidth
        variant="contained"
        color="primary"
        onClick={onPurchase}
        disabled={
          !selectedSponsor || (listing.pointCost || 0) > (selectedSponsor?.totalPoints || 0)
        }
        sx={{ mt: 2 }}
      >
        Add to Cart
      </Button>
    </CardContent>
  </Card>
);

export default ProductCatalog;
