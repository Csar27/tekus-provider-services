import { api } from '../lib/api';
import { buildQuery } from '../utils/query';

export interface ServiceFilters {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  search?: string;
}

export function getServices(params: ServiceFilters) {
  return api.get(`/services?${buildQuery(params)}`).then((r) => r.data);
}

export function createService(data: any) {
  return api.post('/services', data).then((r) => r.data);
}

export function updateService(id: string, data: any) {
  return api.put(`/services/${id}`, data).then((r) => r.data);
}

export function activateService(id: string) {
  return api.post(`/services/${id}/activate`).then((r) => r.data);
}

export function deactivateService(id: string) {
  return api.post(`/services/${id}/deactivate`).then((r) => r.data);
}

export function getServiceCountries(id: string) {
  return api.get(`/services/${id}/countries`).then((r) => r.data);
}

export function setServiceCountries(id: string, countryCodes: string[]) {
  return api.put(`/services/${id}/countries`, { countryCodes }).then((r) => r.data);
}
