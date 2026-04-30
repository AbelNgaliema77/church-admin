import { useEffect, useMemo, useState } from 'react';
import { ErrorBanner } from '../../components/ui/ErrorBanner';
import { ChurchBranding, LoginState } from '../../types/api';
import { storeAuth } from './authStorage';
import { getChurchBranding, login } from './authApi';

type LoginPageProps = {
  churchSlug: string;
  onLogin: (auth: LoginState) => void;
};

function getInitials(name: string) {
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('') || 'CA';
}

export function LoginPage({ churchSlug, onLogin }: LoginPageProps) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [branding, setBranding] = useState<ChurchBranding | null>(null);
  const [pageError, setPageError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isBrandingLoading, setIsBrandingLoading] = useState(true);

  const churchName = branding?.name ?? 'Church Admin';
  const welcomeText = branding?.welcomeText ?? 'Sign in to your church admin portal.';
  const initials = useMemo(() => getInitials(churchName), [churchName]);

  useEffect(() => {
    let cancelled = false;

    async function loadBranding() {
      try {
        setIsBrandingLoading(true);
        setPageError(null);

        const result = await getChurchBranding(churchSlug);

        if (!cancelled) {
          setBranding(result);
        }
      } catch (error) {
        if (!cancelled) {
          setBranding(null);
          setPageError(
            error instanceof Error
              ? error.message
              : 'Church portal could not be loaded.'
          );
        }
      } finally {
        if (!cancelled) {
          setIsBrandingLoading(false);
        }
      }
    }

    loadBranding();

    return () => {
      cancelled = true;
    };
  }, [churchSlug]);

  async function submit() {
    if (!email.trim()) {
      setPageError('Email is required.');
      return;
    }

    if (!password.trim()) {
      setPageError('Password is required.');
      return;
    }

    try {
      setIsLoading(true);
      setPageError(null);

      const result = await login({
        churchSlug,
        email: email.trim(),
        password
      });

      storeAuth(result);
      onLogin(result);
    } catch (error) {
      setPageError(error instanceof Error ? error.message : 'Login failed.');
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <div className="login-page">
      <div className="login-card">
        {branding?.logoUrl ? (
          <img className="login-logo-image" src={branding.logoUrl} alt={`${churchName} logo`} />
        ) : (
          <div className="logo-mark">{initials}</div>
        )}

        <h1>{isBrandingLoading ? 'Loading portal...' : churchName}</h1>
        <p>{welcomeText}</p>

        <ErrorBanner message={pageError} />

        <div className="field">
          <label>Email</label>
          <input
            value={email}
            disabled={isLoading || isBrandingLoading}
            onChange={(event) => setEmail(event.target.value)}
            placeholder="you@church.org"
          />
        </div>

        <div className="field" style={{ marginTop: 14 }}>
          <label>Password</label>
          <input
            type="password"
            value={password}
            disabled={isLoading || isBrandingLoading}
            onChange={(event) => setPassword(event.target.value)}
            placeholder="Enter your password"
            onKeyDown={(event) => {
              if (event.key === 'Enter') {
                submit();
              }
            }}
          />
        </div>

        <button
          className="primary-btn"
          style={{ width: '100%' }}
          onClick={submit}
          disabled={isLoading || isBrandingLoading}
        >
          {isLoading ? 'Signing in...' : 'Sign in'}
        </button>
      </div>
    </div>
  );
}
