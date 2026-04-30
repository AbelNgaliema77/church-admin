import { useEffect, useState } from 'react';
import { Card } from '../../components/ui/Card';
import { Loader } from '../../components/ui/Loader';
import { ErrorBanner } from '../../components/ui/ErrorBanner';
import { SuccessBanner } from '../../components/ui/SuccessBanner';
import { EmptyState } from '../../components/ui/EmptyState';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { getStoredAuth } from '../auth/authStorage';
import { getTeams } from '../teams/teamsApi';
import { Team } from '../../types/domain';
import {
  createWorker,
  deleteWorker,
  getWorkers,
  updateWorker,
  Worker,
} from './workersApi';

type WorkerForm = {
  fullName: string;
  email: string;
  phone: string;
  address: string;
  dateOfBirth: string;
  startedServing: string;
  baptized: boolean;
  teamId: string;
  roleInTeam: number;
  status: 'Active' | 'Inactive';
};

const emptyForm: WorkerForm = {
  fullName: '',
  email: '',
  phone: '',
  address: '',
  dateOfBirth: '',
  startedServing: '',
  baptized: false,
  teamId: '',
  roleInTeam: 0,
  status: 'Active',
};

const roleInTeamLabels: Record<number, string> = {
  0: 'Member',
  1: 'Leader',
  2: 'Deputy Leader',
};

export function WorkersPage() {
  const [workers, setWorkers] = useState<Worker[]>([]);
  const [teams, setTeams] = useState<Team[]>([]);
  const [form, setForm] = useState<WorkerForm>(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [pageError, setPageError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  async function load() {
    const auth = getStoredAuth();
    if (!auth) return;

    try {
      setIsLoading(true);
      setPageError(null);
      const [workersData, teamsData] = await Promise.all([
        getWorkers(auth.token),
        getTeams(auth.token),
      ]);
      setWorkers(workersData);
      setTeams(teamsData);
    } catch (err) {
      setPageError(err instanceof Error ? err.message : 'Failed to load workers.');
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => { load(); }, []);

  function validate() {
    const errors: Record<string, string> = {};
    if (!form.fullName.trim()) errors.fullName = 'Full name is required.';
    if (!form.email.trim()) errors.email = 'Email address is required.';
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email.trim())) errors.email = 'Enter a valid email address.';
    if (!form.phone.trim()) errors.phone = 'Phone number is required.';
    if (!form.address.trim()) errors.address = 'Address is required.';
    if (!form.dateOfBirth) errors.dateOfBirth = 'Date of birth is required.';
    if (!form.startedServing) errors.startedServing = 'Service start date is required.';
    if (!editingId && !form.teamId) errors.teamId = 'Please select a team.';
    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  }

  async function save() {
    if (!validate()) return;
    const auth = getStoredAuth();
    if (!auth) return;

    try {
      setIsSaving(true);
      setPageError(null);
      setSuccessMessage(null);

      if (editingId) {
        await updateWorker(auth.token, editingId, {
          fullName: form.fullName.trim(),
          email: form.email.trim(),
          phone: form.phone.trim(),
          address: form.address.trim(),
          dateOfBirth: form.dateOfBirth,
          startedServing: form.startedServing,
          baptized: form.baptized,
          status: form.status,
        });
        setSuccessMessage('Worker updated successfully.');
      } else {
        await createWorker(auth.token, {
          fullName: form.fullName.trim(),
          email: form.email.trim(),
          phone: form.phone.trim(),
          address: form.address.trim(),
          dateOfBirth: form.dateOfBirth,
          startedServing: form.startedServing,
          baptized: form.baptized,
          teamId: form.teamId,
          roleInTeam: form.roleInTeam,
        });
        setSuccessMessage('Worker created successfully.');
      }

      setForm(emptyForm);
      setEditingId(null);
      setFieldErrors({});
      await load();
    } catch (err) {
      setPageError(err instanceof Error ? err.message : 'Failed to save worker.');
    } finally {
      setIsSaving(false);
    }
  }

  function edit(worker: Worker) {
    setEditingId(worker.id);
    setForm({
      fullName: worker.fullName,
      email: worker.email,
      phone: worker.phone,
      address: worker.address,
      dateOfBirth: worker.dateOfBirth,
      startedServing: worker.startedServing,
      baptized: worker.baptized,
      teamId: worker.teams[0]?.teamId ?? '',
      roleInTeam: 0,
      status: worker.status,
    });
    setFieldErrors({});
    setPageError(null);
    setSuccessMessage(null);
  }

  function cancelEdit() {
    setEditingId(null);
    setForm(emptyForm);
    setFieldErrors({});
  }

  async function confirmDelete() {
    if (!deleteId) return;
    const auth = getStoredAuth();
    if (!auth) return;

    try {
      setPageError(null);
      await deleteWorker(auth.token, deleteId);
      setSuccessMessage('Worker removed successfully.');
      await load();
    } catch (err) {
      setPageError(err instanceof Error ? err.message : 'Failed to remove worker.');
    } finally {
      setDeleteId(null);
    }
  }

  function field(key: keyof WorkerForm, label: string, content: React.ReactNode) {
    return (
      <div className="field">
        <label>{label}</label>
        {content}
        {fieldErrors[key] && <div className="field-error">{fieldErrors[key]}</div>}
      </div>
    );
  }

  return (
    <>
      <h1 className="page-title">Workers</h1>

      <ErrorBanner message={pageError} />
      <SuccessBanner message={successMessage} />

      <Card title={editingId ? 'Edit Worker' : 'Add Worker'}>
        <div className="form-grid">
          {field('fullName', 'Full Name',
            <input
              value={form.fullName}
              disabled={isSaving}
              onChange={(e) => setForm((f) => ({ ...f, fullName: e.target.value }))}
            />
          )}
          {field('email', 'Email Address',
            <input
              type="email"
              value={form.email}
              disabled={isSaving || !!editingId}
              onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
            />
          )}
          {field('phone', 'Phone Number',
            <input
              value={form.phone}
              disabled={isSaving}
              onChange={(e) => setForm((f) => ({ ...f, phone: e.target.value }))}
            />
          )}
          {field('address', 'Home Address',
            <input
              value={form.address}
              disabled={isSaving}
              onChange={(e) => setForm((f) => ({ ...f, address: e.target.value }))}
            />
          )}
          {field('dateOfBirth', 'Date of Birth',
            <input
              type="date"
              value={form.dateOfBirth}
              disabled={isSaving}
              onChange={(e) => setForm((f) => ({ ...f, dateOfBirth: e.target.value }))}
            />
          )}
          {field('startedServing', 'Started Serving',
            <input
              type="date"
              value={form.startedServing}
              disabled={isSaving}
              onChange={(e) => setForm((f) => ({ ...f, startedServing: e.target.value }))}
            />
          )}

          {!editingId && field('teamId', 'Assign to Team',
            <select
              value={form.teamId}
              disabled={isSaving}
              onChange={(e) => setForm((f) => ({ ...f, teamId: e.target.value }))}
            >
              <option value="">— Select a team —</option>
              {teams.map((t) => (
                <option key={t.id} value={t.id}>{t.name}</option>
              ))}
            </select>
          )}

          {!editingId && (
            <div className="field">
              <label>Role in Team</label>
              <select
                value={form.roleInTeam}
                disabled={isSaving}
                onChange={(e) => setForm((f) => ({ ...f, roleInTeam: Number(e.target.value) }))}
              >
                {Object.entries(roleInTeamLabels).map(([value, label]) => (
                  <option key={value} value={value}>{label}</option>
                ))}
              </select>
            </div>
          )}

          {editingId && (
            <div className="field">
              <label>Status</label>
              <select
                value={form.status}
                disabled={isSaving}
                onChange={(e) => setForm((f) => ({ ...f, status: e.target.value as 'Active' | 'Inactive' }))}
              >
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
          )}

          <div className="field" style={{ display: 'flex', alignItems: 'center', gap: 10, paddingTop: 24 }}>
            <input
              type="checkbox"
              id="baptized"
              checked={form.baptized}
              disabled={isSaving}
              onChange={(e) => setForm((f) => ({ ...f, baptized: e.target.checked }))}
              style={{ width: 'auto' }}
            />
            <label htmlFor="baptized" style={{ marginBottom: 0, cursor: 'pointer' }}>Baptized</label>
          </div>
        </div>

        <div style={{ display: 'flex', gap: 10, marginTop: 20 }}>
          <button className="primary-btn" onClick={save} disabled={isSaving}>
            {isSaving ? 'Saving...' : editingId ? 'Save Changes' : 'Add Worker'}
          </button>
          {editingId && (
            <button className="secondary-btn" onClick={cancelEdit} disabled={isSaving}>
              Cancel
            </button>
          )}
        </div>
      </Card>

      <div style={{ height: 18 }} />

      <Card title={`Workers (${workers.length})`}>
        {isLoading ? (
          <Loader message="Loading workers..." />
        ) : workers.length === 0 ? (
          <EmptyState title="No workers yet" message="Add your first worker using the form above." />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Phone</th>
                  <th>Teams</th>
                  <th>Baptized</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {workers.map((w) => (
                  <tr key={w.id}>
                    <td><strong>{w.fullName}</strong></td>
                    <td>{w.email}</td>
                    <td>{w.phone}</td>
                    <td>{w.teams.map((t) => t.teamName).join(', ') || '—'}</td>
                    <td>{w.baptized ? '✓' : '—'}</td>
                    <td>
                      <span style={{
                        padding: '3px 9px',
                        borderRadius: 999,
                        fontSize: 12,
                        fontWeight: 700,
                        background: w.status === 'Active' ? 'rgba(20,83,45,0.35)' : 'rgba(127,29,29,0.3)',
                        color: w.status === 'Active' ? '#86efac' : '#fca5a5',
                      }}>
                        {w.status}
                      </span>
                    </td>
                    <td>
                      <div style={{ display: 'flex', gap: 8 }}>
                        <button className="secondary-btn" onClick={() => edit(w)}>Edit</button>
                        <button className="danger-btn" onClick={() => setDeleteId(w.id)}>Remove</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      <ConfirmDialog
        isOpen={!!deleteId}
        title="Remove Worker"
        message="Remove this worker from the system? This action will soft-delete their record."
        confirmLabel="Remove"
        onConfirm={confirmDelete}
        onCancel={() => setDeleteId(null)}
      />
    </>
  );
}
