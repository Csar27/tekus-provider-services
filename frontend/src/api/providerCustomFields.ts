import { api } from '../lib/api';

export function getProviderCustomFields(providerId: string) {
  return api.get(`/providers/${providerId}/custom-fields`).then((r) => r.data);
}

export function addProviderCustomField(providerId: string, data: any) {
  return api.post(`/providers/${providerId}/custom-fields`, data).then((r) => r.data);
}

export function updateProviderCustomField(providerId: string, fieldId: string, data: any) {
  return api.put(`/providers/${providerId}/custom-fields/${fieldId}`, data).then((r) => r.data);
}

export function deleteProviderCustomField(providerId: string, fieldId: string) {
  return api.delete(`/providers/${providerId}/custom-fields/${fieldId}`).then((r) => r.data);
}
