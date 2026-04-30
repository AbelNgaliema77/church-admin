import { request } from '../../lib/apiClient';

export type WorkerTeamInfo = {
  teamId: string;
  teamName: string;
  roleInTeam: number;
  startDate: string;
};

export type Worker = {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  address: string;
  dateOfBirth: string;
  startedServing: string;
  baptized: boolean;
  status: 'Active' | 'Inactive';
  teams: WorkerTeamInfo[];
};

export type CreateWorkerRequest = {
  fullName: string;
  email: string;
  phone: string;
  address: string;
  dateOfBirth: string;
  startedServing: string;
  baptized: boolean;
  teamId: string;
  roleInTeam: number;
};

export type UpdateWorkerRequest = {
  fullName: string;
  email: string;
  phone: string;
  address: string;
  dateOfBirth: string;
  startedServing: string;
  baptized: boolean;
  status: 'Active' | 'Inactive';
};

export function getWorkers(token: string): Promise<Worker[]> {
  return request('/api/workers', token);
}

export function createWorker(token: string, data: CreateWorkerRequest): Promise<Worker> {
  return request('/api/workers', token, {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export function updateWorker(token: string, id: string, data: UpdateWorkerRequest): Promise<void> {
  return request(`/api/workers/${id}`, token, {
    method: 'PUT',
    body: JSON.stringify(data),
  });
}

export function deleteWorker(token: string, id: string): Promise<void> {
  return request(`/api/workers/${id}`, token, { method: 'DELETE' });
}

export function deactivateWorker(token: string, id: string): Promise<void> {
  return request(`/api/workers/${id}/deactivate`, token, { method: 'PATCH' });
}
