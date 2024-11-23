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
import { useAuth } from "../../service/authContext";
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
  const { user, isLoading, isAuthenticated } = useAuth();
  const [filters, setFilters] = useState({
    viewType: "summary" as "summary" | "detail",
    reportType: "driver-points" as "driver-points",
    selectedDriver: null as number | null,
    dateRange: [null, null] as [Date | null, Date | null],
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
    if (!user?.sponsorDetails?.sponsorID || isLoading || !isAuthenticated) return;

    const fetchDrivers = async () => {
      try {
        const response = await axios.get<Driver[]>(`/api/sponsor/drivers`, {
          params: { sponsorID: user.sponsorDetails?.sponsorID },
        });
        setDrivers(response.data);
      } catch (error) {
        console.error("Error fetching drivers:", error);
        setError("Failed to fetch drivers. Please try again later.");
      }
    };

    fetchDrivers();
  }, [user, isLoading, isAuthenticated]);

  // Fetch reports only when "Fetch Reports" button is clicked
  const fetchReports = async () => {
    if (!user?.sponsorDetails?.sponsorID) {
      setError("Sponsor details are missing.");
      return;
    }

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

  // Format data for ReportChart
  const getChartData = () => {
    if (filters.viewType === "summary") {
      // (Ideal)Summary format - Each bar/pie slice corresponds to a driver with total points
      return reports.map((item: any) => ({
        category: item.driverName,
        value: item.totalPoints,
      }));
    } else if (filters.viewType === "detail") {
      // (Ideal)Detailed format - Stacks for bar chart, grouped by transaction date
      // Ideal bc some db changes need to happen for reports to be proper...
      return reports.map((item: any) => ({
        category: item.driverName,
        group: item.transactionDate,
        value: item.pointsChanged,
      }));
    }
    return [];
  };

  

  const exportCsv = async () => {
    if (!user?.sponsorDetails?.sponsorID) {
        setError("Sponsor details are missing.");
        return;
    }

    try {
        const [startDate, endDate] = filters.dateRange;

        const exportData = {
            reportType: filters.reportType,
            metadata: {
                Sponsor: user.sponsorDetails.companyName,
                StartDate: startDate ? startDate.toISOString().split("T")[0] : "N/A",
                EndDate: endDate ? endDate.toISOString().split("T")[0] : "N/A",
                SelectedDriver: filters.selectedDriver
                    ? drivers.find((d) => d.userID === filters.selectedDriver)?.name || "All Drivers"
                    : "All Drivers",
            },
            data: reports,
        };

        const response = await axios.post(`/api/reports/export-csv`, exportData, {
            responseType: "blob",
        });

        const blob = new Blob([response.data], { type: "text/csv;charset=utf-8;" });
        const url = URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = url;
        link.download = `${filters.reportType}_report.csv`;
        link.click();
    } catch (error) {
        console.error("Error exporting CSV:", error);
        setError("Failed to export CSV. Please try again later.");
    }
};


  const exportPdf = async () => {
    if (!user?.sponsorDetails?.companyName) {
      setError("Sponsor details are missing.");
      return;
    }

    try {
      const [startDate, endDate] = filters.dateRange;

      const exportData = {
        reportType: filters.reportType,
        metadata: {
          Sponsor: user.sponsorDetails.companyName,
          StartDate: startDate ? startDate.toISOString().split("T")[0] : "N/A",
          EndDate: endDate ? endDate.toISOString().split("T")[0] : "N/A",
          SelectedDriver: filters.selectedDriver
            ? drivers.find((d) => d.userID === filters.selectedDriver)?.name || "All Drivers"
            : "All Drivers",
        },
        data: reports,
      };

      const response = await axios.post(`/api/reports/export-pdf`, exportData, {
        responseType: "blob",
      });

      const blob = new Blob([response.data], { type: "application/pdf" });
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = `${filters.reportType}_report.pdf`;
      link.click();
    } catch (error) {
      console.error("Error exporting PDF:", error);
      setError("Failed to export PDF. Please try again later.");
    }
  };

  if (isLoading) {
    return <Typography>Loading sponsor details...</Typography>;
  }

  if (!isAuthenticated || !user?.sponsorDetails) {
    return (
      <Typography color="error">
        Sponsor details are missing or user is not authenticated.
      </Typography>
    );
  }

  return (
    <Box sx={{ padding: 4 }}>
      <Typography variant="h4" gutterBottom>
        Sponsor Reports ({user.sponsorDetails.companyName})
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
        {/* View Type */}
        <FormControl sx={{ minWidth: 200 }}>
          <InputLabel>View Type</InputLabel>
          <Select
            value={filters.viewType}
            onChange={(e) =>
              setFilters((prev) => ({
                ...prev,
                viewType: e.target.value as "summary" | "detail",
              }))
            }
          >
            <MenuItem value="summary">Summary</MenuItem>
            <MenuItem value="detail">Detailed</MenuItem>
          </Select>
        </FormControl>

        {/* Driver Dropdown */}
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
            {drivers.map((driver) => (
              <MenuItem key={driver.userID} value={driver.userID}>
                {driver.name}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
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

      {/* Action Buttons */}
      <Box sx={{ display: "flex", gap: 2, mb: 4 }}>
        <Button variant="contained" onClick={fetchReports}>
          Fetch Reports
        </Button>
        <Button variant="outlined" onClick={exportCsv}>
          Export CSV
        </Button>
        <Button variant="outlined" onClick={exportPdf}>
          Export PDF
        </Button>
      </Box>

      {/* Report Chart */}
      <Box sx={{ mt: 4 }}>
        <Typography variant="h5" gutterBottom>
          Report Chart
        </Typography>
        <ReportChart
          chartType="bar"
          title={
            filters.viewType === "summary"
              ? "Driver Points (Summary)"
              : "Driver Points (Detailed)"
          }
          viewType={filters.viewType}
          data={getChartData()}
        />
      </Box>
    </Box>
  );
};

export default SponsorReports;
