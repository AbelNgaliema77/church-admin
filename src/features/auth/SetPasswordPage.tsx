import { useEffect, useMemo, useState } from 'react';
import { ErrorBanner } from '../../components/ui/ErrorBanner';
import { SuccessBanner } from '../../components/ui/SuccessBanner';
import { ChurchBranding, LoginState } from '../../types/api';
import { storeAuth } from './authStorage';
import { getChurchBranding, setPassword } from './authApi';

type SetPasswordPageProps = {
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

export function SetPasswordPage({ churchSlug, onLogin }: SetPasswordPageProps) {
  const inviteToken = useMemo(() => {
    const params = new URLSearchParams(window.location.search);
    return params.get('token') ?? '';
  }, []);

  const [branding, setBranding] = useState<ChurchBranding | null>(null);
  const [password, setPasswordValue] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [pageError, setPageError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);
  const [isBrandingLoading, setIsBrandingLoading] = useState(true);

  const churchName = branding?.name ?? 'Church Admin';
  const initials = useMemo(() => getInitials(churchName), [churchName]);

  useEffect(() => {
    let cancelled = false;

    async function loadBranding() {
      try {
        setIsBrandingLoading(true);
        const result = await getChurchBranding(churchSlug);

        if (!cancelled) {
          setBranding(result);
        }
      } catch {
        if (!cancelled) {
          setBranding(null);
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
    if (!inviteToken) {
      setPageError('Invite token is missing.');
      return;
    }

    if (password.length < 8) {
      setPageError('Password must be at least 8 characters.');
      return;
    }

    if (password !== confirmPassword) {
      setPageError('Passwords do not match.');
      return;
    }

    try {
      setIsSaving(true);
      setPageError(null);
      setSuccessMessage(null);

      const result = await setPassword({
        token: inviteToken,
        password,
        confirmPassword
      });

      storeAuth(result);
      setSuccessMessage('Password set successfully. Redirecting...');
      window.history.replaceState({}, '', `/${churchSlug}`);

      onLogin(result);
    } catch (error) {
      setPageError(error instanceof Error ? error.message : 'Failed to set password.');
    } finally {
      setIsSaving(false);
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

        <h1>Set Password</h1>
        <p>Create your password for {isBrandingLoading ? 'your church portal' : churchName}.</p>

        <ErrorBanner message={pageError} />
        <SuccessBanner message={successMessage} />

        <div className="field">
          <label>Password</label>
          <input
            type="password"
            value={password}
            disabled={isSaving}
            onChange={(event) => setPasswordValue(event.target.value)}
            placeholder="Minimum 8 characters"
          />
        </div>

        <div className="field" style={{ marginTop: 14 }}>
          <label>Confirm Password</label>
          <input
            type="password"
            value={confirmPassword}
            disabled={isSaving}
            onChange={(event) => setConfirmPassword(event.target.value)}
            placeholder="Confirm password"
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
          disabled={isSaving}
        >
          {isSaving ? 'Saving password...' : 'Set Password'}
        </button>
      </div>
    </div>
  );
}
