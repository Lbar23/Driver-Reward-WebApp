import { useEffect, useState } from 'react';
import { useTheme } from '@mui/material/styles';
import { BarChart, PieChart } from '@mui/x-charts';
import axios from 'axios';

interface ReportChartProps {
  reportType: 'sales' | 'driver-points' | 'invoice';
  viewType: 'summary' | 'detailed';
  selectedSponsor?: number | 'all';
  selectedDriver?: number | 'all';
  dateRange: [Date | null, Date | null];
}

export default function ReportChart({
  reportType,
  viewType,
  selectedSponsor,
  selectedDriver,
  dateRange,
}: ReportChartProps) {
  const theme = useTheme();
  const [chartData, setChartData] = useState<any[]>([]); // Ensure initialization as an array
  const [xLabels, setXLabels] = useState<string[]>([]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const params = {
          viewType,
          selectedSponsor: selectedSponsor === 'all' ? undefined : selectedSponsor,
          selectedDriver: selectedDriver === 'all' ? undefined : selectedDriver,
          startDate: dateRange[0]?.toISOString(),
          endDate: dateRange[1]?.toISOString(),
        };

        const endpoint = `/api/reports/${reportType}`;
        const response = await axios.get(endpoint, { params });

        // Ensure the response data is an array
        const data = Array.isArray(response.data) ? response.data : [];
        setChartData(data);

        // Map xLabels based on report type
        const labels = data.map((item: any) => {
          if (reportType === 'sales') return item.SponsorName || 'Unknown';
          if (reportType === 'driver-points') return item.DriverName || 'Unknown';
          if (reportType === 'invoice') return item.DriverName || 'Unknown';
          return 'Unknown';
        });

        setXLabels(labels);
      } catch (error) {
        console.error('Failed to fetch report data:', error);
        setChartData([]); // Reset chartData on error
        setXLabels([]); // Reset xLabels on error
      }
    };

    fetchData();
  }, [reportType, viewType, selectedSponsor, selectedDriver, dateRange]);

  const renderBarChart = () => {
    const seriesData =
      reportType === 'sales'
        ? chartData.map((item) => item.TotalPoints || 0)
        : reportType === 'driver-points'
        ? chartData.map((item) => item.PointsChanged || 0)
        : chartData.map((item) => item.TotalFee || 0);

    const label =
      reportType === 'sales'
        ? 'Total Sales'
        : reportType === 'driver-points'
        ? 'Points Changed'
        : 'Total Fee';

    return (
      <BarChart
        xAxis={[{ data: xLabels, scaleType: 'band' }]}
        series={[
          {
            data: seriesData,
            label,
            color: theme.palette.primary.main,
          },
        ]}
        height={300}
        grid={{ horizontal: true }}
      />
    );
  };

  const renderPieChart = () => (
    <PieChart
      series={[
        {
          data: chartData.map((item) => ({
            id: item.DriverName || 'Unknown',
            value: item.TotalFee || 0,
            label: item.DriverName || 'Unknown',
            color: theme.palette.primary.main,
          })),
        },
      ]}
      height={300}
    />
  );

  return <div>{reportType === 'invoice' ? renderPieChart() : renderBarChart()}</div>;
}
