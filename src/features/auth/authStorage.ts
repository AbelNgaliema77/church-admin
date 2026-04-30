import { LoginState } from '../../types/api';

const storageKey = 'church-admin-auth';

export function storeAuth(auth: LoginState) {
  localStorage.setItem(storageKey, JSON.stringify(auth));
}

export function getStoredAuth(): LoginState | null {
  const raw = localStorage.getItem(storageKey);

  if (!raw) {
    return null;
  }

  try {
    const parsed = JSON.parse(raw) as LoginState;

    if (!parsed.token || !parsed.user || !parsed.user.displayName) {
      clearStoredAuth();
      return null;
    }

    return parsed;
  } catch {
    clearStoredAuth();
    return null;
  }
}

export function clearStoredAuth() {
  localStorage.removeItem(storageKey);
}