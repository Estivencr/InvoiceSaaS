import React, { useEffect, useState, useCallback } from 'react';
import {
  Box, Button, Chip, Dialog, DialogActions, DialogContent, DialogTitle,
  IconButton, MenuItem, Select, TextField, Typography, Alert, FormControl,
  InputLabel, OutlinedInput, Checkbox, ListItemText
} from '@mui/material';
import { DataGrid, GridColDef, GridPaginationModel } from '@mui/x-data-grid';
import { Add, Edit, ToggleOn, ToggleOff } from '@mui/icons-material';
import { userService } from '../../services/userService';
import { employeeService } from '../../services/employeeService';
import { User, Role } from '../../types';

const EMPTY_FORM = { firstName: '', lastName: '', email: '', password: '', roleIds: [] as string[] };

export default function UserList() {
  const [rows, setRows] = useState<User[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({ page: 0, pageSize: 10 });
  const [dialogOpen, setDialogOpen] = useState(false);
  const [roleDialogUser, setRoleDialogUser] = useState<User | null>(null);
  const [selectedRoleIds, setSelectedRoleIds] = useState<string[]>([]);
  const [form, setForm] = useState(EMPTY_FORM);
  const [roles, setRoles] = useState<Role[]>([]);

  useEffect(() => {
    employeeService.getRoles().then(setRoles).catch(() => {});
  }, []);

  const loadUsers = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const result = await userService.getAll({
        page: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
        search: search || undefined,
      });
      setRows(result.items);
      setTotal(result.totalCount);
    } catch {
      setError('Error al cargar usuarios.');
    } finally {
      setLoading(false);
    }
  }, [paginationModel, search]);

  useEffect(() => { loadUsers(); }, [loadUsers]);

  const handleToggleStatus = async (id: string) => {
    try {
      await userService.toggleStatus(id);
      loadUsers();
    } catch {
      setError('Error al cambiar estado.');
    }
  };

  const openRoleDialog = (user: User) => {
    setRoleDialogUser(user);
    setSelectedRoleIds(
      roles.filter(r => user.roles.includes(r.name)).map(r => r.id)
    );
  };

  const handleSaveRoles = async () => {
    if (!roleDialogUser) return;
    try {
      await userService.changeRole(roleDialogUser.id, selectedRoleIds);
      setRoleDialogUser(null);
      loadUsers();
    } catch {
      setError('Error al cambiar roles.');
    }
  };

  const handleCreate = async () => {
    setError('');
    try {
      await userService.create({
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        password: form.password,
        roleIds: form.roleIds,
      });
      setDialogOpen(false);
      setForm(EMPTY_FORM);
      loadUsers();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al crear usuario.');
    }
  };

  const columns: GridColDef[] = [
    { field: 'fullName', headerName: 'Nombre', flex: 1.5 },
    { field: 'email', headerName: 'Email', flex: 1.5 },
    {
      field: 'roles', headerName: 'Roles', flex: 1,
      renderCell: (p) => (
        <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
          {(p.value as string[]).map((r: string) => (
            <Chip key={r} label={r} size="small" />
          ))}
        </Box>
      ),
    },
    {
      field: 'isActive', headerName: 'Estado', flex: 0.8,
      renderCell: (p) => (
        <Chip
          label={p.value ? 'Activo' : 'Inactivo'}
          size="small"
          color={p.value ? 'success' : 'default'}
        />
      ),
    },
    {
      field: 'lastLogin', headerName: 'Último acceso', flex: 1,
      valueFormatter: (v) => v ? new Date(v as string).toLocaleString() : '-',
    },
    {
      field: 'actions', headerName: '', flex: 0.9, sortable: false,
      renderCell: (p) => (
        <Box>
          <IconButton size="small" title="Cambiar roles" onClick={() => openRoleDialog(p.row)}>
            <Edit fontSize="small" />
          </IconButton>
          <IconButton
            size="small"
            color={p.row.isActive ? 'error' : 'success'}
            title={p.row.isActive ? 'Desactivar' : 'Activar'}
            onClick={() => handleToggleStatus(p.row.id)}
          >
            {p.row.isActive ? <ToggleOff fontSize="small" /> : <ToggleOn fontSize="small" />}
          </IconButton>
        </Box>
      ),
    },
  ];

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Typography variant="h5" fontWeight={700}>Usuarios</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => { setForm(EMPTY_FORM); setDialogOpen(true); }}>
          Nuevo Usuario
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

      {/* Create dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Nuevo Usuario</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField
              label="Nombre"
              value={form.firstName}
              onChange={(e) => setForm(f => ({ ...f, firstName: e.target.value }))}
            />
            <TextField
              label="Apellido"
              value={form.lastName}
              onChange={(e) => setForm(f => ({ ...f, lastName: e.target.value }))}
            />
            <TextField
              label="Email"
              type="email"
              required
              value={form.email}
              onChange={(e) => setForm(f => ({ ...f, email: e.target.value }))}
            />
            <TextField
              label="Contraseña"
              type="password"
              required
              value={form.password}
              onChange={(e) => setForm(f => ({ ...f, password: e.target.value }))}
            />
            <FormControl>
              <InputLabel>Roles</InputLabel>
              <Select
                multiple
                value={form.roleIds}
                onChange={(e) => setForm(f => ({ ...f, roleIds: e.target.value as string[] }))}
                input={<OutlinedInput label="Roles" />}
                renderValue={(selected) =>
                  roles.filter(r => (selected as string[]).includes(r.id)).map(r => r.name).join(', ')
                }
              >
                {roles.map((r) => (
                  <MenuItem key={r.id} value={r.id}>
                    <Checkbox checked={form.roleIds.includes(r.id)} />
                    <ListItemText primary={r.name} />
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancelar</Button>
          <Button
            variant="contained"
            onClick={handleCreate}
            disabled={!form.email || !form.password}
          >
            Crear
          </Button>
        </DialogActions>
      </Dialog>

      {/* Change roles dialog */}
      <Dialog open={Boolean(roleDialogUser)} onClose={() => setRoleDialogUser(null)} maxWidth="xs" fullWidth>
        <DialogTitle>Cambiar Roles — {roleDialogUser?.fullName}</DialogTitle>
        <DialogContent>
          <FormControl fullWidth sx={{ mt: 1 }}>
            <InputLabel>Roles</InputLabel>
            <Select
              multiple
              value={selectedRoleIds}
              onChange={(e) => setSelectedRoleIds(e.target.value as string[])}
              input={<OutlinedInput label="Roles" />}
              renderValue={(selected) =>
                roles.filter(r => (selected as string[]).includes(r.id)).map(r => r.name).join(', ')
              }
            >
              {roles.map((r) => (
                <MenuItem key={r.id} value={r.id}>
                  <Checkbox checked={selectedRoleIds.includes(r.id)} />
                  <ListItemText primary={r.name} />
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRoleDialogUser(null)}>Cancelar</Button>
          <Button variant="contained" onClick={handleSaveRoles}>Guardar</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
