import { request } from '../../lib/apiClient';

export type InventoryCondition = 1 | 2 | 3 | 4 | 5;
export type InventoryStatus = 1 | 2 | 3;

export type InventoryItem = {
  id: string;
  name: string;
  teamId: string;
  teamName: string;
  description: string;
  quantity: number;
  condition: InventoryCondition;
  status: InventoryStatus;
  imageUrl?: string | null;
};

export type InventoryFilters = {
  teamId?: string;
  status?: InventoryStatus | '';
  condition?: InventoryCondition | '';
};

export type CreateInventoryItemRequest = {
  name: string;
  teamId: string;
  description: string;
  quantity: number;
  condition: InventoryCondition;
  status: InventoryStatus;
  imageUrl?: string | null;
};

export type UpdateInventoryItemRequest = CreateInventoryItemRequest;

function buildInventoryQuery(filters?: InventoryFilters): string {
  const params = new URLSearchParams();

  if (filters?.teamId) {
    params.set('teamId', filters.teamId);
  }

  if (filters?.status) {
    params.set('status', String(filters.status));
  }

  if (filters?.condition) {
    params.set('condition', String(filters.condition));
  }

  const query = params.toString();

  return query ? `?${query}` : '';
}

export function getInventory(
  token: string,
  filters?: InventoryFilters
): Promise<InventoryItem[]> {
  return request<InventoryItem[]>(`/api/inventory${buildInventoryQuery(filters)}`, token);
}

export function createInventoryItem(
  token: string,
  data: CreateInventoryItemRequest
): Promise<InventoryItem> {
  return request<InventoryItem>('/api/inventory', token, {
    method: 'POST',
    body: JSON.stringify(data)
  });
}

export function updateInventoryItem(
  token: string,
  id: string,
  data: UpdateInventoryItemRequest
): Promise<void> {
  return request<void>(`/api/inventory/${id}`, token, {
    method: 'PUT',
    body: JSON.stringify(data)
  });
}

export function approveInventoryItem(token: string, id: string): Promise<void> {
  return request<void>(`/api/inventory/${id}/approve`, token, {
    method: 'PATCH'
  });
}

export function retireInventoryItem(token: string, id: string): Promise<void> {
  return request<void>(`/api/inventory/${id}/retire`, token, {
    method: 'PATCH'
  });
}

export function markInventoryItemLost(token: string, id: string): Promise<void> {
  return request<void>(`/api/inventory/${id}/mark-lost`, token, {
    method: 'PATCH'
  });
}

export function deleteInventoryItem(token: string, id: string): Promise<void> {
  return request<void>(`/api/inventory/${id}`, token, {
    method: 'DELETE'
  });
}