import { api } from '../lib/api';
import { buildQuery } from '../utils/query';

export function getCountries(params?: any) {
  return api.get(`/countries?${buildQuery(params || {})}`).then((r) => r.data);
}
