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
  Button,
  Alert,
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import RefreshIcon from '@mui/icons-material/Refresh';
import DownloadIcon from '@mui/icons-material/Download';
import { format } from 'date-fns';
import axios from 'axios';

// Types
export enum AuditLogCategory {
  User = 'User',
  Point = 'Point',
  Purchase = 'Purchase',
  Application = 'Application',
  Product = 'Product',
  System = 'System'
}

interface AuditLog {
  logID: number;
  timestamp: string;
  category: AuditLogCategory;
  userID: number | null;
  userName: string | null;
  description: string;
}

interface AuditLogResponse {
  totalCount: number;
  page: number;
  pageSize: number;
  logs: AuditLog[];
}

interface AuditLogFilter {
  userID?: number;
  category?: AuditLogCategory;
  startDate?: Date;
  endDate?: Date;
  page: number;
  pageSize: number;
}

const AuditLogDashboard: React.FC = () => {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<AuditLogFilter>({
    page: 0,
    pageSize: 10,
  });

  const fetchLogs = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Build query parameters
      const params = new URLSearchParams();
      if (filter.userID) params.append('userId', filter.userID.toString());
      if (filter.category) params.append('category', filter.category);
      if (filter.startDate) params.append('startDate', filter.startDate.toISOString());
      if (filter.endDate) params.append('endDate', filter.endDate.toISOString());
      params.append('page', (filter.page + 1).toString());
      params.append('pageSize', filter.pageSize.toString());

      const response = await axios.get<AuditLogResponse>(`/api/reports/audit-logs?${params.toString()}`);
      setLogs(response.data.logs);
      setTotalCount(response.data.totalCount);
    } catch (err) {
      setError('Failed to fetch audit logs');
      console.error('Error fetching audit logs:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleExport = async () => {
    try {
      const params = new URLSearchParams();
      if (filter.startDate) params.append('startDate', filter.startDate.toISOString());
      if (filter.endDate) params.append('endDate', filter.endDate.toISOString());
      if (filter.category) params.append('category', filter.category);

      const response = await axios.get(`/api/reports/audit-logs/export?${params.toString()}`, {
        responseType: 'blob'
      });

      // Create download link
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `audit_logs_${format(new Date(), 'yyyyMMdd')}.csv`);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (err) {
      setError('Failed to export audit logs');
      console.error('Error exporting audit logs:', err);
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
    const category = event.target.value as AuditLogCategory | '';
    setFilter(prev => ({
      ...prev,
      category: category || undefined,
      page: 0,
    }));
  };

  const getCategoryColor = (category: AuditLogCategory): "default" | "primary" | "secondary" | "error" | "info" | "success" | "warning" => {
    const colors: Record<AuditLogCategory, "default" | "primary" | "secondary" | "error" | "info" | "success" | "warning"> = {
      [AuditLogCategory.User]: 'primary',
      [AuditLogCategory.Point]: 'success',
      [AuditLogCategory.Purchase]: 'info',
      [AuditLogCategory.Application]: 'warning',
      [AuditLogCategory.Product]: 'secondary',
      [AuditLogCategory.System]: 'error'
    };
    return colors[category] || 'default';
  };

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns}>
      <Box sx={{ p: 3 }}>
        <Grid container spacing={3}>
          <Grid item xs={12} sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h4">
              Audit Log Dashboard
            </Typography>
            <Button
              variant="contained"
              startIcon={<DownloadIcon />}
              onClick={handleExport}
              disabled={loading}
            >
              Export CSV
            </Button>
          </Grid>
          
          {error && (
            <Grid item xs={12}>
              <Alert severity="error">{error}</Alert>
            </Grid>
          )}
          
          {/* Filters */}
          <Grid item xs={12}>
            <Paper sx={{ p: 2, mb: 2 }}>
              <Grid container spacing={2} alignItems="center">
                <Grid item xs={12} sm={6} md={2}>
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
                <Grid item xs={12} sm={6} md={2}>
                  <FormControl fullWidth>
                    <InputLabel>Category</InputLabel>
                    <Select
                      value={filter.category || ''}
                      label="Category"
                      onChange={handleCategoryChange}
                    >
                      <MenuItem value="">All</MenuItem>
                      {Object.values(AuditLogCategory).map((category) => (
                        <MenuItem key={category} value={category}>
                          {category}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={12} sm={6} md={2}>
                  <DatePicker
                    label="Start Date"
                    value={filter.startDate}
                    onChange={(date) => setFilter(prev => ({
                      ...prev,
                      startDate: date || undefined,
                      page: 0,
                    }))}
                    slotProps={{ textField: { fullWidth: true } }}
                  />
                </Grid>
                <Grid item xs={12} sm={6} md={2}>
                  <DatePicker
                    label="End Date"
                    value={filter.endDate}
                    onChange={(date) => setFilter(prev => ({
                      ...prev,
                      endDate: date || undefined,
                      page: 0,
                    }))}
                    slotProps={{ textField: { fullWidth: true } }}
                  />
                </Grid>
                <Grid item xs={12} sm={6} md={1}>
                  <IconButton onClick={() => fetchLogs()} disabled={loading}>
                    <RefreshIcon />
                  </IconButton>
                </Grid>
              </Grid>
            </Paper>
          </Grid>

          {/* Logs Table */}
          <Grid item xs={12}>
            <Paper>
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Log ID</TableCell>
                      <TableCell>Timestamp</TableCell>
                      <TableCell>User</TableCell>
                      <TableCell>Category</TableCell>
                      <TableCell>Description</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {loading ? (
                      <TableRow>
                        <TableCell colSpan={5} align="center">Loading...</TableCell>
                      </TableRow>
                    ) : logs.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={5} align="center">No audit logs found</TableCell>
                      </TableRow>
                    ) : (
                      logs.map((log) => (
                        <TableRow key={log.logID}>
                          <TableCell>{log.logID}</TableCell>
                          <TableCell>{format(new Date(log.timestamp), 'MMM dd, yyyy HH:mm:ss')}</TableCell>
                          <TableCell>{log.userName || log.userID || 'N/A'}</TableCell>
                          <TableCell>
                            <Chip
                              label={log.category}
                              size="small"
                              color={getCategoryColor(log.category)}
                            />
                          </TableCell>
                          <TableCell>{log.description}</TableCell>
                        </TableRow>
                      ))
                    )}
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
    </LocalizationProvider>
  );
};

export default AuditLogDashboard;