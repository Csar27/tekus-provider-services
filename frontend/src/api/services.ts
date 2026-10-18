import { api } from '../lib/api';
import { buildQuery } from '../utils/query';

export interface ServiceFilters {
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
export function getServices(params: ServiceFilters) {
  const { page, ...rest } = params;
  const query = buildQuery({ ...rest, pageNumber: page });
  return api.get(`/services?${query}`).then((r) => r.data);
}

export function createService(data: any) {
  return api.post('/services', data).then((r) => r.data);
}

/**
 * Edits a service. When `version` is given the optimistic-concurrency header
 * is sent, so a stale record fails with 409 instead of overwriting a change.
 */
export function updateService(id: string, data: any, version?: number) {
  return api
    .put(`/services/${id}`, data, {
      headers: version !== undefined ? { 'If-Match': String(version) } : undefined,
    })
    .then((r) => r.data);
}

export function activateService(id: string) {
  return api.patch(`/services/${id}/status`, { isActive: true }).then((r) => r.data);
}

export function deactivateService(id: string) {
  return api.patch(`/services/${id}/status`, { isActive: false }).then((r) => r.data);
}

/**
 * The API exposes a `PUT` for service countries but no `GET` collection, so the
 * assigned list is read from the service detail (same pattern as the provider
 * portfolio). The detail reports `code`; the modal keys on `countryCode`.
 */
export function getServiceCountries(id: string) {
  return api.get(`/services/${id}`).then((r) =>
    ((r.data?.countries ?? []) as any[]).map((c) => ({
      countryCode: c.code,
      name: c.name,
    })),
  );
}

export function setServiceCountries(id: string, countryCodes: string[]) {
  return api.put(`/services/${id}/countries`, { countryCodes }).then((r) => r.data);
}
