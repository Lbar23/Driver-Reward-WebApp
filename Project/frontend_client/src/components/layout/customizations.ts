import { alpha, Theme } from '@mui/material/styles';
import { axisClasses, legendClasses, chartsGridClasses } from '@mui/x-charts';
import { menuItemClasses } from '@mui/material/MenuItem';
import { pickersYearClasses, pickersMonthClasses, pickersDayClasses } from '@mui/x-date-pickers';
import { paperClasses } from '@mui/material/Paper';
import { listItemIconClasses } from '@mui/material/ListItemIcon';
import { iconButtonClasses } from '@mui/material/IconButton';
import { checkboxClasses } from '@mui/material/Checkbox';
import { listClasses } from '@mui/material/List';
import { gridClasses } from '@mui/x-data-grid';
import { tablePaginationClasses } from '@mui/material/TablePagination';
import type { ChartsComponents } from '@mui/x-charts/themeAugmentation';
import type { TreeViewComponents } from '@mui/x-tree-view/themeAugmentation';
import type { PickerComponents } from '@mui/x-date-pickers/themeAugmentation';
import type { DataGridComponents } from '@mui/x-data-grid/themeAugmentation';
import { gray, brand } from '../../theme/themePrimitives';

export const dataGridComponents: DataGridComponents<Theme> = {
  MuiDataGrid: {
    styleOverrides: {
      root: ({ theme }) => ({
        '--DataGrid-overlayHeight': '300px',
        overflow: 'clip',
        borderColor: theme.palette.divider,
        backgroundColor: theme.palette.background.default,
        [`& .${gridClasses.columnHeader}`]: {
          backgroundColor: theme.palette.background.paper,
        },
        [`& .${gridClasses.footerContainer}`]: {
          backgroundColor: theme.palette.background.paper,
        },
        [`& .${checkboxClasses.root}`]: {
          padding: theme.spacing(0.5),
          '& > svg': {
            fontSize: '1rem',
          },
        },
        [`& .${tablePaginationClasses.root}`]: {
          marginRight: theme.spacing(1),
          '& .MuiIconButton-root': {
            maxHeight: 32,
            maxWidth: 32,
            '& > svg': {
              fontSize: '1rem',
            },
          },
        },
      }),
      cell: ({ theme }) => ({ borderTopColor: theme.palette.divider }),
      menu: ({ theme }) => ({
        borderRadius: theme.shape.borderRadius,
        backgroundImage: 'none',
        [`& .${paperClasses.root}`]: {
          border: `1px solid ${theme.palette.divider}`,
        },
        [`& .${menuItemClasses.root}`]: {
          margin: '0 4px',
        },
        [`& .${listItemIconClasses.root}`]: {
          marginRight: 0,
        },
        [`& .${listClasses.root}`]: {
          paddingLeft: 0,
          paddingRight: 0,
        },
      }),
      row: ({ theme }) => ({
        '&:last-of-type': { borderBottom: `1px solid ${theme.palette.divider}` },
        '&:hover': {
          backgroundColor: theme.palette.action.hover,
        },
        '&.Mui-selected': {
          background: theme.palette.action.selected,
          '&:hover': {
            backgroundColor: theme.palette.action.hover,
          },
        },
      }),
      iconButtonContainer: ({ theme }) => ({
        [`& .${iconButtonClasses.root}`]: {
          border: 'none',
          backgroundColor: 'transparent',
          '&:hover': {
            backgroundColor: alpha(theme.palette.action.selected, 0.3),
          },
          '&:active': {
            backgroundColor: gray[200],
          },
          ...theme.applyStyles('dark', {
            color: gray[50],
            '&:hover': {
              backgroundColor: gray[800],
            },
            '&:active': {
              backgroundColor: gray[900],
            },
          }),
        },
      }),
      menuIconButton: ({ theme }) => ({
        border: 'none',
        backgroundColor: 'transparent',
        '&:hover': {
          backgroundColor: gray[100],
        },
        '&:active': {
          backgroundColor: gray[200],
        },
        ...theme.applyStyles('dark', {
          color: gray[50],
          '&:hover': {
            backgroundColor: gray[800],
          },
          '&:active': {
            backgroundColor: gray[900],
          },
        }),
      }),
      filterForm: ({ theme }) => ({
        gap: theme.spacing(1),
        alignItems: 'flex-end',
      }),
      columnsManagementHeader: ({ theme }) => ({
        paddingRight: theme.spacing(3),
        paddingLeft: theme.spacing(3),
      }),
      columnHeaderTitleContainer: {
        flexGrow: 1,
        justifyContent: 'space-between',
      },
      columnHeaderDraggableContainer: { paddingRight: 2 },
    },
  },
};

export const chartsComponents: ChartsComponents<Theme> = {
  MuiChartsAxis: {
    styleOverrides: {
      root: ({ theme }) => ({
        [`& .${axisClasses.line}`]: {
          stroke: gray[300],
        },
        [`& .${axisClasses.tick}`]: { stroke: gray[300] },
        [`& .${axisClasses.tickLabel}`]: {
          fill: gray[500],
          fontWeight: 500,
        },
        ...theme.applyStyles('dark', {
          [`& .${axisClasses.line}`]: {
            stroke: gray[700],
          },
          [`& .${axisClasses.tick}`]: { stroke: gray[700] },
          [`& .${axisClasses.tickLabel}`]: {
            fill: gray[300],
            fontWeight: 500,
          },
        }),
      }),
    },
  },
  MuiChartsTooltip: {
    styleOverrides: {
      mark: ({ theme }) => ({
        ry: 6,
        boxShadow: 'none',
        border: `1px solid ${theme.palette.divider}`,
      }),
      table: ({ theme }) => ({
        border: `1px solid ${theme.palette.divider}`,
        borderRadius: theme.shape.borderRadius,
        background: 'hsl(0, 0%, 100%)',
        ...theme.applyStyles('dark', {
          background: gray[900],
        }),
      }),
    },
  },
  MuiChartsLegend: {
    styleOverrides: {
      root: {
        [`& .${legendClasses.mark}`]: {
          ry: 6,
        },
      },
    },
  },
  MuiChartsGrid: {
    styleOverrides: {
      root: ({ theme }) => ({
        [`& .${chartsGridClasses.line}`]: {
          stroke: gray[200],
          strokeDasharray: '4 2',
          strokeWidth: 0.8,
        },
        ...theme.applyStyles('dark', {
          [`& .${chartsGridClasses.line}`]: {
            stroke: gray[700],
            strokeDasharray: '4 2',
            strokeWidth: 0.8,
          },
        }),
      }),
    },
  },
};

export const treeViewComponents: TreeViewComponents<Theme> = {
  MuiTreeItem2: {
    styleOverrides: {
      root: ({ theme }) => ({
        position: 'relative',
        boxSizing: 'border-box',
        padding: theme.spacing(0, 1),
        '& .groupTransition': {
          marginLeft: theme.spacing(2),
          padding: theme.spacing(0),
          borderLeft: '1px solid',
          borderColor: theme.palette.divider,
        },
        '&:focus-visible .focused': {
          outline: `3px solid ${alpha(brand[500], 0.5)}`,
          outlineOffset: '2px',
          '&:hover': {
            backgroundColor: alpha(gray[300], 0.2),
            outline: `3px solid ${alpha(brand[500], 0.5)}`,
            outlineOffset: '2px',
          },
        },
      }),
      content: ({ theme }) => ({
        marginTop: theme.spacing(1),
        padding: theme.spacing(0.5, 1),
        overflow: 'clip',
        '&:hover': {
          backgroundColor: alpha(gray[300], 0.2),
        },

        '&.selected': {
          backgroundColor: alpha(gray[300], 0.4),
          '&:hover': {
            backgroundColor: alpha(gray[300], 0.6),
          },
        },
        ...theme.applyStyles('dark', {
          '&:hover': {
            backgroundColor: alpha(gray[500], 0.2),
          },
          '&:focus-visible': {
            '&:hover': {
              backgroundColor: alpha(gray[500], 0.2),
            },
          },
          '&.selected': {
            backgroundColor: alpha(gray[500], 0.4),
            '&:hover': {
              backgroundColor: alpha(gray[500], 0.6),
            },
          },
        }),
      }),
    },
  },
};

export const datePickersComponents: PickerComponents<Theme> = {
  MuiPickersPopper: {
    styleOverrides: {
      paper: ({ theme }) => ({
        marginTop: 4,
        borderRadius: theme.shape.borderRadius,
        border: `1px solid ${(theme).palette.divider}`,
        backgroundImage: 'none',
        background: 'hsl(0, 0%, 100%)',
        boxShadow:
          'hsla(220, 30%, 5%, 0.07) 0px 4px 16px 0px, hsla(220, 25%, 10%, 0.07) 0px 8px 16px -5px',
        [`& .${menuItemClasses.root}`]: {
          borderRadius: 6,
          margin: '0 6px',
        },
        ...theme.applyStyles('dark', {
          background: gray[900],
          boxShadow:
            'hsla(220, 30%, 5%, 0.7) 0px 4px 16px 0px, hsla(220, 25%, 10%, 0.8) 0px 8px 16px -5px',
        }),
      }),
    },
  },
  MuiPickersArrowSwitcher: {
    styleOverrides: {
      spacer: { width: 16 },
      button: ({ theme }) => ({
        backgroundColor: 'transparent',
        color:theme.palette.grey[500],
        ...theme.applyStyles('dark', {
          color:theme.palette.grey[400],
        }),
      }),
    },
  },
  MuiPickersCalendarHeader: {
    styleOverrides: {
      switchViewButton: {
        padding: 0,
        border: 'none',
      },
    },
  },
  MuiPickersMonth: {
    styleOverrides: {
      monthButton: ({ theme }) => ({
        fontSize: theme.typography.body1.fontSize,
        color:theme.palette.grey[600],
        padding: theme.spacing(0.5),
        borderRadius: theme.shape.borderRadius,
        '&:hover': {
          backgroundColor:theme.palette.action.hover,
        },
        [`&.${pickersMonthClasses.selected}`]: {
          backgroundColor: gray[700],
          fontWeight: theme.typography.fontWeightMedium,
        },
        '&:focus': {
          outline: `3px solid ${alpha(brand[500], 0.5)}`,
          outlineOffset: '2px',
          backgroundColor: 'transparent',
          [`&.${pickersMonthClasses.selected}`]: { backgroundColor: gray[700] },
        },
        ...theme.applyStyles('dark', {
          color:theme.palette.grey[300],
          '&:hover': {
            backgroundColor:theme.palette.action.hover,
          },
          [`&.${pickersMonthClasses.selected}`]: {
            color:theme.palette.common.black,
            fontWeight: theme.typography.fontWeightMedium,
            backgroundColor: gray[300],
          },
          '&:focus': {
            outline: `3px solid ${alpha(brand[500], 0.5)}`,
            outlineOffset: '2px',
            backgroundColor: 'transparent',
            [`&.${pickersMonthClasses.selected}`]: { backgroundColor: gray[300] },
          },
        }),
      }),
    },
  },
  MuiPickersYear: {
    styleOverrides: {
      yearButton: ({ theme }) => ({
        fontSize: theme.typography.body1.fontSize,
        color:theme.palette.grey[600],
        padding: theme.spacing(0.5),
        borderRadius: theme.shape.borderRadius,
        height: 'fit-content',
        '&:hover': {
          backgroundColor:theme.palette.action.hover,
        },
        [`&.${pickersYearClasses.selected}`]: {
          backgroundColor: gray[700],
          fontWeight: theme.typography.fontWeightMedium,
        },
        '&:focus': {
          outline: `3px solid ${alpha(brand[500], 0.5)}`,
          outlineOffset: '2px',
          backgroundColor: 'transparent',
          [`&.${pickersYearClasses.selected}`]: { backgroundColor: gray[700] },
        },
        ...theme.applyStyles('dark', {
          color:theme.palette.grey[300],
          '&:hover': {
            backgroundColor:theme.palette.action.hover,
          },
          [`&.${pickersYearClasses.selected}`]: {
            color:theme.palette.common.black,
            fontWeight: theme.typography.fontWeightMedium,
            backgroundColor: gray[300],
          },
          '&:focus': {
            outline: `3px solid ${alpha(brand[500], 0.5)}`,
            outlineOffset: '2px',
            backgroundColor: 'transparent',
            [`&.${pickersYearClasses.selected}`]: { backgroundColor: gray[300] },
          },
        }),
      }),
    },
  },
  MuiPickersDay: {
    styleOverrides: {
      root: ({ theme }) => ({
        fontSize: theme.typography.body1.fontSize,
        color:theme.palette.grey[600],
        padding: theme.spacing(0.5),
        borderRadius: theme.shape.borderRadius,
        '&:hover': {
          backgroundColor:theme.palette.action.hover,
        },
        [`&.${pickersDayClasses.selected}`]: {
          backgroundColor: gray[700],
          fontWeight: theme.typography.fontWeightMedium,
        },
        '&:focus': {
          outline: `3px solid ${alpha(brand[500], 0.5)}`,
          outlineOffset: '2px',
          backgroundColor: 'transparent',
          [`&.${pickersDayClasses.selected}`]: { backgroundColor: gray[700] },
        },
        ...theme.applyStyles('dark', {
          color:theme.palette.grey[300],
          '&:hover': {
            backgroundColor:theme.palette.action.hover,
          },
          [`&.${pickersDayClasses.selected}`]: {
            color:theme.palette.common.black,
            fontWeight: theme.typography.fontWeightMedium,
            backgroundColor: gray[300],
          },
          '&:focus': {
            outline: `3px solid ${alpha(brand[500], 0.5)}`,
            outlineOffset: '2px',
            backgroundColor: 'transparent',
            [`&.${pickersDayClasses.selected}`]: { backgroundColor: gray[300] },
          },
        }),
      }),
    },
  },
};

