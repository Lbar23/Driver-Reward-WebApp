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
  Chip,
} from '@mui/material';
import axios from 'axios';

interface Transaction {
  date: string;
  points: number;
  type: string;
  reason: string;
  sponsorName?: string;
  status: string;
}

interface PointValue {
  totalPoints: number;
  pointValue: number;
  totalValue: number;
  sponsorName?: string;
}

interface DriverActivity {
  pointValue?: PointValue;
  transactions?: Transaction[];
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
      {data.pointValue && (
        <Card sx={{ mb: 4 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Points Value - {data.pointValue.sponsorName || 'Unknown Sponsor'}
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
      )}

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
            {data.transactions && data.transactions.length > 0 ? (
              data.transactions.map((transaction, index) => (
                <TableRow key={index}>
                  <TableCell>
                    {transaction.date ? new Date(transaction.date).toLocaleDateString() : 'N/A'}
                  </TableCell>
                  <TableCell>{transaction.type}</TableCell>
                  <TableCell
                    sx={{
                      color: transaction.points > 0 ? 'success.main' : 'error.main',
                    }}
                  >
                    {transaction.points > 0 ? '+' : ''}
                    {transaction.points}
                  </TableCell>
                  <TableCell>{transaction.reason}</TableCell>
                  <TableCell>
                      <Chip
                      label={'Ordered'}
                      color={
                        transaction.status == 'Ordered'
                          ? 'success'
                          : transaction.status == 'Cancelled'
                          ? 'error'
                          : 'default'
                      }
                      size="small"
                    />
                  </TableCell>
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell colSpan={5} align="center">
                  No transactions available
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};

export default DriverActivity;
