import {
  BrowserRouter,
  NavLink,
  Outlet,
  Routes,
  Route,
  Navigate,
  useNavigate,
} from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider, useAuth } from './auth/authContext';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { ProvidersPage } from './pages/ProvidersPage';
import { ServicesPage } from './pages/ServicesPage';
import { CountriesPage } from './pages/CountriesPage';
import { initials } from './lib/format';

const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: 1, refetchOnWindowFocus: false } },
});

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { accessToken } = useAuth();
  if (!accessToken) return <Navigate to="/login" replace />;
  return <>{children}</>;
}

const icons = {
  overview: (
    <svg width="16" height="16" viewBox="0 0 16 16" fill="none" aria-hidden="true">
      <rect x="1.5" y="1.5" width="5.5" height="5.5" rx="1" stroke="currentColor" strokeWidth="1.3" />
      <rect x="9" y="1.5" width="5.5" height="5.5" rx="1" stroke="currentColor" strokeWidth="1.3" />
      <rect x="1.5" y="9" width="5.5" height="5.5" rx="1" stroke="currentColor" strokeWidth="1.3" />
      <rect x="9" y="9" width="5.5" height="5.5" rx="1" stroke="currentColor" strokeWidth="1.3" />
    </svg>
  ),
  providers: (
    <svg width="16" height="16" viewBox="0 0 16 16" fill="none" aria-hidden="true">
      <path
        d="M2 13.5V4.2a.7.7 0 0 1 .7-.7h4.1v10H2ZM8.2 13.5v-6h5.1a.7.7 0 0 1 .7.7v5.3H8.2ZM8.2 6.4h5.1"
        stroke="currentColor"
        strokeWidth="1.3"
        strokeLinejoin="round"
      />
    </svg>
  ),
  services: (
    <svg width="16" height="16" viewBox="0 0 16 16" fill="none" aria-hidden="true">
      <path
        d="M8 1.6 14 5v6l-6 3.4L2 11V5l6-3.4Z"
        stroke="currentColor"
        strokeWidth="1.3"
        strokeLinejoin="round"
      />
      <path d="M2 5l6 3.4L14 5M8 8.4V14.4" stroke="currentColor" strokeWidth="1.3" />
    </svg>
  ),
  coverage: (
    <svg width="16" height="16" viewBox="0 0 16 16" fill="none" aria-hidden="true">
      <circle cx="8" cy="8" r="6.3" stroke="currentColor" strokeWidth="1.3" />
      <path d="M1.7 8h12.6M8 1.7c1.7 1.8 2.6 4 2.6 6.3S9.7 12.5 8 14.3C6.3 12.5 5.4 10.3 5.4 8s.9-4.5 2.6-6.3Z" stroke="currentColor" strokeWidth="1.3" />
    </svg>
  ),
};

const nav = [
  { to: '/', label: 'Overview', icon: icons.overview, end: true },
  { to: '/providers', label: 'Providers', icon: icons.providers },
  { to: '/services', label: 'Services', icon: icons.services },
  { to: '/countries', label: 'Coverage', icon: icons.coverage },
];

function Sidebar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

  return (
    <aside className="sidebar">
      <div className="sidebar__brand">
        <div className="sidebar__mark" aria-hidden="true">
          T
        </div>
        <div>
          <div className="sidebar__name">Tekus</div>
          <div className="sidebar__tag">Provider Services</div>
        </div>
      </div>

      <nav className="sidebar__nav" aria-label="Main">
        {nav.map((item) => (
          <NavLink key={item.to} to={item.to} end={item.end} className="navlink">
            {item.icon}
            {item.label}
          </NavLink>
        ))}
      </nav>

      <div className="sidebar__spacer" />

      <div className="sidebar__foot">
        <div className="userchip">
          <div className="userchip__avatar" aria-hidden="true">
            {initials(user?.name)}
          </div>
          <div style={{ minWidth: 0 }}>
            <div className="userchip__name">{user?.name ?? 'Signed in'}</div>
            <div className="userchip__meta">
              {user?.role}
              {user?.email ? ` · ${user.email}` : ''}
            </div>
          </div>
        </div>
        <button className="signout" onClick={handleLogout}>
          Sign out
        </button>
      </div>
    </aside>
  );
}

function Layout() {
  return (
    <div>
      <Sidebar />
      <main className="main">
        <div className="main__inner">
          <Outlet />
        </div>
      </main>
    </div>
  );
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/*"
        element={
          <ProtectedRoute>
            <Layout />
          </ProtectedRoute>
        }
      >
        <Route index element={<DashboardPage />} />
        <Route path="providers" element={<ProvidersPage />} />
        <Route path="services" element={<ServicesPage />} />
        <Route path="countries" element={<CountriesPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <BrowserRouter>
          <AppRoutes />
        </BrowserRouter>
      </AuthProvider>
    </QueryClientProvider>
  );
}
