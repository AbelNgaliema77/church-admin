export type ApiErrorResponse = {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
};

export class ApiError extends Error {
  status: number;
  details?: string[];

  constructor(message: string, status: number, details?: string[]) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.details = details;
  }
}

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '';
const authExpiredEventName = 'church-admin-auth-expired';

export async function request<T>(
  path: string,
  token?: string,
  init?: RequestInit
): Promise<T> {
  const headers = new Headers(init?.headers);

  if (!headers.has('Content-Type') && !(init?.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json');
  }

  if (token) {
    headers.set('Authorization', `Bearer ${token}`);
  }

  let response: Response;

  try {
    response = await fetch(`${apiBaseUrl}${path}`, {
      ...init,
      headers
    });
  } catch {
    throw new ApiError(
      'Unable to connect to the API. Check that the backend is running.',
      0
    );
  }

  if (!response.ok) {
    const apiError = await buildApiError(response);

    if (response.status === 401) {
      localStorage.removeItem('church-admin-auth');
      window.dispatchEvent(new Event(authExpiredEventName));
    }

    throw apiError;
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

async function buildApiError(response: Response): Promise<ApiError> {
  const text = await response.text();

  if (response.status === 401) {
    return new ApiError('Your session has expired. Please log in again.', 401);
  }

  if (response.status === 403) {
    return new ApiError('You do not have permission to perform this action.', 403);
  }

  if (response.status >= 500) {
    return new ApiError(
      'The server failed while processing this request. Check the backend logs.',
      response.status
    );
  }

  if (!text) {
    return new ApiError(`Request failed with status ${response.status}`, response.status);
  }

  try {
    const error = JSON.parse(text) as ApiErrorResponse;

    if (error.errors) {
      const details = Object.values(error.errors).flat();

      return new ApiError(
        details.length > 0 ? details.join('\n') : 'Validation failed.',
        response.status,
        details
      );
    }

    return new ApiError(
      error.detail ?? error.title ?? text,
      response.status
    );
  } catch {
    return new ApiError(text, response.status);
  }
}