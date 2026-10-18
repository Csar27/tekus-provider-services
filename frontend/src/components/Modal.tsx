import { useEffect, type ReactNode } from 'react';
import { Button } from './ui';

interface Props {
  open: boolean;
  onClose: () => void;
  title: string;
  subtitle?: string;
  wide?: boolean;
  children: ReactNode;
  footer?: ReactNode;
}

export function Modal({ open, onClose, title, subtitle, wide, children, footer }: Props) {
  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, [open, onClose]);

  if (!open) return null;

  return (
    <div
      className="modal-backdrop"
      onMouseDown={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
    >
      <div
        className={`modal${wide ? ' modal--wide' : ''}`}
        role="dialog"
        aria-modal="true"
        aria-label={title}
      >
        <div className="modal__head">
          <div>
            <h3 className="modal__title">{title}</h3>
            {subtitle && <p className="modal__sub">{subtitle}</p>}
          </div>
          <Button variant="quiet" size="sm" onClick={onClose} aria-label="Close dialog">
            Close
          </Button>
        </div>
        <div className="modal__body">{children}</div>
        {footer && <div className="modal__foot">{footer}</div>}
      </div>
    </div>
  );
}
