import { useQuery } from '@tanstack/react-query';
import { api } from '../lib/api';

export function CountriesPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['countries'],
    queryFn: () => api.get('/countries?page=1&pageSize=20').then((r) => r.data),
  });

  if (isLoading) return <div>Loading...</div>;
  if (!data) return <div>No data</div>;

  return (
    <div>
      <h2>Countries</h2>
      <ul>
        {data.items.map((c: any) => (
          <li key={c.code}>{c.code} - {c.name} ({c.region})</li>
        ))}
      </ul>
    </div>
  );
}
