import { useEffect, useState } from 'react';
import { Card } from '../../components/ui/Card';
import { ErrorBanner } from '../../components/ui/ErrorBanner';
import { Loader } from '../../components/ui/Loader';
import { SuccessBanner } from '../../components/ui/SuccessBanner';
import { useSortableData } from '../../hooks/useSortableData';
import { getStoredAuth } from '../auth/authStorage';
import { getTeams } from '../teams/teamsApi';
import { Team } from '../../types/domain';
import {
  approveInventoryItem,
  createInventoryItem,
  deleteInventoryItem,
  getInventory,
  InventoryCondition,
  InventoryFilters,
  InventoryItem,
  InventoryStatus,
  markInventoryItemLost,
  retireInventoryItem,
  updateInventoryItem
} from './inventoryApi';

type InventoryForm = {
  name: string;
  teamId: string;
  description: string;
  quantity: number;
  condition: InventoryCondition;
  status: InventoryStatus;
  imageUrl: string;
};

const conditionLabels: Record<InventoryCondition, string> = {
  1: 'New',
  2: 'Good',
  3: 'Damaged',
  4: 'Needs Repair',
  5: 'Lost'
};

const statusLabels: Record<InventoryStatus, string> = {
  1: 'Pending Approval',
  2: 'Approved',
  3: 'Retired'
};

const emptyForm: InventoryForm = {
  name: '',
  teamId: '',
  description: '',
  quantity: 1,
  condition: 2,
  status: 1,
  imageUrl: ''
};

export function InventoryPage() {
  const [items, setItems] = useState<InventoryItem[]>([]);
  const [teams, setTeams] = useState<Team[]>([]);
  const [form, setForm] = useState<InventoryForm>(emptyForm);
  const [filters, setFilters] = useState<InventoryFilters>({
    teamId: '',
    status: '',
    condition: ''
  });

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

  const { sortedItems, requestSort, getSortLabel } = useSortableData(items);

  async function loadPageData(nextFilters = filters) {
    const auth = getStoredAuth();

    if (!auth) {
      setPageError('You are not logged in.');
      setIsLoading(false);
      return;
    }

    try {
      setPageError(null);
      setIsLoading(true);

      const [inventoryResult, teamsResult] = await Promise.all([
        getInventory(auth.token, nextFilters),
        getTeams(auth.token)
      ]);

      setItems(inventoryResult);
      setTeams(teamsResult);

      if (!form.teamId && teamsResult.length > 0) {
        const firstActiveTeam = teamsResult.find((team) => team.isActive);
        setForm((current) => ({
          ...current,
          teamId: firstActiveTeam?.id ?? ''
        }));
      }
    } catch (error) {
      setPageError(error instanceof Error ? error.message : 'Failed to load inventory');
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadPageData();
  }, []);

  function validateForm() {
    const errors: Record<string, string> = {};

    if (!form.name.trim()) errors.name = 'Item name is required.';
    if (form.name.trim().length > 160) errors.name = 'Item name cannot exceed 160 characters.';
    if (!form.teamId) errors.teamId = 'Team is required.';
    if (!form.description.trim()) errors.description = 'Description is required.';
    if (form.description.trim().length > 1000) errors.description = 'Description cannot exceed 1000 characters.';
    if (!Number.isInteger(form.quantity) || form.quantity <= 0) errors.quantity = 'Quantity must be a whole number greater than zero.';
    if (form.imageUrl.trim() && !/^https?:\/\//i.test(form.imageUrl.trim())) errors.imageUrl = 'Image URL must start with http:// or https://.';

    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  }

  async function saveItem() {
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

      const request = {
        name: form.name.trim(),
        teamId: form.teamId,
        description: form.description.trim(),
        quantity: form.quantity,
        condition: form.condition,
        status: form.status,
        imageUrl: form.imageUrl.trim() || null
      };

      if (editingId) {
        await updateInventoryItem(auth.token, editingId, request);
        setSuccessMessage('Inventory item updated successfully.');
      } else {
        await createInventoryItem(auth.token, request);
        setSuccessMessage('Inventory item created successfully.');
      }

      setForm({
        ...emptyForm,
        teamId: teams.find((team) => team.isActive)?.id ?? ''
      });

      setEditingId(null);
      await loadPageData();
    } catch (error) {
      setPageError(error instanceof Error ? error.message : 'Failed to save inventory item');
    } finally {
      setIsSaving(false);
    }
  }

  function editItem(item: InventoryItem) {
    setEditingId(item.id);
    setFieldErrors({});
    setPageError(null);
    setSuccessMessage(null);

    setForm({
      name: item.name,
      teamId: item.teamId,
      description: item.description,
      quantity: item.quantity,
      condition: item.condition,
      status: item.status,
      imageUrl: item.imageUrl ?? ''
    });
  }

  function cancelEdit() {
    setEditingId(null);
    setFieldErrors({});
    setSuccessMessage(null);
    setForm({
      ...emptyForm,
      teamId: teams.find((team) => team.isActive)?.id ?? ''
    });
  }

  async function applyFilters() {
    setSuccessMessage(null);
    await loadPageData(filters);
  }

  async function clearFilters() {
    setSuccessMessage(null);

    const clearedFilters: InventoryFilters = {
      teamId: '',
      status: '',
      condition: ''
    };

    setFilters(clearedFilters);
    await loadPageData(clearedFilters);
  }

  function askApprove(item: InventoryItem) {
    setConfirmAction({
      title: 'Approve inventory item',
      message: `Approve "${item.name}"?`,
      successMessage: 'Inventory item approved successfully.',
      action: async () => {
        const auth = getStoredAuth();

        if (!auth) {
          setPageError('You are not logged in.');
          return;
        }

        await approveInventoryItem(auth.token, item.id);
        await loadPageData();
      }
    });
  }

  function askRetire(item: InventoryItem) {
    setConfirmAction({
      title: 'Retire inventory item',
      message: `Retire "${item.name}"? Retired items remain in history.`,
      successMessage: 'Inventory item retired successfully.',
      action: async () => {
        const auth = getStoredAuth();

        if (!auth) {
          setPageError('You are not logged in.');
          return;
        }

        await retireInventoryItem(auth.token, item.id);
        await loadPageData();
      }
    });
  }

  function askMarkLost(item: InventoryItem) {
    setConfirmAction({
      title: 'Mark item as lost',
      message: `Mark "${item.name}" as lost?`,
      successMessage: 'Inventory item marked as lost successfully.',
      action: async () => {
        const auth = getStoredAuth();

        if (!auth) {
          setPageError('You are not logged in.');
          return;
        }

        await markInventoryItemLost(auth.token, item.id);
        await loadPageData();
      }
    });
  }

  function askDelete(item: InventoryItem) {
    setConfirmAction({
      title: 'Delete inventory item',
      message:
        item.status === 2
          ? `Backend will reject deleting approved item "${item.name}". Retire it instead.`
          : `Delete "${item.name}"? This is a soft delete.`,
      successMessage: 'Inventory item deleted successfully.',
      action: async () => {
        const auth = getStoredAuth();

        if (!auth) {
          setPageError('You are not logged in.');
          return;
        }

        await deleteInventoryItem(auth.token, item.id);
        await loadPageData();
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
      <h1 className="page-title">Inventory</h1>

      <ErrorBanner message={pageError} />
      <SuccessBanner message={successMessage} />

      <Card title={editingId ? 'Edit Inventory Item' : 'Register Inventory Item'}>
        <div className="form-grid">
          <div className="field">
            <label>Item Name</label>
            <input
              value={form.name}
              disabled={isSaving}
              onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))}
            />
            {fieldErrors.name && <div className="field-error">{fieldErrors.name}</div>}
          </div>

          <div className="field">
            <label>Team</label>
            <select
              value={form.teamId}
              disabled={isSaving}
              onChange={(event) => setForm((current) => ({ ...current, teamId: event.target.value }))}
            >
              <option value="">Select team</option>
              {teams
                .filter((team) => team.isActive || team.id === form.teamId)
                .map((team) => (
                  <option key={team.id} value={team.id}>
                    {team.name}
                  </option>
                ))}
            </select>
            {fieldErrors.teamId && <div className="field-error">{fieldErrors.teamId}</div>}
          </div>

          <div className="field">
            <label>Quantity</label>
            <input
              type="number"
              min="1"
              value={form.quantity}
              disabled={isSaving}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  quantity: Number(event.target.value)
                }))
              }
            />
            {fieldErrors.quantity && <div className="field-error">{fieldErrors.quantity}</div>}
          </div>

          <div className="field">
            <label>Condition</label>
            <select
              value={form.condition}
              disabled={isSaving}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  condition: Number(event.target.value) as InventoryCondition
                }))
              }
            >
              <option value={1}>New</option>
              <option value={2}>Good</option>
              <option value={3}>Damaged</option>
              <option value={4}>Needs Repair</option>
              <option value={5}>Lost</option>
            </select>
          </div>

          <div className="field">
            <label>Status</label>
            <select
              value={form.status}
              disabled={isSaving}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  status: Number(event.target.value) as InventoryStatus
                }))
              }
            >
              <option value={1}>Pending Approval</option>
              <option value={2}>Approved</option>
              <option value={3}>Retired</option>
            </select>
          </div>

          <div className="field">
            <label>Image URL</label>
            <input
              value={form.imageUrl}
              placeholder="Optional"
              disabled={isSaving}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  imageUrl: event.target.value
                }))
              }
            />
            {fieldErrors.imageUrl && <div className="field-error">{fieldErrors.imageUrl}</div>}
          </div>

          <div className="field" style={{ gridColumn: '1 / -1' }}>
            <label>Description</label>
            <textarea
              rows={3}
              value={form.description}
              disabled={isSaving}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  description: event.target.value
                }))
              }
            />
            {fieldErrors.description && <div className="field-error">{fieldErrors.description}</div>}
          </div>
        </div>

        <div style={{ display: 'flex', gap: 10, marginTop: 16 }}>
          <button className="primary-btn" onClick={saveItem} disabled={isSaving}>
            {isSaving ? 'Saving...' : editingId ? 'Save Changes' : 'Register Item'}
          </button>

          {editingId && (
            <button className="secondary-btn" onClick={cancelEdit} disabled={isSaving}>
              Cancel
            </button>
          )}
        </div>
      </Card>

      <div style={{ height: 18 }} />

      <Card title="Inventory Filters">
        <div className="form-grid">
          <div className="field">
            <label>Team</label>
            <select
              value={filters.teamId ?? ''}
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  teamId: event.target.value
                }))
              }
            >
              <option value="">All teams</option>
              {teams.map((team) => (
                <option key={team.id} value={team.id}>
                  {team.name}
                </option>
              ))}
            </select>
          </div>

          <div className="field">
            <label>Status</label>
            <select
              value={filters.status ?? ''}
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  status: event.target.value
                    ? (Number(event.target.value) as InventoryStatus)
                    : ''
                }))
              }
            >
              <option value="">All statuses</option>
              <option value={1}>Pending Approval</option>
              <option value={2}>Approved</option>
              <option value={3}>Retired</option>
            </select>
          </div>

          <div className="field">
            <label>Condition</label>
            <select
              value={filters.condition ?? ''}
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  condition: event.target.value
                    ? (Number(event.target.value) as InventoryCondition)
                    : ''
                }))
              }
            >
              <option value="">All conditions</option>
              <option value={1}>New</option>
              <option value={2}>Good</option>
              <option value={3}>Damaged</option>
              <option value={4}>Needs Repair</option>
              <option value={5}>Lost</option>
            </select>
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

      <Card title="Inventory List">
        {isLoading ? (
          <Loader message="Loading inventory..." />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('name')}>
                      Item{getSortLabel('name')}
                    </button>
                  </th>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('teamName')}>
                      Team{getSortLabel('teamName')}
                    </button>
                  </th>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('quantity')}>
                      Quantity{getSortLabel('quantity')}
                    </button>
                  </th>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('condition')}>
                      Condition{getSortLabel('condition')}
                    </button>
                  </th>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('status')}>
                      Status{getSortLabel('status')}
                    </button>
                  </th>
                  <th>Actions</th>
                </tr>
              </thead>

              <tbody>
                {sortedItems.map((item) => (
                  <tr key={item.id}>
                    <td>
                      <strong>{item.name}</strong>
                      <div style={{ color: '#73798c', marginTop: 4 }}>
                        {item.description}
                      </div>
                    </td>
                    <td>{item.teamName}</td>
                    <td>{item.quantity}</td>
                    <td>{conditionLabels[item.condition]}</td>
                    <td>{statusLabels[item.status]}</td>
                    <td>
                      <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                        <button className="secondary-btn" onClick={() => editItem(item)}>
                          Edit
                        </button>

                        {item.status !== 2 && item.status !== 3 && (
                          <button className="secondary-btn" onClick={() => askApprove(item)}>
                            Approve
                          </button>
                        )}

                        {item.condition !== 5 && item.status !== 3 && (
                          <button className="secondary-btn" onClick={() => askMarkLost(item)}>
                            Mark Lost
                          </button>
                        )}

                        {item.status !== 3 && (
                          <button className="secondary-btn" onClick={() => askRetire(item)}>
                            Retire
                          </button>
                        )}

                        <button className="danger-btn" onClick={() => askDelete(item)}>
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}

                {items.length === 0 && (
                  <tr>
                    <td colSpan={6}>No inventory items found.</td>
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