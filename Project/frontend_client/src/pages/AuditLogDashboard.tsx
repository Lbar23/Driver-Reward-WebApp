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

interface AuditLogResponse {
  logs: AuditLog[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
}

const AuditLogDashboard: React.FC = () => {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [filter, setFilter] = useState<AuditLogFilter>({
    page: 0,
    pageSize: 10,
  });

  const fetchLogs = async () => {
    try {
      setLoading(true);
      const queryParams = new URLSearchParams();
      
      if (filter.userID) queryParams.append('userID', filter.userID.toString());
      if (filter.category !== undefined) queryParams.append('category', filter.category.toString());
      if (filter.searchTerm) queryParams.append('searchTerm', filter.searchTerm);
      queryParams.append('page', (filter.page + 1).toString());
      queryParams.append('pageSize', filter.pageSize.toString());

      const response = await fetch(`/api/auditlog?${queryParams.toString()}`);
      const data: AuditLogResponse = await response.json();
      
      setLogs(data.logs);
      setTotalCount(data.totalCount);
    } catch (error) {
      console.error('Error fetching audit logs:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchLogs();
  }, [filter]);

  const handlePageChange = (event: unknown, newPage: number) => {
    setFilter(prev => ({ ...prev, page: newPage }));
  };

  const handleRowsPerPageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilter(prev => ({
      ...prev,
      pageSize: parseInt(event.target.value, 10),
      page: 0,
    }));
  };

  const handleCategoryChange = (event: SelectChangeEvent) => {
    const category = event.target.value === '' ? undefined : parseInt(event.target.value);
    setFilter(prev => ({
      ...prev,
      category: category,
      page: 0,
    }));
  };

  const getCategoryName = (category: number) => {
    return AuditLogCategory[category] || 'Unknown';
  };

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
                  onChange={(e) => setFilter(prev => ({
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
                  onChange={(e) => setFilter(prev => ({
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
              <IconButton onClick={() => fetchLogs()} disabled={loading}>
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
                  {logs.map((log) => (
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
              count={totalCount}
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