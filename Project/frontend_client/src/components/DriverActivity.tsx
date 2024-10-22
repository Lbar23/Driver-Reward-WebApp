// Technically speaking, each of the users' components for the dashboard can be like, in a single component tsx file...
// Like how the library dependencies when importing { foo, foo2, foo3 } do it as well...
// Too tired to do it right now though; that's a Sprint 9 me problem lmao

import React, { useState, useEffect } from 'react';
import {
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
  LinearProgress,
  Card,
  CardContent,
  Chip
} from '@mui/material';
import axios from 'axios';

interface Transaction {
  date: string;
  points: number;
  type: string;
  reason: string;
  sponsorName?: string;
  status?: string;
}

interface PointValue {
  totalPoints: number;
  pointValue: number;
  totalValue: number;
  sponsorName: string;
}

interface DriverActivity {
  pointValue: PointValue;
  transactions: Transaction[];
}

const DriverActivity: React.FC = () => {
  const [data, setData] = useState<DriverActivity | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchActivity = async () => {
      try {
        setLoading(true);
        const response = await axios.get('/api/driver/activity');
        setData(response.data);
      } catch (err) {
        setError('Failed to load driver activity');
        console.error('Error fetching activity:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchActivity();
  }, []);

  if (loading) return <LinearProgress />;
  if (error) return <Typography color="error">{error}</Typography>;
  if (!data) return null;

  return (
    <Box sx={{ p: 3 }}>
      {/* Points Value Card */}
      <Card sx={{ mb: 4 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Points Value - {data.pointValue.sponsorName}
          </Typography>
          <Typography>
            Current Balance: {data.pointValue.totalPoints.toLocaleString()} points
          </Typography>
          <Typography>
            Point Value: ${data.pointValue.pointValue.toFixed(2)} each
          </Typography>
          <Typography variant="h5" sx={{ mt: 2 }}>
            Total Value: ${data.pointValue.totalValue.toFixed(2)}
          </Typography>
        </CardContent>
      </Card>

      {/* Transactions Table */}
      <Typography variant="h6" gutterBottom>
        Activity History
      </Typography>
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Date</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>Points</TableCell>
              <TableCell>Details</TableCell>
              <TableCell>Status</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data.transactions.map((transaction, index) => (
              <TableRow key={index}>
                <TableCell>
                  {new Date(transaction.date).toLocaleDateString()}
                </TableCell>
                <TableCell>{transaction.type}</TableCell>
                <TableCell sx={{ 
                  color: transaction.points > 0 ? 'success.main' : 'error.main' 
                }}>
                  {transaction.points > 0 ? '+' : ''}{transaction.points}
                </TableCell>
                <TableCell>{transaction.reason}</TableCell>
                <TableCell>
                  {transaction.status && (
                    <Chip 
                      label={transaction.status} 
                      color={transaction.status === 'Ordered' ? 'success' : 
                             transaction.status === 'Cancelled' ? 'error' : 'default'}
                      size="small"
                    />
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};

export default DriverActivity;