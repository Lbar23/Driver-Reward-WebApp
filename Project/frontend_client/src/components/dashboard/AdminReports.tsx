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

interface Sponsor {
  sponsorID: number;
  companyName: string;
}

interface Driver {
  userID: number;
  name: string;
}

const AdminReports: React.FC = () => {
  const [filters, setFilters] = useState({
    reportType: "sales-sponsor" as "sales-sponsor" | "sales-driver" | "invoice",
    viewType: "summary" as "summary" | "detailed",
    selectedSponsor: null as number | null,
    selectedDriver: null as number | null,
    dateRange: [null, null] as [Date | null, Date | null],
  });
  const [sponsors, setSponsors] = useState<Sponsor[]>([]);
  const [drivers, setDrivers] = useState<Driver[]>([]);
  const [reports, setReports] = useState<any[]>([]);
  const [error, setError] = useState<string | null>(null);

  // Fetch sponsors on mount
  useEffect(() => {
    const fetchSponsors = async () => {
      try {
        const response = await axios.get<Sponsor[]>("/api/admin/sponsors/details");
        setSponsors(response.data);
      } catch (error) {
        console.error("Error fetching sponsors:", error);
        setError("Failed to fetch sponsors. Please try again later.");
      }
    };
    fetchSponsors();
  }, []);

  // Fetch drivers when a specific sponsor is selected
  useEffect(() => {
    if (filters.reportType === "sales-driver" && filters.selectedSponsor) {
      const fetchDrivers = async () => {
        try {
          const response = await axios.get<Driver[]>(
            `/api/sponsor/drivers`
          );
          setDrivers(response.data);
        } catch (error) {
          console.error("Error fetching drivers:", error);
          setError("Failed to fetch drivers. Please try again later.");
        }
      };
      fetchDrivers();
    } else {
      setDrivers([]); // Clear drivers if sponsor is deselected
    }
  }, [filters.reportType, filters.selectedSponsor]);

  // Fetch reports only when 'Fetch Reports' button is clicked
  const fetchReports = async () => {
    try {
      setError(null);
      const [startDate, endDate] = filters.dateRange;
      const response = await axios.get(`/api/reports/${filters.reportType}`, {
        params: {
          selectedSponsor: filters.selectedSponsor,
          selectedDriver: filters.selectedDriver,
          startDate: startDate ? startDate.toISOString().split("T")[0] : "",
          endDate: endDate ? endDate.toISOString().split("T")[0] : "",
          viewType: filters.viewType,
        },
      });

      const responseData = response.data;

      // Normalize response to ensure it's an array
      if (Array.isArray(responseData)) {
        setReports(responseData);
      } else {
        setReports([]); // Default to an empty array
        setError("Unexpected response format. No data available.");
      }
    } catch (error) {
      console.error("Error fetching reports:", error);
      setError("Failed to fetch reports. Please try again later.");
    }
  };

  return (
    <Box sx={{ padding: 4 }}>
      <Typography variant="h4" gutterBottom>
        Admin Reports
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
                reportType: e.target.value as "sales-sponsor" | "sales-driver" | "invoice",
                selectedDriver: null, // Reset driver when changing report type
              }))
            }
          >
            <MenuItem value="sales-sponsor">Sales by Sponsor</MenuItem>
            <MenuItem value="sales-driver">Sales by Driver</MenuItem>
            <MenuItem value="invoice">Invoice Report</MenuItem>
          </Select>
        </FormControl>

        {/* View Type (only for non-invoice reports) */}
        {filters.reportType !== "invoice" && (
          <FormControl sx={{ minWidth: 200 }}>
            <InputLabel>View Type</InputLabel>
            <Select
              value={filters.viewType}
              onChange={(e) =>
                setFilters((prev) => ({
                  ...prev,
                  viewType: e.target.value as "summary" | "detailed",
                }))
              }
            >
              <MenuItem value="summary">Summary</MenuItem>
              <MenuItem value="detailed">Detailed</MenuItem>
            </Select>
          </FormControl>
        )}

        {/* Sponsor Dropdown */}
        <FormControl sx={{ minWidth: 200 }}>
          <InputLabel>Select Sponsor</InputLabel>
          <Select
            value={filters.selectedSponsor || ""}
            onChange={(e) =>
              setFilters((prev) => ({
                ...prev,
                selectedSponsor: Number(e.target.value) || null,
              }))
            }
          >
            <MenuItem value="">All Sponsors</MenuItem>
            {sponsors.length > 0 ? (
              sponsors.map((sponsor) => (
                <MenuItem key={sponsor.sponsorID} value={sponsor.sponsorID}>
                  {sponsor.companyName}
                </MenuItem>
              ))
            ) : (
              <MenuItem disabled>No sponsors available</MenuItem>
            )}
          </Select>
        </FormControl>

        {/* Driver Dropdown (only for Sales by Driver) */}
        {filters.reportType === "sales-driver" && filters.selectedSponsor && (
          <FormControl sx={{ minWidth: 200 }}>
            <InputLabel>Select Driver</InputLabel>
            <Select
              value={filters.selectedDriver || ""}
              onChange={(e) =>
                setFilters((prev) => ({
                  ...prev,
                  selectedDriver: Number(e.target.value) || null,
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

        {/* Custom Date Range Picker */}
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
        {filters.dateRange[0] && filters.dateRange[1] && (
          <ReportChart
            reportType={filters.reportType === "invoice" ? "invoice" : "sales"}
            viewType={filters.viewType}
            selectedSponsor={filters.selectedSponsor || "all"}
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
        {reports.length > 0 ? (
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

export default AdminReports;
