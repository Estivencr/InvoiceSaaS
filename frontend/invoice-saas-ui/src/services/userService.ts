import api from './api';
import { ApiResponse, PagedResult, User, PaginationParams } from '../types';

const BASE = '/api/security';

export const userService = {
  getAll: async (params?: PaginationParams): Promise<PagedResult<User>> => {
    const { data } = await api.get<ApiResponse<PagedResult<User>>>(`${BASE}/users`, { params });
    return data.data!;
  },

  create: async (payload: object): Promise<User> => {
    const { data } = await api.post<ApiResponse<User>>(`${BASE}/users`, payload);
    return data.data!;
  },

  update: async (id: string, payload: object): Promise<User> => {
    const { data } = await api.put<ApiResponse<User>>(`${BASE}/users/${id}`, payload);
    return data.data!;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`${BASE}/users/${id}`);
  },

  toggleStatus: async (id: string): Promise<User> => {
    const { data } = await api.patch<ApiResponse<User>>(`${BASE}/users/${id}/toggle-status`);
    return data.data!;
  },

  changeRole: async (id: string, roleIds: string[]): Promise<User> => {
    const { data } = await api.patch<ApiResponse<User>>(`${BASE}/users/${id}/change-role`, { roleIds });
    return data.data!;
  },
};
