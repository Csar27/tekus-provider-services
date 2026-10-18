import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { getProviders, createProvider, updateProvider, activateProvider, deactivateProvider } from '../api/providers';
import { TableControls } from '../components/TableControls';
import { Pagination } from '../components/Pagination';
import { Modal } from '../components/Modal';
import { useForm } from 'react-hook-form';
import { getProviderServices, setProviderServices } from '../api/providerServices';
import { getServices } from '../api/services';
import { getProviderCustomFields, addProviderCustomField, deleteProviderCustomField } from '../api/providerCustomFields';

export function ProvidersPage() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState('');
  const [sortBy, setSortBy] = useState('name');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<any>(null);
  const [openServices, setOpenServices] = useState(false);
  const [selectedProvider, setSelectedProvider] = useState<any>(null);
  const [openFields, setOpenFields] = useState(false);

  const qc = useQueryClient();
  const { data, isLoading } = useQuery({ queryKey: ['providers', page, pageSize, search, sortBy, sortDirection], queryFn: () => getProviders({ page, pageSize, search, sortBy, sortDirection }) });

  const createMut = useMutation({ mutationFn: (d: any) => createProvider(d), onSuccess: () => { qc.invalidateQueries({ queryKey: ['providers'] }); setOpen(false); } });
  const updateMut = useMutation({ mutationFn: ({ id, d }: { id: string, d: any }) => updateProvider(id, d), onSuccess: () => { qc.invalidateQueries({ queryKey: ['providers'] }); setOpen(false); } });
  const activateMut = useMutation({ mutationFn: (id: string) => activateProvider(id), onSuccess: () => qc.invalidateQueries({ queryKey: ['providers'] }) });
  const deactivateMut = useMutation({ mutationFn: (id: string) => deactivateProvider(id), onSuccess: () => qc.invalidateQueries({ queryKey: ['providers'] }) });

  const { register, handleSubmit, reset } = useForm({ defaultValues: { nit: '', name: '', email: '', isActive: true } });
  const startEdit = (p: any) => { setEditing(p); reset({ nit: p.nit, name: p.name, email: p.email, isActive: p.isActive }); setOpen(true); };
  const startCreate = () => { setEditing(null); reset({ nit: '', name: '', email: '', isActive: true }); setOpen(true); };

  const submit = (d: any) => {
    if (editing) updateMut.mutate({ id: editing.id, d });
    else createMut.mutate(d);
  };

  if (isLoading) return <div>Loading...</div>;
  if (!data) return <div>No data</div>;

  return (
    <div>
      <div className="page-header">
        <h2 className="page-title">Providers</h2>
        <button onClick={startCreate}>New Provider</button>
      </div>
      <TableControls search={search} sortBy={sortBy} sortDirection={sortDirection} onSearchChange={setSearch} onSortByChange={setSortBy} onSortDirectionChange={setSortDirection} sortOptions={[{ value: 'name', label: 'Name' }, { value: 'nit', label: 'NIT' }, { value: 'createdAtUtc', label: 'Created' }]} />
      <table className="table">
        <thead><tr><th>NIT</th><th>Name</th><th>Email</th><th>Services</th><th>Custom Fields</th><th>Status</th><th>Actions</th></tr></thead>
        <tbody>
          {data.items?.map((p: any) => (
            <tr key={p.id}><td>{p.nit}</td><td>{p.name}</td><td>{p.email}</td><td>{p.serviceCount}</td><td>{p.customFieldCount}</td><td>{p.isActive ? 'Active' : 'Inactive'}</td><td className="actions">
              <button className="secondary" onClick={() => startEdit(p)}>Edit</button>
              {p.isActive ? <button className="secondary" onClick={() => deactivateMut.mutate(p.id)}>Deactivate</button> : <button onClick={() => activateMut.mutate(p.id)}>Activate</button>}
              <button className="secondary" onClick={() => { setSelectedProvider(p); setOpenServices(true); }}>Services</button>
              <button className="secondary" onClick={() => { setSelectedProvider(p); setOpenFields(true); }}>Custom Fields</button>
            </td></tr>
          ))}
        </tbody>
      </table>
      <Pagination page={page} pageSize={pageSize} totalCount={data.totalCount} totalPages={data.totalPages} onPageChange={setPage} onPageSizeChange={setPageSize} />

      <Modal open={open} onClose={() => setOpen(false)} title={editing ? 'Edit Provider' : 'Create Provider'}>
        <form onSubmit={handleSubmit(submit)}>
          <div style={{ marginBottom: 12 }}><input {...register('nit')} placeholder="NIT" style={{ width: '100%', padding: 8 }} /></div>
          <div style={{ marginBottom: 12 }}><input {...register('name')} placeholder="Name" style={{ width: '100%', padding: 8 }} /></div>
          <div style={{ marginBottom: 12 }}><input {...register('email')} placeholder="Email" style={{ width: '100%', padding: 8 }} /></div>
          <button type="submit">Save</button>
        </form>
      </Modal>

      {openServices && selectedProvider && <ProviderServicesModal provider={selectedProvider} onClose={() => setOpenServices(false)} />}
      {openFields && selectedProvider && <ProviderCustomFieldsModal provider={selectedProvider} onClose={() => setOpenFields(false)} />}
    </div>
  );
}

function ProviderServicesModal({ provider, onClose }: { provider: any, onClose: () => void }) {
  const qc = useQueryClient();
  const { data: ps } = useQuery({ queryKey: ['provider-services', provider.id], queryFn: () => getProviderServices(provider.id) });
  const { data: services } = useQuery({ queryKey: ['services-all'], queryFn: () => getServices({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' }) });
  const [selected, setSelected] = useState<string[]>(ps?.map((x: any) => x.serviceId) || []);
  const save = useMutation({ mutationFn: () => setProviderServices(provider.id, selected), onSuccess: () => { qc.invalidateQueries({ queryKey: ['providers'] }); qc.invalidateQueries({ queryKey: ['provider-services', provider.id] }); onClose(); } });

  return (
    <Modal open onClose={onClose} title={`Services for ${provider.name}`}>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(180px, 1fr))', gap: 8 }}>
        {services?.items?.map((s: any) => (
          <label key={s.id}><input type="checkbox" checked={selected.includes(s.id)} onChange={(e) => setSelected(e.target.checked ? [...selected, s.id] : selected.filter((x) => x !== s.id))} /> {s.name}</label>
        ))}
      </div>
      <button style={{ marginTop: 16 }} onClick={() => save.mutate()}>Save</button>
    </Modal>
  );
}

function ProviderCustomFieldsModal({ provider, onClose }: { provider: any, onClose: () => void }) {
  const qc = useQueryClient();
  const { data: fields } = useQuery({ queryKey: ['provider-cf', provider.id], queryFn: () => getProviderCustomFields(provider.id) });
  const [key, setKey] = useState('');
  const [value, setValue] = useState('');
  const add = useMutation({ mutationFn: () => addProviderCustomField(provider.id, { fieldKey: key, fieldValue: value }), onSuccess: () => { setKey(''); setValue(''); qc.invalidateQueries({ queryKey: ['provider-cf', provider.id] }); qc.invalidateQueries({ queryKey: ['providers'] }); } });
  const del = useMutation({ mutationFn: (fid: string) => deleteProviderCustomField(provider.id, fid), onSuccess: () => { qc.invalidateQueries({ queryKey: ['provider-cf', provider.id] }); qc.invalidateQueries({ queryKey: ['providers'] }); } });

  return (
    <Modal open onClose={onClose} title={`Custom Fields for ${provider.name}`}>
      <div style={{ display: 'flex', gap: 8, marginBottom: 12 }}>
        <input value={key} onChange={(e) => setKey(e.target.value)} placeholder="Key" style={{ flex: 1, padding: 8 }} />
        <input value={value} onChange={(e) => setValue(e.target.value)} placeholder="Value" style={{ flex: 1, padding: 8 }} />
        <button onClick={() => add.mutate()} disabled={!key || !value}>Add</button>
      </div>
      <ul>
        {fields?.map((f: any) => (
          <li key={f.id} style={{ display: 'flex', justifyContent: 'space-between', padding: 4 }}>{f.fieldKey}: {f.fieldValue} <button onClick={() => del.mutate(f.id)}>Delete</button></li>
        ))}
      </ul>
    </Modal>
  );
}
