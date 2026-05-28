import api from './api';
import { ApiResponse, PagedResult, Employee, PaginationParams, Role } from '../types';

const BASE = '/api/employees';

export const employeeService = {
  getRoles: async (): Promise<Role[]> => {
    const { data } = await api.get<ApiResponse<Role[]>>(`${BASE}/roles`);
    return data.data!;
  },

  getAll: async (params?: PaginationParams): Promise<PagedResult<Employee>> => {
    const { data } = await api.get<ApiResponse<PagedResult<Employee>>>(BASE, { params });
    return data.data!;
  },

  create: async (payload: object): Promise<Employee> => {
    const { data } = await api.post<ApiResponse<Employee>>(BASE, payload);
    return data.data!;
  },

  update: async (id: string, payload: object): Promise<Employee> => {
    const { data } = await api.put<ApiResponse<Employee>>(`${BASE}/${id}`, payload);
    return data.data!;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`${BASE}/${id}`);
  },
};
