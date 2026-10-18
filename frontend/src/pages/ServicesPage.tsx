import { useQuery } from '@tanstack/react-query';
import { api } from '../lib/api';

export function ServicesPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['services'],
    queryFn: () => api.get('/services?page=1&pageSize=10').then((r) => r.data),
  });

  if (isLoading) return <div>Loading...</div>;
  if (!data) return <div>No data</div>;

  return (
    <div>
      <h2>Services</h2>
      <ul>
        {data.items.map((s: any) => (
          <li key={s.id}>{s.name} - ${s.hourlyRateUsd}/hr</li>
        ))}
      </ul>
    </div>
  );
}
