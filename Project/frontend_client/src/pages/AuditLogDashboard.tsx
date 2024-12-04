import React, { useState, useCallback } from 'react';
import {
  Box,
  Paper,
  Grid,
  Typography,
  IconButton,
  Chip,
  SelectChangeEvent,
  Button,
  Alert,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import RefreshIcon from '@mui/icons-material/Refresh';
import DownloadIcon from '@mui/icons-material/Download';
import { format } from 'date-fns';
import axios from 'axios';
import TableComponent, { TableColumn } from '../components/layout/DataTable';

// Types based on AuditLogs.cs
export enum AuditLogCategory {
  User = 'User',
  Password = 'Password',
  Authentication = 'Authentication'
}

export enum AuditLogAction {
  Add = 'Add',
  Remove = 'Remove',
  Update = 'Update'
}

interface AuditLog {
  logID: number;
  userID: number;
  userName?: string;
  category: AuditLogCategory;
  action: AuditLogAction;
  actionSuccess: boolean;
  timestamp: string;
  additionalDetails?: string;
}

interface AuditLogFilter {
  userID?: number;
  category?: AuditLogCategory;
  startDate?: Date;
  endDate?: Date;
}

const AuditLogDashboard: React.FC = () => {
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [filter, setFilter] = useState<AuditLogFilter>({});

  // Table columns configuration
  const columns: TableColumn<AuditLog>[] = [
    { 
      id: 'logID', 
      label: 'Log ID' 
    },
    { 
      id: 'timestamp', 
      label: 'Timestamp',
      render: (row) => format(new Date(row.timestamp), 'MMM dd, yyyy HH:mm:ss')
    },
    { 
      id: 'userName', 
      label: 'User',
      render: (row) => row.userName || row.userID || 'N/A'
    },
    { 
      id: 'category', 
      label: 'Category',
      render: (row) => (
        <Chip
          label={row.category}
          size="small"
          color={getCategoryColor(row.category)}
        />
      )
    },
    { 
      id: 'action', 
      label: 'Action',
      render: (row) => (
        <Chip
          label={row.action}
          size="small"
          color={getCategoryColor(row.category)}
        />
      )
    },
    { 
      id: 'actionSuccess', 
      label: 'Status',
      render: (row) => (
        <Chip
          label={row.actionSuccess ? 'Success' : 'Failed'}
          size="small"
          color={row.actionSuccess ? 'success' : 'error'}
        />
      )
    },
    { 
      id: 'additionalDetails', 
      label: 'Additional Details',
      render: (row) => {
        if (!row.additionalDetails) return 'N/A';
        try {
          const details = JSON.parse(row.additionalDetails);
          return JSON.stringify(details, null, 2);
        } catch {
          return row.additionalDetails;
        }
      }
    }
  ];

  const getCategoryColor = (category: AuditLogCategory): "default" | "primary" | "secondary" | "error" | "info" | "success" | "warning" => {
    const colors: Record<AuditLogCategory, "default" | "primary" | "secondary" | "error" | "info" | "success" | "warning"> = {
      [AuditLogCategory.User]: 'primary',
      [AuditLogCategory.Password]: 'warning',
      [AuditLogCategory.Authentication]: 'info'
    };
    return colors[category] || 'default';
  };

  // Fetch data function for the DataTable component
  const fetchTableData = useCallback(async (page: number, pageSize: number) => {
    try {
      setLoading(true);
      setError(null);
      
      const params = new URLSearchParams();
      if (filter.userID) params.append('userId', filter.userID.toString());
      if (filter.category) params.append('category', filter.category);
      if (filter.startDate) params.append('startDate', filter.startDate.toISOString());
      if (filter.endDate) params.append('endDate', filter.endDate.toISOString());
      params.append('page', page.toString());
      params.append('pageSize', pageSize.toString());

      const response = await axios.get<{
        totalCount: number;
        page: number;
        pageSize: number;
        logs: AuditLog[];
      }>(`/api/reports/audit-logs?${params.toString()}`);

      return {
        data: response.data.logs,
        totalCount: response.data.totalCount
      };
    } catch (err) {
      setError('Failed to fetch audit logs');
      console.error('Error fetching audit logs:', err);
      return {
        data: [],
        totalCount: 0
      };
    } finally {
      setLoading(false);
    }
  }, [filter]);

  const handleExport = async () => {
    try {
      const params = new URLSearchParams();
      if (filter.startDate) params.append('startDate', filter.startDate.toISOString());
      if (filter.endDate) params.append('endDate', filter.endDate.toISOString());
      if (filter.category) params.append('category', filter.category);

      const response = await axios.get(`/api/reports/audit-logs/export?${params.toString()}`, {
        responseType: 'blob'
      });

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

  const handleCategoryChange = (event: SelectChangeEvent) => {
    const category = event.target.value as AuditLogCategory | '';
    setFilter(prev => ({
      ...prev,
      category: category || undefined
    }));
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
                      userID: e.target.value ? parseInt(e.target.value) : undefined
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
                      startDate: date || undefined
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
                      endDate: date || undefined
                    }))}
                    slotProps={{ textField: { fullWidth: true } }}
                  />
                </Grid>
                <Grid item xs={12} sm={6} md={1}>
                  <IconButton onClick={() => fetchTableData(1, 10)} disabled={loading}>
                    <RefreshIcon />
                  </IconButton>
                </Grid>
              </Grid>
            </Paper>
          </Grid>

          {/* Data Table */}
          <Grid item xs={12}>
            <TableComponent
              columns={columns}
              fetchData={fetchTableData}
              defaultPageSize={10}
              pageSizeOptions={[5, 10, 25, 50]}
              refreshTrigger={filter}
            />
          </Grid>
        </Grid>
      </Box>
    </LocalizationProvider>
  );
};

export default AuditLogDashboard;