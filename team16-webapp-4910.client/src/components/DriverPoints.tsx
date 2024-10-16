import React, { useContext } from 'react';
import { Typography, Card, CardContent } from '@mui/material';
import { UserContext } from '../App';

const DriverPoints = ({ isDriverView = false }) => {
  const { userType } = useContext(UserContext);

  // Placeholder data - replace with actual data fetching logic
  const driverData = {
    name: 'John Doe',
    points: 1000,
    rank: 5,
  };

  return (
    <div>
      <Typography variant="h4">
        {isDriverView ? 'Driver View' : 'Driver Points'}
      </Typography>
      {(userType === 'driver' || isDriverView) && (
        <Card>
          <CardContent>
            <Typography variant="h6">{driverData.name}</Typography>
            <Typography>Points: {driverData.points}</Typography>
            <Typography>Rank: {driverData.rank}</Typography>
          </CardContent>
        </Card>
      )}
      {isDriverView && (userType === 'admin' || userType === 'sponsor') && (
        <Typography>
          This is the driver's view of the system. Use this to troubleshoot or test the system.
        </Typography>
      )}
    </div>
  );
};

export default DriverPoints;