import { navItems, PageKey } from '../../app/navigation';
import { getStoredAuth } from '../../features/auth/authStorage';

type Props = {
  activePage: PageKey;
  onNavigate: (page: PageKey) => void;
};

function getInitials(name: string): string {
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? '')
    .join('') || 'CA';
}

export function Sidebar({ activePage, onNavigate }: Props) {
  const auth = getStoredAuth();
  const churchName = auth?.user.churchName ?? 'Church Admin';
  const initials = getInitials(churchName);

  return (
    <aside className="sidebar">
      <div className="logo">
        <div className="logo-mark">{initials}</div>
        <div className="logo-title">
          {churchName}
          <span>Admin Portal</span>
        </div>
      </div>

      <nav>
        {navItems.map((item) => {
          const Icon = item.icon;
          return (
            <button
              key={item.key}
              className={`nav-button ${activePage === item.key ? 'active' : ''}`}
              onClick={() => onNavigate(item.key)}
            >
              <Icon size={18} />
              <span>{item.label}</span>
            </button>
          );
        })}
      </nav>
    </aside>
  );
}
