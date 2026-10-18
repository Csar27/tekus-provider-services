import { api } from '../lib/api';
import { buildQuery } from '../utils/query';

export interface ProviderFilters {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  search?: string;
}

/**
 * The API binds `pageNumber` (1-based), while the UI speaks in `page`, so the
 * alias is mapped here — otherwise ASP.NET leaves the default of 1 and every
 * request silently returns the first page.
 */
export function getProviders(params: ProviderFilters) {
  const { page, ...rest } = params;
  const query = buildQuery({ ...rest, pageNumber: page });
  return api.get(`/providers?${query}`).then((r) => r.data);
}

export function createProvider(data: any) {
  return api.post('/providers', data).then((r) => r.data);
}

/**
 * Edits a provider. When `version` is given the optimistic-concurrency header
 * is sent, so a stale record fails with 409 instead of overwriting a change.
 */
export function updateProvider(id: string, data: any, version?: number) {
  return api
    .put(`/providers/${id}`, data, {
      headers: version !== undefined ? { 'If-Match': String(version) } : undefined,
    })
    .then((r) => r.data);
}

export function activateProvider(id: string) {
  return api.patch(`/providers/${id}/status`, { isActive: true }).then((r) => r.data);
}

export function deactivateProvider(id: string) {
  return api.patch(`/providers/${id}/status`, { isActive: false }).then((r) => r.data);
}
