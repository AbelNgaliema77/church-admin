import { request } from '../../lib/apiClient';

export type ServiceType = 1 | 2 | 3 | 4 | 5;
export type FinanceCategory = 1 | 2 | 3 | 4 | 5 | 6;
export type PaymentMethod = 1 | 2 | 3 | 4;

export type FinanceEntry = {
  id: string;
  serviceDate: string;
  serviceType: ServiceType;
  category: FinanceCategory;
  amount: number;
  paymentMethod: PaymentMethod;
  isVerified: boolean;
  verifiedAt?: string | null;
  verifiedBy?: string | null;
  correctionOfFinanceEntryId?: string | null;
  notes?: string | null;
};

export type CreateFinanceEntryRequest = {
  serviceDate: string;
  serviceType: ServiceType;
  category: FinanceCategory;
  amount: number;
  paymentMethod: PaymentMethod;
  notes?: string;
};

export type UpdateFinanceEntryRequest = CreateFinanceEntryRequest;

export type CreateFinanceCorrectionRequest = {
  category: FinanceCategory;
  amount: number;
  paymentMethod: PaymentMethod;
  reason: string;
};

export function getFinanceEntries(token: string): Promise<FinanceEntry[]> {
  return request<FinanceEntry[]>('/api/finance', token);
}

export function createFinanceEntry(
  token: string,
  data: CreateFinanceEntryRequest
): Promise<FinanceEntry> {
  return request<FinanceEntry>('/api/finance', token, {
    method: 'POST',
    body: JSON.stringify(data)
  });
}

export function updateFinanceEntry(
  token: string,
  id: string,
  data: UpdateFinanceEntryRequest
): Promise<void> {
  return request<void>(`/api/finance/${id}`, token, {
    method: 'PUT',
    body: JSON.stringify(data)
  });
}

export function verifyFinanceEntry(token: string, id: string): Promise<void> {
  return request<void>(`/api/finance/${id}/verify`, token, {
    method: 'PATCH'
  });
}

export function createFinanceCorrection(
  token: string,
  id: string,
  data: CreateFinanceCorrectionRequest
): Promise<FinanceEntry> {
  return request<FinanceEntry>(`/api/finance/${id}/corrections`, token, {
    method: 'POST',
    body: JSON.stringify(data)
  });
}

export function deleteFinanceEntry(token: string, id: string): Promise<void> {
  return request<void>(`/api/finance/${id}`, token, {
    method: 'DELETE'
  });
}