import { request } from '../../lib/apiClient';

export type ServiceType = 1 | 2 | 3 | 4 | 5;

export type AttendanceRecord = {
  id: string;
  serviceDate: string;
  serviceType: ServiceType;
  men: number;
  women: number;
  children: number;
  visitors: number;
  total: number;
  notes?: string | null;
};

export type CreateAttendanceRequest = {
  serviceDate: string;
  serviceType: ServiceType;
  men: number;
  women: number;
  children: number;
  visitors: number;
  notes?: string;
};

export type UpdateAttendanceRequest = CreateAttendanceRequest;

export function getAttendance(token: string): Promise<AttendanceRecord[]> {
  return request<AttendanceRecord[]>('/api/attendance', token);
}

export function createAttendance(
  token: string,
  data: CreateAttendanceRequest
): Promise<AttendanceRecord> {
  return request<AttendanceRecord>('/api/attendance', token, {
    method: 'POST',
    body: JSON.stringify(data)
  });
}

export function updateAttendance(
  token: string,
  id: string,
  data: UpdateAttendanceRequest
): Promise<void> {
  return request<void>(`/api/attendance/${id}`, token, {
    method: 'PUT',
    body: JSON.stringify(data)
  });
}

export function deleteAttendance(token: string, id: string): Promise<void> {
  return request<void>(`/api/attendance/${id}`, token, {
    method: 'DELETE'
  });
}