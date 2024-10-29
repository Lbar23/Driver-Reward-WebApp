// components/shared/MuiComponents.tsx
import React, { useRef, useEffect } from 'react';
import {
  Typography,
  Box,
  CircularProgress,
  Button,
  Alert,
  Paper,
  MenuItem,
  Select,
  FormControl,
  InputLabel,
  SelectChangeEvent,
  ThemeProvider,
  createTheme,
  useTheme
} from '@mui/material';
import { Link } from 'react-router-dom';
export {
    Typography,
    Box,
    CircularProgress,
    Button,
    Alert,
    Paper,
    MenuItem,
    Select,
    FormControl,
    InputLabel,
    ThemeProvider,
    createTheme,
    useTheme
}
export type { SelectChangeEvent }
// Create a theme with WCAG AA compliant colors
const createAccessibleTheme = () => {
  const baseTheme = useTheme();
  return createTheme({
    ...baseTheme,
    palette: {
      ...baseTheme.palette,
      primary: {
        main: '#1976d2', // WCAG AA compliant blue
        contrastText: '#ffffff',
      },
      secondary: {
        main: '#2e7d32', // WCAG AA compliant green
        contrastText: '#ffffff',
      },
      text: {
        primary: '#202020', // High contrast text
        secondary: '#595959', // WCAG AA compliant secondary text
      },
      background: {
        paper: '#ffffff',
        default: '#f5f5f5',
      },
    },
    components: {
      MuiButton: {
        styleOverrides: {
          root: {
            '&:focus-visible': {
              outline: '3px solid #1976d2',
              outlineOffset: '2px',
            },
          },
        },
      },
      MuiSelect: {
        styleOverrides: {
          root: {
            '&:focus-visible': {
              outline: '3px solid #1976d2',
              outlineOffset: '2px',
            },
          },
        },
      },
    },
  });
};

export const AccessibleThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const theme = createAccessibleTheme();
  return <ThemeProvider theme={theme}>{children}</ThemeProvider>;
};

export const AccessibleSelect = ({
  id,
  label,
  value,
  onChange,
  options,
  required = false,
  helpText,
  ...props
}: {
  id: string;
  label: string;
  value: string;
  onChange: (event: SelectChangeEvent<string>) => void;
  options: Array<{ value: string; label: string }>;
  required?: boolean;
  helpText?: string;
}) => {
  const selectId = `${id}-select`;
  const helpTextId = `${id}-help-text`;

  return (
    <FormControl 
      sx={{ minWidth: 200 }}
      required={required}
    >
      <InputLabel 
        id={`${selectId}-label`}
        required={required}
      >
        {label}
      </InputLabel>
      <Select
        labelId={`${selectId}-label`}
        id={selectId}
        value={value}
        onChange={onChange}
        label={label}
        aria-label={label}
        aria-required={required}
        aria-describedby={helpText ? helpTextId : undefined}
        {...props}
      >
        {options.map((option) => (
          <MenuItem 
            key={option.value} 
            value={option.value}
            role="option"
            aria-selected={value === option.value}
          >
            {option.label}
          </MenuItem>
        ))}
      </Select>
      {helpText && (
        <Typography 
          id={helpTextId} 
          variant="caption" 
          sx={{ mt: 1, color: 'text.secondary' }}
        >
          {helpText}
        </Typography>
      )}
    </FormControl>
  );
};

export const AccessibleButton = ({
  to,
  children,
  color = 'primary',
  disabled = false,
  ...props
}: {
  to: string;
  children: React.ReactNode;
  color?: 'primary' | 'secondary';
  disabled?: boolean;
}) => {
  const buttonRef = useRef<HTMLAnchorElement>(null);

  // Handle keyboard navigation
  const handleKeyDown = (event: React.KeyboardEvent) => {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      buttonRef.current?.click();
    }
  };

  return (
    <Button
      component={Link}
      to={to}
      variant="contained"
      color={color}
      ref={buttonRef}
      onKeyDown={handleKeyDown}
      disabled={disabled}
      sx={{
        textTransform: 'none',
        padding: '8px 16px',
        fontSize: '1rem',
        '&:focus-visible': {
          outline: '3px solid #1976d2',
          outlineOffset: '2px',
        },
      }}
      role="link"
      aria-disabled={disabled}
      {...props}
    >
      {children}
    </Button>
  );
};

export const LoadingSpinner = () => (
  <Box role="status" aria-live="polite" aria-busy="true">
    <CircularProgress 
      aria-label="Loading content"
      sx={{ 
        color: 'primary.main',
        display: 'block',
        margin: 'auto'
      }}
    />
    <Typography 
      sx={{ mt: 2 }}
      variant="body2"
      aria-hidden="true"
    >
      Loading...
    </Typography>
  </Box>
);

export const Section = ({
  title,
  buttons,
  color = 'primary',
  description,
}: {
  title: string;
  buttons: Array<{ label: string; path: string }>;
  color?: 'primary' | 'secondary';
  description?: string;
  ariaLabel?: string;
}) => {
  const sectionId = `section-${title.toLowerCase().replace(/\s/g, '-')}`;
  
  return (
    <Box 
      component="section" 
      aria-labelledby={sectionId}
      sx={{ 
        mt: 4,
        p: 3,
        border: '1px solid',
        borderColor: 'divider',
        borderRadius: 1,
      }}
    >
      <Typography 
        variant="h2" 
        id={sectionId}
        sx={{ fontSize: 'h6.fontSize' }}
      >
        {title}
      </Typography>
      {description && (
        <Typography 
          variant="body2" 
          sx={{ mt: 1, mb: 2 }}
          color="text.secondary"
        >
          {description}
        </Typography>
      )}
      <ButtonGroup 
        buttons={buttons} 
        color={color} 
        aria-label={`${title} actions`}
      />
    </Box>
  );
};

export const ButtonGroup = ({
  buttons,
  color = 'primary',
  'aria-label': ariaLabel,
}: {
  buttons: Array<{ label: string; path: string }>;
  color?: 'primary' | 'secondary';
  'aria-label'?: string;
}) => (
  <Box
    sx={{ 
      display: 'flex', 
      gap: 2, 
      flexWrap: 'wrap', 
      mt: 1 
    }}
    role="group"
    aria-label={ariaLabel}
  >
    {buttons.map((button, index) => (
      <AccessibleButton 
        key={index} 
        to={button.path} 
        color={color}
      >
        {button.label}
      </AccessibleButton>
    ))}
  </Box>
);

export const OverviewItem = ({
  title,
  value,
  icon,
}: {
  title: string;
  value: string;
  icon?: React.ReactNode;
}) => (
  <Paper
    component="article"
    sx={{ 
      p: 3, 
      mb: 2, 
      display: 'flex', 
      justifyContent: 'space-between', 
      alignItems: 'center',
      borderRadius: 1,
      backgroundColor: 'background.paper',
      border: '1px solid',
      borderColor: 'divider',
    }}
  >
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
      {icon && (
        <Box 
          sx={{ 
            color: 'primary.main',
            display: 'flex',
            alignItems: 'center'
          }}
          aria-hidden="true"
        >
          {icon}
        </Box>
      )}
      <Box>
        <Typography 
          variant="h3" 
          component="h3"
          sx={{ 
            fontSize: '1rem',
            fontWeight: 'medium',
            color: 'text.primary'
          }}
        >
          {title}
        </Typography>
        <Typography 
          variant="body2" 
          sx={{ mt: 0.5 }}
          color="text.secondary"
        >
          {value}
        </Typography>
      </Box>
    </Box>
  </Paper>
);