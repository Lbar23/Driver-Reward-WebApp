import { useEffect, useState } from 'react';
import { useTheme } from '@mui/material/styles';
import { BarChart, PieChart } from '@mui/x-charts';

interface ReportChartProps {
  reportType: 'salesBySponsor' | 'salesByDriver' | 'invoice' | 'driverPoints';
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
  const [chartData, setChartData] = useState<any[]>([]);
  const [xLabels, setXLabels] = useState<string[]>([]);

  useEffect(() => {
    const generateData = () => {
      switch (reportType) {
        case 'salesBySponsor':
          if (viewType === 'detailed') {
            return [
              { date: 'Date 1', driver1: 100, driver2: 150, driver3: 200 },
              { date: 'Date 2', driver1: 120, driver2: 140, driver3: 210 },
              { date: 'Date 3', driver1: 130, driver2: 160, driver3: 220 },
            ];
          }
          return [
            { sponsor: 'Sponsor A', totalPoints: 450 },
            { sponsor: 'Sponsor B', totalPoints: 470 },
            { sponsor: 'Sponsor C', totalPoints: 510 },
          ];
        case 'salesByDriver':
          if (viewType === 'detailed') {
            return [
              { date: 'Date 1', driver1: 50, driver2: 75, driver3: 90 },
              { date: 'Date 2', driver1: 60, driver2: 80, driver3: 95 },
              { date: 'Date 3', driver1: 70, driver2: 85, driver3: 100 },
            ];
          }
          return [
            { driver: 'Driver 1', totalPoints: 250 },
            { driver: 'Driver 2', totalPoints: 240 },
            { driver: 'Driver 3', totalPoints: 260 },
          ];
        case 'invoice':
          return [
            { driver: 'Driver 1', redeemedPoints: 120, fee: 10 },
            { driver: 'Driver 2', redeemedPoints: 150, fee: 15 },
            { driver: 'Driver 3', redeemedPoints: 180, fee: 20 },
          ];
        case 'driverPoints':
          if (viewType === 'detailed') {
            return [
              { date: 'Date 1', pointEarned: 50, pointRedeemed: 20 },
              { date: 'Date 2', pointEarned: 70, pointRedeemed: 30 },
              { date: 'Date 3', pointEarned: 60, pointRedeemed: 25 },
            ];
          }
          return [
            { driver: 'Driver 1', totalPoints: 150 },
            { driver: 'Driver 2', totalPoints: 170 },
            { driver: 'Driver 3', totalPoints: 140 },
          ];
        default:
          return [];
      }
    };

    const data = generateData();
    setChartData(data);
    setXLabels(data.map((item: any) => item.date || item.sponsor || item.driver));
  }, [reportType, viewType, selectedSponsor, selectedDriver, dateRange]);

  const renderBarChart = () => (
    <BarChart
      xAxis={[{ data: xLabels, scaleType: 'band' }]}
      series={
        viewType === 'detailed' && (reportType === 'salesBySponsor' || reportType === 'salesByDriver')
          ? [
              { data: chartData.map((item) => item.driver1 || 0), label: 'Driver 1', stack: 'drivers', color: theme.palette.primary.light },
              { data: chartData.map((item) => item.driver2 || 0), label: 'Driver 2', stack: 'drivers', color: theme.palette.primary.main },
              { data: chartData.map((item) => item.driver3 || 0), label: 'Driver 3', stack: 'drivers', color: theme.palette.primary.dark },
            ]
          : viewType === 'detailed' && reportType === 'driverPoints'
          ? [
              { data: chartData.map((item) => item.pointEarned || 0), label: 'Points Earned', color: theme.palette.success.main },
              { data: chartData.map((item) => item.pointRedeemed || 0), label: 'Points Redeemed', color: theme.palette.error.main },
            ]
          : [
              {
                data: chartData.map((item) => item.totalPoints),
                label: reportType === 'salesBySponsor' ? 'Total Sales by Sponsor' : 'Total Sales by Driver',
                color: theme.palette.primary.main,
              },
            ]
      }
      height={300}
      grid={{ horizontal: true }}
    />
  );

  const renderPieChart = () => (
    <PieChart
      series={[
        {
          data: chartData.map((item) => ({
            id: item.driver,
            value: item.redeemedPoints,
            label: item.driver,
            color: theme.palette.primary.main,
          })),
        },
      ]}
      height={300}
    />
  );

  return (
    <div>
      {reportType === 'invoice' ? renderPieChart() : renderBarChart()}
    </div>
  );
}
