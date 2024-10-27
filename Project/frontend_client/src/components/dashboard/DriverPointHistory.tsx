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
} from '@mui/material';
import { useParams } from 'react-router-dom';
import axios from 'axios';

interface Transaction {
  transactionId: number;
  pointsChanged: number;
  reason: string;
  transactionDate: string;
}

const DriverPointsList: React.FC = () => {
  const { driverId } = useParams<{ driverId: string }>();
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchTransactions = async () => {
      try {
        setLoading(true);
        const response = await axios.get(`/api/drivers/${driverId}/transactions`);
        setTransactions(response.data);
        setError('');
      } catch (err) {
        setError('Failed to load transaction data');
        console.error('Error fetching transactions:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchTransactions();
  }, [driverId]);

  if (loading) {
    return (
      <Box sx={{ width: '100%', mt: 2 }}>
        <LinearProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Typography color="error" sx={{ mt: 2 }}>
        {error}
      </Typography>
    );
  }

  return (
    <Paper sx={{ width: '100%', mb: 2, mt: 2 }}>
      <TableContainer>
        <Table aria-label="driver transactions table">
          <TableHead>
            <TableRow>
              <TableCell>Date</TableCell>
              <TableCell>Points Changed</TableCell>
              <TableCell>Reason</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {transactions.map((transaction) => (
              <TableRow key={transaction.transactionId}>
                <TableCell>{new Date(transaction.transactionDate).toLocaleDateString()}</TableCell>
                <TableCell>{transaction.pointsChanged}</TableCell>
                <TableCell>{transaction.reason}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Paper>
  );
};

export default DriverPointsList;
