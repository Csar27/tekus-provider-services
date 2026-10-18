import { useQuery } from '@tanstack/react-query';
import { getCountries } from '../api/countries';
import { TableControls } from '../components/TableControls';
import { Pagination } from '../components/Pagination';
import { useState } from 'react';

export function CountriesPage() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');
  const [sortBy, setSortBy] = useState('code');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');

  const { data, isLoading } = useQuery({
    queryKey: ['countries', page, pageSize, search, sortBy, sortDirection],
    queryFn: () => getCountries({ page, pageSize, search, sortBy, sortDirection }),
  });

  if (isLoading) return <div>Loading...</div>;
  if (!data) return <div>No data</div>;

  return (
    <div>
      <h2>Countries</h2>
      <TableControls
        search={search}
        sortBy={sortBy}
        sortDirection={sortDirection}
        onSearchChange={setSearch}
        onSortByChange={setSortBy}
        onSortDirectionChange={setSortDirection}
        sortOptions={[{ value: 'code', label: 'Code' }, { value: 'name', label: 'Name' }, { value: 'region', label: 'Region' }]}
      />
      <ul>
        {data.items?.map((c: any) => (
          <li key={c.code}>{c.code} - {c.name} ({c.region})</li>
        ))}
      </ul>
      <Pagination page={page} pageSize={pageSize} totalCount={data.totalCount} totalPages={data.totalPages} onPageChange={setPage} onPageSizeChange={setPageSize} />
    </div>
  );
}
