import { api } from '../lib/api';

export function getProviderServices(providerId: string) {
  return api.get(`/providers/${providerId}/services`).then((r) => r.data);
}

export function setProviderServices(providerId: string, serviceIds: string[]) {
  return api.put(`/providers/${providerId}/services`, { serviceIds }).then((r) => r.data);
}
