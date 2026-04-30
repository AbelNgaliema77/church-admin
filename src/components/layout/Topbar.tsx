import { Bell, LogOut } from 'lucide-react';
import { LoginState } from '../../types/api';

type Props = {
  user: LoginState['user'];
  onLogout: () => void;
};

export function Topbar({ user, onLogout }: Props) {
  return (
    <header className="topbar">
      <div>
        <div style={{ display: 'flex', gap: 10, alignItems: 'center', flexWrap: 'wrap' }}>
          <strong>Hello! {user.displayName}</strong>
          <span className="role-pill">{user.role}</span>
        </div>
        <div style={{ fontSize: 12, color: '#73798c', marginTop: 4 }}>
          {user.email}
        </div>
      </div>

      <div style={{ display: 'flex', gap: 14, alignItems: 'center' }}>
        <Bell size={20} />

        <button className="secondary-btn" onClick={onLogout}>
          <LogOut size={16} />
          Logout
        </button>
      </div>
    </header>
  );
}