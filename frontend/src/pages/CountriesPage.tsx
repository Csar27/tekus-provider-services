import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { getCountries } from '../api/countries';
import { TableControls } from '../components/TableControls';
import { Pagination } from '../components/Pagination';
import { EmptyState, PageHead, Panel, QueryError, TableSkeleton } from '../components/ui';
import { readApiError } from '../lib/api';

export function CountriesPage() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');
  const [sortBy, setSortBy] = useState('code');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['countries', page, pageSize, search, sortBy, sortDirection],
    queryFn: () => getCountries({ page, pageSize, search, sortBy, sortDirection }),
  });

  const onSearch = (value: string) => {
    setSearch(value);
    setPage(1);
  };

  return (
    <>
      <PageHead
        title="Country coverage"
        subtitle="ISO catalogue backing service availability. Assignment happens from each service record."
      />

      <TableControls
        search={search}
        sortBy={sortBy}
        sortDirection={sortDirection}
        onSearchChange={onSearch}
        onSortByChange={(v) => {
          setSortBy(v);
          setPage(1);
        }}
        onSortDirectionChange={(v) => {
          setSortDirection(v);
          setPage(1);
        }}
        sortOptions={[
          { value: 'code', label: 'Code' },
          { value: 'name', label: 'Name' },
          { value: 'region', label: 'Region' },
        ]}
        searchPlaceholder="Search by code, name or region…"
      />

      <Panel meta={data ? `${data.totalCount} countries` : undefined}>
        {isLoading && <TableSkeleton rows={8} cols={3} />}

        {isError && (
          <QueryError
            message={readApiError(error, 'The country catalogue could not be loaded.')}
            onRetry={refetch}
          />
        )}

        {!isLoading && !isError && (!data?.items || data.items.length === 0) && (
          <EmptyState
            title={search ? 'No country matches your search' : 'The country catalogue is empty'}
            hint={
              search
                ? 'Try a different code, name or region.'
                : 'The catalogue is refreshed from the external country service on first load.'
            }
          />
        )}

        {!isLoading && data?.items?.length > 0 && (
          <table className="tbl">
            <thead>
              <tr>
                <th style={{ width: '14%' }}>Code</th>
                <th>Country</th>
                <th>Region</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((c: any) => (
                <tr key={c.code}>
                  <td>
                    <span
                      style={{
                        display: 'inline-block',
                        padding: '0.1rem 0.45rem',
                        border: '1px solid var(--color-line-strong)',
                        borderRadius: '3px',
                        fontSize: '12px',
                        fontWeight: 600,
                        letterSpacing: '0.04em',
                        color: 'var(--color-navy)',
                        fontVariantNumeric: 'tabular-nums',
                      }}
                    >
                      {c.code}
                    </span>
                  </td>
                  <td className="cell-title">{c.name}</td>
                  <td style={{ color: 'var(--color-ink-soft)' }}>{c.region || '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>

      <Pagination
        page={page}
        pageSize={pageSize}
        totalCount={data?.totalCount ?? 0}
        totalPages={data?.totalPages ?? 0}
        onPageChange={setPage}
        onPageSizeChange={(s) => {
          setPageSize(s);
          setPage(1);
        }}
      />
    </>
  );
}
