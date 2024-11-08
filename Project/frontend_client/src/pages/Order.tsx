import React, { useState } from 'react';
import { Container, Grid, Box, Typography, TextField, IconButton, Button, Divider, Dialog, DialogTitle, DialogContent, DialogActions } from '@mui/material';
import { Remove, Add } from '@mui/icons-material';

const Order: React.FC = () => {

    const [orderCompleted, setOrderCompleted] = useState(false);

    const handleCompleteOrder = () => {
        setOrderCompleted(true);
      };
    
      const handleCloseDialog = () => {
        setOrderCompleted(false);
      };
  return (
    <Container maxWidth="md">
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

        {/* Order Summary - get items from dbs*/}
        <Grid item xs={12} sm={5}>
          <Box>
            <Typography variant="h6">Order Summary</Typography>
            <Grid container spacing={2} mt={2} alignItems="center">
              {/* Example Product Item */}
              <Grid item xs={8}>
                <Typography variant="subtitle1">Bath Mat</Typography>
                <Typography variant="body2">Size: 61x43cm, Color: White</Typography>
              </Grid>
              <Grid item xs={4} textAlign="right">
                <IconButton size="small"><Remove /></IconButton>
                1
                <IconButton size="small"><Add /></IconButton>
              </Grid>
              <Grid item xs={12}><Divider /></Grid>

              {/* Additional Product Example */}
              <Grid item xs={8}>
                <Typography variant="subtitle1">T-Shirt</Typography>
                <Typography variant="body2">Size: L, Color: White</Typography>
              </Grid>
              <Grid item xs={4} textAlign="right">
                <IconButton size="small"><Remove /></IconButton>
                1
                <IconButton size="small"><Add /></IconButton>
              </Grid>
              <Grid item xs={12}><Divider /></Grid>
            </Grid>

            {/* Total Summary */}
            <Box mt={2} textAlign="right">
              <Typography variant="h6">Total (2 items): $69.46</Typography> {/* price should be obtained from dbs and converted to points*/}
              <Typography variant="h6">Current Points: 6946</Typography> {/* Get points from dbs*/}
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
          <Typography>Your order has been successfully completed! Remaining points: 0</Typography> {/* connect points from dbs */}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog} color="primary">Close</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default Order;
