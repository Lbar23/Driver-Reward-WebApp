import React, { useState, useCallback, useEffect } from 'react';
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
import TableComponent, { TableColumn } from '../layout/DataTable';
import { useAuth } from '../../service/authContext';

// Types and Interfaces matching backend
interface Driver {
  userID: number;
  name: string;
  email: string;
  totalPoints: number;
}

enum AuditLogCategory {
  User = 0,
  Password = 1,
  Authentication = 2
}

enum AuditLogAction {
  Add = 0,
  Remove = 1,
  Update = 2
}

interface AuditLog {
  logID: number;
  userID: number;
  category: AuditLogCategory;
  action: AuditLogAction;
  actionSuccess: boolean;
  timestamp: string;
  additionalDetails?: string;
}

interface AuditLogFilter {
  driverID?: number;
  category?: AuditLogCategory;
  startDate?: Date;
  endDate?: Date;
}

const AuditLogDashboard: React.FC = () => {
  const { user } = useAuth();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [filter, setFilter] = useState<AuditLogFilter>({});
  const [drivers, setDrivers] = useState<Driver[]>([]);

  // Helper function to get category name
  const getCategoryName = (category: AuditLogCategory): string => {
    return AuditLogCategory[category] || 'Unknown';
  };

  // Helper function to get action name
  const getActionName = (action: AuditLogAction): string => {
    return AuditLogAction[action] || 'Unknown';
  };

  useEffect(() => {
    const fetchDrivers = async () => {
      try {
        const response = await axios.get<Driver[]>('/api/sponsor/drivers');
        setDrivers(response.data);
      } catch (err) {
        console.error('Error fetching drivers:', err);
        setError('Failed to fetch drivers list');
      }
    };

    fetchDrivers();
  }, []);

  const columns: TableColumn<AuditLog>[] = [
    { 
      id: 'timestamp', 
      label: 'Date/Time',
      render: (row) => format(new Date(row.timestamp), 'MMM dd, yyyy HH:mm:ss')
    },
    { 
      id: 'userID', 
      label: 'Driver',
      render: (row) => {
        const driver = drivers.find(d => d.userID === row.userID);
        return driver?.name || `Driver ${row.userID}`;
      }
    },
    { 
      id: 'category', 
      label: 'Category',
      render: (row) => (
        <Chip
          label={getCategoryName(row.category)}
          size="small"
          color="primary"
          variant="outlined"
        />
      )
    },
    { 
      id: 'action', 
      label: 'Action',
      render: (row) => (
        <Chip
          label={getActionName(row.action)}
          size="small"
          color="info"
          variant="outlined"
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
      label: 'Details',
      render: (row) => row.additionalDetails || 'N/A'
    }
  ];

  const fetchTableData = useCallback(async (page: number, pageSize: number) => {
    try {
      setLoading(true);
      setError(null);
      
      const params = new URLSearchParams();
      if (filter.driverID) params.append('userId', filter.driverID.toString());
      if (filter.category !== undefined) params.append('category', filter.category.toString());
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

      // Filter logs for sponsor's drivers
      const driverIds = drivers.map(d => d.userID);
      const filteredLogs = response.data.logs.filter(log => 
        driverIds.includes(log.userID)
      );

      return {
        data: filteredLogs,
        totalCount: filteredLogs.length
      };
    } catch (err) {
      setError('Failed to fetch audit logs');
      console.error('Error fetching audit logs:', err);
      return { data: [], totalCount: 0 };
    } finally {
      setLoading(false);
    }
  }, [filter, drivers]);

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns}>
      <Box sx={{ p: 3 }}>
        <Grid container spacing={3}>
          <Grid item xs={12} sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h4">Driver Activity Logs</Typography>
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
                {/* Driver Selection */}
                <Grid item xs={12} md={3}>
                  <FormControl fullWidth>
                    <InputLabel>Select Driver</InputLabel>
                    <Select
                      value={filter.driverID || ''}
                      onChange={(e) => setFilter(prev => ({
                        ...prev,
                        driverID: e.target.value ? Number(e.target.value) : undefined
                      }))}
                    >
                      <MenuItem value="">All Drivers</MenuItem>
                      {drivers.map((driver) => (
                        <MenuItem key={driver.userID} value={driver.userID}>
                          {driver.name}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>

                {/* Category Selection */}
                <Grid item xs={12} md={3}>
                  <FormControl fullWidth>
                    <InputLabel>Category</InputLabel>
                    <Select
                      value={filter.category?.toString() || ''}
                      onChange={(e) => setFilter(prev => ({
                        ...prev,
                        category: e.target.value ? Number(e.target.value) as AuditLogCategory : undefined
                      }))}
                    >
                      <MenuItem value="">All Categories</MenuItem>
                      {Object.entries(AuditLogCategory)
                        .filter(([key]) => !isNaN(Number(key))) // Only show numeric keys
                        .map(([key, value]) => (
                          <MenuItem key={key} value={key}>
                            {value}
                          </MenuItem>
                        ))}
                    </Select>
                  </FormControl>
                </Grid>

                {/* Date Range */}
                <Grid item xs={12} md={2}>
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
                <Grid item xs={12} md={2}>
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

                <Grid item xs={12} md={1}>
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