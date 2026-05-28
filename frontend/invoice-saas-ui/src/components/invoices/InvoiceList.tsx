import React, { useEffect, useState, useCallback } from 'react';
import {
  Box, Button, Chip, IconButton, Typography, Alert, Select, MenuItem, FormControl, InputLabel
} from '@mui/material';
import { DataGrid, GridColDef, GridPaginationModel } from '@mui/x-data-grid';
import { Add, CheckCircle, Cancel, Visibility } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { invoiceService } from '../../services/invoiceService';
import { Invoice } from '../../types';

const STATUS_COLORS: Record<string, 'warning' | 'success' | 'error' | 'default'> = {
  Pending: 'warning',
  Paid: 'success',
  Cancelled: 'error',
};

export default function InvoiceList() {
  const navigate = useNavigate();
  const [rows, setRows] = useState<Invoice[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({ page: 0, pageSize: 10 });

  const fmt = (n: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(n);

  const loadInvoices = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const result = await invoiceService.getAll({
        page: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
        status: statusFilter || undefined,
      });
      setRows(result.items);
      setTotal(result.totalCount);
    } catch {
      setError('Failed to load invoices.');
    } finally {
      setLoading(false);
    }
  }, [paginationModel, statusFilter]);

  useEffect(() => { loadInvoices(); }, [loadInvoices]);

  const handleMarkPaid = async (id: string) => {
    try {
      await invoiceService.updateStatus(id, 'Paid');
      loadInvoices();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Update failed.');
    }
  };

  const handleCancel = async (id: string) => {
    if (!window.confirm('Cancel this invoice?')) return;
    try {
      await invoiceService.updateStatus(id, 'Cancelled');
      loadInvoices();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Cancel failed.');
    }
  };

  const columns: GridColDef[] = [
    { field: 'invoiceNumber', headerName: 'Invoice #', flex: 1 },
    { field: 'customerName', headerName: 'Customer', flex: 1.5 },
    { field: 'issueDate', headerName: 'Issue Date', flex: 1, valueFormatter: (v) => new Date(v as string).toLocaleDateString() },
    { field: 'dueDate', headerName: 'Due Date', flex: 1, valueFormatter: (v) => v ? new Date(v as string).toLocaleDateString() : '-' },
    { field: 'subtotal', headerName: 'Subtotal', flex: 1, valueFormatter: (v) => fmt(v as number) },
    { field: 'total', headerName: 'Total', flex: 1, valueFormatter: (v) => fmt(v as number) },
    {
      field: 'status', headerName: 'Status', flex: 1,
      renderCell: (p) => <Chip label={p.value} size="small" color={STATUS_COLORS[p.value] || 'default'} />
    },
    {
      field: 'actions', headerName: '', flex: 1, sortable: false,
      renderCell: (p) => (
        <Box>
          <IconButton size="small" title="Ver detalle" onClick={() => navigate(`/invoices/${p.row.id}`)}>
            <Visibility fontSize="small" />
          </IconButton>
          {p.row.status === 'Pending' && (
            <>
              <IconButton size="small" color="success" title="Marcar Pagada" onClick={() => handleMarkPaid(p.row.id)}>
                <CheckCircle fontSize="small" />
              </IconButton>
              <IconButton size="small" color="error" title="Cancelar" onClick={() => handleCancel(p.row.id)}>
                <Cancel fontSize="small" />
              </IconButton>
            </>
          )}
        </Box>
      ),
    },
  ];

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Typography variant="h5" fontWeight={700}>Invoices</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/invoices/new')}>Nueva Factura</Button>
      </Box>

      <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
        <FormControl size="small" sx={{ minWidth: 160 }}>
          <InputLabel>Status</InputLabel>
          <Select value={statusFilter} label="Status" onChange={(e) => setStatusFilter(e.target.value)}>
            <MenuItem value="">All</MenuItem>
            <MenuItem value="Pending">Pending</MenuItem>
            <MenuItem value="Paid">Paid</MenuItem>
            <MenuItem value="Cancelled">Cancelled</MenuItem>
          </Select>
        </FormControl>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <DataGrid
        rows={rows}
        columns={columns}
        rowCount={total}
        loading={loading}
        paginationMode="server"
        paginationModel={paginationModel}
        onPaginationModelChange={setPaginationModel}
        pageSizeOptions={[10, 25, 50]}
        autoHeight
        disableRowSelectionOnClick
      />
    </Box>
  );
}
