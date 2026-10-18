import axios from 'axios';

export const api = axios.create({
  baseURL: '/api/v1',
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

/**
 * The access token lives 60 minutes. Without this interceptor an expired session
 * would leave every screen silently empty (all requests 401, nothing tells the
 * user why), so a 401 anywhere except the sign-in call clears the token and
 * sends the user back to the login page.
 */
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      const url = error.config?.url ?? '';
      const isSignIn = url.includes('/auth/login');

      if (!isSignIn) {
        localStorage.removeItem('accessToken');
        if (!window.location.pathname.startsWith('/login')) {
          window.location.assign('/login');
        }
      }
    }

    return Promise.reject(error);
  },
);

/**
 * Turns an API failure into a sentence the user can act on, preferring the
 * server's `detail`, then field-level validation messages, then `title`.
 * Anything unrecognised falls back to the supplied message so a rejection is
 * never shown as a blank space in the UI.
 */
export function readApiError(error: unknown, fallback: string): string {
  if (!axios.isAxiosError(error)) return fallback;

  const data = error.response?.data as
    | { detail?: string; title?: string; errors?: Record<string, string[]>; message?: string }
    | undefined;

  if (data?.detail) return data.detail;
  if (data?.message) return data.message;

  if (data?.errors) {
    const first = Object.values(data.errors).flat()[0];
    if (first) return first;
  }

  if (error.response?.status) {
    if (error.response.status === 401) return 'Your session has expired. Sign in again.';
    if (error.response.status === 403) return 'You do not have permission to make this change.';
    if (error.response.status === 404) return 'That record no longer exists. Reload the list.';
  }

  if (data?.title) return data.title;
  if (!error.response) return 'The API did not respond. Check that it is running.';
  return fallback;
}
