import { createTheme, 
         alpha, 
         PaletteMode, 
         Shadows } from '@mui/material/styles';
import { deepmerge } from '@mui/utils';

declare module '@mui/material/Paper' {
  interface PaperPropsVariantOverrides {
    highlighted: true;
  }
}
declare module '@mui/material/styles/createPalette' {
  interface ColorRange {
    50: string;
    100: string;
    200: string;
    300: string;
    400: string;
    500: string;
    600: string;
    700: string;
    800: string;
    900: string;
  }

  interface PaletteColor extends ColorRange {}

  interface Palette {
    baseShadow: string;
  }
}

const defaultTheme = createTheme();

const customShadows: Shadows = [...defaultTheme.shadows];

export const brand = {
  50: 'hsl(210, 100%, 95%)',
  100: 'hsl(210, 100%, 92%)',
  200: 'hsl(210, 100%, 80%)',
  300: 'hsl(210, 100%, 65%)',
  400: 'hsl(210, 98%, 48%)',
  500: 'hsl(210, 98%, 42%)',
  600: 'hsl(210, 98%, 55%)',
  700: 'hsl(210, 100%, 35%)',
  800: 'hsl(210, 100%, 16%)',
  900: 'hsl(210, 100%, 21%)',
};

export const gray = {
  50: 'hsl(220, 35%, 97%)',
  100: 'hsl(220, 30%, 94%)',
  200: 'hsl(220, 20%, 88%)',
  300: 'hsl(220, 20%, 80%)',
  400: 'hsl(220, 20%, 65%)',
  500: 'hsl(220, 20%, 42%)',
  600: 'hsl(220, 20%, 35%)',
  700: 'hsl(220, 20%, 25%)',
  800: 'hsl(220, 30%, 6%)',
  900: 'hsl(220, 35%, 3%)',
};

export const green = {
  50: 'hsl(120, 80%, 98%)',
  100: 'hsl(120, 75%, 94%)',
  200: 'hsl(120, 75%, 87%)',
  300: 'hsl(120, 61%, 77%)',
  400: 'hsl(120, 44%, 53%)',
  500: 'hsl(120, 59%, 30%)',
  600: 'hsl(120, 70%, 25%)',
  700: 'hsl(120, 75%, 16%)',
  800: 'hsl(120, 84%, 10%)',
  900: 'hsl(120, 87%, 6%)',
};

export const orange = {
  50: 'hsl(45, 100%, 97%)',
  100: 'hsl(45, 92%, 90%)',
  200: 'hsl(45, 94%, 80%)',
  300: 'hsl(45, 90%, 65%)',
  400: 'hsl(45, 90%, 40%)',
  500: 'hsl(45, 90%, 35%)',
  600: 'hsl(45, 91%, 25%)',
  700: 'hsl(45, 94%, 20%)',
  800: 'hsl(45, 95%, 16%)',
  900: 'hsl(45, 93%, 12%)',
};

export const red = {
  50: 'hsl(0, 100%, 97%)',
  100: 'hsl(0, 92%, 90%)',
  200: 'hsl(0, 94%, 80%)',
  300: 'hsl(0, 90%, 65%)',
  400: 'hsl(0, 90%, 40%)',
  500: 'hsl(0, 90%, 30%)',
  600: 'hsl(0, 91%, 25%)',
  700: 'hsl(0, 94%, 18%)',
  800: 'hsl(0, 95%, 12%)',
  900: 'hsl(0, 93%, 6%)',
};


export const getDesignTokens = (mode: PaletteMode, fontSize: number = 14) => {
  customShadows[1] =
    mode === 'dark'
      ? 'hsla(220, 30%, 5%, 0.7) 0px 4px 16px 0px, hsla(220, 25%, 10%, 0.8) 0px 8px 16px -5px'
      : 'hsla(220, 30%, 5%, 0.07) 0px 4px 16px 0px, hsla(220, 25%, 10%, 0.07) 0px 8px 16px -5px';

  const baseTypography = {
    h1: { fontSize: `${fontSize * 3.43}px`, fontWeight: 600, lineHeight: 1.2 },
    h2: { fontSize: `${fontSize * 2.57}px`, fontWeight: 600, lineHeight: 1.2 },
    h3: { fontSize: `${fontSize * 2.14}px`, lineHeight: 1.2 },
    h4: { fontSize: `${fontSize * 1.71}px`, fontWeight: 600, lineHeight: 1.5 },
    h5: { fontSize: `${fontSize * 1.43}px`, fontWeight: 600 },
    h6: { fontSize: `${fontSize * 1.29}px`, fontWeight: 600 },
    body1: { fontSize: `${fontSize}px` },
    body2: { fontSize: `${fontSize}px`, fontWeight: 400 },
    caption: { fontSize: `${fontSize * 0.86}px`, fontWeight: 400 },
  };

  return {
    palette: {
      mode,
      primary: {
        light: brand[200],
        main: brand[400],
        dark: brand[700],
        contrastText: brand[50],
      },
      secondary: {
        light: green[200],
        main: green[400],
        dark: green[700],
        contrastText: gray[50],
      },
      error: {
        light: red[300],
        main: red[500],
        dark: red[700],
      },
      warning: {
        light: orange[300],
        main: orange[400],
        dark: orange[800],
      },
      success: {
        light: green[300],
        main: green[400],
        dark: green[800],
      },
      grey: { ...gray },
      background: {
        default: mode === 'dark' ? gray[900] : 'hsl(0, 0%, 99%)',
        paper: mode === 'dark' ? gray[800] : 'hsl(220, 35%, 97%)',
      },
      text: {
        primary: mode === 'dark' ? 'hsl(0, 0%, 100%)' : gray[800],
        secondary: mode === 'dark' ? gray[400] : gray[600],
      },
      divider: mode === 'dark' ? alpha(gray[700], 0.6) : alpha(gray[300], 0.4),
      action: {
        hover: alpha(mode === 'dark' ? gray[600] : gray[200], 0.2),
        selected: alpha(mode === 'dark' ? gray[600] : gray[200], 0.3),
      },
    },
    typography: {
      fontFamily: ['"Inter", "sans-serif"'].join(','),
      fontSize, // Base font size
      ...baseTypography,
    },
    shadows: customShadows,
    shape: { borderRadius: 8 },
  };
};

export const colorSchemes = {
  light: {
    palette: {
      primary: {
        light: brand[200],
        main: brand[400],
        dark: brand[700],
        contrastText: brand[50],
      },
      info: {
        light: brand[100],
        main: brand[300],
        dark: brand[600],
        contrastText: gray[50],
      },
      warning: {
        light: orange[300],
        main: orange[400],
        dark: orange[800],
      },
      error: {
        light: red[300],
        main: red[400],
        dark: red[800],
      },
      success: {
        light: green[300],
        main: green[400],
        dark: green[800],
      },
      grey: {
        ...gray,
      },
      divider: alpha(gray[300], 0.4),
      background: {
        default: 'hsl(0, 0%, 99%)',
        paper: 'hsl(220, 35%, 97%)',
      },
      text: {
        primary: gray[800],
        secondary: gray[600],
        warning: orange[400],
      },
      action: {
        hover: alpha(gray[200], 0.2),
        selected: `${alpha(gray[200], 0.3)}`,
      },
      baseShadow:
        'hsla(220, 30%, 5%, 0.07) 0px 4px 16px 0px, hsla(220, 25%, 10%, 0.07) 0px 8px 16px -5px',
    },
  },
  dark: {
    palette: {
      primary: {
        contrastText: brand[50],
        light: brand[300],
        main: brand[400],
        dark: brand[700],
      },
      info: {
        contrastText: brand[300],
        light: brand[500],
        main: brand[700],
        dark: brand[900],
      },
      warning: {
        light: orange[400],
        main: orange[500],
        dark: orange[700],
      },
      error: {
        light: red[400],
        main: red[500],
        dark: red[700],
      },
      success: {
        light: green[400],
        main: green[500],
        dark: green[700],
      },
      grey: {
        ...gray,
      },
      divider: alpha(gray[700], 0.6),
      background: {
        default: gray[900],
        paper: 'hsl(220, 30%, 7%)',
      },
      text: {
        primary: 'hsl(0, 0%, 100%)',
        secondary: gray[400],
      },
      action: {
        hover: alpha(gray[600], 0.2),
        selected: alpha(gray[600], 0.3),
      },
      baseShadow:
        'hsla(220, 30%, 5%, 0.7) 0px 4px 16px 0px, hsla(220, 25%, 10%, 0.8) 0px 8px 16px -5px',
    },
  },
};

export const shape = {
  borderRadius: 8,
};

// @ts-ignore
const defaultShadows: Shadows = [
  'none',
  'var(--template-palette-baseShadow)',
  ...defaultTheme.shadows.slice(2),
];
export const shadows = defaultShadows;

export const getAccessibilityTokens = (profile: string) => {
  switch (profile) {
    case 'highContrast':
      return {
        palette: {
          text: {
            primary: 'hsl(0, 0%, 100%)', // White text for high contrast
            secondary: 'hsl(0, 0%, 90%)',
          },
          background: {
            default: 'hsl(0, 0%, 10%)', // Dark background for contrast
            paper: 'hsl(0, 0%, 15%)',
          },
        },
      };
      case 'RGColorblind': // Red-Green Colorblind Profile
      return {
        palette: {
          primary: { main: 'hsl(220, 79%, 50%)' }, // Adjusted colors for better distinction
          secondary: { main: 'hsl(51, 90%, 50%)' },
          success: { main: 'hsl(180, 50%, 40%)' },
          warning: { main: 'hsl(35, 90%, 60%)' },
          error: { main: 'hsl(0, 70%, 50%)' },
        },
      };
    case 'BYColorblind': // Blue-Yellow Colorblind Profile
      return {
        palette: {
          primary: { main: 'hsl(210, 80%, 40%)' }, // Adjusted colors for better distinction
          secondary: { main: 'hsl(45, 75%, 50%)' },
          success: { main: 'hsl(100, 40%, 50%)' },
          warning: { main: 'hsl(60, 70%, 60%)' },
          error: { main: 'hsl(10, 70%, 50%)' },
        },
      };
    case 'monochrome':
      return {
        palette: {
          primary: { main: 'hsl(0, 0%, 50%)' },
          text: { primary: 'hsl(0, 0%, 20%)', secondary: 'hsl(0, 0%, 50%)' },
        },
      };
    default:
      return {}; // No accessibility changes
  }
};

export const createCustomTheme = (
  mode: PaletteMode,
  profile: string = '',
  fontSize: number = 14) => {
  const baseTokens = getDesignTokens(mode);
  const accessibilityTokens = getAccessibilityTokens(profile);

  const mergedTokens = deepmerge(baseTokens, accessibilityTokens);

  return createTheme({
    ...mergedTokens,
    typography: {
      ...mergedTokens.typography,
      fontSize, // Allow fontSize overrides
    },
  });
};


