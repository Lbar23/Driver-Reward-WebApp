import React, { useEffect, useState } from 'react';
import { Typography, Box, CircularProgress } from '@mui/material';
import axios from 'axios';

// define the type for the about information
interface AboutInfo {
    team: string;
    version: number;
    release: Date;
    product: string;
    description: string;
}

const About: React.FC = () => {
    const [aboutInfo, setAboutInfo] = useState<AboutInfo | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState<boolean>(true);

    useEffect(() => {
        const fetchAboutInfo = async () => {
            try {
                const response = await axios.get('/api/admin/about');
                
                console.log('API Response:', response.data); 
                setAboutInfo(response.data);
            } catch (error: any) {
                setError(error.message);
            } finally {
                setLoading(false);
            }
        };
        fetchAboutInfo();
    }, []);

    if (loading) return <CircularProgress />;
    if (error) return <Typography color="error">Error: {error}</Typography>;
    if (!aboutInfo) return <Typography>No information available.</Typography>;

    return (
        <Box 
          sx={{
            maxWidth: 600, 
            margin: 'auto', 
            padding: 3, 
            boxShadow: 3, 
            borderRadius: 2, 
            backgroundColor: 'background.paper'
          }}
        >
            <Typography variant="h4" gutterBottom>About Information</Typography>
            <Box sx={{ p: 2, mb: 2, border: '1px solid #ddd', borderRadius: 2 }}>
                <Typography variant="body1"><strong>Team:</strong> {aboutInfo.team}</Typography>
                <Typography variant="body1"><strong>Version:</strong> {aboutInfo.version}</Typography>
                <Typography variant="body1"><strong>Release Date:</strong> {new Date(aboutInfo.release).toLocaleDateString()}</Typography>
                <Typography variant="body1"><strong>Product:</strong> {aboutInfo.product}</Typography>
                <Typography variant="body1"><strong>Description:</strong> {aboutInfo.description}</Typography>
            </Box>
        </Box>
    );

};

export default About;
