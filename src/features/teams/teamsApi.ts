import { request } from '../../lib/apiClient';
import { Team } from '../../types/domain';

export type CreateTeamRequest = {
  name: string;
  description?: string;
};

export type UpdateTeamRequest = {
  name: string;
  description?: string;
  isActive: boolean;
};

export function getTeams(token: string): Promise<Team[]> {
  return request<Team[]>('/api/teams', token);
}

export function createTeam(
  token: string,
  data: CreateTeamRequest
): Promise<Team> {
  return request<Team>('/api/teams', token, {
    method: 'POST',
    body: JSON.stringify(data)
  });
}

export function updateTeam(
  token: string,
  id: string,
  data: UpdateTeamRequest
): Promise<void> {
  return request<void>(`/api/teams/${id}`, token, {
    method: 'PUT',
    body: JSON.stringify(data)
  });
}

export function deactivateTeam(token: string, id: string): Promise<void> {
  return request<void>(`/api/teams/${id}/deactivate`, token, {
    method: 'PATCH'
  });
}

export function deleteTeam(token: string, id: string): Promise<void> {
  return request<void>(`/api/teams/${id}`, token, {
    method: 'DELETE'
  });
}