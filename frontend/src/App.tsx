import { BrowserRouter, Routes, Route, Navigate, Link } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider, useAuth } from './auth/authContext';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { ProvidersPage } from './pages/ProvidersPage';
import { ServicesPage } from './pages/ServicesPage';
import { CountriesPage } from './pages/CountriesPage';

const queryClient = new QueryClient();

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { accessToken } = useAuth();
  if (!accessToken) return <Navigate to="/login" replace />;
  return <>{children}</>;
}

function Layout() {
  return (
    <div className="layout">
      <nav className="navbar">
        <div className="navbar-brand">Tekus</div>
        <div className="navbar-links">
          <Link className="navbar-link" to="/">Dashboard</Link>
          <Link className="navbar-link" to="/providers">Providers</Link>
          <Link className="navbar-link" to="/services">Services</Link>
          <Link className="navbar-link" to="/countries">Countries</Link>
        </div>
      </nav>
      <div className="content">
        <Routes>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/providers" element={<ProvidersPage />} />
          <Route path="/services" element={<ServicesPage />} />
          <Route path="/countries" element={<CountriesPage />} />
        </Routes>
      </div>
    </div>
  );
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/*" element={<ProtectedRoute><Layout /></ProtectedRoute>} />
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
