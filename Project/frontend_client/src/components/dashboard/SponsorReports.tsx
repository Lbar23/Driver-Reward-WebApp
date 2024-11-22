import React, { useState, useEffect } from "react";
import {
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Chip
} from "@mui/material";
import axios from "axios";
import CustomDateRangePicker from "../form-elements/CustomDatePicker";
import ReportChart from "./ReportChart";
import { format } from "date-fns";

interface Driver {
  userID: number;
  name: string;
}

//Added interfaces from AuditLogDashboard
enum AuditLogCategory {
  User = 'User',
  Points = 'Points',
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

const SponsorReports: React.FC = () => {
  const [filters, setFilters] = useState({
    reportType: "driver-points" as "driver-points" | "audit-log",
    selectedDriver: null as number | null,
    dateRange: [null, null] as [Date | null, Date | null],
    auditCategory: null as string | null,
  });
  const [drivers, setDrivers] = useState<Driver[]>([]);
  const [reports, setReports] = useState<any[]>([]);
  const [error, setError] = useState<string | null>(null);

  //New audit log states
  const [auditLogs, setAuditLogs] = useState<AuditLog[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);

  const getCategoryColor = (category: AuditLogCategory): "default" | "primary" | "secondary" | "error" | "info" | "success" | "warning" => {
    const colors: Record<AuditLogCategory, "default" | "primary" | "secondary" | "error" | "info" | "success" | "warning"> = {
      [AuditLogCategory.User]: 'primary',
      [AuditLogCategory.Points]: 'success',
      [AuditLogCategory.Purchase]: 'info',
      [AuditLogCategory.Application]: 'warning',
      [AuditLogCategory.Product]: 'secondary',
      [AuditLogCategory.System]: 'error'
    };
    return colors[category] || 'default';
  };

  // Fetch drivers on mount
  useEffect(() => {
    const fetchDrivers = async () => {
      try {
        const response = await axios.get<Driver[]>("/api/sponsor/drivers");
        setDrivers(response.data);
      } catch (error) {
        console.error("Error fetching drivers:", error);
        setError("Failed to fetch drivers. Please try again later.");
      }
    };
    fetchDrivers();
  }, []);

  // Fetch reports only when "Fetch Reports" button is clicked
  const fetchReports = async () => {
    try {
      setError(null);
      
      const [startDate, endDate] = filters.dateRange;
  
      if (filters.reportType === "audit-log") {
        // Build query parameters for audit logs
        const params = new URLSearchParams();
        
        if (filters.selectedDriver) 
          params.append('userId', filters.selectedDriver.toString());
        
        if (filters.auditCategory) 
          params.append('category', filters.auditCategory);
        
        if (startDate) 
          params.append('startDate', startDate.toISOString());
        
        if (endDate) 
          params.append('endDate', endDate.toISOString());
        
        // Pagination parameters
        params.append('page', (page + 1).toString());
        params.append('pageSize', pageSize.toString());
  
        // Fetch audit logs - backend handles sponsor vs admin filtering
        const response = await axios.get<AuditLogResponse>(`/api/reports/audit-logs?${params.toString()}`);
        setAuditLogs(response.data.logs);
        setTotalCount(response.data.totalCount);
        setReports([]); // Clear other report type data
      } 
      else if (filters.reportType === "driver-points") {
        // Original driver-points logic
        const params: any = {
          startDate: startDate ? startDate.toISOString().split("T")[0] : "",
          endDate: endDate ? endDate.toISOString().split("T")[0] : "",
        };
  
        if (filters.selectedDriver) {
          params.driverId = filters.selectedDriver;
        }
  
        const response = await axios.get('/api/reports/driver-points', { params });
        setReports(response.data);
        setAuditLogs([]); // Clear audit logs when viewing driver points
        setTotalCount(0); // Reset pagination count
      }
    } catch (error) {
      console.error("Error fetching reports:", error);
      setError("Failed to fetch reports. Please try again later.");
    }
  };
  
  // Add useEffects for pagination and filter changes
  useEffect(() => {
    if (filters.reportType === 'audit-log' && filters.dateRange[0] && filters.dateRange[1]) {
      fetchReports();
    }
  }, [page, pageSize]);
  
  useEffect(() => {
    if (filters.dateRange[0] && filters.dateRange[1]) {
      fetchReports();
    }
  }, [filters.reportType, filters.selectedDriver, filters.auditCategory, filters.dateRange]);
  
  // Add pagination handlers
  const handlePageChange = (event: unknown, newPage: number) => {
    setPage(newPage);
  };
  
  const handleRowsPerPageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setPageSize(parseInt(event.target.value, 10));
    setPage(0);
  };

  return (
    <Box sx={{ padding: 4 }}>
      <Typography variant="h4" gutterBottom>
        Sponsor Reports
      </Typography>

      {/* Filters */}
      <Box
        sx={{
          display: "flex",
          flexWrap: "wrap",
          gap: 3,
          alignItems: "center",
          mb: 3,
        }}
      >
        {/* Report Type */}
        <FormControl sx={{ minWidth: 200 }}>
          <InputLabel>Report Type</InputLabel>
          <Select
            value={filters.reportType}
            onChange={(e) =>
              setFilters((prev) => ({
                ...prev,
                reportType: e.target.value as "driver-points" | "audit-log",
              }))
            }
          >
            <MenuItem value="driver-points">Driver Point Tracking</MenuItem>
            <MenuItem value="audit-log">Audit Log</MenuItem>
          </Select>
        </FormControl>

        {/* Driver Dropdown (for Driver Point Tracking) */}
        {filters.reportType === "driver-points" && (
          <FormControl sx={{ minWidth: 200 }}>
            <InputLabel>Select Driver</InputLabel>
            <Select
              value={filters.selectedDriver || ""}
              onChange={(e) =>
                setFilters((prev) => ({
                  ...prev,
                  selectedDriver: e.target.value ? Number(e.target.value) : null,
                }))
              }
            >
              <MenuItem value="">All Drivers</MenuItem>
              {drivers.length > 0 ? (
                drivers.map((driver) => (
                  <MenuItem key={driver.userID} value={driver.userID}>
                    {driver.name}
                  </MenuItem>
                ))
              ) : (
                <MenuItem disabled>No drivers available</MenuItem>
              )}
            </Select>
          </FormControl>
        )}

        {/* Audit Log Category (for Audit Log) */}
        {filters.reportType === "audit-log" && (
          <FormControl sx={{ minWidth: 200 }}>
            <InputLabel>Audit Log Category</InputLabel>
            <Select
              value={filters.auditCategory || ""}
              onChange={(e) =>
                setFilters((prev) => ({
                  ...prev,
                  auditCategory: e.target.value || null,
                }))
              }
            >
              <MenuItem value="">All Categories</MenuItem>
              <MenuItem value="User">User</MenuItem>
              <MenuItem value="System">System</MenuItem>
              <MenuItem value="Transaction">Transaction</MenuItem>
            </Select>
          </FormControl>
        )}
      </Box>

      {/* Custom Date Range Picker */}
      <Box sx={{ mb: 3 }}>
        <CustomDateRangePicker
          dateRange={filters.dateRange}
          setDateRange={(range) =>
            setFilters((prev) => ({
              ...prev,
              dateRange: range,
            }))
          }
        />
      </Box>

      {/* Fetch Button */}
      <Button variant="contained" onClick={fetchReports}>
        Fetch Reports
      </Button>

      {/* Report Chart */}
      <Box sx={{ mt: 4 }}>
        <Typography variant="h5" gutterBottom>
          Report Chart
        </Typography>
        {filters.dateRange[0] && filters.dateRange[1] && filters.reportType === "driver-points" && (
          <ReportChart
            reportType="points"
            viewType="detailed"
            selectedDriver={filters.selectedDriver || "all"}
            dateRange={filters.dateRange}
          />
        )}
      </Box>

      {/* Report Results */}
      <Box sx={{ mt: 4 }}>
  <Typography variant="h5" gutterBottom>
    Report Results
  </Typography>
  {error && <Typography color="error">{error}</Typography>}
  
  {filters.reportType === "audit-log" ? (
    // Audit Log Table Display
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
            {auditLogs.length === 0 ? (
              <TableRow>
                <TableCell colSpan={5} align="center">No audit logs found</TableCell>
              </TableRow>
            ) : (
              auditLogs.map((log) => (
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
        page={page}
        onPageChange={handlePageChange}
        rowsPerPage={pageSize}
        onRowsPerPageChange={handleRowsPerPageChange}
        rowsPerPageOptions={[5, 10, 25, 50]}
      />
    </Paper>
  ) : (
    // Original Driver Points Table Display
    Array.isArray(reports) && reports.length > 0 ? (
      <Box component="table" sx={{ width: "100%", borderCollapse: "collapse" }}>
        <Box component="thead">
          <Box component="tr">
            {Object.keys(reports[0]).map((key) => (
              <Box
                component="th"
                key={key}
                sx={{
                  border: "1px solid",
                  padding: "8px",
                  textAlign: "left",
                  backgroundColor: "lightgray",
                }}
              >
                {key}
              </Box>
            ))}
          </Box>
        </Box>
        <Box component="tbody">
          {reports.map((report, index) => (
            <Box component="tr" key={index}>
              {Object.values(report).map((value, idx) => (
                <Box
                  component="td"
                  key={idx}
                  sx={{ border: "1px solid", padding: "8px" }}
                >
                  {value !== null && value !== undefined ? value.toString() : "N/A"}
                </Box>
              ))}
            </Box>
          ))}
        </Box>
      </Box>
    ) : (
      !error && <Typography>No data available for the selected report type.</Typography>
    )
  )}
</Box>
    </Box>
  );
};

export default SponsorReports;
