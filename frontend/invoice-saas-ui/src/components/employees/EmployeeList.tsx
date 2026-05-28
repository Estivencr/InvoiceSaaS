import React, { useEffect, useState, useCallback } from 'react';
import {
  Box, Button, Chip, Dialog, DialogActions, DialogContent, DialogTitle,
  IconButton, MenuItem, Select, TextField, Typography, Alert, FormControl, InputLabel
} from '@mui/material';
import { DataGrid, GridColDef, GridPaginationModel } from '@mui/x-data-grid';
import { Add, Edit, Delete } from '@mui/icons-material';
import { employeeService } from '../../services/employeeService';
import { Employee, Role } from '../../types';

const STATUS_COLORS: Record<string, 'success' | 'warning' | 'error' | 'default'> = {
  active: 'success',
  inactive: 'warning',
  terminated: 'error',
};

const EMPTY_FORM = { name: '', email: '', position: '', roleId: '', hireDate: '', status: 'active' };

export default function EmployeeList() {
  const [rows, setRows] = useState<Employee[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({ page: 0, pageSize: 10 });
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState(EMPTY_FORM);
  const [roles, setRoles] = useState<Role[]>([]);

  useEffect(() => {
    employeeService.getRoles().then(setRoles).catch(() => {});
  }, []);

  const loadEmployees = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const result = await employeeService.getAll({
        page: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
        search: search || undefined,
      });
      setRows(result.items);
      setTotal(result.totalCount);
    } catch {
      setError('Error al cargar empleados.');
    } finally {
      setLoading(false);
    }
  }, [paginationModel, search]);

  useEffect(() => { loadEmployees(); }, [loadEmployees]);

  const openCreate = () => {
    setEditingId(null);
    setForm(EMPTY_FORM);
    setDialogOpen(true);
  };

  const openEdit = (emp: Employee) => {
    setEditingId(emp.id);
    setForm({
      name: emp.name,
      email: emp.email,
      position: emp.position || '',
      roleId: emp.roleId,
      hireDate: emp.hireDate ? emp.hireDate.split('T')[0] : '',
      status: emp.status,
    });
    setDialogOpen(true);
  };

  const handleSave = async () => {
    setError('');
    try {
      const payload = {
        name: form.name,
        email: form.email,
        position: form.position || undefined,
        roleId: form.roleId,
        hireDate: form.hireDate || undefined,
        status: form.status,
      };
      if (editingId) {
        await employeeService.update(editingId, payload);
      } else {
        await employeeService.create(payload);
      }
      setDialogOpen(false);
      loadEmployees();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al guardar.');
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('¿Eliminar este empleado?')) return;
    try {
      await employeeService.delete(id);
      loadEmployees();
    } catch {
      setError('Error al eliminar.');
    }
  };

  const columns: GridColDef[] = [
    { field: 'name', headerName: 'Nombre', flex: 1.5 },
    { field: 'email', headerName: 'Email', flex: 1.5 },
    { field: 'position', headerName: 'Cargo', flex: 1 },
    { field: 'roleName', headerName: 'Rol', flex: 1 },
    {
      field: 'hireDate', headerName: 'Ingreso', flex: 1,
      valueFormatter: (v) => v ? new Date(v as string).toLocaleDateString() : '-',
    },
    {
      field: 'status', headerName: 'Estado', flex: 0.8,
      renderCell: (p) => (
        <Chip label={p.value} size="small" color={STATUS_COLORS[p.value] || 'default'} />
      ),
    },
    {
      field: 'actions', headerName: '', flex: 0.7, sortable: false,
      renderCell: (p) => (
        <Box>
          <IconButton size="small" onClick={() => openEdit(p.row)}>
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
        <Typography variant="h5" fontWeight={700}>Empleados</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={openCreate}>
          Nuevo Empleado
        </Button>
      </Box>

      <TextField
        placeholder="Buscar por nombre o email..."
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
        <DialogTitle>{editingId ? 'Editar Empleado' : 'Nuevo Empleado'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField
              label="Nombre completo"
              required
              value={form.name}
              onChange={(e) => setForm(f => ({ ...f, name: e.target.value }))}
            />
            <TextField
              label="Email"
              type="email"
              required
              value={form.email}
              onChange={(e) => setForm(f => ({ ...f, email: e.target.value }))}
            />
            <TextField
              label="Cargo / Posición"
              value={form.position}
              onChange={(e) => setForm(f => ({ ...f, position: e.target.value }))}
            />
            <FormControl required>
              <InputLabel>Rol</InputLabel>
              <Select
                value={form.roleId}
                label="Rol"
                onChange={(e) => setForm(f => ({ ...f, roleId: e.target.value }))}
              >
                {roles.map((r) => (
                  <MenuItem key={r.id} value={r.id}>{r.name}</MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              label="Fecha de ingreso"
              type="date"
              value={form.hireDate}
              onChange={(e) => setForm(f => ({ ...f, hireDate: e.target.value }))}
              InputLabelProps={{ shrink: true }}
            />
            {editingId && (
              <FormControl>
                <InputLabel>Estado</InputLabel>
                <Select
                  value={form.status}
                  label="Estado"
                  onChange={(e) => setForm(f => ({ ...f, status: e.target.value }))}
                >
                  <MenuItem value="active">Activo</MenuItem>
                  <MenuItem value="inactive">Inactivo</MenuItem>
                  <MenuItem value="terminated">Terminado</MenuItem>
                </Select>
              </FormControl>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancelar</Button>
          <Button
            variant="contained"
            onClick={handleSave}
            disabled={!form.name || !form.email || !form.roleId}
          >
            Guardar
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
