import { request } from '../../lib/apiClient';
import { ServiceType } from '../attendance/attendanceApi';

export type ReportFilters = {
  from: string;
  to: string;
  serviceType?: ServiceType | '';
};

export type AttendanceTrend = {
  year: number;
  month: number;
  monthName: string;
  men: number;
  women: number;
  children: number;
  visitors: number;
  total: number;
};

export type AttendanceServiceTypeSummary = {
  serviceType: ServiceType;
  servicesCount: number;
  totalAttendance: number;
  averageAttendance: number;
};

export type FinanceTrend = {
  year: number;
  month: number;
  monthName: string;
  totalAmount: number;
};

export type FinanceCategorySummary = {
  category: number;
  totalAmount: number;
};

function buildReportQuery(filters: ReportFilters): string {
  const params = new URLSearchParams();

  params.set('from', filters.from);
  params.set('to', filters.to);

  if (filters.serviceType) {
    params.set('serviceType', String(filters.serviceType));
  }

  return `?${params.toString()}`;
}

export function getAttendanceTrends(
  token: string,
  filters: ReportFilters
): Promise<AttendanceTrend[]> {
  return request<AttendanceTrend[]>(
    `/api/reports/attendance-trends${buildReportQuery(filters)}`,
    token
  );
}

export function getAttendanceByServiceType(
  token: string,
  filters: ReportFilters
): Promise<AttendanceServiceTypeSummary[]> {
  return request<AttendanceServiceTypeSummary[]>(
    `/api/reports/attendance-by-service-type${buildReportQuery(filters)}`,
    token
  );
}

export function getFinanceTrends(
  token: string,
  filters: ReportFilters
): Promise<FinanceTrend[]> {
  return request<FinanceTrend[]>(
    `/api/reports/finance-trends${buildReportQuery(filters)}`,
    token
  );
}

export function getFinanceByCategory(
  token: string,
  filters: ReportFilters
): Promise<FinanceCategorySummary[]> {
  return request<FinanceCategorySummary[]>(
    `/api/reports/finance-by-category${buildReportQuery(filters)}`,
    token
  );
}