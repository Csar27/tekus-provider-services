import type { ButtonHTMLAttributes, ReactNode } from 'react';

/* ------------------------------------------------------------------ button */

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: 'primary' | 'outline' | 'quiet' | 'danger';
  size?: 'md' | 'sm';
};

export function Button({ variant = 'primary', size = 'md', className = '', ...rest }: ButtonProps) {
  const classes = ['btn', `btn--${variant}`, size === 'sm' ? 'btn--sm' : '', className]
    .filter(Boolean)
    .join(' ');
  return <button className={classes} {...rest} />;
}

/* ------------------------------------------------------------ status pill */

export function StatusPill({ active }: { active: boolean }) {
  return (
    <span className={`pill ${active ? 'pill--active' : 'pill--inactive'}`}>
      {active ? 'Active' : 'Inactive'}
    </span>
  );
}

/* ------------------------------------------------------------ page header */

export function PageHead({
  title,
  subtitle,
  actions,
}: {
  title: string;
  subtitle?: string;
  actions?: ReactNode;
}) {
  return (
    <header className="pagehead">
      <div className="pagehead__rule" aria-hidden="true" />
      <div className="pagehead__row">
        <div>
          <h2 className="pagehead__title">{title}</h2>
          {subtitle && <p className="pagehead__sub">{subtitle}</p>}
        </div>
        {actions && <div className="pagehead__actions">{actions}</div>}
      </div>
    </header>
  );
}

/* ------------------------------------------------------------------ panel */

export function Panel({
  title,
  meta,
  children,
}: {
  title?: string;
  meta?: ReactNode;
  children: ReactNode;
}) {
  return (
    <section className="panel">
      {(title || meta) && (
        <div className="panel__head">
          {title && <h3 className="panel__title">{title}</h3>}
          {meta && <span className="panel__meta">{meta}</span>}
        </div>
      )}
      {children}
    </section>
  );
}

/* ------------------------------------------------------------ empty state */

export function EmptyState({ title, hint }: { title: string; hint?: string }) {
  return (
    <div className="empty">
      <div className="empty__title">{title}</div>
      {hint && <div className="empty__hint">{hint}</div>}
    </div>
  );
}

/**
 * Shown when a query fails (API down, session expired, network). Rendering this
 * instead of the empty state is what keeps an outage from looking like an empty
 * database.
 */
export function QueryError({ message, onRetry }: { message: string; onRetry?: () => void }) {
  return (
    <div className="empty">
      <div className="empty__title">We could not load this data</div>
      <div className="empty__hint">{message}</div>
      {onRetry && (
        <div style={{ marginTop: '1rem' }}>
          <Button variant="outline" onClick={onRetry}>
            Try again
          </Button>
        </div>
      )}
    </div>
  );
}

/* ---------------------------------------------------------------- loading */

export function TableSkeleton({ rows = 6, cols = 4 }: { rows?: number; cols?: number }) {
  return (
    <div style={{ padding: '1.15rem' }}>
      {Array.from({ length: rows }).map((_, r) => (
        <div key={r} style={{ display: 'flex', gap: '1.5rem', marginBottom: '0.9rem' }}>
          {Array.from({ length: cols }).map((__, c) => (
            <div
              key={c}
              className="skeleton"
              style={{ height: 14, flex: c === 0 ? '0 0 22%' : '1 1 auto' }}
            />
          ))}
        </div>
      ))}
    </div>
  );
}
