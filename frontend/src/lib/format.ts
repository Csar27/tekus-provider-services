const money = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  minimumFractionDigits: 2,
});

const number = new Intl.NumberFormat('en-US');

/** Formats an hourly rate as a currency string: 300 -> "$300.00". */
export function formatMoney(value: number | null | undefined): string {
  if (value === null || value === undefined || Number.isNaN(value)) return '—';
  return money.format(value);
}

/** Formats a plain number with thousands separators. */
export function formatNumber(value: number | null | undefined): string {
  if (value === null || value === undefined || Number.isNaN(value)) return '—';
  return number.format(value);
}

/** Short date shown in tables: Oct 18, 2026. */
export function formatDate(value: string | null | undefined): string {
  if (!value) return '—';
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return '—';
  return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

/** Two-letter monogram for the sidebar avatar. */
export function initials(name: string | null | undefined): string {
  if (!name) return '··';
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return '··';
  const first = parts[0][0];
  const last = parts.length > 1 ? parts[parts.length - 1][0] : '';
  return (first + last).toUpperCase();
}
