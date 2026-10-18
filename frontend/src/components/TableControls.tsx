interface Props {
  search: string;
  sortBy: string;
  sortDirection: 'asc' | 'desc';
  onSearchChange: (s: string) => void;
  onSortByChange: (s: string) => void;
  onSortDirectionChange: (d: 'asc' | 'desc') => void;
  sortOptions: Array<{ value: string; label: string }>;
  searchPlaceholder?: string;
}

export function TableControls({
  search,
  sortBy,
  sortDirection,
  onSearchChange,
  onSortByChange,
  onSortDirectionChange,
  sortOptions,
  searchPlaceholder = 'Search…',
}: Props) {
  return (
    <div className="toolbar">
      <input
        className="input"
        type="search"
        value={search}
        onChange={(e) => onSearchChange(e.target.value)}
        placeholder={searchPlaceholder}
        aria-label="Search"
      />
      <select
        className="select"
        value={sortBy}
        onChange={(e) => onSortByChange(e.target.value)}
        aria-label="Sort field"
      >
        {sortOptions.map((o) => (
          <option key={o.value} value={o.value}>
            Sort · {o.label}
          </option>
        ))}
      </select>
      <select
        className="select"
        value={sortDirection}
        onChange={(e) => onSortDirectionChange(e.target.value as 'asc' | 'desc')}
        aria-label="Sort direction"
      >
        <option value="asc">Ascending</option>
        <option value="desc">Descending</option>
      </select>
    </div>
  );
}
