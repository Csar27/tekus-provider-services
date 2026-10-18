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
    <div className="login-container">
      <div className="login-card">
        <h2 className="login-title">Tekus Provider Services</h2>
        <p className="login-subtitle">Sign in to access the portal</p>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Email</label>
            <input className="form-input" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Email" />
          </div>
          <div className="form-group">
            <label className="form-label">Password</label>
            <input className="form-input" type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Password" />
          </div>
          {error && <div className="error-text">{error}</div>}
          <button type="submit" className="btn-full">Sign in</button>
        </form>
      </div>
    </div>
  );
}
