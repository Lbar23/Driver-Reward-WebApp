import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import axios from 'axios';

// Interface definitions
interface SponsorDetails {
  sponsorID: number;
  companyName: string;
  IsPrimary: boolean;
  joinDate: string;
  sponsorRole: number;
}

interface User {
  id: number;
  userName: string;
  email: string;
  userType: 'Admin' | 'Driver' | 'Sponsor' | 'Guest';
  createdAt: string;
  lastLogin: string;
  roles: string[];
  sponsorDetails?: SponsorDetails;
}

interface NotifySettings {
  purchaseConfirmation: boolean;
  pointEarned: boolean;
  pointBalanceDrop: boolean;
  orderIssue: boolean;
  systemDrop: boolean;
  applicationApproved: boolean;
}

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

const defaultNotifySettings: NotifySettings = {
  purchaseConfirmation: true,
  pointEarned: true,
  pointBalanceDrop: true,
  orderIssue: true,
  systemDrop: true,
  applicationApproved: true,
};

// Create context with undefined check
const AuthContext = createContext<AuthContextProps | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  // State management with proper initial values
  const [isAuthenticated, setIsAuthenticated] = useState(() => 
    sessionStorage.getItem('isAuthenticated') === 'true'
  );
  
  const [user, setUser] = useState<User | null>(() => {
    const savedUser = sessionStorage.getItem('user');
    try {
      return savedUser ? JSON.parse(savedUser) : null;
    } catch {
      return null;
    }
  });

  const [viewRole, setViewRoleState] = useState<string | null>(() => 
    sessionStorage.getItem('viewRole') || null
  );

  const [isLoading, setIsLoading] = useState(!isAuthenticated);

  const [notifySettings, setNotifySettings] = useState<NotifySettings>(() => {
    const savedSettings = localStorage.getItem('notifySettings');
    try {
      return savedSettings ? JSON.parse(savedSettings) : defaultNotifySettings;
    } catch {
      return defaultNotifySettings;
    }
  });

  // Handle view role changes
  const setViewRole = useCallback((role: string | null) => {
    setViewRoleState(role);
    if (role) {
      sessionStorage.setItem('viewRole', role);
    } else {
      sessionStorage.removeItem('viewRole');
    }
  }, []);

  // Update notification settings
  const updateNotifySetting = useCallback((key: keyof NotifySettings, value: boolean) => {
    if (key === 'systemDrop' || key === 'applicationApproved') return;

    setNotifySettings(prev => {
      const updated = { ...prev, [key]: value };
      localStorage.setItem('notifySettings', JSON.stringify(updated));
      return updated;
    });
  }, []);

  // Check authentication status
  const checkAuth = useCallback(async () => {
    setIsLoading(true);
    try {
      const { data } = await axios.get<User>('/api/user/currentuser', { 
        withCredentials: true 
      });

      // Ensure proper typing of user data
      const currentUser: User = {
        ...data,
        sponsorDetails: data.userType === 'Sponsor' ? {
          sponsorID: data.sponsorDetails?.sponsorID ?? 0,
          companyName: data.sponsorDetails?.companyName ?? '',
          IsPrimary: data.sponsorDetails?.IsPrimary ?? false,
          joinDate: data.sponsorDetails?.joinDate ?? '',
          sponsorRole: data.sponsorDetails?.sponsorRole ?? 0,
        } : undefined
      };

      setUser(currentUser);
      setIsAuthenticated(true);
      sessionStorage.setItem('user', JSON.stringify(currentUser));
      sessionStorage.setItem('isAuthenticated', 'true');
    } catch (error) {
      console.error('Auth check failed:', error);
      handleLogout();
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Handle logout
  const handleLogout = useCallback(() => {
    setUser(null);
    setIsAuthenticated(false);
    setViewRole(null);
    sessionStorage.clear();
  }, [setViewRole]);

  const logout = useCallback(async () => {
    if (!user?.userType) {
      handleLogout();
      return;
    }

    try {
      await axios.post('/api/system/logout', {}, { 
        withCredentials: true 
      });
    } catch (error) {
      console.error('Logout failed:', error);
    } finally {
      handleLogout();
    }
  }, [user, handleLogout]);

  // Initial auth check
  useEffect(() => {
    if (!isAuthenticated) {
      checkAuth();
    } else {
      setIsLoading(false);
    }
  }, [isAuthenticated, checkAuth]);

  // Provide context value
  const contextValue = {
    user,
    isAuthenticated,
    isLoading,
    viewRole,
    setViewRole,
    checkAuth,
    logout,
    notifySettings,
    updateNotifySetting,
  };

  return (
    <AuthContext.Provider value={contextValue}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

// Optional: Export types for use in other components
export type { User, SponsorDetails, NotifySettings, AuthContextProps };