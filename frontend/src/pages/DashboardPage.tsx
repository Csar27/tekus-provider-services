import { useQuery } from '@tanstack/react-query';
import { getDashboard } from '../api/dashboard';

export function DashboardPage() {
  const { data, isLoading } = useQuery({ queryKey: ['dashboard'], queryFn: getDashboard });

  if (isLoading) return <div>Loading...</div>;
  if (!data) return <div>No data</div>;

  return (
    <div>
      <div className="page-header">
        <h2 className="page-title">Dashboard</h2>
      </div>
      <div className="dashboard-grid">
        <div className="kpi-card">
          <div className="kpi-title">Providers</div>
          <div className="kpi-value">{data.totals?.providers}</div>
          <div style={{ color: 'var(--text-muted)', fontSize: '0.75rem' }}>Active: {data.totals?.activeProviders}</div>
        </div>
        <div className="kpi-card">
          <div className="kpi-title">Services</div>
          <div className="kpi-value">{data.totals?.services}</div>
          <div style={{ color: 'var(--text-muted)', fontSize: '0.75rem' }}>Active: {data.totals?.activeServices}</div>
        </div>
        <div className="kpi-card">
          <div className="kpi-title">Countries</div>
          <div className="kpi-value">{data.totals?.countries}</div>
        </div>
        <div className="kpi-card">
          <div className="kpi-title">Offerings</div>
          <div className="kpi-value">{data.totals?.offerings}</div>
        </div>
      </div>

      <h3 className="section-title">Top Providers by Services</h3>
      <ul>
        {data.topProvidersByServices?.map((p: any) => (
          <li key={p.providerId}>{p.name} ({p.nit}) - {p.serviceCount} services (${p.totalHourlyRateUsd})</li>
        ))}
      </ul>
    </div>
  );
}
