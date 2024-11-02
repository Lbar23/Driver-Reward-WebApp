
import React, { useState, useEffect } from 'react';
import { Typography, Box, CircularProgress } from '@mui/material';
import axios from 'axios';

// Define interfaces for our data types
interface User {
    id: string;
    userName: string;
    email: string;
    userType: string;
    lastLogin?: Date;
}

const ManageUsers: React.FC = () => {
    const [users, setUsers] = useState<User[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState<boolean>(true);

    useEffect(() => {
        console.log("testing, does this page load")
        const fetchUsers = async () => {
            try {
                console.log('Fetching users...');
                const response = await axios.get('/api/user/all');
                console.log('Users response:', response.data);
                setUsers(response.data);
            } catch (error: any) {
                console.error('Error fetching users:', error);
                setError(error.message);
            } finally {
                setLoading(false);
            }
        };

        fetchUsers();
    }, []);

    const handleChangeUserType = async (userId: string, newUserType: string) => {
        try {
            await axios.post('/api/user/change-user-type', { userId, newUserType });
            // Refresh the users list
            const response = await axios.get('/api/user/all');
            setUsers(response.data);
        } catch (error: any) {
            setError(error.message);
        }
    };

    const handleRemoveUser = async (userId: string) => {
        try {
            await axios.delete(`/api/user/remove-user/${userId}`);
            // Refresh the users list
            const response = await axios.get('/api/user/all');
            setUsers(response.data);
        } catch (error: any) {
            setError(error.message);
        }
    };

    if (loading) return (
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
            <CircularProgress />
        </Box>
    );

    if (error) return (
        <Typography color="error">Error: {error}</Typography>
    );

    if (users.length === 0) return (
        <Typography>No users found.</Typography>
    );

    return (
        <Box sx={{ maxWidth: 800, margin: 'auto', padding: 3, boxShadow: 3, borderRadius: 2, backgroundColor: 'background.paper' }}>
            <Typography variant="h4" gutterBottom>Admin Dashboard</Typography>
            <Typography variant="h6" gutterBottom>User Management</Typography>
            
            <Box sx={{ mt: 3 }}>
                {users.map((user) => (
                    <Box key={user.id} sx={{ p: 2, mb: 2, border: '1px solid #ddd', borderRadius: 2 }}>
                        <Typography variant="body1"><strong>Username:</strong> {user.userName}</Typography>
                        <Typography variant="body1"><strong>Email:</strong> {user.email}</Typography>
                        <Typography variant="body1"><strong>Type:</strong> {user.userType}</Typography>
                        {user.lastLogin && (
                            <Typography variant="body1">
                                <strong>Last Login:</strong> {new Date(user.lastLogin).toLocaleDateString()}
                            </Typography>
                        )}
                        <Box sx={{ mt: 2 }}>
                            <select
                                value={user.userType}
                                onChange={(e) => handleChangeUserType(user.id, e.target.value)}
                                style={{ marginRight: '10px', padding: '5px' }}
                            >
                                <option value="Driver">Driver</option>
                                <option value="Sponsor">Sponsor</option>
                                <option value="Admin">Admin</option>
                            </select>
                            <button
                                onClick={() => handleRemoveUser(user.id)}
                                style={{ padding: '5px 10px', backgroundColor: '#ff4444', color: 'white', border: 'none', borderRadius: '4px' }}
                            >
                                Remove User
                            </button>
                        </Box>
                    </Box>
                ))}
            </Box>
        </Box>
    );
};

export default ManageUsers;
