import { request } from '../../lib/apiClient';
import { ChurchBranding, LoginState } from '../../types/api';

export type LoginRequest = {
  churchSlug: string;
  email: string;
  password: string;
};

export type SetPasswordRequest = {
  token: string;
  password: string;
  confirmPassword: string;
};

type FlatBackendAuthResponse = {
  token: string;
  id?: string;
  userId?: string;
  email: string;
  displayName: string;
  role: LoginState['user']['role'];
  isActive: boolean;
  churchId?: string;
  churchSlug?: string;
  churchName?: string;
};

type BackendAuthResponse = LoginState | FlatBackendAuthResponse;

function isLoginState(response: BackendAuthResponse): response is LoginState {
  return 'user' in response && response.user !== undefined;
}

function normalizeAuthResponse(response: BackendAuthResponse): LoginState {
  if (isLoginState(response)) {
    return response;
  }

  return {
    token: response.token,
    user: {
      id: response.id ?? response.userId ?? '',
      email: response.email,
      displayName: response.displayName,
      role: response.role,
      isActive: response.isActive,
      churchId: response.churchId,
      churchSlug: response.churchSlug,
      churchName: response.churchName
    }
  };
}

export async function getChurchBranding(churchSlug: string): Promise<ChurchBranding> {
  return request<ChurchBranding>(
    `/api/churches/by-slug/${encodeURIComponent(churchSlug)}`
  );
}

export async function login(data: LoginRequest): Promise<LoginState> {
  const response = await request<BackendAuthResponse>('/api/auth/login', undefined, {
    method: 'POST',
    body: JSON.stringify(data)
  });

  return normalizeAuthResponse(response);
}

export async function setPassword(data: SetPasswordRequest): Promise<LoginState> {
  const response = await request<BackendAuthResponse>(
    '/api/auth/set-password',
    undefined,
    {
      method: 'POST',
      body: JSON.stringify(data)
    }
  );

  return normalizeAuthResponse(response);
}
