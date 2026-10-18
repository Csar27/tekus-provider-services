import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../lib/api';
import { useAuth } from '../auth/authContext';
import { Button } from '../components/ui';

export function LoginPage() {
  const [email, setEmail] = useState('admin@tekus.co');
  const [password, setPassword] = useState('Tekus.2026!');
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setBusy(true);
    try {
      const res = await api.post('/auth/login', { email, password });
      login(res.data.accessToken);
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.detail || 'We could not sign you in. Check your credentials.');
    } finally {
      setBusy(false);
    }
  };

  return (
    <div style={{ display: 'flex', minHeight: '100vh' }}>
      {/* Brand panel */}
      <div
        style={{
          flex: '1 1 46%',
          background: 'var(--color-navy-deep)',
          color: '#e8edf4',
          padding: '3rem',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'space-between',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.7rem' }}>
          <div className="sidebar__mark" aria-hidden="true">
            T
          </div>
          <div>
            <div className="sidebar__name">Tekus</div>
            <div className="sidebar__tag">Provider Services</div>
          </div>
        </div>

        <div style={{ maxWidth: '34ch' }}>
          <div className="pagehead__rule" aria-hidden="true" style={{ marginBottom: '1.4rem' }} />
          <h1
            style={{
              fontFamily: 'var(--font-display)',
              fontSize: '2.6rem',
              fontWeight: 600,
              lineHeight: 1.1,
              letterSpacing: '-0.015em',
              color: '#fff',
              margin: 0,
            }}
          >
            The provider catalogue, in one register.
          </h1>
          <p style={{ marginTop: '1rem', fontSize: '15px', color: 'rgba(232,237,244,.62)', lineHeight: 1.6 }}>
            Administrate suppliers, their service portfolio and the countries each service is
            offered in.
          </p>
        </div>

        <p style={{ fontSize: '12.5px', color: 'rgba(232,237,244,.4)', margin: 0 }}>
          Internal use only · Access is logged
        </p>
      </div>

      {/* Form panel */}
      <div
        style={{
          flex: '1 1 54%',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          padding: '2rem',
        }}
      >
        <form
          onSubmit={handleSubmit}
          style={{ width: '100%', maxWidth: '360px' }}
          aria-label="Sign in"
        >
          <div className="pagehead__rule" aria-hidden="true" />
          <h2
            style={{
              fontFamily: 'var(--font-display)',
              fontSize: '28px',
              fontWeight: 600,
              margin: '0.6rem 0 0.25rem',
            }}
          >
            Sign in
          </h2>
          <p style={{ color: 'var(--color-ink-soft)', fontSize: '14px', margin: '0 0 1.75rem' }}>
            Use your Tekus account to continue.
          </p>

          <div style={{ marginBottom: '1rem' }}>
            <label className="field__label" htmlFor="email">
              Email
            </label>
            <input
              id="email"
              className="input"
              type="email"
              autoComplete="username"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div style={{ marginBottom: '1.1rem' }}>
            <label className="field__label" htmlFor="password">
              Password
            </label>
            <input
              id="password"
              className="input"
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          {error && (
            <div className="form-error" role="alert" style={{ marginBottom: '1rem' }}>
              {error}
            </div>
          )}

          <Button type="submit" disabled={busy} style={{ width: '100%', padding: '0.65rem' }}>
            {busy ? 'Signing in…' : 'Sign in'}
          </Button>
        </form>
      </div>
    </div>
  );
}
