import React, { useState, useEffect } from "react";
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
import { useAuth } from "../../service/authContext";

interface Driver {
  userID: number;
  name: string;
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

      const endpoint = "/api/reports/driver-points";
      const [startDate, endDate] = filters.dateRange;

      const params: any = {
        sponsorID: user.sponsorDetails.sponsorID,
        startDate: startDate ? startDate.toISOString().split("T")[0] : "",
        endDate: endDate ? endDate.toISOString().split("T")[0] : "",
      };

      if (filters.reportType === "driver-points") {
        params.driverID = filters.selectedDriver;
      }

      const response = await axios.get(endpoint, { params });
      setReports(response.data);
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
