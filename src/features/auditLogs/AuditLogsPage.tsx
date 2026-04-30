import { useEffect, useState } from 'react';
import { Card } from '../../components/ui/Card';
import { ErrorBanner } from '../../components/ui/ErrorBanner';
import { Loader } from '../../components/ui/Loader';
import { useSortableData } from '../../hooks/useSortableData';
import { getStoredAuth } from '../auth/authStorage';
import { AuditAction, AuditLog, AuditLogFilters, getAuditLogs } from './auditLogsApi';

const actionLabels: Record<AuditAction, string> = {
  1: 'Created',
  2: 'Updated',
  3: 'Deleted',
  10: 'Activated',
  11: 'Deactivated',
  20: 'Assigned',
  21: 'Removed',
  30: 'Approved',
  31: 'Rejected',
  32: 'Verified',
  33: 'Retired',
  40: 'Corrected',
  41: 'Reconciled',
  50: 'Logged In',
  51: 'Logged Out'
};

export function AuditLogsPage() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [filters, setFilters] = useState<AuditLogFilters>({
    entityName: '',
    entityId: ''
  });

  const [selectedLog, setSelectedLog] = useState<AuditLog | null>(null);
  const [pageError, setPageError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const { sortedItems, requestSort, getSortLabel } = useSortableData(logs);

  async function loadLogs(nextFilters = filters) {
    const auth = getStoredAuth();

    if (!auth) {
      setPageError('You are not logged in.');
      setIsLoading(false);
      return;
    }

    try {
      setPageError(null);
      setIsLoading(true);

      const result = await getAuditLogs(auth.token, nextFilters);
      setLogs(result);
    } catch (error) {
      setPageError(error instanceof Error ? error.message : 'Failed to load audit logs');
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadLogs();
  }, []);

  async function applyFilters() {
    await loadLogs(filters);
  }

  async function clearFilters() {
    const clearedFilters = {
      entityName: '',
      entityId: ''
    };

    setFilters(clearedFilters);
    await loadLogs(clearedFilters);
  }

  return (
    <>
      <h1 className="page-title">Audit Logs</h1>

      <ErrorBanner message={pageError} />

      <Card title="Audit Filters">
        <div className="form-grid">
          <div className="field">
            <label>Entity Name</label>
            <input
              placeholder="Example: InventoryItem"
              value={filters.entityName ?? ''}
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  entityName: event.target.value
                }))
              }
            />
          </div>

          <div className="field">
            <label>Entity Id</label>
            <input
              placeholder="Optional GUID"
              value={filters.entityId ?? ''}
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  entityId: event.target.value
                }))
              }
            />
          </div>
        </div>

        <div style={{ display: 'flex', gap: 10, marginTop: 16 }}>
          <button className="primary-btn" onClick={applyFilters}>
            Apply Filters
          </button>
          <button className="secondary-btn" onClick={clearFilters}>
            Clear
          </button>
        </div>
      </Card>

      <div style={{ height: 18 }} />

      <Card title="Latest Audit Logs">
        {isLoading ? (
          <Loader message="Loading audit logs..." />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('createdAt')}>
                      Date{getSortLabel('createdAt')}
                    </button>
                  </th>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('entityName')}>
                      Entity{getSortLabel('entityName')}
                    </button>
                  </th>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('action')}>
                      Action{getSortLabel('action')}
                    </button>
                  </th>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('createdBy')}>
                      Created By{getSortLabel('createdBy')}
                    </button>
                  </th>
                  <th>Reason</th>
                  <th>Details</th>
                </tr>
              </thead>

              <tbody>
                {sortedItems.map((log) => (
                  <tr key={log.id}>
                    <td>{new Date(log.createdAt).toLocaleString()}</td>
                    <td>
                      <strong>{log.entityName}</strong>
                      <div style={{ color: '#73798c', marginTop: 4 }}>
                        {log.entityId}
                      </div>
                    </td>
                    <td>{actionLabels[log.action] ?? log.action}</td>
                    <td>{log.createdBy}</td>
                    <td>{log.reason ?? '-'}</td>
                    <td>
                      <button
                        className="secondary-btn"
                        onClick={() => setSelectedLog(log)}
                      >
                        View
                      </button>
                    </td>
                  </tr>
                ))}

                {logs.length === 0 && (
                  <tr>
                    <td colSpan={6}>No audit logs found.</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {selectedLog && (
        <div className="modal-backdrop">
          <div className="confirm-modal" style={{ width: 'min(900px, 100%)' }}>
            <h2>Audit Log Details</h2>

            <p>
              <strong>{selectedLog.entityName}</strong> —{' '}
              {actionLabels[selectedLog.action] ?? selectedLog.action}
            </p>

            <div className="form-grid">
              <div className="field">
                <label>Before</label>
                <textarea
                  rows={12}
                  readOnly
                  value={formatJson(selectedLog.beforeJson)}
                />
              </div>

              <div className="field">
                <label>After</label>
                <textarea
                  rows={12}
                  readOnly
                  value={formatJson(selectedLog.afterJson)}
                />
              </div>
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: 16 }}>
              <button className="primary-btn" onClick={() => setSelectedLog(null)}>
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

function formatJson(value?: string | null) {
  if (!value) return '';

  try {
    return JSON.stringify(JSON.parse(value), null, 2);
  } catch {
    return value;
  }
}