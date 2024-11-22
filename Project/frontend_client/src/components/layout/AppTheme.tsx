import React, { createContext, useContext, useState, useMemo, useEffect } from 'react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { deepmerge } from '@mui/utils';
import { getDesignTokens, getAccessibilityTokens, colorSchemes } from '../../theme/themePrimitives';

interface AppThemeContextProps {
  fontSize: number;
  setFontSize: (size: number) => void;
  accessibilityProfile: string;
  setAccessibilityProfile: (profile: string) => void;
}

const LOCAL_STORAGE_KEY = 'appThemeSettings';

const AppThemeContext = createContext<AppThemeContextProps | undefined>(undefined);

export const useAppTheme = () => {
  const context = useContext(AppThemeContext);
  if (!context) {
    throw new Error('useAppTheme must be used within an AppThemeProvider');
  }
  return context;
};

const AppThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  // State initialization with defaults and local storage retrieval
  const [fontSize, setFontSize] = useState<number>(() => {
    try {
      const savedSettings = localStorage.getItem(LOCAL_STORAGE_KEY);
      return savedSettings ? JSON.parse(savedSettings).fontSize : 14; // Default font size
    } catch {
      return 14; // Fallback to default if local storage is corrupted
    }
  });

  const [accessibilityProfile, setAccessibilityProfile] = useState<string>(() => {
    try {
      const savedSettings = localStorage.getItem(LOCAL_STORAGE_KEY);
      return savedSettings ? JSON.parse(savedSettings).accessibilityProfile : 'default'; // Default profile
    } catch {
      return 'default'; // Fallback to default
    }
  });

  // Persist settings to local storage whenever they change
  useEffect(() => {
    const settings = { fontSize, accessibilityProfile };
    localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(settings));
  }, [fontSize, accessibilityProfile]);

  // Generate theme dynamically based on current settings
  const theme = useMemo(() => {
    const baseTokens = getDesignTokens('light', fontSize); // Default light mode
    const accessibleTokens = getAccessibilityTokens(accessibilityProfile);
    const mergedTokens = deepmerge(baseTokens, accessibleTokens || {});

    return createTheme({
      ...mergedTokens,
      cssVariables: {
        colorSchemeSelector: 'data-mui-color-scheme',
        cssVarPrefix: 'template',
      },
      colorSchemes,
    });
  }, [fontSize, accessibilityProfile]);
  
  const value = useMemo(
    () => ({
      fontSize,
      setFontSize,
      accessibilityProfile,
      setAccessibilityProfile,
    }),
    [fontSize, accessibilityProfile]
  );

  return (
    <AppThemeContext.Provider value={value}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        {children}
      </ThemeProvider>
    </AppThemeContext.Provider>
  );
};

export default AppThemeProvider;
