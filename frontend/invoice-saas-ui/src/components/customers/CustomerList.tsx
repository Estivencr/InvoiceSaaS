import React, { useEffect, useState, useCallback } from 'react';
import {
  Box, Button, Chip, Dialog, DialogActions, DialogContent, DialogTitle,
  IconButton, TextField, Typography, Alert, CircularProgress
} from '@mui/material';
import { DataGrid, GridColDef, GridPaginationModel } from '@mui/x-data-grid';
import { Add, Edit, Delete } from '@mui/icons-material';
import { customerService } from '../../services/customerService';
import { Customer } from '../../types';

export default function CustomerList() {
  const [rows, setRows] = useState<Customer[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({ page: 0, pageSize: 10 });
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingCustomer, setEditingCustomer] = useState<Partial<Customer> | null>(null);

  const loadCustomers = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const result = await customerService.getAll({
        page: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
        search: search || undefined,
      });
      setRows(result.items);
      setTotal(result.totalCount);
    } catch {
      setError('Failed to load customers.');
    } finally {
      setLoading(false);
    }
  }, [paginationModel, search]);

  useEffect(() => { loadCustomers(); }, [loadCustomers]);

  const handleSave = async () => {
    if (!editingCustomer) return;
    try {
      if (editingCustomer.id) {
        await customerService.update(editingCustomer.id, editingCustomer);
      } else {
        await customerService.create(editingCustomer);
      }
      setDialogOpen(false);
      loadCustomers();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Save failed.');
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Delete this customer?')) return;
    try {
      await customerService.delete(id);
      loadCustomers();
    } catch {
      setError('Delete failed.');
    }
  };

  const columns: GridColDef[] = [
    { field: 'name', headerName: 'Name', flex: 1.5 },
    { field: 'document', headerName: 'Document', flex: 1 },
    { field: 'email', headerName: 'Email', flex: 1.5 },
    { field: 'phone', headerName: 'Phone', flex: 1 },
    { field: 'city', headerName: 'City', flex: 1 },
    {
      field: 'status', headerName: 'Status', flex: 0.8,
      renderCell: (p) => <Chip label={p.value} size="small" color={p.value === 'active' ? 'success' : 'default'} />
    },
    {
      field: 'actions', headerName: '', flex: 0.8, sortable: false,
      renderCell: (p) => (
        <Box>
          <IconButton size="small" onClick={() => { setEditingCustomer(p.row); setDialogOpen(true); }}>
            <Edit fontSize="small" />
          </IconButton>
          <IconButton size="small" color="error" onClick={() => handleDelete(p.row.id)}>
            <Delete fontSize="small" />
          </IconButton>
        </Box>
      ),
    },
  ];

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Typography variant="h5" fontWeight={700}>Customers</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => { setEditingCustomer({}); setDialogOpen(true); }}>
          New Customer
        </Button>
      </Box>

      <TextField
        placeholder="Search by name, document or email..."
        size="small"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        sx={{ mb: 2, width: 350 }}
      />

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

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingCustomer?.id ? 'Edit Customer' : 'New Customer'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField label="Name" required value={editingCustomer?.name || ''} onChange={(e) => setEditingCustomer(p => ({ ...p, name: e.target.value }))} />
            <TextField label="Document" required value={editingCustomer?.document || ''} onChange={(e) => setEditingCustomer(p => ({ ...p, document: e.target.value }))} />
            <TextField label="Email" type="email" required value={editingCustomer?.email || ''} onChange={(e) => setEditingCustomer(p => ({ ...p, email: e.target.value }))} />
            <TextField label="Phone" value={editingCustomer?.phone || ''} onChange={(e) => setEditingCustomer(p => ({ ...p, phone: e.target.value }))} />
            <TextField label="Address" value={editingCustomer?.address || ''} onChange={(e) => setEditingCustomer(p => ({ ...p, address: e.target.value }))} />
            <TextField label="City" value={editingCustomer?.city || ''} onChange={(e) => setEditingCustomer(p => ({ ...p, city: e.target.value }))} />
            <TextField label="Country" value={editingCustomer?.country || ''} onChange={(e) => setEditingCustomer(p => ({ ...p, country: e.target.value }))} />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSave}>Save</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
