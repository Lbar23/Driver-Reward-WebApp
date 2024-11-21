import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import axios from 'axios';

// SponsorDetails interface
interface SponsorDetails {
  sponsorID: number;
  companyName: string;
  isPrimarySponsor: boolean;
  joinDate: string;
  sponsorRole: number;
}

// User interface definition
interface User {
  id: number;
  userName: string;
  email: string;
  userType: 'Admin' | 'Driver' | 'Sponsor' | 'Guest';
  createdAt: string;
  lastLogin: string;
  roles: string[];
  sponsorDetails?: SponsorDetails; // Optional field for Sponsor users
}

interface NotifySettings {
  purchaseConfirmation: boolean;
  pointEarned: boolean;
  pointBalanceDrop: boolean;
  orderIssue: boolean;
  systemDrop: boolean;
  applicationApproved: boolean;
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
  notifySettings: NotifySettings;
  updateNotifySetting: (key: keyof NotifySettings, value: boolean) => void;
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
    
  // Initial notification settings, with local storage fallback
  const defaultNotifySettings: NotifySettings = {
    purchaseConfirmation: true,
    pointEarned: true,
    pointBalanceDrop: true,
    orderIssue: true,
    systemDrop: true, // Cannot be disabled
    applicationApproved: true, // Cannot be disabled
  };

  const [notifySettings, setNotifySettings] = useState<NotifySettings>(() => {
    const savedSettings = localStorage.getItem('notifySettings');
    return savedSettings ? JSON.parse(savedSettings) : defaultNotifySettings;
  });

  // Function to update notification settings
  const updateNotifySetting = (key: keyof NotifySettings, value: boolean) => {
    // Prevent disabling 'systemDrop' and 'applicationApproved'
    if (key === 'systemDrop' || key === 'applicationApproved') return;

    setNotifySettings((prevSettings) => {
      const updatedSettings = { ...prevSettings, [key]: value };
      localStorage.setItem('notifySettings', JSON.stringify(updatedSettings));
      return updatedSettings;
    });
  };



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
      const currentUser = response.data;
      // Check if the user is a sponsor and has sponsorDetails
      if (response.data.userType === 'Sponsor' && response.data.sponsorDetails) {
        currentUser.sponsorDetails = {
          sponsorID: currentUser.sponsorDetails.sponsorID,
          companyName: currentUser.sponsorDetails.companyName,
          isPrimarySponsor: currentUser.sponsorDetails.isPrimarySponsor,
          joinDate: currentUser.sponsorDetails.joinDate,
          sponsorRole: currentUser.sponsorDetails.sponsorRole,
        };
      }
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
    // Check if userType is null
    if (!user || !user.userType) {
      sessionStorage.clear();
      setViewRole(null);
      setIsAuthenticated(false);
      console.warn('User type is already null. Skipping logout process.');
      return;
    }

    try {
      await axios.post('/api/system/logout', {}, { withCredentials: true });
      setUser(null);
      setIsAuthenticated(false);
      setViewRole(null); // Reset viewRole on logout
      sessionStorage.clear(); // Clear all session data
    } 
    catch (error) {
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
      logout,
      notifySettings,
      updateNotifySetting }}>
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
