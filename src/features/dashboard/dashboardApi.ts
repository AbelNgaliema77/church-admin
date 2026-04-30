import { request } from '../../lib/apiClient';

export type DashboardSummary = {
  activeWorkers: number;
  activeTeams: number;
  inventoryItems: number;
  pendingInventoryItems: number;
  attendanceThisMonth: number;
  financeThisMonth: number;
};

export function getDashboardSummary(token: string) {
  return request<DashboardSummary>('/api/dashboard/summary', token);
}