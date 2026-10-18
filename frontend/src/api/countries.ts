import { api } from '../lib/api';
import { buildQuery } from '../utils/query';

/**
 * The API binds `pageNumber` (1-based), while the UI speaks in `page`, so the
 * alias is mapped here — otherwise ASP.NET leaves the default of 1 and every
 * request silently returns the first page.
 */
export function getCountries(params?: any) {
  const { page, ...rest } = params || {};
  const query = buildQuery({ ...rest, pageNumber: page });
  return api.get(`/countries?${query}`).then((r) => r.data);
}
