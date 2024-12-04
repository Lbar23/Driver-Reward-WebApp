import React, { useState, useEffect, useCallback } from "react";
import {
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Typography,
} from "@mui/material";
import axios from "axios";
import CustomDateRangePicker from "../form-elements/CustomDatePicker";
import ReportChart from "./ReportChart";
import TableComponent, { TableColumn } from "../layout/DataTable";
import { useAuth } from "../../service/authContext";

interface Driver {
  userID: number;
  name: string;
}

// Define the base interfaces for different data types
interface BaseReportData {
  driverName?: string;
  totalPoints?: number;
  pointsChanged?: number;
  transactionDate?: string;
  userID?: number;
  logID?: number;
  category?: number;
  description?: string;
  timestamp?: string;
}


// Extend the base interface for type safety
interface ReportData extends BaseReportData {}

// API response interfaces
interface AuditLogEntry {
  userID: number;
  logID: number;
  category: number;
  description: string;
  timestamp: string;
}

interface AuditLogResponse {
  totalCount: number;
  page: number;
  pageSize: number;
  logs: AuditLogEntry[];
}

const SponsorReports: React.FC = () => {
  const { user, isLoading, isAuthenticated } = useAuth();
  const [filters, setFilters] = useState({
    viewType: "summary" as "summary" | "detail",
    reportType: "driver-points" as "driver-points" | "audit-log",
    auditCategory: null as string | null,
    selectedDriver: null as number | null,
    dateRange: [null, null] as [Date | null, Date | null],
  });
  const [drivers, setDrivers] = useState<Driver[]>([]);
  const [reports, setReports] = useState<ReportData[]>([]);
  const [error, setError] = useState<string | null>(null);


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

  // Fetch reports
  const fetchReports = async () => {
    if (!user?.sponsorDetails?.sponsorID) {
      setError("Sponsor details are missing.");
      return;
    }

    try {
      setError(null);

      const endpoint = filters.reportType === "driver-points" 
        ? "/api/reports/driver-points"
        : "/api/reports/audit-logs";
        
      const [startDate, endDate] = filters.dateRange;

      const params: any = {
        sponsorID: user.sponsorDetails.sponsorID,
        startDate: startDate ? startDate.toISOString().split("T")[0] : "",
        endDate: endDate ? endDate.toISOString().split("T")[0] : "",
      };

      if (filters.reportType === "driver-points" && filters.selectedDriver) {
        params.driverID = filters.selectedDriver;
      }

      if (filters.reportType === "audit-log" && filters.auditCategory) {
        params.category = filters.auditCategory;
      }

      const response = await axios.get(endpoint, { params });
      
      if (filters.reportType === "audit-log") {
        const auditData = response.data as AuditLogResponse;
        setReports(auditData.logs as ReportData[]);
      } else {
        setReports(response.data as ReportData[]);
      }
    } catch (error) {
      console.error("Error fetching reports:", error);
      setError("Failed to fetch reports. Please try again later.");
    }
  };
  const getChartData = () => {
    if (filters.reportType === "driver-points") {
      return filters.viewType === "summary"
        ? reports.map((item) => ({
            category: item.driverName || '',
            value: item.totalPoints || 0,
          }))
        : reports.map((item) => ({
            category: item.driverName || '',
            group: item.transactionDate || '',
            value: item.pointsChanged || 0,
          }));
    } else {
      // Group by category and count occurrences
      const categoryCounts: Record<number, number> = {};
      reports.forEach((log) => {
        if (log.category !== undefined) {
          categoryCounts[log.category] = (categoryCounts[log.category] || 0) + 1;
        }
      });
      
      return Object.entries(categoryCounts).map(([category, count]) => ({
        category: getCategoryName(Number(category)),
        value: count,
      }));
    }
  };

  const getCategoryName = (categoryId: number): string => {
    const categories: Record<number, string> = {
      1: "Points",
      2: "User",
      3: "Purchase",
      4: "Application",
      5: "Product",
      6: "System",
    };
    return categories[categoryId] || "Unknown";
  };

  const getTableColumns = () => {
    if (filters.reportType === "driver-points") {
      return [
        { id: "driverName", label: "Driver Name" },
        { id: "totalPoints", label: "Total Points" },
        { id: "transactionDate", label: "Date" },
        { id: "pointsChanged", label: "Points Changed" },
      ] as TableColumn<ReportData>[];
    } else {
      return [
        { id: "timestamp", label: "Timestamp" },
        { 
          id: "category", 
          label: "Category", 
          render: (row: ReportData) => 
            row.category !== undefined ? getCategoryName(row.category) : 'Unknown'
        },
        { 
          id: "description", 
          label: "Description",
          render: (row: ReportData) => {
            if (!row.description) return 'N/A';
            try {
              const parsed = JSON.parse(row.description);
              return `${parsed.action}: ${parsed.reason} (${parsed.amount} points)`;
            } catch {
              return row.description;
            }
          }
        },
      ] as TableColumn<ReportData>[];
    }
  };
  // Create memoized fetch function for the table
  const fetchTableData = useCallback(async (page: number, pageSize: number) => {
    if (!user?.sponsorDetails?.sponsorID) {
      throw new Error("Sponsor details are missing.");
    }

    const endpoint = filters.reportType === "driver-points" 
      ? "/api/reports/driver-points"
      : "/api/reports/audit-logs";
      
    const [startDate, endDate] = filters.dateRange;

    const params: any = {
      sponsorID: user.sponsorDetails.sponsorID,
      startDate: startDate ? startDate.toISOString().split("T")[0] : "",
      endDate: endDate ? endDate.toISOString().split("T")[0] : "",
      page,
      pageSize,
    };

    if (filters.reportType === "driver-points" && filters.selectedDriver) {
      params.driverID = filters.selectedDriver;
    }

    const response = await axios.get(endpoint, { params });
    
    return {
      data: response.data,
      totalCount: response.data.length,
    };
  }, [filters, user?.sponsorDetails?.sponsorID]); // Dependencies for useCallback

  const exportCsv = async () => {
    try {
      const [startDate, endDate] = filters.dateRange;

      const exportData = {
        reportType: filters.reportType,
        metadata: {
          Sponsor: user?.sponsorDetails?.companyName,
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
    try {
      const [startDate, endDate] = filters.dateRange;

      const exportData = {
        reportType: filters.reportType,
        metadata: {
          Sponsor: user?.sponsorDetails?.companyName,
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
        {/* Report Type */}
        <FormControl sx={{ minWidth: 200 }}>
          <InputLabel>Report Type</InputLabel>
          <Select
            value={filters.reportType}
            onChange={(e) =>
              setFilters((prev) => ({
                ...prev,
                reportType: e.target.value as "driver-points" | "audit-log",
                // Reset category when switching report types
                auditCategory: null,
              }))
            }
          >
            <MenuItem value="driver-points">Driver Points</MenuItem>
          </Select>
        </FormControl>

        {/* View Type - Only show for driver points */}
        {filters.reportType === "driver-points" && (
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
        )}

        {/* Driver Dropdown - Only show for driver points */}
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
              {drivers.map((driver) => (
                <MenuItem key={driver.userID} value={driver.userID}>
                  {driver.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        )}

        {/* Audit Category - Only show for audit log */}
        {filters.reportType === "audit-log" && (
          <FormControl sx={{ minWidth: 200 }}>
            <InputLabel>Audit Category</InputLabel>
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
              <MenuItem value="Points">Points</MenuItem>
              <MenuItem value="Purchase">Purchase</MenuItem>
              <MenuItem value="Application">Application</MenuItem>
              <MenuItem value="Product">Product</MenuItem>
              <MenuItem value="System">System</MenuItem>
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

      {error && (
        <Typography color="error" sx={{ mb: 2 }}>
          {error}
        </Typography>
      )}

      {/* Report Chart */}
      <Box sx={{ mt: 4, mb: 4 }}>
        <Typography variant="h5" gutterBottom>
          Report Chart
        </Typography>
        <ReportChart
          chartType={filters.reportType === "driver-points" ? "bar" : "pie"}
          title={
            filters.reportType === "driver-points"
              ? filters.viewType === "summary"
                ? "Driver Points (Summary)"
                : "Driver Points (Detailed)"
              : "Audit Log Breakdown"
          }
          viewType={filters.viewType}
          data={getChartData()}
        />
      </Box>

       {/* Data Table */}
       <Box sx={{ mt: 4 }}>
        <Typography variant="h5" gutterBottom>
          Report Details
        </Typography>
        <TableComponent
          columns={getTableColumns()}
          fetchData={fetchTableData}
          refreshTrigger={filters} // Refresh when filters change
        />
      </Box>
    </Box>
  );
};

export default SponsorReports;