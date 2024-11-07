import React, { useState } from 'react';
import { Box, Button, Stack, Checkbox, FormControlLabel } from '@mui/material';
import CalendarTodayRoundedIcon from '@mui/icons-material/CalendarTodayRounded';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { LocalizationProvider, DatePicker } from '@mui/x-date-pickers';

interface CustomDateRangePickerProps {
  dateRange: [Date | null, Date | null];
  setDateRange: (range: [Date | null, Date | null]) => void;
}

const CustomDateRangePicker: React.FC<CustomDateRangePickerProps> = ({ dateRange, setDateRange }) => {
  const [startDate, endDate] = dateRange;
  const [isCustomRange, setIsCustomRange] = useState(false);

  // Helper function for quick date range selections
  const setQuickDateRange = (days: number) => {
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - days);
    setDateRange([start, end]);
  };

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns}>
      <Stack direction="column" spacing={2} alignItems="flex-start" sx={{ mb: 2 }}>
        
        {/* Custom Range Checkbox */}
        <FormControlLabel
          control={<Checkbox checked={isCustomRange} onChange={() => setIsCustomRange(!isCustomRange)} />}
          label="Custom Date Range"
        />

        {/* Start and End Date Pickers - Only shown if Custom Range is selected */}
        {isCustomRange ? (
          <Stack direction="row" spacing={2} alignItems="center">
            <Box display="flex" alignItems="center" gap={1}>
              <CalendarTodayRoundedIcon fontSize="small" />
              <DatePicker
                label="Start"
                value={startDate}
                onChange={(newValue) => setDateRange([newValue, endDate])}
                disableFuture
              />
            </Box>
            <Box display="flex" alignItems="center" gap={1}>
              <CalendarTodayRoundedIcon fontSize="small" />
              <DatePicker
                label="End"
                value={endDate}
                onChange={(newValue) => setDateRange([startDate, newValue])}
                disableFuture
              />
            </Box>
          </Stack>
        ) : (
          // Quick Date Range Selection Buttons - Only shown if Custom Range is not selected
          <Stack direction="row" spacing={1}>
            <Button variant="outlined" size="small" onClick={() => setQuickDateRange(7)}>
              Last 7 Days
            </Button>
            <Button variant="outlined" size="small" onClick={() => setQuickDateRange(30)}>
              Last 30 Days
            </Button>
            <Button variant="outlined" size="small" onClick={() => setQuickDateRange(365)}>
              Last Year
            </Button>
          </Stack>
        )}
      </Stack>
    </LocalizationProvider>
  );
};

export default CustomDateRangePicker;
