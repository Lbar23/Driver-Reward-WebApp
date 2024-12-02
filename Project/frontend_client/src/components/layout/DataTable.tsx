import React, { useState, useEffect } from "react";
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Paper,
  CircularProgress,
  Box,
} from "@mui/material";

export interface TableColumn<T> {
  id: keyof T;
  label: string;
  render?: (row: T) => React.ReactNode;
}

export interface TableComponentProps<T> {
  columns: TableColumn<T>[];
  fetchData: (page: number, pageSize: number) => Promise<{
    data: T[];
    totalCount: number;
  }>;
  defaultPageSize?: number;
  pageSizeOptions?: number[];
  refreshTrigger?: any; // Optional prop to force refresh when filters change
}

function TableComponent<T>({
  columns,
  fetchData,
  defaultPageSize = 10,
  pageSizeOptions = [5, 10, 25, 50],
  refreshTrigger,
}: TableComponentProps<T>) {
  const [data, setData] = useState<T[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(defaultPageSize);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await fetchData(page + 1, pageSize); // Convert to 1-based for API
      setData(response.data);
      setTotalCount(response.totalCount);
    } catch (err) {
      console.error("Error fetching data:", err);
      setError("Failed to fetch data. Please try again later.");
      setData([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  };

  // Load data on mount and when pagination or refreshTrigger changes
  useEffect(() => {
    loadData();
  }, [page, pageSize, refreshTrigger]);

  const handlePageChange = (_: any, newPage: number) => {
    setPage(newPage);
  };

  const handlePageSizeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newPageSize = parseInt(event.target.value, 10);
    setPageSize(newPageSize);
    setPage(0); // Reset to first page when changing page size
  };

  return (
    <Paper>
      {error && (
        <Box sx={{ p: 2, color: "error.main" }}>
          {error}
        </Box>
      )}
      <TableContainer>
        <Table>
          <TableHead>
            <TableRow>
              {columns.map((col) => (
                <TableCell key={col.id as string}>{col.label}</TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={columns.length} align="center" sx={{ py: 3 }}>
                  <CircularProgress size={24} />
                </TableCell>
              </TableRow>
            ) : data.length === 0 ? (
              <TableRow>
                <TableCell colSpan={columns.length} align="center">
                  No data found.
                </TableCell>
              </TableRow>
            ) : (
              data.map((row, rowIndex) => (
                <TableRow key={rowIndex}>
                  {columns.map((col) => (
                    <TableCell key={col.id as string}>
                      {col.render
                        ? col.render(row)
                        : row[col.id] !== undefined && row[col.id] !== null
                        ? String(row[col.id])
                        : "N/A"}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
      <TablePagination
        component="div"
        count={totalCount}
        page={page}
        rowsPerPage={pageSize}
        onPageChange={handlePageChange}
        onRowsPerPageChange={handlePageSizeChange}
        rowsPerPageOptions={pageSizeOptions}
      />
    </Paper>
  );
}

export default TableComponent;