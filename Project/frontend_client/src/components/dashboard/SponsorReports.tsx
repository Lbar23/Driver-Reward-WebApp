import React, { useState } from 'react';
import {
  Box,
  Typography,
  Stack,
  Select,
  MenuItem,
  InputLabel,
  FormControl,
} from '@mui/material';
import CustomDateRangePicker from '../form-elements/CustomDatePicker';
import ReportChart from './ReportChart';

export default function SponsorReports() {
  const [reportType, setReportType] = useState<'driverPoints'>('driverPoints');
  const [viewType, setViewType] = useState<'summary' | 'detailed'>('summary');
  const [selectedDriver, setSelectedDriver] = useState<'all' | number>('all');
  const [dateRange, setDateRange] = useState<[Date | null, Date | null]>([null, null]);

  return (
    <Box>
      <Typography variant="h5" sx={{ mb: 2 }} aria-label="Sponsor Reports Dashboard">Sponsor Reports Dashboard</Typography>

      <Stack
        direction="row"
        spacing={2}
        alignItems="center"
        sx={{
          mb: 3,
          flexWrap: 'wrap',
          gap: 2,
        }}
      >
        <FormControl sx={{ minWidth: 180 }}>
          <InputLabel id="report-type-label">Report Type</InputLabel>
          <Select
            labelId="report-type-label"
            value={reportType}
            onChange={(e) => setReportType(e.target.value as 'driverPoints')}
            label="Report Type"
            aria-label="Select Report Type for Sponsor"
          >
            <MenuItem value="driverPoints">Driver Points Tracking</MenuItem>
          </Select>
        </FormControl>

        <FormControl sx={{ minWidth: 180 }}>
          <InputLabel id="view-type-label">View Type</InputLabel>
          <Select
            labelId="view-type-label"
            value={viewType}
            onChange={(e) => setViewType(e.target.value as 'summary' | 'detailed')}
            label="View Type"
            aria-label="Select View Type for Sponsor"
          >
            <MenuItem value="summary">Summary</MenuItem>
            <MenuItem value="detailed">Detailed</MenuItem>
          </Select>
        </FormControl>

        <FormControl sx={{ minWidth: 180 }}>
          <InputLabel id="select-driver-label">Select Driver</InputLabel>
          <Select
            labelId="select-driver-label"
            value={selectedDriver}
            onChange={(e) => setSelectedDriver(e.target.value as 'all' | number)}
            label="Select Driver"
            aria-label="Select Driver for Driver Points Tracking"
          >
            <MenuItem value="all">All Drivers</MenuItem>
            <MenuItem value={1}>Driver 1</MenuItem>
            <MenuItem value={2}>Driver 2</MenuItem>
          </Select>
        </FormControl>
      </Stack>

      <CustomDateRangePicker dateRange={dateRange} setDateRange={setDateRange} />

      <Box sx={{ mt: 4 }}>
        <ReportChart
          reportType={reportType}
          viewType={viewType}
          selectedDriver={selectedDriver}
          dateRange={dateRange}
          aria-label="Sponsor Report Chart"
        />
      </Box>
    </Box>
  );
}
