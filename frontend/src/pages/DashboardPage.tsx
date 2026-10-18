import { useQuery } from '@tanstack/react-query';
import { getDashboard } from '../api/dashboard';
import { EmptyState, PageHead, Panel, TableSkeleton } from '../components/ui';
import { formatMoney, formatNumber } from '../lib/format';

export function DashboardPage() {
  const { data, isLoading } = useQuery({ queryKey: ['dashboard'], queryFn: getDashboard });

  const totals = data?.totals;
  const rows = data?.topProvidersByServices ?? [];
  const maxCount = rows.reduce((m: number, r: any) => Math.max(m, r.serviceCount ?? 0), 0) || 1;

  return (
    <>
      <PageHead
        title="Overview"
        subtitle="Consolidated register of suppliers, their service portfolio and the countries covered."
      />

      {isLoading && (
        <div className="rail" style={{ marginBottom: '1.75rem' }}>
          {[0, 1, 2, 3].map((i) => (
            <div className="rail__cell" key={i}>
              <div className="skeleton" style={{ height: 12, width: '55%' }} />
              <div className="skeleton" style={{ height: 32, width: '40%', marginTop: '0.7rem' }} />
            </div>
          ))}
        </div>
      )}

      {!isLoading && totals && (
        <div className="rail" style={{ marginBottom: '1.75rem' }}>
          <div className="rail__cell">
            <div className="rail__label">Providers</div>
            <div className="rail__figure">{formatNumber(totals.providers)}</div>
            <div className="rail__note">{formatNumber(totals.activeProviders)} active</div>
          </div>
          <div className="rail__cell">
            <div className="rail__label">Services</div>
            <div className="rail__figure">{formatNumber(totals.services)}</div>
            <div className="rail__note">{formatNumber(totals.activeServices)} active</div>
          </div>
          <div className="rail__cell">
            <div className="rail__label">Countries served</div>
            <div className="rail__figure">{formatNumber(totals.countries)}</div>
            <div className="rail__note">ISO catalogue</div>
          </div>
          <div className="rail__cell">
            <div className="rail__label">Offerings</div>
            <div className="rail__figure">{formatNumber(totals.offerings)}</div>
            <div className="rail__note">Service-to-country links</div>
          </div>
        </div>
      )}

      <Panel
        title="Providers by service portfolio"
        meta={rows.length ? `${rows.length} ranked` : undefined}
      >
        {isLoading && <TableSkeleton rows={5} cols={4} />}

        {!isLoading && rows.length === 0 && (
          <EmptyState
            title="No providers registered yet"
            hint="Create a provider from the Providers page to see it ranked here."
          />
        )}

        {!isLoading && rows.length > 0 && (
          <table className="tbl">
            <thead>
              <tr>
                <th style={{ width: '38%' }}>Provider</th>
                <th>NIT</th>
                <th className="num">Services</th>
                <th className="num">Portfolio rate</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((p: any) => (
                <tr key={p.providerId}>
                  <td>
                    <span className="cell-title">{p.name}</span>
                    <span className="bar" aria-hidden="true">
                      <span style={{ width: `${Math.round((p.serviceCount / maxCount) * 100)}%` }} />
                    </span>
                  </td>
                  <td style={{ color: 'var(--color-ink-soft)' }}>{p.nit}</td>
                  <td className="num">{formatNumber(p.serviceCount)}</td>
                  <td className="num">{formatMoney(p.totalHourlyRateUsd)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>
    </>
  );
}
