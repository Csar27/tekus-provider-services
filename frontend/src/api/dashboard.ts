import { api } from '../lib/api';

export function getDashboard() {
  return api.get('/dashboard/summary').then((r) => r.data);
}
