import * as React from 'react';
import Button from '@mui/material/Button';
import CalendarTodayRoundedIcon from '@mui/icons-material/CalendarTodayRounded';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'; // Change this line to your chosen adapter
import { UseDateFieldProps } from '@mui/x-date-pickers/DateField';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import {
  BaseSingleInputFieldProps,
  DateValidationError,
  FieldSection,
} from '@mui/x-date-pickers/models';

interface ButtonFieldProps
  extends UseDateFieldProps<Date, false>,
    BaseSingleInputFieldProps<
      Date | null,
      Date,
      FieldSection,
      false,
      DateValidationError
    > {
  setOpen?: React.Dispatch<React.SetStateAction<boolean>>;
}

function ButtonField(props: ButtonFieldProps) {
  const {
    setOpen,
    label,
    id,
    disabled,
    InputProps: { ref } = {},
    inputProps: { 'aria-label': ariaLabel } = {},
  } = props;

  return (
    <Button
      variant="outlined"
      id={id}
      disabled={disabled}
      ref={ref}
      aria-label={ariaLabel}
      size="small"
      onClick={() => setOpen?.((prev) => !prev)}
      startIcon={<CalendarTodayRoundedIcon fontSize="small" />}
      sx={{ minWidth: 'fit-content' }}
    >
      {label ? `${label}` : 'Pick a date'}
    </Button>
  );
}

export default function CustomDatePicker() {
  const [value, setValue] = React.useState<Date | null>(new Date('2023-04-17'));
  const [open, setOpen] = React.useState(false);

  const formatDate = (date: Date | null): string => {
    return date ? date.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' }) : '';
  };

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns}> {/* AdapterDateFns is still required by the picker */}
      <DatePicker
        value={value}
        label={formatDate(value)}
        onChange={(newValue) => setValue(newValue)}
        slots={{ field: ButtonField }}
        slotProps={{
          field: { setOpen } as any,
          nextIconButton: { size: 'small' },
          previousIconButton: { size: 'small' },
        }}
        open={open}
        onClose={() => setOpen(false)}
        onOpen={() => setOpen(true)}
        views={['day', 'month', 'year']}
      />
    </LocalizationProvider>
  );
}
