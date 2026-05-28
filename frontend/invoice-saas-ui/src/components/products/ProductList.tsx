import React, { useCallback, useEffect, useState } from 'react';
import {
  Box, Button, Chip, Dialog, DialogActions, DialogContent, DialogTitle,
  IconButton, TextField, Typography, Alert, Grid, InputAdornment,
  FormControlLabel, Switch, MenuItem, Select, FormControl, InputLabel
} from '@mui/material';
import { DataGrid, GridColDef, GridPaginationModel } from '@mui/x-data-grid';
import { Add, Edit, Delete, Search, Inventory } from '@mui/icons-material';
import { productService } from '../../services/productService';
import { Product } from '../../types';

const UNITS = ['unit', 'hour', 'kg', 'g', 'L', 'm', 'm²', 'service'];

const fmt = (n: number) =>
  new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(n);

interface FormState {
  name: string; description: string; sku: string; category: string;
  unitPrice: number; stock: number; unit: string; isActive: boolean;
}

const emptyForm = (): FormState => ({
  name: '', description: '', sku: '', category: '',
  unitPrice: 0, stock: 0, unit: 'unit', isActive: true,
});

export default function ProductList() {
  const [rows, setRows] = useState<Product[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({ page: 0, pageSize: 10 });

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm());
  const [formError, setFormError] = useState('');
  const [saving, setSaving] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const result = await productService.getAll({
        page: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
        search: search || undefined,
      });
      setRows(result.items);
      setTotal(result.totalCount);
    } catch {
      setError('Error al cargar los productos.');
    } finally {
      setLoading(false);
    }
  }, [paginationModel, search]);

  useEffect(() => { load(); }, [load]);

  const openCreate = () => { setEditId(null); setForm(emptyForm()); setFormError(''); setDialogOpen(true); };

  const openEdit = (p: Product) => {
    setEditId(p.id);
    setForm({ name: p.name, description: p.description || '', sku: p.sku || '',
      category: p.category || '', unitPrice: p.unitPrice, stock: p.stock,
      unit: p.unit, isActive: p.isActive });
    setFormError('');
    setDialogOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('¿Eliminar este producto?')) return;
    try {
      await productService.delete(id);
      load();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al eliminar.');
    }
  };

  const handleSave = async () => {
    setFormError('');
    if (!form.name.trim()) { setFormError('El nombre es obligatorio.'); return; }
    if (form.unitPrice <= 0) { setFormError('El precio debe ser mayor a 0.'); return; }

    setSaving(true);
    try {
      if (editId) {
        await productService.update(editId, form);
      } else {
        await productService.create(form);
      }
      setDialogOpen(false);
      load();
    } catch (err: any) {
      setFormError(err.response?.data?.message || 'Error al guardar.');
    } finally {
      setSaving(false);
    }
  };

  const columns: GridColDef[] = [
    { field: 'sku', headerName: 'SKU', width: 110, renderCell: (p) => p.value || <Typography color="text.disabled" variant="body2">—</Typography> },
    { field: 'name', headerName: 'Nombre', flex: 2 },
    { field: 'category', headerName: 'Categoría', flex: 1, renderCell: (p) => p.value || '—' },
    { field: 'unitPrice', headerName: 'Precio', flex: 1, valueFormatter: (v) => fmt(v as number) },
    { field: 'stock', headerName: 'Stock', width: 90, align: 'center', headerAlign: 'center' },
    { field: 'unit', headerName: 'Unidad', width: 90 },
    {
      field: 'isActive', headerName: 'Estado', width: 100,
      renderCell: (p) => <Chip label={p.value ? 'Activo' : 'Inactivo'} size="small" color={p.value ? 'success' : 'default'} />,
    },
    {
      field: 'actions', headerName: '', width: 90, sortable: false,
      renderCell: (p) => (
        <Box>
          <IconButton size="small" onClick={() => openEdit(p.row)}><Edit fontSize="small" /></IconButton>
          <IconButton size="small" color="error" onClick={() => handleDelete(p.row.id)}><Delete fontSize="small" /></IconButton>
        </Box>
      ),
    },
  ];

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Inventory color="primary" />
          <Typography variant="h5" fontWeight={700}>Productos</Typography>
        </Box>
        <Button variant="contained" startIcon={<Add />} onClick={openCreate}>Nuevo Producto</Button>
      </Box>

      <Box sx={{ mb: 2 }}>
        <TextField
          size="small" placeholder="Buscar por nombre, SKU o categoría..."
          value={search} onChange={(e) => { setSearch(e.target.value); setPaginationModel(m => ({ ...m, page: 0 })); }}
          sx={{ width: 320 }}
          InputProps={{ startAdornment: <InputAdornment position="start"><Search fontSize="small" /></InputAdornment> }}
        />
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <DataGrid
        rows={rows} columns={columns} rowCount={total} loading={loading}
        paginationMode="server" paginationModel={paginationModel}
        onPaginationModelChange={setPaginationModel}
        pageSizeOptions={[10, 25, 50]} autoHeight disableRowSelectionOnClick
      />

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editId ? 'Editar Producto' : 'Nuevo Producto'}</DialogTitle>
        <DialogContent>
          {formError && <Alert severity="error" sx={{ mb: 2 }}>{formError}</Alert>}
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid item xs={12} sm={8}>
              <TextField label="Nombre *" value={form.name} onChange={(e) => setForm(f => ({ ...f, name: e.target.value }))}
                fullWidth size="small" />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField label="SKU" value={form.sku} onChange={(e) => setForm(f => ({ ...f, sku: e.target.value }))}
                fullWidth size="small" />
            </Grid>
            <Grid item xs={12}>
              <TextField label="Descripción" value={form.description} onChange={(e) => setForm(f => ({ ...f, description: e.target.value }))}
                fullWidth size="small" multiline rows={2} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField label="Categoría" value={form.category} onChange={(e) => setForm(f => ({ ...f, category: e.target.value }))}
                fullWidth size="small" />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth size="small">
                <InputLabel>Unidad</InputLabel>
                <Select label="Unidad" value={form.unit} onChange={(e) => setForm(f => ({ ...f, unit: e.target.value }))}>
                  {UNITS.map(u => <MenuItem key={u} value={u}>{u}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField label="Precio unitario *" type="number" value={form.unitPrice}
                onChange={(e) => setForm(f => ({ ...f, unitPrice: Number(e.target.value) }))}
                fullWidth size="small"
                InputProps={{ startAdornment: <InputAdornment position="start">$</InputAdornment> }} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField label="Stock" type="number" value={form.stock}
                onChange={(e) => setForm(f => ({ ...f, stock: Number(e.target.value) }))}
                fullWidth size="small" />
            </Grid>
            {editId && (
              <Grid item xs={12}>
                <FormControlLabel
                  control={<Switch checked={form.isActive} onChange={(e) => setForm(f => ({ ...f, isActive: e.target.checked }))} />}
                  label="Producto activo" />
              </Grid>
            )}
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancelar</Button>
          <Button variant="contained" onClick={handleSave} disabled={saving}>
            {saving ? 'Guardando...' : 'Guardar'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
