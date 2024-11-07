import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import axios from 'axios';

// User interface definition
interface User {
  id: number;
  userName: string;
  email: string;
  userType: 'Admin' | 'Driver' | 'Sponsor' | 'Guest';
  createdAt: string;
  lastLogin: string;
  roles: string[];
}

// Context properties for authentication handling
interface AuthContextProps {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  viewRole: string | null;
  setViewRole: (role: string | null) => void;
  checkAuth: () => Promise<void>;
  logout: () => Promise<void>;
}

// Initialize the AuthContext
const AuthContext = createContext<AuthContextProps | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  // Retrieve session data for user and viewRole
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(() => {
    return sessionStorage.getItem('isAuthenticated') === 'true';
  });

  const [user, setUser] = useState<User | null>(() => {
    const cachedUser = sessionStorage.getItem('user');
    return cachedUser ? JSON.parse(cachedUser) : null;
  });

  const [viewRole, setViewRoleState] = useState<string | null>(() => {
    return sessionStorage.getItem('viewRole') || null;
  });

  const [isLoading, setIsLoading] = useState<boolean>(!isAuthenticated);

  // Function to handle view role updates and storage
  const setViewRole = useCallback((role: string | null) => {
    setViewRoleState(role);
    if (role) {
      sessionStorage.setItem('viewRole', role);
    } else {
      sessionStorage.removeItem('viewRole');
    }
  }, []);

  useEffect(() => {
    if (!isAuthenticated) {
      checkAuth();
    } else {
      setIsLoading(false);
    }
  }, []);

  // Checks the user authentication status from the API
  const checkAuth = async () => {
    setIsLoading(true);
    try {
      const response = await axios.get('/api/user/currentuser', { withCredentials: true });
      setUser(response.data);
      setIsAuthenticated(true);

      sessionStorage.setItem('user', JSON.stringify(response.data));
      sessionStorage.setItem('isAuthenticated', 'true');
    } catch (error) {
      setUser(null);
      setIsAuthenticated(false);
      sessionStorage.removeItem('user');
      sessionStorage.removeItem('isAuthenticated');
    } finally {
      setIsLoading(false);
    }
  };

  // Logs out the user, clears session data, and redirects to login
  const logout = async () => {
    try {
      await axios.post('/api/user/logout', {}, { withCredentials: true });
      setUser(null);
      setIsAuthenticated(false);
      setViewRole(null); // Reset viewRole on logout
      sessionStorage.clear(); // Clear all session data
    } catch (error) {
      console.error('Logout failed:', error);
    }
  };

  return (
    <AuthContext.Provider value={
      { user, 
      isAuthenticated, 
      isLoading, 
      viewRole, 
      setViewRole, 
      checkAuth, 
      logout }}>
      {children}
    </AuthContext.Provider>
  );
};

// Custom hook for easy access to AuthContext
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within an AuthProvider');
  return context;
};
