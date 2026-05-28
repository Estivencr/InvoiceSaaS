import api from './api';
import { ApiResponse, LoginResponse } from '../types';

export const authService = {
  login: async (email: string, password: string): Promise<LoginResponse> => {
    const { data } = await api.post<ApiResponse<LoginResponse>>('/api/auth/login', { email, password });
    return data.data!;
  },

  register: async (payload: object): Promise<LoginResponse> => {
    const { data } = await api.post<ApiResponse<LoginResponse>>('/api/auth/register', payload);
    return data.data!;
  },

  logout: async (): Promise<void> => {
    await api.post('/api/auth/logout');
    localStorage.clear();
  },

  changePassword: async (currentPassword: string, newPassword: string, confirmNewPassword: string): Promise<void> => {
    await api.post('/api/auth/change-password', { currentPassword, newPassword, confirmNewPassword });
  },
};
