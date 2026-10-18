import { useQuery } from '@tanstack/react-query';
import { api } from '../lib/api';

export function ProvidersPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['providers'],
    queryFn: () => api.get('/providers?page=1&pageSize=10').then((r) => r.data),
  });

  if (isLoading) return <div>Loading...</div>;
  if (!data) return <div>No data</div>;

  return (
    <div>
      <h2>Providers</h2>
      <ul>
        {data.items.map((p: any) => (
          <li key={p.id}>{p.name} ({p.nit}) - {p.serviceCount} services</li>
        ))}
      </ul>
    </div>
  );
}
