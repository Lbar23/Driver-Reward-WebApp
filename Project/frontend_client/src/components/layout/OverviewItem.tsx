import * as React from 'react';
import { Typography, Box } from '@mui/material';

interface OverviewItemProps {
  title: string;
  value: string;
}

const OverviewItem: React.FC<OverviewItemProps> = ({ title, value }) => {
  return (
    <Box sx={{ mb: 1 }}>
      <Typography variant="body1" component="span" fontWeight="bold">
        {title}:
      </Typography>
      <Typography variant="body1" component="span" sx={{ ml: 1 }}>
        {value}
      </Typography>
    </Box>
  );
};

export default OverviewItem;
