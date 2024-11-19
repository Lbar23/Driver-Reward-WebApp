import React, { useState } from 'react';
import axios from 'axios';
import { Container, Grid, Box, Typography, TextField, Button, Dialog, DialogTitle, DialogContent, DialogActions } from '@mui/material';
import { useLocation, useNavigate } from 'react-router-dom';

interface CartItem {
  productID: number;
  productId?: number;
  name: string;
  price: string;
  pointCost?: number;
}

const Order: React.FC = () => {

    const [orderCompleted, setOrderCompleted] = useState(false);
    const [error, setError] = useState<string | null>(null);
    
      const handleCloseDialog = () => {
        setOrderCompleted(false);
        navigate('/catalog');
      };      

    const location = useLocation<{ cartItems: CartItem[]; sponsorId: number; points: number; pointValue: number }>();
    const { cartItems, sponsorId, points, pointValue } = location.state || {};
    const navigate = useNavigate();

    const calculateCartTotalPoints = (): number => {
      return cartItems?.reduce((sum, item) => {
        const numericPrice = parseFloat(item.price.split(' ')[0]); // Assuming `price` is a string like "$100"
        return sum + Math.ceil(numericPrice / pointValue);
      }, 0) || 0; // Default to 0 if cartItems is undefined
    };

    const handleCompleteOrder = async () => {
      try {
        // Create a purchase request for each item
        const purchasePromises = cartItems.map(item => {
          const pointsForItem = Math.ceil(parseFloat(item.price.split(' ')[0]) / pointValue);
          
          return axios.post('/api/driver/purchase', {
            SponsorID: sponsorId,
            ProductID: item.productID || item.productId, // Use productId from cart item
            PointsSpent: pointsForItem
          });
        });

        await Promise.all(purchasePromises);
        setOrderCompleted(true);
      } catch (err: any) {
        console.error('Purchase error:', err);
        const errorMessage = err.response?.data?.message || 'Failed to complete order';
        alert(errorMessage);
      }
    };

  return (
    <Container maxWidth="md">
      <Button variant="outlined" onClick={() => navigate('/catalog')} sx={{ mb: 2 }}>
        Back
      </Button>
      <Typography variant="h4" align="center" gutterBottom>
        Secure Checkout
      </Typography>
      <Grid container spacing={4}>
        {/* Delivery Address Form */}
        <Grid item xs={12} sm={7}>
          <Box>
            <Typography variant="h6">1. Delivery Address</Typography>
            <Typography variant="body2">All fields marked with * are required</Typography>
            <Grid container spacing={2} mt={2}> {/*fill these out with dbs stuff from the driver user*/}
              <Grid item xs={12}>
                <TextField label="Email address *" fullWidth variant="outlined" />
              </Grid>
              <Grid item xs={6}>
                <TextField label="First name *" fullWidth variant="outlined" />
              </Grid>
              <Grid item xs={6}>
                <TextField label="Last name *" fullWidth variant="outlined" />
              </Grid>
              <Grid item xs={12}>
                <TextField label="Address *" fullWidth variant="outlined" />
              </Grid>
              <Grid item xs={6}>
                <TextField label="City *" fullWidth variant="outlined" />
              </Grid>
              <Grid item xs={6}>
                <TextField label="ZIP code *" fullWidth variant="outlined" />
              </Grid>
              <Grid item xs={12}>
                <TextField label="Country *" fullWidth variant="outlined" defaultValue="United States" />
              </Grid>
            </Grid>
          </Box>
        </Grid>

        {/* Order Summary - get items from catalog*/}
        <Grid item xs={12} sm={5}>
          <Box>
            <Typography variant="h6">Order Summary</Typography>
            <Grid container spacing={2} mt={2} alignItems="center">
            {cartItems && cartItems.map((item, index) => (
              <Grid container key={index}>
                <Grid item xs={8}>
                  <Typography variant="subtitle1">{item.name}</Typography>
                </Grid>
                <Grid item xs={4} textAlign="right">
                  <Typography>{Math.ceil(parseFloat(item.price.split(' ')[0]) / pointValue)} points</Typography>
                </Grid>
              </Grid>
            ))}
            </Grid>

            {/* Total Summary */}
            <Box mt={2} textAlign="right">
              <Typography variant="h6">Total: {calculateCartTotalPoints()} points</Typography> 
              <Typography variant="h6">Current Points: {points}</Typography>
              <Button 
                variant="contained" 
                color="primary" 
                fullWidth 
                onClick={handleCompleteOrder} 
                sx={{ mt: 2 }}
              >
                Complete Order
              </Button>
            </Box>
          </Box>
        </Grid>
      </Grid>

      {/* Order Completion Dialog */}
      <Dialog open={orderCompleted} onClose={handleCloseDialog}>
        <DialogTitle>Order Complete</DialogTitle>
        <DialogContent>
          <Typography>Your order has been successfully completed! Remaining points: {points - calculateCartTotalPoints()}</Typography> {/* connect points from dbs */}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog} color="primary">Close</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default Order;
