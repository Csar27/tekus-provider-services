import { Button } from './ui';

interface Props {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  onPageChange: (p: number) => void;
  onPageSizeChange: (s: number) => void;
}

export function Pagination({
  page,
  pageSize,
  totalCount,
  totalPages,
  onPageChange,
  onPageSizeChange,
}: Props) {
  const last = totalPages || 1;
  const first = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, totalCount);

  return (
    <nav className="pager" aria-label="Pagination">
      <span className="pager__count">
        {first}–{to} of {totalCount}
      </span>

      <div style={{ display: 'flex', alignItems: 'center', gap: '0.4rem', marginLeft: 'auto' }}>
        <label className="pager__count" htmlFor="page-size" style={{ color: 'var(--color-ink-faint)' }}>
          Rows
        </label>
        <select
          id="page-size"
          className="select"
          value={pageSize}
          onChange={(e) => onPageSizeChange(Number(e.target.value))}
        >
          {[5, 10, 20, 50].map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </select>
        <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => onPageChange(page - 1)}>
          Previous
        </Button>
        <span className="pager__count">
          Page {page} / {last}
        </span>
        <Button variant="outline" size="sm" disabled={page >= last} onClick={() => onPageChange(page + 1)}>
          Next
        </Button>
      </div>
    </nav>
  );
}
