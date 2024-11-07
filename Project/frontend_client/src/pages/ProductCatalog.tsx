import React, { useState, useEffect } from 'react';
import { Container, Typography, Card, CardMedia, CardContent, CircularProgress, Alert, Box } from '@mui/material';
import axios, { AxiosResponse } from 'axios';
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
              <OverviewItem 
                title="Point Cost" 
                value={`${listing.pointCost?.toLocaleString() || 0} points`} 
              />
              <Button
                fullWidth
                variant="contained"
                color="primary"
                onClick={() => handlePurchase(listing)}
                disabled={!selectedSponsor || (listing.pointCost || 0) > selectedSponsor.totalPoints}
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