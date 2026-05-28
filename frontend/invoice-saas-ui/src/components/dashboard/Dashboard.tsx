import React, { useEffect, useState } from 'react';
import {
  Box, Card, CardContent, Grid, Typography, CircularProgress, Chip, Alert
} from '@mui/material';
import { Receipt, People, TrendingUp, HourglassEmpty } from '@mui/icons-material';
import { reportService } from '../../services/invoiceService';
import { DashboardData } from '../../types';

function StatCard({ title, value, icon, color }: { title: string; value: string; icon: React.ReactNode; color: string }) {
  return (
    <Card elevation={2}>
      <CardContent>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
          <Box>
            <Typography variant="body2" color="text.secondary">{title}</Typography>
            <Typography variant="h5" fontWeight={700} sx={{ mt: 0.5 }}>{value}</Typography>
          </Box>
          <Box sx={{ p: 1, borderRadius: 2, bgcolor: color, color: 'white' }}>{icon}</Box>
        </Box>
      </CardContent>
    </Card>
  );
}

export default function Dashboard() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    reportService.getDashboard()
      .then(setData)
      .catch(() => setError('Failed to load dashboard data.'))
      .finally(() => setLoading(false));
  }, []);

  const fmt = (n: number) => new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(n);

  if (loading) return <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}><CircularProgress /></Box>;
  if (error) return <Alert severity="error">{error}</Alert>;
  if (!data) return null;

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} sx={{ mb: 3 }}>Dashboard</Typography>

      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard title="Total Revenue" value={fmt(data.totalRevenue)} icon={<TrendingUp />} color="primary.main" />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard title="This Month" value={fmt(data.thisMonthRevenue)} icon={<TrendingUp />} color="success.main" />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard title="Pending" value={fmt(data.pendingAmount)} icon={<HourglassEmpty />} color="warning.main" />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard title="Active Customers" value={data.activeCustomers.toString()} icon={<People />} color="info.main" />
        </Grid>
      </Grid>

      <Grid container spacing={3}>
        <Grid item xs={12} md={4}>
          <Card elevation={2}>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2 }}>Invoices by Status</Typography>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography>Total</Typography>
                  <Chip label={data.totalInvoices} size="small" />
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography>Pending</Typography>
                  <Chip label={data.invoicesByStatus.pending} size="small" color="warning" />
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography>Paid</Typography>
                  <Chip label={data.invoicesByStatus.paid} size="small" color="success" />
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography>Cancelled</Typography>
                  <Chip label={data.invoicesByStatus.cancelled} size="small" color="error" />
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={8}>
          <Card elevation={2}>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2 }}>Recent Invoices</Typography>
              {data.recentInvoices.slice(0, 5).map((inv) => (
                <Box key={inv.id} sx={{ display: 'flex', justifyContent: 'space-between', py: 1, borderBottom: '1px solid', borderColor: 'divider' }}>
                  <Box>
                    <Typography variant="body2" fontWeight={600}>{inv.invoiceNumber}</Typography>
                    <Typography variant="caption" color="text.secondary">{inv.customerName}</Typography>
                  </Box>
                  <Box sx={{ textAlign: 'right' }}>
                    <Typography variant="body2">{fmt(inv.total)}</Typography>
                    <Chip
                      label={inv.statusName}
                      size="small"
                      color={inv.status === 'Paid' ? 'success' : inv.status === 'Cancelled' ? 'error' : 'warning'}
                    />
                  </Box>
                </Box>
              ))}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
