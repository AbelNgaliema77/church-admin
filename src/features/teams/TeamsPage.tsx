import { useEffect, useState } from 'react';
import { Card } from '../../components/ui/Card';
import { Loader } from '../../components/ui/Loader';
import { ErrorBanner } from '../../components/ui/ErrorBanner';
import { SuccessBanner } from '../../components/ui/SuccessBanner';
import { useSortableData } from '../../hooks/useSortableData';
import { getStoredAuth } from '../auth/authStorage';
import {
  createTeam,
  deactivateTeam,
  deleteTeam,
  getTeams,
  updateTeam
} from './teamsApi';
import { Team } from '../../types/domain';

type TeamForm = {
  name: string;
  description: string;
  isActive: boolean;
};

const emptyForm: TeamForm = {
  name: '',
  description: '',
  isActive: true
};

export function TeamsPage() {
  const [teams, setTeams] = useState<Team[]>([]);
  const [form, setForm] = useState<TeamForm>(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);

  const [confirmAction, setConfirmAction] = useState<{
    title: string;
    message: string;
    successMessage: string;
    action: () => Promise<void>;
  } | null>(null);

  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [pageError, setPageError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);

  const { sortedItems, requestSort, getSortLabel } = useSortableData(teams);

  async function loadTeams() {
    const auth = getStoredAuth();

    if (!auth) {
      setPageError('You are not logged in.');
      setIsLoading(false);
      return;
    }

    try {
      setPageError(null);
      setIsLoading(true);

      const result = await getTeams(auth.token);
      setTeams(result);
    } catch (error) {
      setPageError(error instanceof Error ? error.message : 'Failed to load teams');
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadTeams();
  }, []);

  function validateForm() {
    const errors: Record<string, string> = {};

    if (!form.name.trim()) errors.name = 'Team name is required.';
    if (form.name.trim().length > 120) errors.name = 'Team name cannot exceed 120 characters.';
    if (form.description.length > 500) errors.description = 'Description cannot exceed 500 characters.';

    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  }

  async function saveTeam() {
    setSuccessMessage(null);

    if (!validateForm()) return;

    const auth = getStoredAuth();

    if (!auth) {
      setPageError('You are not logged in.');
      return;
    }

    try {
      setIsSaving(true);
      setPageError(null);

      if (editingId) {
        await updateTeam(auth.token, editingId, {
          name: form.name.trim(),
          description: form.description.trim(),
          isActive: form.isActive
        });

        setSuccessMessage('Team updated successfully.');
      } else {
        await createTeam(auth.token, {
          name: form.name.trim(),
          description: form.description.trim()
        });

        setSuccessMessage('Team created successfully.');
      }

      setForm(emptyForm);
      setEditingId(null);
      await loadTeams();
    } catch (error) {
      setPageError(error instanceof Error ? error.message : 'Failed to save team');
    } finally {
      setIsSaving(false);
    }
  }

  function editTeam(team: Team) {
    setEditingId(team.id);
    setForm({
      name: team.name,
      description: team.description ?? '',
      isActive: team.isActive
    });

    setFieldErrors({});
    setPageError(null);
    setSuccessMessage(null);
  }

  function cancelEdit() {
    setEditingId(null);
    setForm(emptyForm);
    setFieldErrors({});
    setSuccessMessage(null);
  }

  function askDeactivate(team: Team) {
    setConfirmAction({
      title: 'Deactivate team',
      message: `Deactivate "${team.name}"? This keeps history but prevents future use.`,
      successMessage: 'Team deactivated successfully.',
      action: async () => {
        const auth = getStoredAuth();

        if (!auth) {
          setPageError('You are not logged in.');
          return;
        }

        await deactivateTeam(auth.token, team.id);
        await loadTeams();
      }
    });
  }

  function askDelete(team: Team) {
    setConfirmAction({
      title: 'Delete team',
      message: `Delete "${team.name}"? This only works if the team has no history. Deactivation is safer.`,
      successMessage: 'Team deleted successfully.',
      action: async () => {
        const auth = getStoredAuth();

        if (!auth) {
          setPageError('You are not logged in.');
          return;
        }

        await deleteTeam(auth.token, team.id);
        await loadTeams();
      }
    });
  }

  async function runConfirmedAction() {
    if (!confirmAction) return;

    try {
      setPageError(null);
      setSuccessMessage(null);

      await confirmAction.action();
      setSuccessMessage(confirmAction.successMessage);
    } catch (error) {
      setPageError(error instanceof Error ? error.message : 'Action failed');
    } finally {
      setConfirmAction(null);
    }
  }

  return (
    <>
      <h1 className="page-title">Teams</h1>

      <ErrorBanner message={pageError} />
      <SuccessBanner message={successMessage} />

      <Card title={editingId ? 'Edit Team' : 'Create Team'}>
        <div className="form-grid">
          <div className="field">
            <label>Team Name</label>
            <input
              value={form.name}
              disabled={isSaving}
              onChange={(event) =>
                setForm((current) => ({ ...current, name: event.target.value }))
              }
            />
            {fieldErrors.name && <div className="field-error">{fieldErrors.name}</div>}
          </div>

          <div className="field">
            <label>Status</label>
            <select
              value={form.isActive ? 'Active' : 'Inactive'}
              disabled={!editingId || isSaving}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  isActive: event.target.value === 'Active'
                }))
              }
            >
              <option>Active</option>
              <option>Inactive</option>
            </select>
          </div>

          <div className="field" style={{ gridColumn: '1 / -1' }}>
            <label>Description</label>
            <textarea
              rows={3}
              value={form.description}
              disabled={isSaving}
              onChange={(event) =>
                setForm((current) => ({ ...current, description: event.target.value }))
              }
            />
            {fieldErrors.description && <div className="field-error">{fieldErrors.description}</div>}
          </div>
        </div>

        <div style={{ display: 'flex', gap: 10, marginTop: 16 }}>
          <button className="primary-btn" onClick={saveTeam} disabled={isSaving}>
            {isSaving ? 'Saving...' : editingId ? 'Save Changes' : 'Create Team'}
          </button>

          {editingId && (
            <button className="secondary-btn" onClick={cancelEdit} disabled={isSaving}>
              Cancel
            </button>
          )}
        </div>
      </Card>

      <div style={{ height: 18 }} />

      <Card title="Team List">
        {isLoading ? (
          <Loader message="Loading teams..." />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('name')}>
                      Team{getSortLabel('name')}
                    </button>
                  </th>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('description')}>
                      Description{getSortLabel('description')}
                    </button>
                  </th>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('isActive')}>
                      Status{getSortLabel('isActive')}
                    </button>
                  </th>
                  <th>Actions</th>
                </tr>
              </thead>

              <tbody>
                {sortedItems.map((team) => (
                  <tr key={team.id}>
                    <td><strong>{team.name}</strong></td>
                    <td>{team.description}</td>
                    <td>{team.isActive ? 'Active' : 'Inactive'}</td>
                    <td>
                      <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                        <button className="secondary-btn" onClick={() => editTeam(team)}>
                          Edit
                        </button>

                        {team.isActive && (
                          <button className="secondary-btn" onClick={() => askDeactivate(team)}>
                            Deactivate
                          </button>
                        )}

                        <button className="danger-btn" onClick={() => askDelete(team)}>
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}

                {teams.length === 0 && (
                  <tr>
                    <td colSpan={4}>No teams found.</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {confirmAction && (
        <div className="modal-backdrop">
          <div className="confirm-modal">
            <h2>{confirmAction.title}</h2>
            <p>{confirmAction.message}</p>

            <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end' }}>
              <button className="secondary-btn" onClick={() => setConfirmAction(null)}>
                Cancel
              </button>

              <button className="danger-btn" onClick={runConfirmedAction}>
                Confirm
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}