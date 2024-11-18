import React, { useEffect, useState } from 'react';
import { Typography, Box, CircularProgress } from '@mui/material';
import axios from 'axios';
import OverviewItem from '../components/layout/OverviewItem';

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

    //Add another clean check here...
    useEffect(() => {
        const fetchAboutInfo = async () => {
            try {
                const response = await axios.get('/api/system/about', { withCredentials: true});
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
            p: 3, 
            boxShadow: 3, 
            borderRadius: 2, 
            backgroundColor: 'background.paper'
          }}
        >
            <Typography variant="h4" gutterBottom>About Information</Typography>
            <Box sx={{ p: 2, mb: 2, border: 1, borderColor: 'divider', borderRadius: 2 }}>
                <OverviewItem title="Team" value={aboutInfo.team} />
                <OverviewItem title="Version" value={aboutInfo.version.toString()} />
                <OverviewItem title="Release Date" value={new Date(aboutInfo.release).toLocaleDateString()} />
                <OverviewItem title="Product" value={aboutInfo.product} />
                <OverviewItem title="Description" value={aboutInfo.description} />
            </Box>
        </Box>
    );
};

export default About;
