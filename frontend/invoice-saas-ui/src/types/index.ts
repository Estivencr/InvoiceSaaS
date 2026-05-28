export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: Record<string, string[]>;
  timestamp: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface UserInfo {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  companyId: string;
  roles: string[];
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserInfo;
}

export interface Customer {
  id: string;
  companyId: string;
  name: string;
  document: string;
  phone?: string;
  email: string;
  address?: string;
  city?: string;
  country?: string;
  status: string;
  createdAt: string;
}

export interface InvoiceDetail {
  id: string;
  invoiceId: string;
  description: string;
  quantity: number;
  unitPrice: number;
  amount: number;
  sequence: number;
}

export interface Invoice {
  id: string;
  companyId: string;
  invoiceNumber: string;
  customerId: string;
  customerName: string;
  customerDocument: string;
  createdById: string;
  createdByName: string;
  issueDate: string;
  dueDate?: string;
  subtotal: number;
  taxRate: number;
  taxAmount: number;
  total: number;
  status: 'Pending' | 'Paid' | 'Cancelled';
  statusName: string;
  paymentDate?: string;
  notes?: string;
  createdAt: string;
  details: InvoiceDetail[];
}

export interface DashboardData {
  totalInvoices: number;
  totalRevenue: number;
  pendingAmount: number;
  activeCustomers: number;
  thisMonthRevenue: number;
  invoicesByStatus: {
    pending: number;
    paid: number;
    cancelled: number;
  };
  recentInvoices: Invoice[];
}

export interface PaginationParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: string;
  dateFrom?: string;
  dateTo?: string;
}

export interface Product {
  id: string;
  companyId: string;
  name: string;
  description?: string;
  sku?: string;
  category?: string;
  unitPrice: number;
  stock: number;
  unit: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Role {
  id: string;
  name: string;
  description?: string;
}

export interface Employee {
  id: string;
  companyId: string;
  name: string;
  email: string;
  position?: string;
  roleId: string;
  roleName: string;
  hireDate?: string;
  status: string;
  createdAt: string;
}

export interface User {
  id: string;
  companyId: string;
  email: string;
  firstName?: string;
  lastName?: string;
  fullName: string;
  isActive: boolean;
  lastLogin?: string;
  roles: string[];
  createdAt: string;
}

export interface MonthlySales {
  year: number;
  month: number;
  monthName: string;
  invoiceCount: number;
  totalRevenue: number;
  paidAmount: number;
  pendingAmount: number;
}
