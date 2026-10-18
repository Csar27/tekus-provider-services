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
    <div style={{ padding: '1rem 2rem' }}>
      <nav style={{ display: 'flex', gap: 16, marginBottom: 24 }}>
        <Link to="/">Dashboard</Link>
        <Link to="/providers">Providers</Link>
        <Link to="/services">Services</Link>
        <Link to="/countries">Countries</Link>
      </nav>
      <Routes>
        <Route path="/" element={<DashboardPage />} />
        <Route path="/providers" element={<ProvidersPage />} />
        <Route path="/services" element={<ServicesPage />} />
        <Route path="/countries" element={<CountriesPage />} />
      </Routes>
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
