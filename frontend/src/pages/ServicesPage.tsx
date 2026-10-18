import { useEffect, useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import {
  getServices,
  createService,
  updateService,
  activateService,
  deactivateService,
  getServiceCountries,
  setServiceCountries,
} from '../api/services';
import { getCountries } from '../api/countries';
import { TableControls } from '../components/TableControls';
import { Pagination } from '../components/Pagination';
import { Modal } from '../components/Modal';
import { Button, EmptyState, PageHead, Panel, StatusPill, TableSkeleton } from '../components/ui';
import { formatDate, formatMoney } from '../lib/format';

export function ServicesPage() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState('');
  const [sortBy, setSortBy] = useState('name');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');

  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<any>(null);
  const [openCountries, setOpenCountries] = useState(false);
  const [selectedService, setSelectedService] = useState<any>(null);
  const [saveError, setSaveError] = useState<string | null>(null);

  const qc = useQueryClient();
  const { data, isLoading } = useQuery({
    queryKey: ['services', page, pageSize, search, sortBy, sortDirection],
    queryFn: () => getServices({ page, pageSize, search, sortBy, sortDirection }),
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ['services'] });

  const createMut = useMutation({
    mutationFn: (d: any) => createService(d),
    onSuccess: () => {
      invalidate();
      setOpen(false);
      setSaveError(null);
    },
    onError: (e: any) => setSaveError(readApiError(e, 'The service could not be created.')),
  });
  const updateMut = useMutation({
    mutationFn: ({ id, d, version }: { id: string; d: any; version?: number }) =>
      updateService(id, d, version),
    onSuccess: () => {
      invalidate();
      setOpen(false);
      setSaveError(null);
    },
    onError: (e: any) => setSaveError(readApiError(e, 'The service could not be updated.')),
  });
  const activateMut = useMutation({
    mutationFn: (id: string) => activateService(id),
    onSuccess: invalidate,
    onError: (e: any) => setSaveError(readApiError(e, 'The service could not be activated.')),
  });
  const deactivateMut = useMutation({
    mutationFn: (id: string) => deactivateService(id),
    onSuccess: invalidate,
    onError: (e: any) => setSaveError(readApiError(e, 'The service could not be deactivated.')),
  });

  const { register, handleSubmit, reset } = useForm({
    defaultValues: { name: '', description: '', hourlyRateUsd: 0 },
  });

  const startEdit = (s: any) => {
    setEditing(s);
    reset({ name: s.name, description: s.description ?? '', hourlyRateUsd: s.hourlyRateUsd });
    setOpen(true);
  };
  const startCreate = () => {
    setEditing(null);
    reset({ name: '', description: '', hourlyRateUsd: 0 });
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

  const openForCountries = (s: any) => {
    setSelectedService(s);
    setOpenCountries(true);
  };

  return (
    <>
      <PageHead
        title="Services"
        subtitle="Hourly engagement catalogue. Each service can be offered in one or more countries."
        actions={
          <Button onClick={startCreate} disabled={createMut.isPending}>
            New service
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
          { value: 'hourlyRateUsd', label: 'Rate' },
          { value: 'createdAtUtc', label: 'Created' },
        ]}
        searchPlaceholder="Search by name or description…"
      />

      <Panel meta={data ? `${data.totalCount} services` : undefined}>
        {isLoading && <TableSkeleton rows={7} cols={5} />}

        {!isLoading && (!data?.items || data.items.length === 0) && (
          <EmptyState
            title={search ? 'No service matches your search' : 'No services registered'}
            hint={
              search
                ? 'Try a different name or rate.'
                : 'Add your first engagement type to start building provider portfolios.'
            }
          />
        )}

        {!isLoading && data?.items?.length > 0 && (
          <table className="tbl">
            <thead>
              <tr>
                <th style={{ width: '28%' }}>Service</th>
                <th className="num">Rate</th>
                <th className="num">Countries</th>
                <th>Status</th>
                <th>Registered</th>
                <th style={{ textAlign: 'right' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((s: any) => (
                <tr key={s.id}>
                  <td>
                    <div className="cell-title">{s.name}</div>
                    {s.description && (
                      <div className="cell-sub" style={{ maxWidth: '46ch' }}>
                        {s.description}
                      </div>
                    )}
                  </td>
                  <td className="num">{formatMoney(s.hourlyRateUsd)}</td>
                  <td className="num">{s.countryCodes?.length ?? 0}</td>
                  <td>
                    <StatusPill active={s.isActive} />
                  </td>
                  <td style={{ color: 'var(--color-ink-soft)', whiteSpace: 'nowrap' }}>
                    {formatDate(s.createdAtUtc)}
                  </td>
                  <td>
                    <div className="row-actions">
                      <Button variant="quiet" size="sm" onClick={() => startEdit(s)}>
                        Edit
                      </Button>
                      {s.isActive ? (
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => deactivateMut.mutate(s.id)}
                          disabled={deactivateMut.isPending}
                        >
                          Deactivate
                        </Button>
                      ) : (
                        <Button
                          size="sm"
                          onClick={() => activateMut.mutate(s.id)}
                          disabled={activateMut.isPending}
                        >
                          Activate
                        </Button>
                      )}
                      <Button variant="quiet" size="sm" onClick={() => openForCountries(s)}>
                        Countries
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
        title={editing ? 'Edit service' : 'New service'}
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
              {editing ? 'Save changes' : 'Create service'}
            </Button>
          </>
        }
      >
        <form onSubmit={handleSubmit(submit)}>
          <div style={{ marginBottom: '1rem' }}>
            <label className="field__label" htmlFor="svc-name">
              Name
            </label>
            <input
              id="svc-name"
              className="input"
              {...register('name')}
              placeholder="Cloud infrastructure audit"
              required
            />
          </div>
          <div style={{ marginBottom: '1rem' }}>
            <label className="field__label" htmlFor="svc-desc">
              Description
            </label>
            <input
              id="svc-desc"
              className="input"
              {...register('description')}
              placeholder="Short summary shown in reports"
            />
          </div>
          <div style={{ marginBottom: '0.4rem' }}>
            <label className="field__label" htmlFor="svc-rate">
              Hourly rate (USD)
            </label>
            <input
              id="svc-rate"
              className="input"
              type="number"
              step="0.01"
              min="0"
              {...register('hourlyRateUsd', { valueAsNumber: true })}
              required
            />
          </div>
          <button type="submit" hidden aria-hidden="true" tabIndex={-1} />
        </form>
      </Modal>

      {selectedService && (
        <ServiceCountriesModal
          open={openCountries}
          service={selectedService}
          onClose={() => setOpenCountries(false)}
        />
      )}
    </>
  );
}

function ServiceCountriesModal({
  open,
  service,
  onClose,
}: {
  open: boolean;
  service: any;
  onClose: () => void;
}) {
  const qc = useQueryClient();
  const { data: assigned } = useQuery({
    queryKey: ['service-countries', service.id],
    queryFn: () => getServiceCountries(service.id),
    enabled: open,
  });
  const { data: all } = useQuery({
    queryKey: ['countries-all'],
    queryFn: () => getCountries({ pageSize: 500, sortBy: 'name', sortDirection: 'asc' }),
    enabled: open,
  });

  const [selected, setSelected] = useState<string[]>([]);

  // Codes arrive asynchronously; mirror them into the selection once loaded.
  useEffect(() => {
    if (assigned) setSelected(assigned.map((x: any) => x.countryCode));
  }, [assigned]);

  const save = useMutation({
    mutationFn: () => setServiceCountries(service.id, selected),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['services'] });
      qc.invalidateQueries({ queryKey: ['service-countries', service.id] });
      onClose();
    },
  });

  const toggle = (code: string) =>
    setSelected((prev) => (prev.includes(code) ? prev.filter((c) => c !== code) : [...prev, code]));

  const options = all?.items ?? [];
  const dirty =
    !!assigned &&
    (assigned.length !== selected.length ||
      !assigned.every((x: any) => selected.includes(x.countryCode)));

  return (
    <Modal
      open={open}
      onClose={onClose}
      wide
      title={`Countries for ${service.name}`}
      subtitle={`${selected.length} of ${options.length} countries selected`}
      footer={
        <>
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button onClick={() => save.mutate()} disabled={save.isPending || !dirty}>
            {save.isPending ? 'Saving…' : 'Save coverage'}
          </Button>
        </>
      }
    >
      {options.length === 0 ? (
        <EmptyState
          title="No countries available"
          hint="The country catalogue is loaded from the external service on first access."
        />
      ) : (
        <div className="checkgrid">
          {options.map((c: any) => (
            <label key={c.code} className="check">
              <input
                type="checkbox"
                checked={selected.includes(c.code)}
                onChange={() => toggle(c.code)}
              />
              <span style={{ minWidth: 0 }}>
                <span style={{ display: 'block', fontWeight: 500 }}>{c.name}</span>
                <span className="cell-sub">
                  {c.code}
                  {c.region ? ` · ${c.region}` : ''}
                </span>
              </span>
            </label>
          ))}
        </div>
      )}
    </Modal>
  );
}
