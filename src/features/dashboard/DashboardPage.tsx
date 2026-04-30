import { useEffect, useState } from 'react';
import { getDashboardSummary } from './dashboardApi';
import { getStoredAuth } from '../auth/authStorage';
import { Loader } from '../../components/ui/Loader';
import { ErrorBanner } from '../../components/ui/ErrorBanner';

type DashboardSummary = {
  activeWorkers: number;
  activeTeams: number;
  inventoryItems: number;
  pendingInventoryItems: number;
  attendanceThisMonth: number;
  financeThisMonth: number;
};

type StatCard = {
  title: string;
  value: string | number;
  subtitle?: string;
};

export function DashboardPage() {
  const [data, setData] = useState<DashboardSummary | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      const auth = getStoredAuth();
      if (!auth) return;

      try {
        setIsLoading(true);
        setError(null);
        const result = await getDashboardSummary(auth.token);
        if (!cancelled) setData(result);
      } catch (err) {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Failed to load dashboard.');
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    }

    load();
    return () => { cancelled = true; };
  }, []);

  if (isLoading) return <Loader message="Loading dashboard..." />;
  if (error) return <ErrorBanner message={error} />;
  if (!data) return null;

  const stats: StatCard[] = [
    { title: 'Active Workers', value: data.activeWorkers, subtitle: 'Serving members' },
    { title: 'Active Teams', value: data.activeTeams, subtitle: 'Ministry teams' },
    { title: 'Inventory Items', value: data.inventoryItems, subtitle: 'Total assets' },
    { title: 'Pending Inventory', value: data.pendingInventoryItems, subtitle: 'Requires attention' },
    { title: 'Attendance This Month', value: data.attendanceThisMonth, subtitle: 'Total count' },
    {
      title: 'Finance This Month',
      value: `R ${data.financeThisMonth.toLocaleString()}`,
      subtitle: 'Total collected'
    },
  ];

  return (
    <>
      <h1 className="page-title">Dashboard</h1>
      <div className="dashboard-grid">
        {stats.map((stat) => (
          <StatCard key={stat.title} title={stat.title} value={stat.value} subtitle={stat.subtitle} />
        ))}
      </div>
    </>
  );
}

function StatCard({ title, value, subtitle }: StatCard) {
  return (
    <div className="card">
      <div className="card-title" style={{ fontSize: 13, fontWeight: 700, color: '#94a3b8', marginBottom: 10 }}>
        {title}
      </div>
      <div className="card-value">{value}</div>
      {subtitle && (
        <div style={{ marginTop: 6, fontSize: 12, color: '#64748b' }}>{subtitle}</div>
      )}
    </div>
  );
}
