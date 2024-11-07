import React, { useState, useEffect } from 'react';
import { Container, Typography, Card, CardMedia, CardContent, CircularProgress, Alert, Box, TextField, FormControlLabel, Checkbox, Button } from '@mui/material';
import axios, { AxiosResponse } from 'axios';
import OverviewItem from '../components/layout/OverviewItem';

interface Listing {
  name: string;
  price: string;
  imageUrl: string;
  isOutOfStock?: boolean;
  pointCost?: number;
}

const CACHE_KEY = 'productCatalog';
const CACHE_EXPIRY = 10 * 60 * 1000; // 10 minutes in milliseconds

const ProductCatalog: React.FC = () => {
  const [listings, setListings] = useState<Listing[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchListings = async () => {
    try {
      const response: AxiosResponse<Listing[]> = await axios.get(`/api/catalog/products`);
      const fetchedListings = response.data.map(listing => ({
        ...listing,
        isOutOfStock: listing.isOutOfStock || false,
      }));
      setListings(fetchedListings);
      setLoading(false);

      // Cache data and timestamp
      localStorage.setItem(CACHE_KEY, JSON.stringify({ listings: fetchedListings, timestamp: Date.now() }));
    } catch (error: any) {
      setError(error.response?.data || 'Failed to load products.');
      setLoading(false);
    }
  };

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

    // Fetch fresh data if no valid cache is available
    fetchListings();
  }, []);

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;

  const selectedSponsor = sponsorPoints.find(s => s.sponsorId === selectedSponsorId);

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
              {/* Editable Price Input */}
              <TextField
                label="Price"
                variant="outlined"
                value={listing.price}
                onChange={(e) => handlePriceChange(index, e.target.value)}
                fullWidth
                margin="dense"
              />
              {/* Out of Stock Checkbox */}
              <FormControlLabel
                control={
                  <Checkbox
                    checked={listing.isOutOfStock || false}
                    onChange={(e) => handleStockStatusChange(index, e.target.checked)}
                  />
                }
                label="Out of Stock"
              />
              {/* Displayed Overview Items */}
              <OverviewItem title="Price" value={listing.price} />
              <OverviewItem 
                title="Point Cost" 
                value={`${listing.pointCost?.toLocaleString() || 0} points`} 
              />
              {/* Redeem with Points Button */}
              <Button
                fullWidth
                variant="contained"
                color="primary"
                onClick={() => handlePurchase(listing)}
                disabled={!selectedSponsor || (listing.pointCost || 0) > selectedSponsor.totalPoints || listing.isOutOfStock}
                sx={{ mt: 2 }}
              >
                Redeem with Points
              </Button>
            </CardContent>
          </Card>
        ))}
      </Box>
    </Container>
  );
};

export default ProductCatalog;
