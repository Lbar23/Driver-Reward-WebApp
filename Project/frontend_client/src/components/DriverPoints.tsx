import React, { useState, useEffect } from 'react';
import { Typography, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper } from '@mui/material';
import axios from 'axios';

interface DriverPoint {
    driverId: number;
    driverName: string;
    points: number;
}

const DriverPoints: React.FC = () => {
    const [driverPoints, setDriverPoints] = useState<DriverPoint[]>([]);

    useEffect(() => {
        const fetchDriverPoints = async () => {
            try {
                const response = await axios.get('/api/driver-points', {
                    headers: { Authorization: `Bearer ${localStorage.getItem('userToken')}` }
                });
                setDriverPoints(response.data);
            } catch (error) {
                console.error('Error fetching driver points:', error);
            }
        };

        fetchDriverPoints();
    }, []);

    return (
        <div>
            <Typography variant="h4" gutterBottom>
                Driver Points
            </Typography>
            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Driver ID</TableCell>
                            <TableCell>Driver Name</TableCell>
                            <TableCell align="right">Points</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {driverPoints.map((driver) => (
                            <TableRow key={driver.driverId}>
                                <TableCell>{driver.driverId}</TableCell>
                                <TableCell>{driver.driverName}</TableCell>
                                <TableCell align="right">{driver.points}</TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        </div>
    );
};

export default DriverPoints;