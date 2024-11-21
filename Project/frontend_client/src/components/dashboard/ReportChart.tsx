import React from 'react';
import { BarChart, PieChart } from '@mui/x-charts';
import { useTheme } from '@mui/material/styles';
import { Typography, Box } from '@mui/material';

interface ReportChartProps {
  chartType: 'bar' | 'pie'; // Determines the chart type
  title: string; // Title for the chart
  viewType: 'summary' | 'detailed'; // Determines the level of details in the chart
  data: any[]; // Chart data
  categoryKey?: string; // Key for x-axis categories
  valueKey?: string; // Key for y-axis or pie chart values
  stackKey?: string; // Key for grouping data in stacked bar charts
}
{/* <ReportChart 
  chartType="bar"
  viewType="summary"
  title="Driver Points (Summary)"
  data={[
    { category: 'Driver A', value: 120 },
    { category: 'Driver B', value: 95 },
    { category: 'Driver C', value: 110 },
  ]}
/> */} // <---- this is the input format expected when calling this component for summary

{/* <ReportChart
  chartType="bar"
  viewType="detailed"
  title="Driver Points (Detailed)"
  data={[
    { category: 'Driver A', group: 'Category 1', value: 60 },
    { category: 'Driver A', group: 'Category 2', value: 60 },
    { category: 'Driver B', group: 'Category 1', value: 95 },
  ]}
/> */} // <---- this is the input format expected when calling this component for detailed
export default function ReportChart({
  chartType,
  title,
  viewType,
  data,
  categoryKey = 'category', // Default key for categories
  valueKey = 'value', // Default key for values
  stackKey = 'group', // Default key for grouping in stacked charts
}: ReportChartProps) {
  const theme = useTheme();

  const renderBarChart = () => {
    // Extract x-axis categories
    const xAxisData = Array.from(new Set(data.map((item) => item[categoryKey])));

    // Group data for series
    const groupedData: Record<string, number[]> = {};
    data.forEach((item) => {
      const group = item[stackKey] || 'Default';
      if (!groupedData[group]) groupedData[group] = Array(xAxisData.length).fill(0);
      const index = xAxisData.indexOf(item[categoryKey]);
      if (index >= 0) groupedData[group][index] += item[valueKey] || 0;
    });

    // Create series
    const series = Object.entries(groupedData).map(([group, values]) => ({
      label: group,
      data: values,
      color: theme.palette.secondary.main,
    }));

    // Render bar chart
    return (
      <BarChart
        xAxis={[{ scaleType: 'band', data: xAxisData }]}
        series={series}
        height={300}
        grid={{ horizontal: true }}
      />
    );
  };

  const renderPieChart = () => {
    if (viewType === 'summary') {
      return (
        <PieChart
          series={[
            {
              data: data.map((item) => ({
                id: item[categoryKey] || 'Unknown',
                value: item[valueKey] || 0,
                label: item[categoryKey] || 'Unknown',
              })),
            },
          ]}
          height={300}
        />
      );
    }

    if (viewType === 'detailed') {
      // Detailed: Two-level pie chart
      const groupedData: Record<string, any[]> = {};
      data.forEach((item) => {
        const group = item[stackKey] || 'Default';
        if (!groupedData[group]) groupedData[group] = [];
        groupedData[group].push({
          id: item[categoryKey] || 'Unknown',
          value: item[valueKey] || 0,
          label: item[categoryKey] || 'Unknown',
        });
      });

      const pieData = Object.entries(groupedData).map(([group, children]) => ({
        id: group,
        value: children.reduce((sum, item) => sum + item.value, 0),
        label: group,
        children,
      }));

      return (
        <PieChart
          series={[
            {
              data: pieData,
            },
          ]}
          height={300}
        />
      );
    }

    return null;
  };

  return (
    <Box>
      <Typography variant="h6" align="center" gutterBottom>
        {title}
      </Typography>
      {chartType === 'bar' ? renderBarChart() : renderPieChart()}
    </Box>
  );
}
