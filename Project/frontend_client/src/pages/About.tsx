import React, { useEffect, useState } from 'react';
import { 
  Typography, 
  Box, 
  Container, 
  Paper, 
  Grid, 
  Fade, 
  Skeleton 
} from '@mui/material';
import { Info as InfoIcon, Build as BuildIcon, CalendarToday as CalendarIcon } from '@mui/icons-material';
import axios from 'axios';

interface AboutInfo {
    team: string;
    version: number;
    release: Date;
    product: string;
    description: string;
}

const AboutInfoItem: React.FC<{ 
  icon: React.ReactNode, 
  title: string, 
  value: string 
}> = ({ icon, title, value }) => (
  <Grid 
    item 
    xs={12} 
    sx={{
      display: 'flex',
      alignItems: 'center',
      p: 2,
      borderBottom: '1px solid',
      borderColor: 'divider',
      transition: 'background-color 0.3s ease',
      '&:hover': {
        backgroundColor: 'action.hover'
      }
    }}
  >
    <Box sx={{ 
      mr: 3, 
      color: 'primary.main',
      display: 'flex',
      alignItems: 'center'
    }}>
      {icon}
    </Box>
    <Box>
      <Typography variant="subtitle1" fontWeight="bold" color="text.secondary">
        {title}
      </Typography>
      <Typography variant="body1">{value}</Typography>
      </Box>
    </Grid>
);

const About: React.FC = () => {
    const [aboutInfo, setAboutInfo] = useState<AboutInfo | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState<boolean>(true);

    useEffect(() => {
        const fetchAboutInfo = async () => {
            try {
                const response = await axios.get('/api/system/about', { withCredentials: true });
                setAboutInfo(response.data);
            } catch (error: any) {
                setError(error.message);
            } finally {
                setLoading(false);
            }
        };
        fetchAboutInfo();
    }, []);

    if (error) return (
        <Container maxWidth="sm">
            <Paper 
                elevation={3} 
                sx={{ 
                    p: 3, 
                    mt: 4, 
                    textAlign: 'center',
                    backgroundColor: 'error.light',
                    color: 'error.contrastText'
                }}
            >
                <Typography variant="h6">Error Loading Information</Typography>
                <Typography>{error}</Typography>
            </Paper>
        </Container>
    );

    return (
        <Container maxWidth="md">
            <Fade in={!loading} timeout={500}>
                <Paper 
                    elevation={4} 
                    sx={{ 
                        p: 4, 
                        mt: 4,
                        borderRadius: 2,
                        background: 'linear-gradient(145deg, #f0f4f8 0%, #ffffff 100%)'
                    }}
                >
                    <Typography 
                        variant="h4" 
                        component="h1" 
                        gutterBottom 
                        sx={{ 
                            textAlign: 'center', 
                            mb: 4,
                            fontWeight: 600,
                            color: 'primary.main'
                        }}
                    >
                        About Our Product
                    </Typography>

                    {loading ? (
                        <>
                            <Skeleton variant="rectangular" width="100%" height={60} sx={{ mb: 2 }} />
                            <Skeleton variant="rectangular" width="100%" height={60} sx={{ mb: 2 }} />
                            <Skeleton variant="rectangular" width="100%" height={60} />
                        </>
                    ) : (
                        <Grid container spacing={2}>
                            <AboutInfoItem 
                                icon={<InfoIcon />} 
                                title="Team" 
                                value={aboutInfo?.team || 'N/A'} 
                            />
                            <AboutInfoItem 
                                icon={<BuildIcon />} 
                                title="Version" 
                                value={aboutInfo?.version.toString() || 'N/A'} 
                            />
                            <AboutInfoItem 
                                icon={<CalendarIcon />} 
                                title="Release Date" 
                                value={aboutInfo ? new Date(aboutInfo.release).toLocaleDateString() : 'N/A'} 
                            />
                            <AboutInfoItem 
                                icon={<InfoIcon />} 
                                title="Product" 
                                value={aboutInfo?.product || 'N/A'} 
                            />
                            <Grid 
                                item 
                                xs={12} 
                                sx={{ 
                                    p: 2, 
                                    backgroundColor: 'background.default',
                                    borderRadius: 1
                                }}
                            >
                                <Typography variant="h6" gutterBottom>
                                    Description
                                </Typography>
                                <Typography variant="body1">
                                    {aboutInfo?.description || 'No description available.'}
                                </Typography>
                            </Grid>
                        </Grid>
                    )}
                </Paper>
            </Fade>
        </Container>
    );
};

export default About;