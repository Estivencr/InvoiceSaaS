import api from './api';
import { ApiResponse, PagedResult, Customer, PaginationParams } from '../types';

const BASE = '/api/customers';

export const customerService = {
  getAll: async (params?: PaginationParams): Promise<PagedResult<Customer>> => {
    const { data } = await api.get<ApiResponse<PagedResult<Customer>>>(BASE, { params });
    return data.data!;
  },

  getById: async (id: string): Promise<Customer> => {
    const { data } = await api.get<ApiResponse<Customer>>(`${BASE}/${id}`);
    return data.data!;
  },

  search: async (term: string): Promise<Customer[]> => {
    const { data } = await api.get<ApiResponse<Customer[]>>(`${BASE}/search`, { params: { term } });
    return data.data!;
  },

  create: async (payload: Partial<Customer>): Promise<Customer> => {
    const { data } = await api.post<ApiResponse<Customer>>(BASE, payload);
    return data.data!;
  },

  update: async (id: string, payload: Partial<Customer>): Promise<Customer> => {
    const { data } = await api.put<ApiResponse<Customer>>(`${BASE}/${id}`, payload);
    return data.data!;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`${BASE}/${id}`);
  },
};
