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
    viewType: "summary" as "summary" | "detail",
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
          const response = await axios.get<Driver[]>(`/api/sponsor/drivers`, {
            params: { sponsorID: filters.selectedSponsor },
          });
          setDrivers(response.data);
        } catch (error) {
          console.error("Error fetching drivers:", error);
          setError("Failed to fetch drivers. Please try again later.");
        }
      };
      fetchDrivers();
    } else {
      setDrivers([]);
    }
  }, [filters.reportType, filters.selectedSponsor]);

  // Fetch reports only when 'Fetch Reports' button is clicked
  const fetchReports = async () => {
    try {
      setError(null);
      const [startDate, endDate] = filters.dateRange;
      const response = await axios.get(`/api/reports/${filters.reportType}`, {
        params: {
          sponsorID: filters.selectedSponsor,
          driverID: filters.selectedDriver,
          startDate: startDate ? startDate.toISOString().split("T")[0] : "",
          endDate: endDate ? endDate.toISOString().split("T")[0] : "",
          viewType: filters.viewType,
        },
      });
      setReports(Array.isArray(response.data) ? response.data : []);
    } catch (error) {
      console.error("Error fetching reports:", error);
      setError("Failed to fetch reports. Please try again later.");
    }
  };

  const exportCsv = async () => {

    try {
        const [startDate, endDate] = filters.dateRange;

        const exportData = {
            reportType: filters.reportType,
            metadata: {
                SelectedSponsor: filters.selectedSponsor
                    ? drivers.find((d) => d.userID === filters.selectedSponsor)?.name || "All Sponsors"
                    : "All Sponsors",
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
          SelectedSponsor: filters.selectedSponsor
                    ? drivers.find((d) => d.userID === filters.selectedSponsor)?.name || "All Sponsors"
                    : "All Sponsors",
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

  // Format data for ReportChart
  const getChartData = () => {
    switch (filters.reportType) {
      case "sales-sponsor":
        return filters.viewType === "summary"
          ? reports.map((item: any) => ({
              category: item.sponsorName,
              value: item.totalSales,
            }))
          : reports.map((item: any) => ({
              category: item.sponsorName,
              group: item.transactionDate,
              value: item.saleAmount,
            }));
      case "sales-driver":
        return filters.viewType === "summary"
          ? reports.map((item: any) => ({
              category: item.driverName,
              value: item.totalSales,
            }))
          : reports.map((item: any) => ({
              category: item.driverName,
              group: item.transactionDate,
              value: item.saleAmount,
            }));
      case "invoice":
        return reports.map((item: any) => ({
          category: item.sponsorName,
          value: item.invoiceTotal,
        }));
      default:
        return [];
    }
  };

  return (
    <Box sx={{ padding: 4 }}>
      <Typography variant="h4" gutterBottom>
        Admin Reports
      </Typography>

      {/* Filters */}
      <Box sx={{ display: "flex", gap: 3, mb: 3 }}>
        {/* Report Type */}
        <FormControl sx={{ minWidth: 200 }}>
          <InputLabel>Report Type</InputLabel>
          <Select
            value={filters.reportType}
            onChange={(e) =>
              setFilters((prev) => ({
                ...prev,
                reportType: e.target.value as "sales-sponsor" | "sales-driver" | "invoice",
                selectedDriver: null,
              }))
            }
          >
            <MenuItem value="sales-sponsor">Sales by Sponsor</MenuItem>
            <MenuItem value="sales-driver">Sales by Driver</MenuItem>
            <MenuItem value="invoice">Invoice Report</MenuItem>
          </Select>
        </FormControl>

        {/* View Type */}
        {filters.reportType !== "invoice" && (
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
            {sponsors.map((sponsor) => (
              <MenuItem key={sponsor.sponsorID} value={sponsor.sponsorID}>
                {sponsor.companyName}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        {/* Driver Dropdown */}
        {filters.reportType === "sales-driver" && (
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
              {drivers.map((driver) => (
                <MenuItem key={driver.userID} value={driver.userID}>
                  {driver.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        )}
      </Box>

      {/* Custom Date Picker */}
      <CustomDateRangePicker
        dateRange={filters.dateRange}
        setDateRange={(range) =>
          setFilters((prev) => ({
            ...prev,
            dateRange: range,
          }))
        }
      />

      {/* Actions */}
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
      <ReportChart
        chartType="bar"
        title={`${filters.reportType} Report (${filters.viewType})`}
        viewType={filters.viewType}
        data={getChartData()}
      />
    </Box>
  );
};

export default AdminReports;
