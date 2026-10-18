import { useQuery } from '@tanstack/react-query';
import { getDashboard } from '../api/dashboard';

export function DashboardPage() {
  const { data, isLoading } = useQuery({ queryKey: ['dashboard'], queryFn: getDashboard });

  if (isLoading) return <div>Loading...</div>;
  if (!data) return <div>No data</div>;

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-3xl font-bold text-slate-200">Dashboard</h2>
      </div>
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
        <div className="bg-slate-900 border border-slate-700 rounded-lg p-5">
          <div className="text-xs font-semibold uppercase tracking-wide text-slate-400 mb-2">Providers</div>
          <div className="text-3xl font-bold text-slate-200">{data.totals?.providers}</div>
          <div className="text-xs text-slate-400 mt-1">Active: {data.totals?.activeProviders}</div>
        </div>
        <div className="bg-slate-900 border border-slate-700 rounded-lg p-5">
          <div className="text-xs font-semibold uppercase tracking-wide text-slate-400 mb-2">Services</div>
          <div className="text-3xl font-bold text-slate-200">{data.totals?.services}</div>
          <div className="text-xs text-slate-400 mt-1">Active: {data.totals?.activeServices}</div>
        </div>
        <div className="bg-slate-900 border border-slate-700 rounded-lg p-5">
          <div className="text-xs font-semibold uppercase tracking-wide text-slate-400 mb-2">Countries</div>
          <div className="text-3xl font-bold text-slate-200">{data.totals?.countries}</div>
        </div>
        <div className="bg-slate-900 border border-slate-700 rounded-lg p-5">
          <div className="text-xs font-semibold uppercase tracking-wide text-slate-400 mb-2">Offerings</div>
          <div className="text-3xl font-bold text-slate-200">{data.totals?.offerings}</div>
        </div>
      </div>

      <h3 className="text-xl font-semibold text-slate-200 mb-4 mt-8">Top Providers by Services</h3>
      <div className="bg-slate-900 border border-slate-700 rounded-lg overflow-hidden">
        <table className="min-w-full divide-y divide-slate-700">
          <thead className="bg-slate-950">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-slate-400 uppercase tracking-wider">Provider</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-slate-400 uppercase tracking-wider">NIT</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-slate-400 uppercase tracking-wider">Services</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-slate-400 uppercase tracking-wider">Total Rate</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-700">
            {data.topProvidersByServices?.map((p: any) => (
              <tr key={p.providerId} className="hover:bg-slate-800">
                <td className="px-6 py-4 text-sm text-slate-200">{p.name}</td>
                <td className="px-6 py-4 text-sm text-slate-400">{p.nit}</td>
                <td className="px-6 py-4 text-sm text-slate-200">{p.serviceCount}</td>
                <td className="px-6 py-4 text-sm text-slate-200">${p.totalHourlyRateUsd}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <ul>
        {data.topProvidersByServices?.map((p: any) => (
          <li key={p.providerId}>{p.name} ({p.nit}) - {p.serviceCount} services (${p.totalHourlyRateUsd})</li>
        ))}
      </ul>
    </div>
  );
}
