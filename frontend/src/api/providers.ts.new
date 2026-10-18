import { api } from '../lib/api';
import { buildQuery } from '../utils/query';

export interface ProviderFilters {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  search?: string;
}

export function getProviders(params: ProviderFilters) {
  return api.get(`/providers?${buildQuery(params)}`).then((r) => r.data);
}

export function createProvider(data: any) {
  return api.post('/providers', data).then((r) => r.data);
}

export function updateProvider(id: string, data: any) {
  return api.put(`/providers/${id}`, data).then((r) => r.data);
}

export function activateProvider(id: string) {
  return api.patch(`/providers/${id}/status`, { isActive: true }).then((r) => r.data);
}

export function deactivateProvider(id: string) {
  return api.patch(`/providers/${id}/status`, { isActive: false }).then((r) => r.data);
}
