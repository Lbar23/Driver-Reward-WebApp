import React, { createContext, useContext, useState, ReactNode, useCallback } from 'react';
import axios from 'axios';

interface User {
  id: number;
  userName: string;
  email: string;
  userType: 'Admin' | 'Driver' | 'Sponsor' | 'Guest';
}

interface ViewContextType {
  currentView: string;
  setCurrentView: React.Dispatch<React.SetStateAction<string>>;
  isImpersonating: boolean;
  impersonatedUser: User | null;
  fetchUsersByRole: (role: string) => Promise<User[]>;
  impersonateUser: (userId: number) => Promise<void>;
  stopImpersonation: () => Promise<void>;
}

const ViewContext = createContext<ViewContextType | undefined>(undefined);

export const useView = (): ViewContextType => {
  const context = useContext(ViewContext);
  if (!context) {
    throw new Error('useView must be used within a ViewProvider');
  }
  return context;
};

export const ViewProvider = ({ children }: { children: ReactNode }) => {
  const [currentView, setCurrentView] = useState<string>('MAIN');
  const [impersonatedUser, setImpersonatedUser] = useState<User | null>(null);
  const [isImpersonating, setIsImpersonating] = useState<boolean>(false);

  
  // Fetch users by role
  const fetchUsersByRole = useCallback(async (role: string): Promise<User[]> => {
    const endpointMap: Record<string, string> = {
      Driver: '/api/admin/drivers/details',
      Sponsor: '/api/admin/sponsors/details',
      Admin: '/api/admin/admins/details',
    };
    const endpoint = endpointMap[role];
    if (!endpoint) throw new Error('Invalid role');
    const response = await axios.get(endpoint, { withCredentials: true });
    return response.data.map((user: any) => ({
      id: user.userId,
      userName: user.name,
      email: user.email,
      userType: role,
    }));
  }, []);

  // Start impersonating a user
  const impersonateUser = useCallback(async (userId: number) => {
    try {
      const response = await axios.post(
        '/api/admin/impersonate-user',
        { userId },
        { withCredentials: true }
      );
      setImpersonatedUser(response.data); // Update impersonated user
      setIsImpersonating(true); // Set impersonation state
    } catch (error) {
      console.error('Failed to impersonate user:', error);
      throw new Error('Failed to impersonate user');
    }
  }, []);

  // Stop impersonation
  const stopImpersonation = useCallback(async () => {
    try {
      await axios.post('/api/admin/stop-impersonation', {}, { withCredentials: true });
      setImpersonatedUser(null); // Clear impersonated user
      setIsImpersonating(false); // Clear impersonation state
    } catch (error) {
      console.error('Failed to stop impersonation:', error);
      throw new Error('Failed to stop impersonation');
    }
  }, []);

  return (
    <ViewContext.Provider
      value={{
        currentView,
        setCurrentView,
        isImpersonating,
        impersonatedUser,
        fetchUsersByRole,
        impersonateUser,
        stopImpersonation,
      }}
    >
      {children}
    </ViewContext.Provider>
  );
};
