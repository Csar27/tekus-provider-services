interface Props {
  open: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
}

export function Modal({ open, onClose, title, children }: Props) {
  if (!open) return null;
  return (
    <div className="modal-backdrop">
      <div className="modal">
        <div className="modal-header">
          <h3 className="modal-title">{title}</h3>
          <button className="secondary" onClick={onClose}>Close</button>
        </div>
        <div className="modal-content">{children}</div>
      </div>
    </div>
  );
}
