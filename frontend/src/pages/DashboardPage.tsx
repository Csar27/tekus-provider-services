import { useQuery } from '@tanstack/react-query';
import { getDashboard } from '../api/dashboard';

export function DashboardPage() {
  const { data, isLoading } = useQuery({ queryKey: ['dashboard'], queryFn: getDashboard });

  if (isLoading) return <div>Loading...</div>;
  if (!data) return <div>No data</div>;

  return (
    <div>
      <h2>Dashboard</h2>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 16 }}>
        <div style={{ border: '1px solid #ddd', padding: 16, borderRadius: 8 }}>
          <h4>Providers</h4>
          <div>Total: {data.totals?.providers}</div>
          <div>Active: {data.totals?.activeProviders}</div>
        </div>
        <div style={{ border: '1px solid #ddd', padding: 16, borderRadius: 8 }}>
          <h4>Services</h4>
          <div>Total: {data.totals?.services}</div>
          <div>Active: {data.totals?.activeServices}</div>
        </div>
        <div style={{ border: '1px solid #ddd', padding: 16, borderRadius: 8 }}>
          <h4>Countries/Offerings</h4>
          <div>Countries: {data.totals?.countries}</div>
          <div>Offerings: {data.totals?.offerings}</div>
        </div>
      </div>

      <h3 style={{ marginTop: 24 }}>Top Providers by Services</h3>
      <ul>
        {data.topProvidersByServices?.map((p: any) => (
          <li key={p.providerId}>{p.name} ({p.nit}) - {p.serviceCount} services (${p.totalHourlyRateUsd})</li>
        ))}
      </ul>
    </div>
  );
}
