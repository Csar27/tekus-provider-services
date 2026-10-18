import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../lib/api';
import { useAuth } from '../auth/authContext';

export function LoginPage() {
  const [email, setEmail] = useState('admin@tekus.co');
  const [password, setPassword] = useState('Tekus.2026!');
  const { login } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      const res = await api.post('/auth/login', { email, password });
      login(res.data.accessToken);
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.detail || 'Login failed');
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-b from-slate-800 to-slate-950">
      <div className="bg-slate-900 border border-slate-700 rounded-xl p-8 w-full max-w-md shadow-xl">
        <h2 className="text-2xl font-bold text-center mb-2 text-slate-200">Tekus Provider Services</h2>
        <p className="text-center text-sm text-slate-400 mb-6">Sign in to access the portal</p>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-slate-200 mb-1">Email</label>
            <input className="w-full bg-slate-900 border border-slate-700 text-slate-200 px-3 py-2 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600 focus:border-blue-600" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Email" />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-200 mb-1">Password</label>
            <input className="w-full bg-slate-900 border border-slate-700 text-slate-200 px-3 py-2 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600 focus:border-blue-600" type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Password" />
          </div>
          {error && <div className="text-sm text-red-500">{error}</div>}
          <button type="submit" className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2.5 rounded-lg font-medium transition-colors">Sign in</button>
        </form>
      </div>
    </div>
  );
}
