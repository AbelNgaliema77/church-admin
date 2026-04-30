import { request } from '../../lib/apiClient';

export type AuditAction =
  | 1
  | 2
  | 3
  | 10
  | 11
  | 20
  | 21
  | 30
  | 31
  | 32
  | 33
  | 40
  | 41
  | 50
  | 51;

export type AuditLog = {
  id: string;
  entityName: string;
  entityId: string;
  action: AuditAction;
  beforeJson?: string | null;
  afterJson?: string | null;
  reason?: string | null;
  createdAt: string;
  createdBy: string;
};

export type AuditLogFilters = {
  entityName?: string;
  entityId?: string;
};

function buildAuditLogQuery(filters?: AuditLogFilters): string {
  const params = new URLSearchParams();

  if (filters?.entityName?.trim()) {
    params.set('entityName', filters.entityName.trim());
  }

  if (filters?.entityId?.trim()) {
    params.set('entityId', filters.entityId.trim());
  }

  const query = params.toString();

  return query ? `?${query}` : '';
}

export function getAuditLogs(
  token: string,
  filters?: AuditLogFilters
): Promise<AuditLog[]> {
  return request<AuditLog[]>(`/api/audit-logs${buildAuditLogQuery(filters)}`, token);
}

export function getAuditLogById(token: string, id: string): Promise<AuditLog> {
  return request<AuditLog>(`/api/audit-logs/${id}`, token);
}