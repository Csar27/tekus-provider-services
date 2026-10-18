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

/**
 * The API caps `pageSize` at 100 (larger values return 400), while the external
 * catalogue holds ~250 countries. Pickers that need the whole list must page
 * through it instead of asking for an impossible single page.
 */
export async function getAllCountries(params: any = {}) {
  const pageSize = 100;
  const first = await getCountries({ ...params, pageSize, page: 1 });
  const items: any[] = [...(first?.items ?? [])];
  const totalCount: number = first?.totalCount ?? items.length;

  for (let pageNumber = 2; items.length < totalCount; pageNumber++) {
    const next = await getCountries({ ...params, pageSize, page: pageNumber });
    const chunk: any[] = next?.items ?? [];
    if (chunk.length === 0) break; // guard against a shifting total
    items.push(...chunk);
  }

  return items;
}
