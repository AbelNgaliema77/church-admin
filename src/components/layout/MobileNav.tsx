import { mobileNavItems, PageKey } from '../../app/navigation';

type Props = {
  activePage: PageKey;
  onNavigate: (page: PageKey) => void;
};

export function MobileNav({ activePage, onNavigate }: Props) {
  return (
    <nav className="mobile-nav">
      {mobileNavItems.map((item) => {
        const Icon = item.icon;
        return (
          <button
            key={item.key}
            className={activePage === item.key ? 'active' : ''}
            onClick={() => onNavigate(item.key)}
          >
            <Icon size={20} />
            <span>{item.label}</span>
          </button>
        );
      })}
    </nav>
  );
}
