import { request } from '../../lib/apiClient';

export type UserRole = 0 | 1 | 2 | 3 | 4 | 5 | 6;

export type UserAccount = {
  id: string;
  email: string;
  displayName: string;
  externalProvider?: string | null;
  role: UserRole;
  roleName: string;
  isActive: boolean;
  inviteLink?: string | null;
};

export type CreateUserRequest = {
  email: string;
  displayName: string;
  role: UserRole;
  isActive: boolean;
};

export type UpdateUserRequest = {
  displayName: string;
  role: UserRole;
  isActive: boolean;
};

export type UpdateUserRoleRequest = {
  role: UserRole;
};

export function getUsers(token: string): Promise<UserAccount[]> {
  return request<UserAccount[]>('/api/users', token);
}

export function createUser(
  token: string,
  data: CreateUserRequest
): Promise<UserAccount> {
  return request<UserAccount>('/api/users', token, {
    method: 'POST',
    body: JSON.stringify(data)
  });
}

export function updateUser(
  token: string,
  id: string,
  data: UpdateUserRequest
): Promise<void> {
  return request<void>(`/api/users/${id}`, token, {
    method: 'PUT',
    body: JSON.stringify(data)
  });
}

export function updateUserRole(
  token: string,
  id: string,
  data: UpdateUserRoleRequest
): Promise<void> {
  return request<void>(`/api/users/${id}/role`, token, {
    method: 'PATCH',
    body: JSON.stringify(data)
  });
}

export function activateUser(token: string, id: string): Promise<void> {
  return request<void>(`/api/users/${id}/activate`, token, {
    method: 'PATCH'
  });
}

export function deactivateUser(token: string, id: string): Promise<void> {
  return request<void>(`/api/users/${id}/deactivate`, token, {
    method: 'PATCH'
  });
}