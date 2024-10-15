import React, { useEffect, useState } from 'react';
import '../pages/About.css';

// Define the type for the about information
interface AboutInfo {
    team: string;
    version: string;
    release: string;
    product: string;
    description: string;
}

const About: React.FC = () => {
    const [aboutInfo, setAboutInfo] = useState<AboutInfo[] | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchAboutInfo = async () => {
            try {
                const response = await fetch('http://3.136.81.78:5000/api/About');
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                const data: AboutInfo[] = await response.json();
                setAboutInfo(data);
            } catch (error) {
                if (error instanceof Error) {
                    setError(error.message);
                } else {
                    setError('An unknown error occurred');
                }
            }
        };

        fetchAboutInfo();
    }, []);

    if (error) return <div>Error: {error}</div>;
    if (!aboutInfo || aboutInfo.length === 0) return <div>Loading...</div>;

    return (
        <div className="about-container">
            <h1>About Information</h1>
            {aboutInfo.map((info, index) => (
                <div key={index} className="about-box">
                    <p><strong>Team:</strong> {info.team}</p>
                    <p><strong>Version:</strong> {info.version}</p>
                    <p><strong>Release Date:</strong> {new Date(info.release).toLocaleDateString()}</p>
                    <p><strong>Product:</strong> {info.product}</p>
                    <p><strong>Description:</strong> {info.description}</p>
                </div>
            ))}
        </div>
    );
};

export default About;