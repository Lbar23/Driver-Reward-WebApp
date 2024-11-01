import * as React from 'react';
import { Box, Typography, Button } from '@mui/material';
import { Link } from 'react-router-dom';

interface ButtonInfo {
  label: string;
  path: string;
}

interface SectionProps {
  title: string;
  buttons: ButtonInfo[];
}

const Section: React.FC<SectionProps> = ({ title, buttons }) => {
  return (
    <Box sx={{ mt: 3 }}>
      <Typography variant="h6" gutterBottom>
        {title}
      </Typography>
      {buttons.map((button, index) => (
        <Button
          key={index}
          component={Link}
          to={button.path}
          variant="contained"
          sx={{ m: 1 }}
        >
          {button.label}
        </Button>
      ))}
    </Box>
  );
};

export default Section;
