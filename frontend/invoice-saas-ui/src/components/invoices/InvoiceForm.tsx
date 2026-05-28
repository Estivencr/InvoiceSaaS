import React, { useEffect, useState } from 'react';
import {
  Box, Button, TextField, Typography, Paper, Grid, IconButton,
  Autocomplete, Alert, Divider, Table, TableHead, TableRow,
  TableCell, TableBody, InputAdornment
} from '@mui/material';
import { Add, Delete, ArrowBack } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { customerService } from '../../services/customerService';
import { invoiceService } from '../../services/invoiceService';
import { Customer } from '../../types';

interface DetailLine {
  description: string;
  quantity: number;
  unitPrice: number;
}

const emptyLine = (): DetailLine => ({ description: '', quantity: 1, unitPrice: 0 });

const fmt = (n: number) =>
  new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(n);

export default function InvoiceForm() {
  const navigate = useNavigate();

  const [customer, setCustomer] = useState<Customer | null>(null);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [customerInput, setCustomerInput] = useState('');

  const today = new Date().toISOString().slice(0, 10);
  const [issueDate, setIssueDate] = useState(today);
  const [dueDate, setDueDate] = useState('');
  const [taxRate, setTaxRate] = useState(19);
  const [notes, setNotes] = useState('');
  const [lines, setLines] = useState<DetailLine[]>([emptyLine()]);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    const delay = setTimeout(async () => {
      if (customerInput.length < 2) return;
      try {
        const results = await customerService.search(customerInput);
        setCustomers(results);
      } catch { /* ignore */ }
    }, 300);
    return () => clearTimeout(delay);
  }, [customerInput]);

  const subtotal = lines.reduce((sum, l) => sum + l.quantity * l.unitPrice, 0);
  const taxAmount = subtotal * (taxRate / 100);
  const total = subtotal + taxAmount;

  const updateLine = (i: number, field: keyof DetailLine, value: string | number) => {
    setLines(prev => prev.map((l, idx) => idx === i ? { ...l, [field]: value } : l));
  };

  const addLine = () => setLines(prev => [...prev, emptyLine()]);
  const removeLine = (i: number) => setLines(prev => prev.filter((_, idx) => idx !== i));

  const handleSubmit = async () => {
    setError('');
    if (!customer) { setError('Selecciona un cliente.'); return; }
    if (lines.some(l => !l.description.trim())) { setError('Todos los ítems deben tener descripción.'); return; }
    if (lines.some(l => l.quantity <= 0 || l.unitPrice <= 0)) { setError('Cantidad y precio deben ser mayores a 0.'); return; }

    setSaving(true);
    try {
      await invoiceService.create({
        customerId: customer.id,
        issueDate: new Date(issueDate).toISOString(),
        dueDate: dueDate ? new Date(dueDate).toISOString() : undefined,
        taxRate,
        notes: notes || undefined,
        details: lines.map((l, idx) => ({
          description: l.description,
          quantity: l.quantity,
          unitPrice: l.unitPrice,
          sequence: idx + 1,
        })),
      });
      navigate('/invoices');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al crear la factura.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
        <IconButton onClick={() => navigate('/invoices')}><ArrowBack /></IconButton>
        <Typography variant="h5" fontWeight={700}>Nueva Factura</Typography>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Grid container spacing={3}>
        {/* Left column */}
        <Grid item xs={12} md={8}>
          <Paper sx={{ p: 3, mb: 3 }}>
            <Typography variant="subtitle1" fontWeight={600} gutterBottom>Cliente</Typography>
            <Autocomplete
              options={customers}
              getOptionLabel={(o) => `${o.name} — ${o.document}`}
              value={customer}
              onChange={(_, val) => setCustomer(val)}
              inputValue={customerInput}
              onInputChange={(_, val) => setCustomerInput(val)}
              filterOptions={(x) => x}
              renderInput={(params) => (
                <TextField {...params} label="Buscar cliente" size="small" fullWidth placeholder="Escribe el nombre o NIT..." />
              )}
            />
          </Paper>

          <Paper sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="subtitle1" fontWeight={600}>Ítems</Typography>
              <Button size="small" startIcon={<Add />} onClick={addLine}>Agregar ítem</Button>
            </Box>

            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell sx={{ width: '45%' }}>Descripción</TableCell>
                  <TableCell align="right" sx={{ width: '15%' }}>Cant.</TableCell>
                  <TableCell align="right" sx={{ width: '20%' }}>Precio unit.</TableCell>
                  <TableCell align="right" sx={{ width: '15%' }}>Total</TableCell>
                  <TableCell sx={{ width: '5%' }} />
                </TableRow>
              </TableHead>
              <TableBody>
                {lines.map((line, i) => (
                  <TableRow key={i}>
                    <TableCell>
                      <TextField
                        value={line.description}
                        onChange={(e) => updateLine(i, 'description', e.target.value)}
                        size="small" fullWidth variant="standard"
                        placeholder="Descripción del servicio o producto"
                      />
                    </TableCell>
                    <TableCell>
                      <TextField
                        value={line.quantity}
                        onChange={(e) => updateLine(i, 'quantity', Number(e.target.value))}
                        type="number" size="small" variant="standard"
                        inputProps={{ min: 0.01, step: 1, style: { textAlign: 'right' } }}
                      />
                    </TableCell>
                    <TableCell>
                      <TextField
                        value={line.unitPrice}
                        onChange={(e) => updateLine(i, 'unitPrice', Number(e.target.value))}
                        type="number" size="small" variant="standard"
                        inputProps={{ min: 0, step: 1000, style: { textAlign: 'right' } }}
                      />
                    </TableCell>
                    <TableCell align="right">
                      <Typography variant="body2">{fmt(line.quantity * line.unitPrice)}</Typography>
                    </TableCell>
                    <TableCell>
                      <IconButton size="small" onClick={() => removeLine(i)} disabled={lines.length === 1}>
                        <Delete fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </Paper>
        </Grid>

        {/* Right column */}
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 3, mb: 3 }}>
            <Typography variant="subtitle1" fontWeight={600} gutterBottom>Datos de la factura</Typography>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField label="Fecha de emisión" type="date" value={issueDate}
                  onChange={(e) => setIssueDate(e.target.value)} size="small" fullWidth
                  InputLabelProps={{ shrink: true }} />
              </Grid>
              <Grid item xs={12}>
                <TextField label="Fecha de vencimiento" type="date" value={dueDate}
                  onChange={(e) => setDueDate(e.target.value)} size="small" fullWidth
                  InputLabelProps={{ shrink: true }} />
              </Grid>
              <Grid item xs={12}>
                <TextField label="IVA (%)" type="number" value={taxRate}
                  onChange={(e) => setTaxRate(Number(e.target.value))}
                  size="small" fullWidth
                  InputProps={{ endAdornment: <InputAdornment position="end">%</InputAdornment> }} />
              </Grid>
              <Grid item xs={12}>
                <TextField label="Notas" value={notes} onChange={(e) => setNotes(e.target.value)}
                  size="small" fullWidth multiline rows={3} />
              </Grid>
            </Grid>
          </Paper>

          <Paper sx={{ p: 3 }}>
            <Typography variant="subtitle1" fontWeight={600} gutterBottom>Resumen</Typography>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
              <Typography variant="body2" color="text.secondary">Subtotal</Typography>
              <Typography variant="body2">{fmt(subtotal)}</Typography>
            </Box>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
              <Typography variant="body2" color="text.secondary">IVA ({taxRate}%)</Typography>
              <Typography variant="body2">{fmt(taxAmount)}</Typography>
            </Box>
            <Divider sx={{ my: 1 }} />
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
              <Typography variant="subtitle1" fontWeight={700}>Total</Typography>
              <Typography variant="subtitle1" fontWeight={700} color="primary">{fmt(total)}</Typography>
            </Box>
            <Button variant="contained" fullWidth size="large" onClick={handleSubmit} disabled={saving}>
              {saving ? 'Guardando...' : 'Crear Factura'}
            </Button>
            <Button variant="text" fullWidth sx={{ mt: 1 }} onClick={() => navigate('/invoices')}>
              Cancelar
            </Button>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
}
