import {
  BarChart3,
  Boxes,
  CalendarCheck,
  FileClock,
  Home,
  LineChart,
  Settings,
  Shield,
  Users,
  WalletCards
} from 'lucide-react';

export type PageKey =
  | 'dashboard'
  | 'attendance'
  | 'workers'
  | 'teams'
  | 'finance'
  | 'inventory'
  | 'reports'
  | 'auditLogs'
  | 'users'
  | 'settings';

export const navItems = [
  { key: 'dashboard' as PageKey, label: 'Dashboard', icon: Home },
  { key: 'attendance' as PageKey, label: 'Attendance', icon: CalendarCheck },
  { key: 'workers' as PageKey, label: 'Workers', icon: Users },
  { key: 'teams' as PageKey, label: 'Teams', icon: BarChart3 },
  { key: 'finance' as PageKey, label: 'Finance', icon: WalletCards },
  { key: 'inventory' as PageKey, label: 'Inventory', icon: Boxes },
  { key: 'reports' as PageKey, label: 'Reports', icon: LineChart },
  { key: 'auditLogs' as PageKey, label: 'Audit Logs', icon: FileClock },
  { key: 'users' as PageKey, label: 'Users & Roles', icon: Shield },
  { key: 'settings' as PageKey, label: 'Settings', icon: Settings }
];

export const mobileNavItems = navItems.filter(
  (item) =>
    item.key !== 'settings' &&
    item.key !== 'teams' &&
    item.key !== 'auditLogs' &&
    item.key !== 'reports' &&
    item.key !== 'users'
);