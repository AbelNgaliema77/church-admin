import { useEffect, useState } from 'react';
import { Card } from '../../components/ui/Card';
import { ErrorBanner } from '../../components/ui/ErrorBanner';
import { Loader } from '../../components/ui/Loader';
import { SuccessBanner } from '../../components/ui/SuccessBanner';
import { EmptyState } from '../../components/ui/EmptyState';
import { useSortableData } from '../../hooks/useSortableData';
import { getStoredAuth } from '../auth/authStorage';
import {
  activateUser,
  createUser,
  deactivateUser,
  getUsers,
  updateUser,
  UserAccount,
  UserRole
} from './usersApi';

type UserForm = {
  email: string;
  displayName: string;
  role: UserRole;
  isActive: boolean;
};

const emptyForm: UserForm = {
  email: '',
  displayName: '',
  role: 6,
  isActive: true
};

const roleLabels: Record<UserRole, string> = {
  0: 'Pending',
  1: 'Worker',
  2: 'Team Lead',
  3: 'Admin',
  4: 'Finance',
  5: 'Pastor',
  6: 'Viewer'
};

export function UsersPage() {
  const [users, setUsers] = useState<UserAccount[]>([]);
  const [form, setForm] = useState<UserForm>(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [latestInviteLink, setLatestInviteLink] = useState<string | null>(null);

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

  const { sortedItems, requestSort, getSortLabel } = useSortableData(users);

  async function loadUsers() {
    const auth = getStoredAuth();

    if (!auth) {
      setPageError('You are not logged in.');
      setIsLoading(false);
      return;
    }

    try {
      setPageError(null);
      setIsLoading(true);

      const result = await getUsers(auth.token);
      setUsers(result);
    } catch (error) {
      setPageError(error instanceof Error ? error.message : 'Failed to load users.');
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadUsers();
  }, []);

  function validateForm() {
    const errors: Record<string, string> = {};

    if (!form.displayName.trim()) {
      errors.displayName = 'Display name is required.';
    }

    if (form.displayName.trim().length > 160) {
      errors.displayName = 'Display name cannot exceed 160 characters.';
    }

    if (!editingId && !form.email.trim()) {
      errors.email = 'Email is required.';
    }

    if (!editingId && form.email.trim().length > 256) {
      errors.email = 'Email cannot exceed 256 characters.';
    }

    if (!editingId && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email.trim())) {
      errors.email = 'Enter a valid email address.';
    }

    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  }

  async function saveUser() {
    setSuccessMessage(null);
    setLatestInviteLink(null);

    if (!validateForm()) {
      return;
    }

    const auth = getStoredAuth();

    if (!auth) {
      setPageError('You are not logged in.');
      return;
    }

    try {
      setIsSaving(true);
      setPageError(null);

      if (editingId) {
        await updateUser(auth.token, editingId, {
          displayName: form.displayName.trim(),
          role: form.role,
          isActive: form.isActive
        });

        setSuccessMessage('User updated successfully.');
      } else {
        const createdUser = await createUser(auth.token, {
          email: form.email.trim(),
          displayName: form.displayName.trim(),
          role: form.role,
          isActive: form.isActive
        });

        setLatestInviteLink(createdUser.inviteLink ?? null);
        setSuccessMessage('User created successfully. Copy the invite link below.');
      }

      setForm(emptyForm);
      setEditingId(null);
      setFieldErrors({});
      await loadUsers();
    } catch (error) {
      setPageError(error instanceof Error ? error.message : 'Failed to save user.');
    } finally {
      setIsSaving(false);
    }
  }

  async function copyInviteLink() {
    if (!latestInviteLink) {
      return;
    }

    await navigator.clipboard.writeText(latestInviteLink);
    setSuccessMessage('Invite link copied.');
  }

  function editUser(user: UserAccount) {
    setEditingId(user.id);
    setLatestInviteLink(null);

    setForm({
      email: user.email,
      displayName: user.displayName,
      role: user.role,
      isActive: user.isActive
    });

    setFieldErrors({});
    setPageError(null);
    setSuccessMessage(null);
  }

  function cancelEdit() {
    setEditingId(null);
    setForm(emptyForm);
    setFieldErrors({});
    setPageError(null);
    setSuccessMessage(null);
    setLatestInviteLink(null);
  }

  function askActivate(user: UserAccount) {
    setConfirmAction({
      title: 'Activate user',
      message: `Activate "${user.displayName}"?`,
      successMessage: 'User activated successfully.',
      action: async () => {
        const auth = getStoredAuth();

        if (!auth) {
          setPageError('You are not logged in.');
          return;
        }

        await activateUser(auth.token, user.id);
        await loadUsers();
      }
    });
  }

  function askDeactivate(user: UserAccount) {
    setConfirmAction({
      title: 'Deactivate user',
      message: `Deactivate "${user.displayName}"? They will no longer be able to use the admin portal.`,
      successMessage: 'User deactivated successfully.',
      action: async () => {
        const auth = getStoredAuth();

        if (!auth) {
          setPageError('You are not logged in.');
          return;
        }

        await deactivateUser(auth.token, user.id);
        await loadUsers();
      }
    });
  }

  async function runConfirmedAction() {
    if (!confirmAction) {
      return;
    }

    try {
      setPageError(null);
      setSuccessMessage(null);

      await confirmAction.action();
      setSuccessMessage(confirmAction.successMessage);
    } catch (error) {
      setPageError(error instanceof Error ? error.message : 'Action failed.');
    } finally {
      setConfirmAction(null);
    }
  }

  return (
    <>
      <h1 className="page-title">Users & Roles</h1>

      <ErrorBanner message={pageError} />
      <SuccessBanner message={successMessage} />

      {latestInviteLink && (
        <Card title="Invite Link">
          <div className="field">
            <label>Send this link to the user</label>
            <input readOnly value={latestInviteLink} />
          </div>

          <div style={{ display: 'flex', gap: 10, marginTop: 16 }}>
            <button className="primary-btn" onClick={copyInviteLink}>
              Copy Invite Link
            </button>
          </div>
        </Card>
      )}

      <Card title={editingId ? 'Edit User' : 'Create User'}>
        <div className="form-grid">
          <div className="field">
            <label>Email</label>
            <input
              value={form.email}
              disabled={isSaving || editingId !== null}
              placeholder="person@example.com"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  email: event.target.value
                }))
              }
            />
            {fieldErrors.email && (
              <div className="field-error">{fieldErrors.email}</div>
            )}
          </div>

          <div className="field">
            <label>Display Name</label>
            <input
              value={form.displayName}
              disabled={isSaving}
              placeholder="Full name"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  displayName: event.target.value
                }))
              }
            />
            {fieldErrors.displayName && (
              <div className="field-error">{fieldErrors.displayName}</div>
            )}
          </div>

          <div className="field">
            <label>Role</label>
            <select
              value={form.role}
              disabled={isSaving}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  role: Number(event.target.value) as UserRole
                }))
              }
            >
              <option value={0}>Pending</option>
              <option value={1}>Worker</option>
              <option value={2}>Team Lead</option>
              <option value={3}>Admin</option>
              <option value={4}>Finance</option>
              <option value={5}>Pastor</option>
              <option value={6}>Viewer</option>
            </select>
          </div>

          <div className="field">
            <label>Status</label>
            <select
              value={form.isActive ? 'Active' : 'Inactive'}
              disabled={isSaving}
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
        </div>

        <div style={{ display: 'flex', gap: 10, marginTop: 16 }}>
          <button className="primary-btn" onClick={saveUser} disabled={isSaving}>
            {isSaving ? 'Saving...' : editingId ? 'Save Changes' : 'Create User'}
          </button>

          {editingId && (
            <button className="secondary-btn" onClick={cancelEdit} disabled={isSaving}>
              Cancel
            </button>
          )}
        </div>
      </Card>

      <div style={{ height: 18 }} />

      <Card title="User List">
        {isLoading ? (
          <Loader message="Loading users..." />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>
                    <button
                      className="table-sort-btn"
                      onClick={() => requestSort('displayName')}
                    >
                      Name{getSortLabel('displayName')}
                    </button>
                  </th>
                  <th>
                    <button
                      className="table-sort-btn"
                      onClick={() => requestSort('email')}
                    >
                      Email{getSortLabel('email')}
                    </button>
                  </th>
                  <th>
                    <button
                      className="table-sort-btn"
                      onClick={() => requestSort('role')}
                    >
                      Role{getSortLabel('role')}
                    </button>
                  </th>
                  <th>
                    <button
                      className="table-sort-btn"
                      onClick={() => requestSort('isActive')}
                    >
                      Status{getSortLabel('isActive')}
                    </button>
                  </th>
                  <th>Provider</th>
                  <th>Actions</th>
                </tr>
              </thead>

              <tbody>
                {sortedItems.map((user) => (
                  <tr key={user.id}>
                    <td>
                      <strong>{user.displayName}</strong>
                    </td>
                    <td>{user.email}</td>
                    <td>{roleLabels[user.role]}</td>
                    <td>{user.isActive ? 'Active' : 'Inactive'}</td>
                    <td>{user.externalProvider ?? '-'}</td>
                    <td>
                      <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                        <button
                          className="secondary-btn"
                          onClick={() => editUser(user)}
                        >
                          Edit
                        </button>

                        {user.isActive ? (
                          <button
                            className="danger-btn"
                            onClick={() => askDeactivate(user)}
                          >
                            Deactivate
                          </button>
                        ) : (
                          <button
                            className="secondary-btn"
                            onClick={() => askActivate(user)}
                          >
                            Activate
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}

                {users.length === 0 && (
                  <tr>
                    <td colSpan={6}>
                      <EmptyState
                        title="No users"
                        message="Create your first admin portal user."
                      />
                    </td>
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
              <button
                className="secondary-btn"
                onClick={() => setConfirmAction(null)}
              >
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