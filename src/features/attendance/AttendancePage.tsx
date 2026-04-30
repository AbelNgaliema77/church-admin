import { useEffect, useState } from 'react';
import { Card } from '../../components/ui/Card';
import { getStoredAuth } from '../auth/authStorage';
import {
  AttendanceRecord,
  createAttendance,
  deleteAttendance,
  getAttendance,
  ServiceType,
  updateAttendance
} from './attendanceApi';

type AttendanceForm = {
  serviceDate: string;
  serviceType: ServiceType;
  men: number;
  women: number;
  children: number;
  visitors: number;
  notes: string;
};

const emptyForm: AttendanceForm = {
  serviceDate: '',
  serviceType: 1,
  men: 0,
  women: 0,
  children: 0,
  visitors: 0,
  notes: ''
};

const serviceTypeLabels: Record<ServiceType, string> = {
  1: 'Sunday',
  2: 'Friday',
  3: 'Special',
  4: 'Prayer',
  5: 'Conference'
};

export function AttendancePage() {
  const [records, setRecords] = useState<AttendanceRecord[]>([]);
  const [form, setForm] = useState<AttendanceForm>(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [pageError, setPageError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [confirmDeleteId, setConfirmDeleteId] = useState<string | null>(null);

  async function loadAttendance() {
    const auth = getStoredAuth();

    if (!auth) {
      setPageError('You are not logged in.');
      setIsLoading(false);
      return;
    }

    try {
      setPageError(null);
      setIsLoading(true);
      const result = await getAttendance(auth.token);
      setRecords(result);
    } catch (error) {
      setPageError(error instanceof Error ? error.message : 'Failed to load attendance.');
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadAttendance();
  }, []);

  function validateForm() {
    const errors: Record<string, string> = {};

    if (!form.serviceDate) {
      errors.serviceDate = 'Service date is required.';
    }

    const today = new Date().toISOString().slice(0, 10);

    if (form.serviceDate && form.serviceDate > today) {
      errors.serviceDate = 'Service date cannot be in the future.';
    }

    if (form.men < 0) {
      errors.men = 'Men cannot be negative.';
    }

    if (form.women < 0) {
      errors.women = 'Women cannot be negative.';
    }

    if (form.children < 0) {
      errors.children = 'Children cannot be negative.';
    }

    if (form.visitors < 0) {
      errors.visitors = 'Visitors cannot be negative.';
    }

    if (form.men + form.women + form.children + form.visitors <= 0) {
      errors.total = 'At least one attendance count must be greater than zero.';
    }

    if (form.notes.length > 1000) {
      errors.notes = 'Notes cannot exceed 1000 characters.';
    }

    setFieldErrors(errors);

    return Object.keys(errors).length === 0;
  }

  async function saveAttendance() {
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

      const payload = {
        serviceDate: form.serviceDate,
        serviceType: form.serviceType,
        men: form.men,
        women: form.women,
        children: form.children,
        visitors: form.visitors,
        notes: form.notes.trim()
      };

      if (editingId) {
        await updateAttendance(auth.token, editingId, payload);
      } else {
        await createAttendance(auth.token, payload);
      }

      setForm(emptyForm);
      setEditingId(null);
      setFieldErrors({});
      await loadAttendance();
    } catch (error) {
      setPageError(error instanceof Error ? error.message : 'Failed to save attendance.');
    } finally {
      setIsSaving(false);
    }
  }

  function editAttendance(record: AttendanceRecord) {
    setEditingId(record.id);
    setForm({
      serviceDate: record.serviceDate,
      serviceType: record.serviceType,
      men: record.men,
      women: record.women,
      children: record.children,
      visitors: record.visitors,
      notes: record.notes ?? ''
    });
    setFieldErrors({});
    setPageError(null);
  }

  function cancelEdit() {
    setEditingId(null);
    setForm(emptyForm);
    setFieldErrors({});
  }

  async function confirmDelete() {
    if (!confirmDeleteId) {
      return;
    }

    const auth = getStoredAuth();

    if (!auth) {
      setPageError('You are not logged in.');
      return;
    }

    try {
      setPageError(null);
      await deleteAttendance(auth.token, confirmDeleteId);
      await loadAttendance();
    } catch (error) {
      setPageError(error instanceof Error ? error.message : 'Failed to delete attendance.');
    } finally {
      setConfirmDeleteId(null);
    }
  }

  return (
    <>
      <h1 className="page-title">Attendance Registry</h1>

      {pageError && <div className="form-error">{pageError}</div>}

      <Card title={editingId ? 'Edit Attendance' : 'Capture Attendance'}>
        <div className="form-grid">
          <div className="field">
            <label>Service Date</label>
            <input
              type="date"
              value={form.serviceDate}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  serviceDate: event.target.value
                }))
              }
            />
            {fieldErrors.serviceDate && (
              <div className="field-error">{fieldErrors.serviceDate}</div>
            )}
          </div>

          <div className="field">
            <label>Service Type</label>
            <select
              value={form.serviceType}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  serviceType: Number(event.target.value) as ServiceType
                }))
              }
            >
              <option value={1}>Sunday</option>
              <option value={2}>Friday</option>
              <option value={3}>Special</option>
              <option value={4}>Prayer</option>
              <option value={5}>Conference</option>
            </select>
          </div>

          <NumberField
            label="Men"
            value={form.men}
            error={fieldErrors.men}
            onChange={(value) =>
              setForm((current) => ({ ...current, men: value }))
            }
          />

          <NumberField
            label="Women"
            value={form.women}
            error={fieldErrors.women}
            onChange={(value) =>
              setForm((current) => ({ ...current, women: value }))
            }
          />

          <NumberField
            label="Children"
            value={form.children}
            error={fieldErrors.children}
            onChange={(value) =>
              setForm((current) => ({ ...current, children: value }))
            }
          />

          <NumberField
            label="Visitors"
            value={form.visitors}
            error={fieldErrors.visitors}
            onChange={(value) =>
              setForm((current) => ({ ...current, visitors: value }))
            }
          />

          <div className="field" style={{ gridColumn: '1 / -1' }}>
            <label>Notes</label>
            <textarea
              rows={3}
              value={form.notes}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  notes: event.target.value
                }))
              }
            />
            {fieldErrors.notes && (
              <div className="field-error">{fieldErrors.notes}</div>
            )}
          </div>
        </div>

        {fieldErrors.total && (
          <div className="form-error" style={{ marginTop: 16 }}>
            {fieldErrors.total}
          </div>
        )}

        <div style={{ display: 'flex', gap: 10, marginTop: 16 }}>
          <button
            className="primary-btn"
            onClick={saveAttendance}
            disabled={isSaving}
          >
            {isSaving
              ? 'Saving...'
              : editingId
                ? 'Save Changes'
                : 'Save Attendance'}
          </button>

          {editingId && (
            <button className="secondary-btn" onClick={cancelEdit}>
              Cancel
            </button>
          )}
        </div>
      </Card>

      <div style={{ height: 18 }} />

      <Card title="Attendance Records">
        {isLoading ? (
          <div>Loading attendance...</div>
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Service</th>
                  <th>Men</th>
                  <th>Women</th>
                  <th>Children</th>
                  <th>Visitors</th>
                  <th>Total</th>
                  <th>Actions</th>
                </tr>
              </thead>

              <tbody>
                {records.map((record) => (
                  <tr key={record.id}>
                    <td>{record.serviceDate}</td>
                    <td>{serviceTypeLabels[record.serviceType]}</td>
                    <td>{record.men}</td>
                    <td>{record.women}</td>
                    <td>{record.children}</td>
                    <td>{record.visitors}</td>
                    <td>
                      <strong>{record.total}</strong>
                    </td>
                    <td>
                      <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                        <button
                          className="secondary-btn"
                          onClick={() => editAttendance(record)}
                        >
                          Edit
                        </button>

                        <button
                          className="danger-btn"
                          onClick={() => setConfirmDeleteId(record.id)}
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}

                {records.length === 0 && (
                  <tr>
                    <td colSpan={8}>No attendance records found.</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {confirmDeleteId && (
        <div className="modal-backdrop">
          <div className="confirm-modal">
            <h2>Delete attendance record</h2>
            <p>
              Delete this attendance record? This is a soft delete on the backend,
              but it should still be used carefully.
            </p>

            <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end' }}>
              <button
                className="secondary-btn"
                onClick={() => setConfirmDeleteId(null)}
              >
                Cancel
              </button>

              <button className="danger-btn" onClick={confirmDelete}>
                Confirm
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

function NumberField({
  label,
  value,
  error,
  onChange
}: {
  label: string;
  value: number;
  error?: string;
  onChange: (value: number) => void;
}) {
  return (
    <div className="field">
      <label>{label}</label>
      <input
        type="number"
        min="0"
        value={value}
        onChange={(event) => onChange(Number(event.target.value))}
      />
      {error && <div className="field-error">{error}</div>}
    </div>
  );
}