import { api } from '../lib/api';

/**
 * The provider portfolio lives on the provider detail (`services`); there is no
 * `GET /providers/{id}/services` collection route — that path only accepts PUT.
 * Returning plain ids keeps the caller's selection state trivial to compare.
 */
export async function getProviderServices(providerId: string): Promise<string[]> {
  const { data } = await api.get(`/providers/${providerId}`);
  const services = data.services ?? [];
  return services.map((s: { id: string }) => s.id);
}

/** Replaces the whole set of services offered by the provider. */
export function setProviderServices(providerId: string, serviceIds: string[]) {
  return api.put(`/providers/${providerId}/services`, { serviceIds }).then((r) => r.data);
}
