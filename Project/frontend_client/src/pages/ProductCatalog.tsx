import React, { useState, useEffect } from 'react';
import { Container, Typography, Grid2, Card, CardMedia, CardContent, CircularProgress, Alert, Link } from '@mui/material';
import axios from 'axios';
import OverviewItem from '../components/layout/OverviewItem'; 

interface Listing {
  listing_id: number;
  title: string;
  price: string;
  currency_code: string;
  url: string;
  image_url: string; 
}

const ProductCatalog: React.FC = () => {
  const [listings, setListings] = useState<Listing[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchListings = async () => {
      try {
        const response = await axios.get(`/api/catalog/products`);
        setListings(response.data.results || []); 
      } catch (error: any) {
        setError(error.response?.data || 'Failed to load products.');
      } finally {
        setLoading(false);
      }
    };
    
    fetchListings();
  }, []);

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;

  return (
    <Container>
      <Typography variant="h4" component="h1" gutterBottom>
        Product Catalog
      </Typography>
      <Grid2 container spacing={3}>
        {listings.map((listing) => (
          <Grid2 key={listing.listing_id}>
            <Card>
              <CardMedia
                component="img"
                height="200"
                image={listing.image_url || '/placeholder.jpg'} // Fallback image if no URL
                alt={listing.title}/>
              <CardContent>
                <OverviewItem title="Title" value={listing.title} />
                <OverviewItem title="Price" value={`${listing.currency_code} ${listing.price}`} />
                <Typography variant="body2" color="primary">
                  <Link href={listing.url} target="_blank" rel="noopener noreferrer">
                    View on Etsy
                  </Link>
                </Typography>
              </CardContent>
            </Card>
          </Grid2>
        ))}
      </Grid2>
    </Container>
  );
};

export default ProductCatalog;
