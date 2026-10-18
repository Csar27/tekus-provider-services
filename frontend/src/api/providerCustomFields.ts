import { api } from '../lib/api';

/**
 * Provider custom fields are stored as key/value pairs and addressed by their
 * key (the API has no field id and no collection endpoint for them):
 *
 *   - read   GET  /providers/{id}            -> detail.customFields
 *   - upsert PUT  /providers/{id}/custom-fields/{key}  body { value }
 *   - remove DELETE /providers/{id}/custom-fields/{key}
 */

export interface ProviderCustomField {
  key: string;
  value: string;
}

/** Reads the custom fields from the provider detail (there is no list endpoint). */
export async function getProviderCustomFields(providerId: string): Promise<ProviderCustomField[]> {
  const { data } = await api.get(`/providers/${providerId}`);
  const fields = data.customFields ?? [];
  return fields.map((f: any) => ({ key: f.key, value: f.value }));
}

/** Creates or overwrites a field, matched by key. */
export function upsertProviderCustomField(providerId: string, key: string, value: string) {
  return api
    .put(`/providers/${providerId}/custom-fields/${encodeURIComponent(key)}`, { value })
    .then((r) => r.data);
}

/** Removes a single field by key. */
export function deleteProviderCustomField(providerId: string, key: string) {
  return api
    .delete(`/providers/${providerId}/custom-fields/${encodeURIComponent(key)}`)
    .then((r) => r.data);
}
