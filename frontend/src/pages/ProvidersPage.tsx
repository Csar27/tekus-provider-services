import { useEffect, useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import {
  getProviders,
  createProvider,
  updateProvider,
  activateProvider,
  deactivateProvider,
} from '../api/providers';
import { getProviderServices, setProviderServices } from '../api/providerServices';
import {
  getProviderCustomFields,
  upsertProviderCustomField,
  deleteProviderCustomField,
} from '../api/providerCustomFields';
import { getServices } from '../api/services';
import { TableControls } from '../components/TableControls';
import { Pagination } from '../components/Pagination';
import { Modal } from '../components/Modal';
import { Button, EmptyState, PageHead, Panel, StatusPill, TableSkeleton } from '../components/ui';
import { formatDate, formatMoney } from '../lib/format';
import { readApiError } from '../lib/api';

export function ProvidersPage() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState('');
  const [sortBy, setSortBy] = useState('name');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');

  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<any>(null);
  const [saveError, setSaveError] = useState<string | null>(null);
  const [openServices, setOpenServices] = useState(false);
  const [openFields, setOpenFields] = useState(false);
  const [selectedProvider, setSelectedProvider] = useState<any>(null);

  const qc = useQueryClient();
  const { data, isLoading } = useQuery({
    queryKey: ['providers', page, pageSize, search, sortBy, sortDirection],
    queryFn: () => getProviders({ page, pageSize, search, sortBy, sortDirection }),
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ['providers'] });

  const createMut = useMutation({
    mutationFn: (d: any) => createProvider(d),
    onSuccess: () => {
      invalidate();
      setOpen(false);
      setSaveError(null);
    },
    onError: (e: any) => setSaveError(readApiError(e, 'The provider could not be created.')),
  });
  const updateMut = useMutation({
    mutationFn: ({ id, d, version }: { id: string; d: any; version?: number }) =>
      updateProvider(id, d, version),
    onSuccess: () => {
      invalidate();
      setOpen(false);
      setSaveError(null);
    },
    onError: (e: any) => setSaveError(readApiError(e, 'The provider could not be updated.')),
  });
  const activateMut = useMutation({
    mutationFn: (id: string) => activateProvider(id),
    onSuccess: invalidate,
    onError: (e: any) => setSaveError(readApiError(e, 'The provider could not be activated.')),
  });
  const deactivateMut = useMutation({
    mutationFn: (id: string) => deactivateProvider(id),
    onSuccess: invalidate,
    onError: (e: any) => setSaveError(readApiError(e, 'The provider could not be deactivated.')),
  });

  const { register, handleSubmit, reset } = useForm({
    defaultValues: { nit: '', name: '', email: '', isActive: true },
  });

  const startEdit = (p: any) => {
    setEditing(p);
    setSaveError(null);
    reset({ nit: p.nit, name: p.name, email: p.email, isActive: p.isActive });
    setOpen(true);
  };
  const startCreate = () => {
    setEditing(null);
    setSaveError(null);
    reset({ nit: '', name: '', email: '', isActive: true });
    setOpen(true);
  };
  const submit = (d: any) => {
    if (editing) updateMut.mutate({ id: editing.id, d, version: editing.version });
    else createMut.mutate(d);
  };

  const onSearch = (value: string) => {
    setSearch(value);
    setPage(1);
  };
  const changeSort = (field: string) => {
    setSortBy(field);
    setPage(1);
  };
  const changeDirection = (d: 'asc' | 'desc') => {
    setSortDirection(d);
    setPage(1);
  };

  const openForServices = (p: any) => {
    setSelectedProvider(p);
    setOpenServices(true);
  };
  const openForFields = (p: any) => {
    setSelectedProvider(p);
    setOpenFields(true);
  };

  return (
    <>
      <PageHead
        title="Providers"
        subtitle="Suppliers in the Tekus register, with their portfolio and any client-specific fields."
        actions={
          <Button onClick={startCreate} disabled={createMut.isPending}>
            New provider
          </Button>
        }
      />

      <TableControls
        search={search}
        sortBy={sortBy}
        sortDirection={sortDirection}
        onSearchChange={onSearch}
        onSortByChange={changeSort}
        onSortDirectionChange={changeDirection}
        sortOptions={[
          { value: 'name', label: 'Name' },
          { value: 'nit', label: 'NIT' },
          { value: 'createdAtUtc', label: 'Created' },
        ]}
        searchPlaceholder="Search by name, NIT or email…"
      />

      <Panel meta={data ? `${data.totalCount} providers` : undefined}>
        {isLoading && <TableSkeleton rows={7} cols={7} />}

        {!isLoading && (!data?.items || data.items.length === 0) && (
          <EmptyState
            title={search ? 'No provider matches your search' : 'No providers registered'}
            hint={
              search
                ? 'Try a different name or NIT.'
                : 'Register your first supplier to start building service portfolios.'
            }
          />
        )}

        {!isLoading && data?.items?.length > 0 && (
          <table className="tbl">
            <thead>
              <tr>
                <th style={{ width: '15%' }}>NIT</th>
                <th style={{ width: '24%' }}>Provider</th>
                <th>Contact</th>
                <th className="num">Services</th>
                <th className="num">Fields</th>
                <th>Status</th>
                <th style={{ textAlign: 'right' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((p: any) => (
                <tr key={p.id}>
                  <td style={{ fontVariantNumeric: 'tabular-nums', color: 'var(--color-ink-soft)' }}>
                    {p.nit}
                  </td>
                  <td>
                    <div className="cell-title">{p.name}</div>
                    <div className="cell-sub">Added {formatDate(p.createdAtUtc)}</div>
                  </td>
                  <td style={{ color: 'var(--color-ink-soft)' }}>{p.email}</td>
                  <td className="num">{p.serviceCount}</td>
                  <td className="num">{p.customFieldCount}</td>
                  <td>
                    <StatusPill active={p.isActive} />
                  </td>
                  <td>
                    <div className="row-actions">
                      <Button variant="quiet" size="sm" onClick={() => startEdit(p)}>
                        Edit
                      </Button>
                      {p.isActive ? (
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => deactivateMut.mutate(p.id)}
                          disabled={deactivateMut.isPending}
                        >
                          Deactivate
                        </Button>
                      ) : (
                        <Button
                          size="sm"
                          onClick={() => activateMut.mutate(p.id)}
                          disabled={activateMut.isPending}
                        >
                          Activate
                        </Button>
                      )}
                      <Button variant="quiet" size="sm" onClick={() => openForServices(p)}>
                        Services
                      </Button>
                      <Button variant="quiet" size="sm" onClick={() => openForFields(p)}>
                        Fields
                      </Button>
                    </div>
                  </td>
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

      <Modal
        open={open}
        onClose={() => setOpen(false)}
        title={editing ? 'Edit provider' : 'New provider'}
        subtitle={editing ? `Registered ${formatDate(editing.createdAtUtc)}` : undefined}
        footer={
          <>
            <Button variant="outline" onClick={() => setOpen(false)}>
              Cancel
            </Button>
            <Button
              onClick={handleSubmit(submit)}
              disabled={createMut.isPending || updateMut.isPending}
            >
              {editing ? 'Save changes' : 'Create provider'}
            </Button>
          </>
        }
      >
        <form onSubmit={handleSubmit(submit)}>
          {saveError && <p className="form-error">{saveError}</p>}
          <div style={{ marginBottom: '1rem' }}>
            <label className="field__label" htmlFor="nit">
              NIT
            </label>
            <input
              id="nit"
              className="input"
              {...register('nit')}
              placeholder="811000555-1"
              required
            />
          </div>
          <div style={{ marginBottom: '1rem' }}>
            <label className="field__label" htmlFor="name">
              Legal name
            </label>
            <input id="name" className="input" {...register('name')} placeholder="Acme Consulting" required />
          </div>
          <div style={{ marginBottom: '1rem' }}>
            <label className="field__label" htmlFor="email">
              Contact email
            </label>
            <input
              id="email"
              className="input"
              type="email"
              {...register('email')}
              placeholder="contact@acme.example"
              required
            />
          </div>
          <label className="check" style={{ width: 'fit-content' }}>
            <input type="checkbox" {...register('isActive')} />
            Active in the register
          </label>
          <button type="submit" hidden aria-hidden="true" tabIndex={-1} />
        </form>
      </Modal>

      {selectedProvider && (
        <>
          <ProviderServicesModal
            open={openServices}
            provider={selectedProvider}
            onClose={() => setOpenServices(false)}
          />
          <ProviderFieldsModal
            open={openFields}
            provider={selectedProvider}
            onClose={() => setOpenFields(false)}
          />
        </>
      )}
    </>
  );
}

/* ------------------------------------------------------ services portfolio */

function ProviderServicesModal({
  open,
  provider,
  onClose,
}: {
  open: boolean;
  provider: any;
  onClose: () => void;
}) {
  const qc = useQueryClient();
  const { data: assigned, isError: assignedError } = useQuery({
    queryKey: ['provider-services', provider.id],
    queryFn: () => getProviderServices(provider.id),
    enabled: open,
  });
  const { data: all } = useQuery({
    queryKey: ['services-all'],
    queryFn: () => getServices({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' }),
    enabled: open,
  });

  const [selected, setSelected] = useState<string[]>([]);
  const [saveError, setSaveError] = useState<string | null>(null);

  // Clear any stale error from a previous attempt whenever the dialog opens.
  useEffect(() => {
    if (open) setSaveError(null);
  }, [open]);

  // The assigned list arrives asynchronously; sync the selection once it does.
  useEffect(() => {
    if (assigned) setSelected(assigned);
  }, [assigned]);

  const save = useMutation({
    mutationFn: () => setProviderServices(provider.id, selected),
    onSuccess: () => {
      setSaveError(null);
      qc.invalidateQueries({ queryKey: ['providers'] });
      qc.invalidateQueries({ queryKey: ['provider-services', provider.id] });
      onClose();
    },
    onError: (e: any) => setSaveError(readApiError(e, 'The portfolio could not be saved.')),
  });

  const toggle = (id: string) =>
    setSelected((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]));

  const options = all?.items ?? [];
  const dirty =
    !!assigned &&
    (assigned.length !== selected.length ||
      !assigned.every((id) => selected.includes(id)));

  return (
    <Modal
      open={open}
      onClose={onClose}
      wide
      title={`Portfolio of ${provider.name}`}
      subtitle={`${selected.length} of ${options.length} services selected`}
      footer={
        <>
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button onClick={() => save.mutate()} disabled={save.isPending || !dirty}>
            {save.isPending ? 'Saving…' : 'Save portfolio'}
          </Button>
        </>
      }
    >
      {saveError && <p className="form-error">{saveError}</p>}
      {assignedError && (
        <p className="form-error">The current portfolio could not be loaded.</p>
      )}

      {options.length === 0 ? (
        <EmptyState title="No services available" hint="Create services first, then assign them here." />
      ) : (
        <div className="checkgrid">
          {options.map((s: any) => (
            <label key={s.id} className="check">
              <input type="checkbox" checked={selected.includes(s.id)} onChange={() => toggle(s.id)} />
              <span style={{ minWidth: 0 }}>
                <span style={{ display: 'block', fontWeight: 500 }}>{s.name}</span>
                <span className="cell-sub">{formatMoney(s.hourlyRateUsd)} / hr</span>
              </span>
            </label>
          ))}
        </div>
      )}
    </Modal>
  );
}

/* ---------------------------------------------------------- custom fields */

function ProviderFieldsModal({
  open,
  provider,
  onClose,
}: {
  open: boolean;
  provider: any;
  onClose: () => void;
}) {
  const qc = useQueryClient();
  const { data: fields, isLoading } = useQuery({
    queryKey: ['provider-cf', provider.id],
    queryFn: () => getProviderCustomFields(provider.id),
    enabled: open,
  });

  const [key, setKey] = useState('');
  const [value, setValue] = useState('');
  const [editingKey, setEditingKey] = useState<string | null>(null);
  const [editValue, setEditValue] = useState('');
  const [error, setError] = useState<string | null>(null);

  const refresh = () => {
    qc.invalidateQueries({ queryKey: ['provider-cf', provider.id] });
    qc.invalidateQueries({ queryKey: ['providers'] });
  };

  // Fields are addressed by key, so a create and an edit run the same upsert.
  const save = useMutation({
    mutationFn: ({ fieldKey, fieldValue }: { fieldKey: string; fieldValue: string }) =>
      upsertProviderCustomField(provider.id, fieldKey, fieldValue),
    onSuccess: () => {
      setKey('');
      setValue('');
      setEditingKey(null);
      setEditValue('');
      setError(null);
      refresh();
    },
    onError: (e: any) => setError(e?.response?.data?.detail ?? 'Could not save the field.'),
  });

  const remove = useMutation({
    mutationFn: (fieldKey: string) => deleteProviderCustomField(provider.id, fieldKey),
    onSuccess: refresh,
    onError: (e: any) => setError(e?.response?.data?.detail ?? 'Could not delete the field.'),
  });

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={`Custom fields`}
      subtitle={`Values stored on ${provider.name}`}
    >
      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1.1rem' }}>
        <input
          className="input"
          value={key}
          onChange={(e) => setKey(e.target.value)}
          placeholder="Field key"
          aria-label="Field key"
        />
        <input
          className="input"
          value={value}
          onChange={(e) => setValue(e.target.value)}
          placeholder="Value"
          aria-label="Field value"
        />
        <Button
          variant="outline"
          onClick={() => save.mutate({ fieldKey: key, fieldValue: value })}
          disabled={!key || !value || save.isPending}
        >
          Add
        </Button>
      </div>

      {error && <p className="form-error">{error}</p>}

      {isLoading && <TableSkeleton rows={3} cols={2} />}

      {!isLoading && (!fields || fields.length === 0) && (
        <EmptyState
          title="No custom fields"
          hint="Use key/value pairs to store client-specific data such as billing terms."
        />
      )}

      {!isLoading && fields && fields.length > 0 && (
        <div style={{ borderTop: '1px solid var(--color-line)' }}>
          {fields.map((f: any) => (
            <div
              key={f.key}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '0.75rem',
                padding: '0.6rem 0',
                borderBottom: '1px solid var(--color-line)',
              }}
            >
              <span
                style={{
                  flex: '0 0 34%',
                  fontSize: '12.5px',
                  fontWeight: 600,
                  color: 'var(--color-navy)',
                }}
              >
                {f.key}
              </span>

              {editingKey === f.key ? (
                <>
                  <input
                    className="input"
                    value={editValue}
                    onChange={(e) => setEditValue(e.target.value)}
                    aria-label={`Value for ${f.key}`}
                  />
                  <Button
                    size="sm"
                    onClick={() => save.mutate({ fieldKey: f.key, fieldValue: editValue })}
                    disabled={save.isPending}
                  >
                    Save
                  </Button>
                  <Button variant="quiet" size="sm" onClick={() => setEditingKey(null)}>
                    Cancel
                  </Button>
                </>
              ) : (
                <>
                  <span style={{ flex: 1, fontSize: '13.5px' }}>{f.value}</span>
                  <Button
                    variant="quiet"
                    size="sm"
                    onClick={() => {
                      setEditingKey(f.key);
                      setEditValue(f.value);
                    }}
                  >
                    Edit
                  </Button>
                  <Button
                    variant="danger"
                    size="sm"
                    onClick={() => remove.mutate(f.key)}
                    disabled={remove.isPending}
                  >
                    Delete
                  </Button>
                </>
              )}
            </div>
          ))}
        </div>
      )}
    </Modal>
  );
}
