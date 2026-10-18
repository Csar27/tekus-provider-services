interface Props {
  search: string;
  sortBy: string;
  sortDirection: 'asc' | 'desc';
  onSearchChange: (s: string) => void;
  onSortByChange: (s: string) => void;
  onSortDirectionChange: (d: 'asc' | 'desc') => void;
  sortOptions: Array<{ value: string; label: string }>;
}

export function TableControls({ search, sortBy, sortDirection, onSearchChange, onSortByChange, onSortDirectionChange, sortOptions }: Props) {
  return (
    <div className="controls">
      <input value={search} onChange={(e) => onSearchChange(e.target.value)} placeholder="Search..." style={{ flex: 1 }} />
      <select value={sortBy} onChange={(e) => onSortByChange(e.target.value)}>
        {sortOptions.map((o) => (
          <option key={o.value} value={o.value}>{o.label}</option>
        ))}
      </select>
      <select value={sortDirection} onChange={(e) => onSortDirectionChange(e.target.value as 'asc' | 'desc')}>
        <option value="asc">Asc</option>
        <option value="desc">Desc</option>
      </select>
    </div>
  );
}
