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

export default function AdminReports() {
  const [reportType, setReportType] = useState<'salesBySponsor' | 'salesByDriver' | 'invoice'>('salesBySponsor');
  const [viewType, setViewType] = useState<'summary' | 'detailed'>('summary');
  const [dateRange, setDateRange] = useState<[Date | null, Date | null]>([null, null]);
  const [selectedSponsor, setSelectedSponsor] = useState<'all' | number>('all');
  const [selectedDriver, setSelectedDriver] = useState<'all' | number>('all');

  const isSalesByDriverWithIndividualSponsor = reportType === 'salesByDriver' && selectedSponsor !== 'all';

  return (
    <Box>
      <Typography variant="h5" sx={{ mb: 2 }} aria-label="Admin Reports Dashboard">Admin Reports Dashboard</Typography>

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
            onChange={(e) => setReportType(e.target.value as 'salesBySponsor' | 'salesByDriver' | 'invoice')}
            label="Report Type"
            aria-label="Select Report Type"
          >
            <MenuItem value="salesBySponsor">Sales by Sponsor</MenuItem>
            <MenuItem value="salesByDriver">Sales by Driver</MenuItem>
            <MenuItem value="invoice">Invoice Report</MenuItem>
          </Select>
        </FormControl>

        <FormControl sx={{ minWidth: 180 }}>
          <InputLabel id="view-type-label">View Type</InputLabel>
          <Select
            labelId="view-type-label"
            value={viewType}
            onChange={(e) => setViewType(e.target.value as 'summary' | 'detailed')}
            label="View Type"
            aria-label="Select View Type"
          >
            <MenuItem value="summary">Summary</MenuItem>
            <MenuItem value="detailed">Detailed</MenuItem>
          </Select>
        </FormControl>

        {(reportType === 'salesBySponsor' || reportType === 'invoice') && (
          <FormControl sx={{ minWidth: 180 }}>
            <InputLabel id="select-sponsor-label">Select Sponsor</InputLabel>
            <Select
              labelId="select-sponsor-label"
              value={selectedSponsor}
              onChange={(e) => setSelectedSponsor(e.target.value as 'all' | number)}
              label="Select Sponsor"
              aria-label="Select Sponsor"
            >
              <MenuItem value="all">All Sponsors</MenuItem>
              <MenuItem value={101}>Sponsor A</MenuItem>
              <MenuItem value={102}>Sponsor B</MenuItem>
            </Select>
          </FormControl>
        )}

        {reportType === 'salesByDriver' && (
          <>
            <FormControl sx={{ minWidth: 180 }}>
              <InputLabel id="select-sponsor-label">Select Sponsor</InputLabel>
              <Select
                labelId="select-sponsor-label"
                value={selectedSponsor}
                onChange={(e) => setSelectedSponsor(e.target.value as 'all' | number)}
                label="Select Sponsor"
                aria-label="Select Sponsor for Sales by Driver"
              >
                <MenuItem value="all">All Sponsors</MenuItem>
                <MenuItem value={101}>Sponsor A</MenuItem>
                <MenuItem value={102}>Sponsor B</MenuItem>
              </Select>
            </FormControl>

            {isSalesByDriverWithIndividualSponsor && (
              <FormControl sx={{ minWidth: 180 }}>
                <InputLabel id="select-driver-label">Select Driver</InputLabel>
                <Select
                  labelId="select-driver-label"
                  value={selectedDriver}
                  onChange={(e) => setSelectedDriver(e.target.value as 'all' | number)}
                  label="Select Driver"
                  aria-label="Select Driver for Individual Sponsor"
                >
                  <MenuItem value="all">All Drivers</MenuItem>
                  <MenuItem value={1}>Driver 1</MenuItem>
                  <MenuItem value={2}>Driver 2</MenuItem>
                </Select>
              </FormControl>
            )}
          </>
        )}
      </Stack>

      <CustomDateRangePicker dateRange={dateRange} setDateRange={setDateRange} />

      <Box sx={{ mt: 4 }}>
        <ReportChart
          reportType={reportType}
          viewType={viewType}
          selectedSponsor={selectedSponsor}
          selectedDriver={selectedDriver}
          dateRange={dateRange}
          aria-label="Admin Report Chart"
        />
      </Box>
    </Box>
  );
}
