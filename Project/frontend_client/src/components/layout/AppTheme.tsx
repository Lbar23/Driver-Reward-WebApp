import React, { createContext, useContext, useState, useMemo } from 'react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { deepmerge } from '@mui/utils';
import { getDesignTokens, getAccessibilityTokens } from '../../theme/themePrimitives';

interface AppThemeContextProps {
  fontSize: number;
  setFontSize: (size: number) => void;
  accessibleTokens: any;
  setAccessibleTokens: (tokens: any) => void;
}

const AppThemeContext = createContext<AppThemeContextProps | undefined>(undefined);

export const useAppTheme = () => {
  const context = useContext(AppThemeContext);
  if (!context) {
    throw new Error('useAppTheme must be used within an AppThemeProvider');
  }
  return context;
};

const AppThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [fontSize, setFontSize] = useState<number>(14); // Default font size
  const [accessibleTokens, setAccessibleTokens] = useState<any>(getAccessibilityTokens('default')); // Default profile

  const theme = useMemo(() => {
    const baseTokens = getDesignTokens('light', fontSize); // Default light mode
    const mergedTokens = deepmerge(baseTokens, accessibleTokens || {});

    return createTheme({
      ...mergedTokens,
      cssVariables: {
        colorSchemeSelector: 'data-mui-color-scheme',
        cssVarPrefix: 'template', // Ensure CSS variables stay intact
      },
    });
  }, [fontSize, accessibleTokens]);

  const value = useMemo(
    () => ({
      fontSize,
      setFontSize,
      accessibleTokens,
      setAccessibleTokens,
    }),
    [fontSize, accessibleTokens]
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
