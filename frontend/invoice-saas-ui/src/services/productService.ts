import api from './api';
import { ApiResponse, PagedResult, Product, PaginationParams } from '../types';

const BASE = '/api/products';

export const productService = {
  getAll: async (params?: PaginationParams): Promise<PagedResult<Product>> => {
    const { data } = await api.get<ApiResponse<PagedResult<Product>>>(BASE, { params });
    return data.data!;
  },

  getById: async (id: string): Promise<Product> => {
    const { data } = await api.get<ApiResponse<Product>>(`${BASE}/${id}`);
    return data.data!;
  },

  search: async (term: string): Promise<Product[]> => {
    const { data } = await api.get<ApiResponse<Product[]>>(`${BASE}/search`, { params: { term } });
    return data.data!;
  },

  create: async (payload: Partial<Product>): Promise<Product> => {
    const { data } = await api.post<ApiResponse<Product>>(BASE, payload);
    return data.data!;
  },

  update: async (id: string, payload: Partial<Product>): Promise<Product> => {
    const { data } = await api.put<ApiResponse<Product>>(`${BASE}/${id}`, payload);
    return data.data!;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`${BASE}/${id}`);
  },
};
