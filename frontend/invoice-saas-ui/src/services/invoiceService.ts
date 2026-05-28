import api from './api';
import { ApiResponse, PagedResult, Invoice, PaginationParams } from '../types';

const BASE = '/api/invoices';

export const invoiceService = {
  getAll: async (params?: PaginationParams): Promise<PagedResult<Invoice>> => {
    const { data } = await api.get<ApiResponse<PagedResult<Invoice>>>(BASE, { params });
    return data.data!;
  },

  getById: async (id: string): Promise<Invoice> => {
    const { data } = await api.get<ApiResponse<Invoice>>(`${BASE}/${id}`);
    return data.data!;
  },

  create: async (payload: object): Promise<Invoice> => {
    const { data } = await api.post<ApiResponse<Invoice>>(BASE, payload);
    return data.data!;
  },

  update: async (id: string, payload: object): Promise<Invoice> => {
    const { data } = await api.put<ApiResponse<Invoice>>(`${BASE}/${id}`, payload);
    return data.data!;
  },

  updateStatus: async (id: string, status: string, paymentDate?: string): Promise<Invoice> => {
    const { data } = await api.patch<ApiResponse<Invoice>>(`${BASE}/${id}/status`, { status, paymentDate });
    return data.data!;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`${BASE}/${id}`);
  },
};

export const reportService = {
  getDashboard: async () => {
    const { data } = await api.get('/api/reports/dashboard');
    return data.data;
  },
  getMonthlySales: async (months = 12) => {
    const { data } = await api.get('/api/reports/monthly-sales', { params: { months } });
    return data.data;
  },
};
