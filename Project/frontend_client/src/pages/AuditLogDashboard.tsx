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
  TablePagination,
  TextField,
  MenuItem,
  FormControl,
  InputLabel,
  Select,
  Grid,
  Typography,
  IconButton,
  Chip,
  SelectChangeEvent,
} from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import { format } from 'date-fns';

// Types
interface AuditLog {
  logID: number;
  userID: number;
  category: number;
  description: string;
  timestamp: string;
}

const AuditLogCategory = {
  1: 'User',
  2: 'Point',
  3: 'Purchase',
  4: 'Application',
  5: 'Product',
  6: 'System',
};

interface AuditLogFilter {
  userID?: number;
  category?: number;
  searchTerm?: string;
  page: number;
  pageSize: number;
}

const mockLogs: AuditLog[] = [
  { logID: 1, userID: 1, category: 1, description: 'Admin User logged in', timestamp: '2024-11-06T14:23:30Z' },
  { logID: 2, userID: 10, category: 2, description: 'Points added to DriverTR1', timestamp: '2024-11-06T14:50:00Z' },
  { logID: 3, userID: 10, category: 3, description: 'DriverTR1 purchased glasses', timestamp: '2024-11-06T15:10:00Z' },
  { logID: 4, userID: 10, category: 4, description: 'Applied for sponsorship with Speedy Stats', timestamp: '2024-11-06T15:10:00Z' },
  { logID: 5, userID: 1, category: 5, description: 'New Product Added to catalog', timestamp: '2024-11-06T15:10:00Z' },
  { logID: 6, userID: 1, category: 6, description: 'System back up complete', timestamp: '2024-11-06T15:10:00Z' }

];

const AuditLogDashboard: React.FC = () => {
  const [logs, setLogs] = useState<AuditLog[]>(mockLogs);
  const [totalCount, setTotalCount] = useState(mockLogs.length);
  const [filter, setFilter] = useState<AuditLogFilter>({
    page: 0,
    pageSize: 10,
  });

  const handlePageChange = (event: unknown, newPage: number) => {
    setFilter((prev) => ({ ...prev, page: newPage }));
  };

  const handleRowsPerPageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilter((prev) => ({
      ...prev,
      pageSize: parseInt(event.target.value, 10),
      page: 0,
    }));
  };

  const handleCategoryChange = (event: SelectChangeEvent) => {
    const category = event.target.value === '' ? undefined : parseInt(event.target.value);
    setFilter((prev) => ({
      ...prev,
      category,
      page: 0,
    }));
  };

  const getCategoryName = (category: number) => {
    return AuditLogCategory[category] || 'Unknown';
  };

  const filteredLogs = logs.filter((log) => {
    const matchesUser = filter.userID ? log.userID === filter.userID : true;
    const matchesCategory = filter.category ? log.category === filter.category : true;
    const matchesSearch = filter.searchTerm ? log.description.toLowerCase().includes(filter.searchTerm.toLowerCase()) : true;
    return matchesUser && matchesCategory && matchesSearch;
  });

  const paginatedLogs = filteredLogs.slice(filter.page * filter.pageSize, (filter.page + 1) * filter.pageSize);

  return (
    <Box sx={{ p: 3 }}>
      <Grid container spacing={3}>
        <Grid item xs={12}>
          <Typography variant="h4" gutterBottom>
            Audit Log Dashboard
          </Typography>
        </Grid>
        
        {/* Filters */}
        <Grid item xs={12}>
          <Paper sx={{ p: 2, mb: 2 }}>
            <Grid container spacing={2} alignItems="center">
              <Grid item xs={12} sm={6} md={3}>
                <TextField
                  fullWidth
                  label="User ID"
                  type="number"
                  value={filter.userID || ''}
                  onChange={(e) => setFilter((prev) => ({
                    ...prev,
                    userID: e.target.value ? parseInt(e.target.value) : undefined,
                    page: 0,
                  }))}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth>
                  <InputLabel>Category</InputLabel>
                  <Select
                    value={filter.category?.toString() || ''}
                    label="Category"
                    onChange={handleCategoryChange}
                  >
                    <MenuItem value="">All</MenuItem>
                    {Object.keys(AuditLogCategory).map((key) => (
                      <MenuItem key={key} value={key}>
                        {AuditLogCategory[parseInt(key)]}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Search Description"
                  value={filter.searchTerm || ''}
                  onChange={(e) => setFilter((prev) => ({
                    ...prev,
                    searchTerm: e.target.value || undefined,
                    page: 0,
                  }))}
                />
              </Grid>
            </Grid>
          </Paper>
        </Grid>

        {/* Logs Table */}
        <Grid item xs={12}>
          <Paper>
            <Box sx={{ display: 'flex', justifyContent: 'flex-end', p: 2 }}>
              <IconButton onClick={() => setLogs([...mockLogs])}>
                <RefreshIcon />
              </IconButton>
            </Box>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Log ID</TableCell>
                    <TableCell>User ID</TableCell>
                    <TableCell>Category</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell>Timestamp</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {paginatedLogs.map((log) => (
                    <TableRow key={log.logID}>
                      <TableCell>{log.logID}</TableCell>
                      <TableCell>{log.userID}</TableCell>
                      <TableCell>
                        <Chip
                          label={getCategoryName(log.category)}
                          size="small"
                          color={getCategoryColor(log.category)}
                        />
                      </TableCell>
                      <TableCell>{log.description}</TableCell>
                      <TableCell>
                        {format(new Date(log.timestamp), 'MMM dd, yyyy HH:mm:ss')}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
            <TablePagination
              component="div"
              count={filteredLogs.length}
              page={filter.page}
              onPageChange={handlePageChange}
              rowsPerPage={filter.pageSize}
              onRowsPerPageChange={handleRowsPerPageChange}
              rowsPerPageOptions={[5, 10, 25, 50]}
            />
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

const getCategoryColor = (category: number) => {
  const colors: Record<number, string> = {
    0: 'primary',
    1: 'secondary',
    2: 'success',
    3: 'info',
    4: 'warning',
    5: 'error',
  };
  return colors[category] || 'default';
};

export default AuditLogDashboard;
