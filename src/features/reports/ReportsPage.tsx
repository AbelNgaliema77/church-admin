import { useEffect, useState } from 'react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis
} from 'recharts';
import { Card } from '../../components/ui/Card';
import { Loader } from '../../components/ui/Loader';
import { ErrorBanner } from '../../components/ui/ErrorBanner';
import { getStoredAuth } from '../auth/authStorage';
import { ServiceType } from '../attendance/attendanceApi';
import {
  AttendanceServiceTypeSummary,
  AttendanceTrend,
  FinanceCategorySummary,
  FinanceTrend,
  getAttendanceByServiceType,
  getAttendanceTrends,
  getFinanceByCategory,
  getFinanceTrends,
  ReportFilters
} from './reportsApi';

const serviceTypeLabels: Record<ServiceType, string> = {
  1: 'Sunday',
  2: 'Friday',
  3: 'Special',
  4: 'Prayer',
  5: 'Conference'
};

const financeCategoryLabels: Record<number, string> = {
  1: 'Tithe',
  2: 'Thanksgiving',
  3: 'Normal Offering',
  4: 'Special Offering',
  5: 'Building Fund',
  6: 'Other'
};

function getDefaultFromDate() {
  const date = new Date();
  date.setMonth(date.getMonth() - 6);
  return date.toISOString().slice(0, 10);
}

function getToday() {
  return new Date().toISOString().slice(0, 10);
}

export function ReportsPage() {
  const [filters, setFilters] = useState<ReportFilters>({
    from: getDefaultFromDate(),
    to: getToday(),
    serviceType: ''
  });

  const [attendanceTrends, setAttendanceTrends] = useState<AttendanceTrend[]>([]);
  const [attendanceByService, setAttendanceByService] = useState<
    AttendanceServiceTypeSummary[]
  >([]);
  const [financeTrends, setFinanceTrends] = useState<FinanceTrend[]>([]);
  const [financeByCategory, setFinanceByCategory] = useState<
    FinanceCategorySummary[]
  >([]);

  const [pageError, setPageError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  async function loadReports(nextFilters = filters) {
    const auth = getStoredAuth();

    if (!auth) {
      setPageError('You are not logged in.');
      setIsLoading(false);
      return;
    }

    try {
      setPageError(null);
      setIsLoading(true);

      const [
        attendanceTrendResult,
        attendanceByServiceResult,
        financeTrendResult,
        financeByCategoryResult
      ] = await Promise.all([
        getAttendanceTrends(auth.token, nextFilters),
        getAttendanceByServiceType(auth.token, nextFilters),
        getFinanceTrends(auth.token, nextFilters),
        getFinanceByCategory(auth.token, nextFilters)
      ]);

      setAttendanceTrends(attendanceTrendResult);
      setAttendanceByService(attendanceByServiceResult);
      setFinanceTrends(financeTrendResult);
      setFinanceByCategory(financeByCategoryResult);
    } catch (error) {
      setPageError(error instanceof Error ? error.message : 'Failed to load reports');
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadReports();
  }, []);

  const totalAttendance = attendanceTrends.reduce(
    (total, item) => total + item.total,
    0
  );

  const totalFinance = financeTrends.reduce(
    (total, item) => total + item.totalAmount,
    0
  );

  return (
    <>
      <h1 className="page-title">Reports</h1>

      <ErrorBanner message={pageError} />

      <Card title="Report Filters">
        <div className="form-grid">
          <input
            type="date"
            value={filters.from}
            onChange={(e) => setFilters({ ...filters, from: e.target.value })}
          />

          <input
            type="date"
            value={filters.to}
            onChange={(e) => setFilters({ ...filters, to: e.target.value })}
          />

          <select
            value={filters.serviceType ?? ''}
            onChange={(e) =>
              setFilters({
                ...filters,
                serviceType: e.target.value
                  ? (Number(e.target.value) as ServiceType)
                  : ''
              })
            }
          >
            <option value="">All services</option>
            <option value={1}>Sunday</option>
            <option value={2}>Friday</option>
            <option value={3}>Special</option>
            <option value={4}>Prayer</option>
            <option value={5}>Conference</option>
          </select>
        </div>

        <div style={{ marginTop: 16 }}>
          <button className="primary-btn" onClick={() => loadReports(filters)}>
            Load Reports
          </button>
        </div>
      </Card>

      <div style={{ height: 18 }} />

      {isLoading ? (
        <Loader message="Loading reports..." />
      ) : (
        <>
          <div className="dashboard-grid">
            <Card title="Total Attendance">
              <div className="card-value">{totalAttendance}</div>
            </Card>

            <Card title="Total Finance">
              <div className="card-value">
                {totalFinance.toLocaleString('en-ZA', {
                  style: 'currency',
                  currency: 'ZAR'
                })}
              </div>
            </Card>
          </div>

          <div style={{ height: 18 }} />

          <Card title="Attendance Trend">
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={attendanceTrends}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="monthName" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Line dataKey="total" />
              </LineChart>
            </ResponsiveContainer>
          </Card>

          <div style={{ height: 18 }} />

          <Card title="Finance Trend">
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={financeTrends}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="monthName" />
                <YAxis />
                <Tooltip />
                <Bar dataKey="totalAmount" />
              </BarChart>
            </ResponsiveContainer>
          </Card>
        </>
      )}
    </>
  );
}