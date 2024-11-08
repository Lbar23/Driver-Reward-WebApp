import React, { createContext, useContext, useState, ReactNode } from 'react';

interface ViewContextType {
  currentView: string;
  setCurrentView: React.Dispatch<React.SetStateAction<string>>;
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

  return (
    <ViewContext.Provider value={{ currentView, setCurrentView }}>
      {children}
    </ViewContext.Provider>
  );
};
