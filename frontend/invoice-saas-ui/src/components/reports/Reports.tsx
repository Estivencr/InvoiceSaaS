import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Alert, Paper, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, FormControl, InputLabel,
  Select, MenuItem, CircularProgress
} from '@mui/material';
import { TrendingUp } from '@mui/icons-material';
import { reportService } from '../../services/invoiceService';
import { MonthlySales } from '../../types';

const fmt = (n: number) =>
  new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(n);

export default function Reports() {
  const [data, setData] = useState<MonthlySales[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [months, setMonths] = useState(12);

  useEffect(() => {
    setLoading(true);
    setError('');
    reportService.getMonthlySales(months)
      .then((result) => setData(result ?? []))
      .catch(() => setError('Error al cargar reportes.'))
      .finally(() => setLoading(false));
  }, [months]);

  const totals = data.reduce(
    (acc, row) => ({
      invoiceCount: acc.invoiceCount + row.invoiceCount,
      totalRevenue: acc.totalRevenue + row.totalRevenue,
      paidAmount: acc.paidAmount + row.paidAmount,
      pendingAmount: acc.pendingAmount + row.pendingAmount,
    }),
    { invoiceCount: 0, totalRevenue: 0, paidAmount: 0, pendingAmount: 0 }
  );

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <TrendingUp color="primary" />
          <Typography variant="h5" fontWeight={700}>Reportes de Ventas</Typography>
        </Box>
        <FormControl size="small" sx={{ minWidth: 160 }}>
          <InputLabel>Período</InputLabel>
          <Select value={months} label="Período" onChange={(e) => setMonths(Number(e.target.value))}>
            <MenuItem value={3}>Últimos 3 meses</MenuItem>
            <MenuItem value={6}>Últimos 6 meses</MenuItem>
            <MenuItem value={12}>Últimos 12 meses</MenuItem>
            <MenuItem value={24}>Últimos 24 meses</MenuItem>
          </Select>
        </FormControl>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 6 }}>
          <CircularProgress />
        </Box>
      ) : (
        <TableContainer component={Paper} elevation={2}>
          <Table size="small">
            <TableHead>
              <TableRow sx={{ bgcolor: 'primary.main' }}>
                <TableCell sx={{ color: 'white', fontWeight: 700 }}>Mes</TableCell>
                <TableCell sx={{ color: 'white', fontWeight: 700 }} align="right">Facturas</TableCell>
                <TableCell sx={{ color: 'white', fontWeight: 700 }} align="right">Ingresos Totales</TableCell>
                <TableCell sx={{ color: 'white', fontWeight: 700 }} align="right">Cobrado</TableCell>
                <TableCell sx={{ color: 'white', fontWeight: 700 }} align="right">Pendiente</TableCell>
                <TableCell sx={{ color: 'white', fontWeight: 700 }} align="right">% Cobrado</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} align="center" sx={{ py: 4, color: 'text.secondary' }}>
                    Sin datos para el período seleccionado.
                  </TableCell>
                </TableRow>
              ) : (
                data.map((row) => {
                  const pct = row.totalRevenue > 0
                    ? ((row.paidAmount / row.totalRevenue) * 100).toFixed(1)
                    : '0.0';
                  return (
                    <TableRow key={`${row.year}-${row.month}`} hover>
                      <TableCell>{row.monthName} {row.year}</TableCell>
                      <TableCell align="right">{row.invoiceCount}</TableCell>
                      <TableCell align="right">{fmt(row.totalRevenue)}</TableCell>
                      <TableCell align="right" sx={{ color: 'success.main' }}>{fmt(row.paidAmount)}</TableCell>
                      <TableCell align="right" sx={{ color: 'warning.main' }}>{fmt(row.pendingAmount)}</TableCell>
                      <TableCell align="right">{pct}%</TableCell>
                    </TableRow>
                  );
                })
              )}
            </TableBody>
            {data.length > 0 && (
              <TableHead>
                <TableRow sx={{ bgcolor: 'grey.100' }}>
                  <TableCell sx={{ fontWeight: 700 }}>Total</TableCell>
                  <TableCell align="right" sx={{ fontWeight: 700 }}>{totals.invoiceCount}</TableCell>
                  <TableCell align="right" sx={{ fontWeight: 700 }}>{fmt(totals.totalRevenue)}</TableCell>
                  <TableCell align="right" sx={{ fontWeight: 700, color: 'success.main' }}>{fmt(totals.paidAmount)}</TableCell>
                  <TableCell align="right" sx={{ fontWeight: 700, color: 'warning.main' }}>{fmt(totals.pendingAmount)}</TableCell>
                  <TableCell align="right" sx={{ fontWeight: 700 }}>
                    {totals.totalRevenue > 0
                      ? ((totals.paidAmount / totals.totalRevenue) * 100).toFixed(1)
                      : '0.0'}%
                  </TableCell>
                </TableRow>
              </TableHead>
            )}
          </Table>
        </TableContainer>
      )}
    </Box>
  );
}
