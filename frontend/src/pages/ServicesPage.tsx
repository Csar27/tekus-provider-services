import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { getServices, createService, updateService, activateService, deactivateService, getServiceCountries, setServiceCountries } from '../api/services';
import { TableControls } from '../components/TableControls';
import { Pagination } from '../components/Pagination';
import { Modal } from '../components/Modal';
import { getCountries } from '../api/countries';
import { useForm } from 'react-hook-form';

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

  const qc = useQueryClient();
  const { data, isLoading } = useQuery({ queryKey: ['services', page, pageSize, search, sortBy, sortDirection], queryFn: () => getServices({ page, pageSize, search, sortBy, sortDirection }) });

  const createMut = useMutation({ mutationFn: (d: any) => createService(d), onSuccess: () => { qc.invalidateQueries({ queryKey: ['services'] }); setOpen(false); } });
  const updateMut = useMutation({ mutationFn: ({ id, d }: { id: string, d: any }) => updateService(id, d), onSuccess: () => { qc.invalidateQueries({ queryKey: ['services'] }); setOpen(false); } });
  const activateMut = useMutation({ mutationFn: (id: string) => activateService(id), onSuccess: () => qc.invalidateQueries({ queryKey: ['services'] }) });
  const deactivateMut = useMutation({ mutationFn: (id: string) => deactivateService(id), onSuccess: () => qc.invalidateQueries({ queryKey: ['services'] }) });

  const { register, handleSubmit, reset } = useForm({ defaultValues: { name: '', description: '', hourlyRateUsd: 0 } });
  const startEdit = (s: any) => { setEditing(s); reset({ name: s.name, description: s.description, hourlyRateUsd: s.hourlyRateUsd }); setOpen(true); };
  const startCreate = () => { setEditing(null); reset({ name: '', description: '', hourlyRateUsd: 0 }); setOpen(true); };

  const submit = (d: any) => {
    if (editing) updateMut.mutate({ id: editing.id, d });
    else createMut.mutate(d);
  };

  if (isLoading) return <div>Loading...</div>;
  if (!data) return <div>No data</div>;

  return (
    <div>
      <div className="page-header">
        <h2 className="page-title">Services</h2>
        <button onClick={startCreate}>New Service</button>
      </div>
      <TableControls search={search} sortBy={sortBy} sortDirection={sortDirection} onSearchChange={setSearch} onSortByChange={setSortBy} onSortDirectionChange={setSortDirection} sortOptions={[{ value: 'name', label: 'Name' }, { value: 'hourlyRateUsd', label: 'Rate' }, { value: 'createdAtUtc', label: 'Created' }]} />
      <table className="table">
        <thead><tr><th>Name</th><th>Rate (USD/hr)</th><th>Status</th><th>Actions</th></tr></thead>
        <tbody>
          {data.items?.map((s: any) => (
            <tr key={s.id}><td>{s.name}</td><td>${s.hourlyRateUsd}</td><td>{s.isActive ? 'Active' : 'Inactive'}</td><td className="actions">
              <button className="secondary" onClick={() => startEdit(s)}>Edit</button>
              {s.isActive ? <button className="secondary" onClick={() => deactivateMut.mutate(s.id)}>Deactivate</button> : <button onClick={() => activateMut.mutate(s.id)}>Activate</button>}
              <button className="secondary" onClick={() => { setSelectedService(s); setOpenCountries(true); }}>Countries</button>
            </td></tr>
          ))}
        </tbody>
      </table>
      <Pagination page={page} pageSize={pageSize} totalCount={data.totalCount} totalPages={data.totalPages} onPageChange={setPage} onPageSizeChange={setPageSize} />

      <Modal open={open} onClose={() => setOpen(false)} title={editing ? 'Edit Service' : 'Create Service'}>
        <form onSubmit={handleSubmit(submit)}>
          <div style={{ marginBottom: 12 }}><input {...register('name')} placeholder="Name" style={{ width: '100%', padding: 8 }} /></div>
          <div style={{ marginBottom: 12 }}><input {...register('description')} placeholder="Description" style={{ width: '100%', padding: 8 }} /></div>
          <div style={{ marginBottom: 12 }}><input type="number" step="0.01" {...register('hourlyRateUsd', { valueAsNumber: true })} placeholder="Hourly Rate USD" style={{ width: '100%', padding: 8 }} /></div>
          <button type="submit">Save</button>
        </form>
      </Modal>

      {openCountries && selectedService && <ServiceCountriesModal service={selectedService} onClose={() => setOpenCountries(false)} />}
    </div>
  );
}

function ServiceCountriesModal({ service, onClose }: { service: any, onClose: () => void }) {
  const qc = useQueryClient();
  const { data: sc } = useQuery({ queryKey: ['service-countries', service.id], queryFn: () => getServiceCountries(service.id) });
  const { data: countries } = useQuery({ queryKey: ['countries-all'], queryFn: () => getCountries({ pageSize: 100, sortBy: 'code', sortDirection: 'asc' }) });
  const [selected, setSelected] = useState<string[]>(sc?.map((x: any) => x.countryCode) || []);
  const save = useMutation({ mutationFn: () => setServiceCountries(service.id, selected), onSuccess: () => { qc.invalidateQueries({ queryKey: ['service-countries', service.id] }); onClose(); } });

  return (
    <Modal open onClose={onClose} title={`Countries for ${service.name}`}>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(120px, 1fr))', gap: 8 }}>
        {countries?.items?.map((c: any) => (
          <label key={c.code}><input type="checkbox" checked={selected.includes(c.code)} onChange={(e) => setSelected(e.target.checked ? [...selected, c.code] : selected.filter((x) => x !== c.code))} /> {c.code}</label>
        ))}
      </div>
      <button style={{ marginTop: 16 }} onClick={() => save.mutate()}>Save</button>
    </Modal>
  );
}
