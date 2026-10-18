import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';

export interface AuthUser {
  email: string;
  name: string;
  role: string;
}

interface AuthContextType {
  accessToken: string | null;
  user: AuthUser | null;
  login: (token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

/**
 * Reads the claims out of a JWT payload. The API writes them with the long
 * `ClaimTypes.*` URIs, but depending on the token handler they may be shortened
 * to `email` / `role`, so every claim is looked up through a list of aliases.
 */
function readClaims(token: string): AuthUser | null {
  try {
    const payload = token.split('.')[1];
    const base = payload.replace(/-/g, '+').replace(/_/g, '/');
    const json = decodeURIComponent(
      atob(base)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join(''),
    );
    const claims: Record<string, unknown> = JSON.parse(json);

    const pick = (aliases: string[]) => {
      for (const key of aliases) {
        const hit = Object.keys(claims).find((k) => k === key || k.endsWith(`/${key}`));
        if (hit && typeof claims[hit] === 'string') return claims[hit] as string;
      }
      return '';
    };

    const email = pick(['email', 'emailaddress']);
    const name = pick(['full_name', 'name', 'unique_name']);
    const role = pick(['role', 'roles']);

    if (!email && !name) return null;
    return { email, name: name || email, role: role || 'User' };
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [accessToken, setAccessToken] = useState<string | null>(
    () => localStorage.getItem('accessToken'),
  );

  const user = useMemo(() => (accessToken ? readClaims(accessToken) : null), [accessToken]);

  const login = (token: string) => {
    localStorage.setItem('accessToken', token);
    setAccessToken(token);
  };

  const logout = () => {
    localStorage.removeItem('accessToken');
    setAccessToken(null);
  };

  return (
    <AuthContext.Provider value={{ accessToken, user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
