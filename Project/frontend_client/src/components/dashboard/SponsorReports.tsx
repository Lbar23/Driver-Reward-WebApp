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

interface Driver {
  userID: number;
  name: string;
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

      const endpoint =
        filters.reportType === "audit-log"
          ? "/api/reports/audit-log"
          : "/api/reports/driver-points";

      const [startDate, endDate] = filters.dateRange;

      const params: any = {
        startDate: startDate ? startDate.toISOString().split("T")[0] : "",
        endDate: endDate ? endDate.toISOString().split("T")[0] : "",
      };

      if (filters.reportType === "driver-points") {
        params.selectedDriver = filters.selectedDriver;
      } else if (filters.reportType === "audit-log") {
        params.category = filters.auditCategory;
      }

      const response = await axios.get(endpoint, { params });
      setReports(response.data);
    } catch (error) {
      console.error("Error fetching reports:", error);
      setError("Failed to fetch reports. Please try again later.");
    }
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
        {Array.isArray(reports) && reports.length > 0 ? (
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
        )}
      </Box>
    </Box>
  );
};

export default SponsorReports;
