import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box, Button, Chip, CircularProgress, Divider, Paper,
  Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Typography, Alert
} from '@mui/material';
import { ArrowBack, CheckCircle, Cancel, Print } from '@mui/icons-material';
import { invoiceService } from '../../services/invoiceService';
import { Invoice } from '../../types';

const STATUS_COLORS: Record<string, 'warning' | 'success' | 'error' | 'default'> = {
  Pending: 'warning',
  Paid: 'success',
  Cancelled: 'error',
};

const fmt = (n: number) =>
  new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(n);

const fmtDate = (d?: string) => d ? new Date(d).toLocaleDateString('es-CO') : '-';

export default function InvoiceDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [invoice, setInvoice] = useState<Invoice | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [actionError, setActionError] = useState('');

  useEffect(() => {
    if (!id) return;
    invoiceService.getById(id)
      .then(setInvoice)
      .catch(() => setError('Factura no encontrada.'))
      .finally(() => setLoading(false));
  }, [id]);

  const handleMarkPaid = async () => {
    if (!invoice) return;
    setActionError('');
    try {
      const updated = await invoiceService.updateStatus(invoice.id, 'Paid');
      setInvoice(updated);
    } catch (err: any) {
      setActionError(err.response?.data?.message || 'Error al marcar como pagada.');
    }
  };

  const handleCancel = async () => {
    if (!invoice || !window.confirm('¿Cancelar esta factura?')) return;
    setActionError('');
    try {
      const updated = await invoiceService.updateStatus(invoice.id, 'Cancelled');
      setInvoice(updated);
    } catch (err: any) {
      setActionError(err.response?.data?.message || 'Error al cancelar.');
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error || !invoice) {
    return (
      <Box>
        <Button startIcon={<ArrowBack />} onClick={() => navigate('/invoices')} sx={{ mb: 2 }}>
          Volver
        </Button>
        <Alert severity="error">{error || 'Factura no encontrada.'}</Alert>
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Button startIcon={<ArrowBack />} onClick={() => navigate('/invoices')}>
          Volver
        </Button>
        <Box sx={{ display: 'flex', gap: 1 }}>
          {invoice.status === 'Pending' && (
            <>
              <Button
                variant="contained"
                color="success"
                startIcon={<CheckCircle />}
                onClick={handleMarkPaid}
              >
                Marcar Pagada
              </Button>
              <Button
                variant="outlined"
                color="error"
                startIcon={<Cancel />}
                onClick={handleCancel}
              >
                Cancelar
              </Button>
            </>
          )}
          <Button variant="outlined" startIcon={<Print />} onClick={() => window.print()}>
            Imprimir
          </Button>
        </Box>
      </Box>

      {actionError && <Alert severity="error" sx={{ mb: 2 }}>{actionError}</Alert>}

      <Paper elevation={2} sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
          <Box>
            <Typography variant="h5" fontWeight={700}>{invoice.invoiceNumber}</Typography>
            <Chip
              label={invoice.statusName || invoice.status}
              color={STATUS_COLORS[invoice.status] || 'default'}
              size="small"
              sx={{ mt: 1 }}
            />
          </Box>
          <Box sx={{ textAlign: 'right' }}>
            <Typography variant="h4" fontWeight={700} color="primary">
              {fmt(invoice.total)}
            </Typography>
            <Typography variant="body2" color="text.secondary">Total</Typography>
          </Box>
        </Box>

        <Divider sx={{ my: 2 }} />

        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 3 }}>
          <Box sx={{ flex: '1 1 200px' }}>
            <Typography variant="subtitle2" color="text.secondary">Cliente</Typography>
            <Typography fontWeight={600}>{invoice.customerName}</Typography>
            <Typography variant="body2" color="text.secondary">{invoice.customerDocument}</Typography>
          </Box>
          <Box sx={{ flex: '1 1 200px' }}>
            <Typography variant="subtitle2" color="text.secondary">Creado por</Typography>
            <Typography>{invoice.createdByName}</Typography>
          </Box>
          <Box sx={{ flex: '1 1 130px' }}>
            <Typography variant="subtitle2" color="text.secondary">Fecha emisión</Typography>
            <Typography>{fmtDate(invoice.issueDate)}</Typography>
          </Box>
          <Box sx={{ flex: '1 1 130px' }}>
            <Typography variant="subtitle2" color="text.secondary">Fecha vencimiento</Typography>
            <Typography>{fmtDate(invoice.dueDate)}</Typography>
          </Box>
          <Box sx={{ flex: '1 1 130px' }}>
            <Typography variant="subtitle2" color="text.secondary">Fecha de pago</Typography>
            <Typography>{fmtDate(invoice.paymentDate)}</Typography>
          </Box>
          <Box sx={{ flex: '1 1 130px' }}>
            <Typography variant="subtitle2" color="text.secondary">Creado el</Typography>
            <Typography>{fmtDate(invoice.createdAt)}</Typography>
          </Box>
          {invoice.notes && (
            <Box sx={{ flex: '1 1 100%' }}>
              <Typography variant="subtitle2" color="text.secondary">Notas</Typography>
              <Typography>{invoice.notes}</Typography>
            </Box>
          )}
        </Box>
      </Paper>

      <Paper elevation={2} sx={{ mb: 3 }}>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow sx={{ bgcolor: 'grey.50' }}>
                <TableCell sx={{ fontWeight: 700 }}>#</TableCell>
                <TableCell sx={{ fontWeight: 700 }}>Descripción</TableCell>
                <TableCell sx={{ fontWeight: 700 }} align="right">Cantidad</TableCell>
                <TableCell sx={{ fontWeight: 700 }} align="right">Precio unitario</TableCell>
                <TableCell sx={{ fontWeight: 700 }} align="right">Total</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {invoice.details.map((d) => (
                <TableRow key={d.id} hover>
                  <TableCell>{d.sequence}</TableCell>
                  <TableCell>{d.description}</TableCell>
                  <TableCell align="right">{d.quantity}</TableCell>
                  <TableCell align="right">{fmt(d.unitPrice)}</TableCell>
                  <TableCell align="right">{fmt(d.amount)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>

      <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
        <Paper elevation={1} sx={{ p: 2, minWidth: 280 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
            <Typography color="text.secondary">Subtotal</Typography>
            <Typography>{fmt(invoice.subtotal)}</Typography>
          </Box>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
            <Typography color="text.secondary">IVA ({invoice.taxRate}%)</Typography>
            <Typography>{fmt(invoice.taxAmount)}</Typography>
          </Box>
          <Divider sx={{ my: 1 }} />
          <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
            <Typography fontWeight={700} variant="subtitle1">Total</Typography>
            <Typography fontWeight={700} variant="subtitle1" color="primary">
              {fmt(invoice.total)}
            </Typography>
          </Box>
        </Paper>
      </Box>
    </Box>
  );
}
