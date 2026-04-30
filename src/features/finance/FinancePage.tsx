import { useEffect, useState } from 'react';
import { Card } from '../../components/ui/Card';
import { Loader } from '../../components/ui/Loader';
import { ErrorBanner } from '../../components/ui/ErrorBanner';
import { SuccessBanner } from '../../components/ui/SuccessBanner';
import { useSortableData } from '../../hooks/useSortableData';
import { getStoredAuth } from '../auth/authStorage';
import {
  createFinanceCorrection,
  createFinanceEntry,
  deleteFinanceEntry,
  FinanceCategory,
  FinanceEntry,
  getFinanceEntries,
  PaymentMethod,
  ServiceType,
  updateFinanceEntry,
  verifyFinanceEntry
} from './financeApi';

type FinanceForm = {
  serviceDate: string;
  serviceType: ServiceType;
  category: FinanceCategory;
  amount: number;
  paymentMethod: PaymentMethod;
  notes: string;
};

type CorrectionForm = {
  category: FinanceCategory;
  amount: number;
  paymentMethod: PaymentMethod;
  reason: string;
};

const emptyForm: FinanceForm = {
  serviceDate: '',
  serviceType: 1,
  category: 3,
  amount: 0,
  paymentMethod: 1,
  notes: ''
};

const emptyCorrectionForm: CorrectionForm = {
  category: 3,
  amount: 0,
  paymentMethod: 1,
  reason: ''
};

const serviceTypeLabels: Record<ServiceType, string> = {
  1: 'Sunday',
  2: 'Friday',
  3: 'Special',
  4: 'Prayer',
  5: 'Conference'
};

const categoryLabels: Record<FinanceCategory, string> = {
  1: 'Tithe',
  2: 'Thanksgiving',
  3: 'Normal Offering',
  4: 'Special Offering',
  5: 'Building Fund',
  6: 'Other'
};

const paymentMethodLabels: Record<PaymentMethod, string> = {
  1: 'Cash',
  2: 'EFT',
  3: 'Card',
  4: 'Bank Deposit'
};

export function FinancePage() {
  const [entries, setEntries] = useState<FinanceEntry[]>([]);
  const [form, setForm] = useState<FinanceForm>(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [pageError, setPageError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);

  const [confirmDeleteId, setConfirmDeleteId] = useState<string | null>(null);
  const [confirmVerifyId, setConfirmVerifyId] = useState<string | null>(null);

  const [correctionEntry, setCorrectionEntry] = useState<FinanceEntry | null>(null);
  const [correctionForm, setCorrectionForm] =
    useState<CorrectionForm>(emptyCorrectionForm);

  const { sortedItems, requestSort, getSortLabel } = useSortableData(entries);

  async function loadFinance() {
    const auth = getStoredAuth();

    if (!auth) {
      setPageError('You are not logged in.');
      setIsLoading(false);
      return;
    }

    try {
      setPageError(null);
      setIsLoading(true);

      const result = await getFinanceEntries(auth.token);
      setEntries(result);
    } catch (error) {
      setPageError(
        error instanceof Error ? error.message : 'Failed to load finance entries.'
      );
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadFinance();
  }, []);

  function validateForm() {
    const errors: Record<string, string> = {};

    if (!form.serviceDate) errors.serviceDate = 'Service date is required.';

    const today = new Date().toISOString().slice(0, 10);

    if (form.serviceDate && form.serviceDate > today) {
      errors.serviceDate = 'Service date cannot be in the future.';
    }

    if (form.amount <= 0) errors.amount = 'Amount must be greater than zero.';
    if (form.notes.length > 1000) errors.notes = 'Notes cannot exceed 1000 characters.';

    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  }

  function validateCorrectionForm() {
    const errors: Record<string, string> = {};

    if (correctionForm.amount === 0) errors.amount = 'Correction amount cannot be zero.';
    if (!correctionForm.reason.trim()) errors.reason = 'Correction reason is required.';
    if (correctionForm.reason.length > 1000) errors.reason = 'Reason cannot exceed 1000 characters.';

    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  }

  async function saveFinanceEntry() {
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

      const payload = {
        serviceDate: form.serviceDate,
        serviceType: form.serviceType,
        category: form.category,
        amount: form.amount,
        paymentMethod: form.paymentMethod,
        notes: form.notes.trim()
      };

      if (editingId) {
        await updateFinanceEntry(auth.token, editingId, payload);
        setSuccessMessage('Finance entry updated successfully.');
      } else {
        await createFinanceEntry(auth.token, payload);
        setSuccessMessage('Finance entry created successfully.');
      }

      setForm(emptyForm);
      setEditingId(null);
      setFieldErrors({});
      await loadFinance();
    } catch (error) {
      setPageError(
        error instanceof Error ? error.message : 'Failed to save finance entry.'
      );
    } finally {
      setIsSaving(false);
    }
  }

  function editEntry(entry: FinanceEntry) {
    if (entry.isVerified) {
      setPageError('Verified finance entries cannot be edited. Use correction instead.');
      setSuccessMessage(null);
      return;
    }

    setEditingId(entry.id);
    setForm({
      serviceDate: entry.serviceDate,
      serviceType: entry.serviceType,
      category: entry.category,
      amount: entry.amount,
      paymentMethod: entry.paymentMethod,
      notes: entry.notes ?? ''
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

  async function confirmVerify() {
    if (!confirmVerifyId) return;

    const auth = getStoredAuth();

    if (!auth) {
      setPageError('You are not logged in.');
      return;
    }

    try {
      setPageError(null);
      setSuccessMessage(null);

      await verifyFinanceEntry(auth.token, confirmVerifyId);
      setSuccessMessage('Finance entry verified successfully.');
      await loadFinance();
    } catch (error) {
      setPageError(
        error instanceof Error ? error.message : 'Failed to verify finance entry.'
      );
    } finally {
      setConfirmVerifyId(null);
    }
  }

  async function confirmDelete() {
    if (!confirmDeleteId) return;

    const auth = getStoredAuth();

    if (!auth) {
      setPageError('You are not logged in.');
      return;
    }

    try {
      setPageError(null);
      setSuccessMessage(null);

      await deleteFinanceEntry(auth.token, confirmDeleteId);
      setSuccessMessage('Finance entry deleted successfully.');
      await loadFinance();
    } catch (error) {
      setPageError(
        error instanceof Error ? error.message : 'Failed to delete finance entry.'
      );
    } finally {
      setConfirmDeleteId(null);
    }
  }

  function openCorrection(entry: FinanceEntry) {
    if (!entry.isVerified) {
      setPageError('Only verified entries need correction. Edit this entry instead.');
      setSuccessMessage(null);
      return;
    }

    setCorrectionEntry(entry);
    setCorrectionForm({
      category: entry.category,
      amount: 0,
      paymentMethod: entry.paymentMethod,
      reason: ''
    });

    setFieldErrors({});
    setPageError(null);
    setSuccessMessage(null);
  }

  async function submitCorrection() {
    setSuccessMessage(null);

    if (!correctionEntry || !validateCorrectionForm()) return;

    const auth = getStoredAuth();

    if (!auth) {
      setPageError('You are not logged in.');
      return;
    }

    try {
      setIsSaving(true);
      setPageError(null);

      await createFinanceCorrection(auth.token, correctionEntry.id, {
        category: correctionForm.category,
        amount: correctionForm.amount,
        paymentMethod: correctionForm.paymentMethod,
        reason: correctionForm.reason.trim()
      });

      setSuccessMessage('Correction created successfully.');
      setCorrectionEntry(null);
      setCorrectionForm(emptyCorrectionForm);
      setFieldErrors({});
      await loadFinance();
    } catch (error) {
      setPageError(
        error instanceof Error ? error.message : 'Failed to create correction.'
      );
    } finally {
      setIsSaving(false);
    }
  }

  const totalAmount = entries.reduce((sum, entry) => sum + entry.amount, 0);

  return (
    <>
      <h1 className="page-title">Financial Entry</h1>

      <div className="alert" style={{ marginBottom: 18 }}>
        Finance records can be edited only before verification. After verification,
        use correction entries.
      </div>

      <ErrorBanner message={pageError} />
      <SuccessBanner message={successMessage} />

      <Card title={editingId ? 'Edit Finance Entry' : 'Capture Finance Entry'}>
        <div className="form-grid">
          <div className="field">
            <label>Service Date</label>
            <input
              type="date"
              value={form.serviceDate}
              disabled={isSaving}
              onChange={(event) =>
                setForm((current) => ({ ...current, serviceDate: event.target.value }))
              }
            />
            {fieldErrors.serviceDate && <div className="field-error">{fieldErrors.serviceDate}</div>}
          </div>

          <div className="field">
            <label>Service Type</label>
            <select
              value={form.serviceType}
              disabled={isSaving}
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

          <div className="field">
            <label>Category</label>
            <select
              value={form.category}
              disabled={isSaving}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  category: Number(event.target.value) as FinanceCategory
                }))
              }
            >
              <option value={1}>Tithe</option>
              <option value={2}>Thanksgiving</option>
              <option value={3}>Normal Offering</option>
              <option value={4}>Special Offering</option>
              <option value={5}>Building Fund</option>
              <option value={6}>Other</option>
            </select>
          </div>

          <div className="field">
            <label>Amount</label>
            <input
              type="number"
              min="0"
              value={form.amount}
              disabled={isSaving}
              onChange={(event) =>
                setForm((current) => ({ ...current, amount: Number(event.target.value) }))
              }
            />
            {fieldErrors.amount && <div className="field-error">{fieldErrors.amount}</div>}
          </div>

          <div className="field">
            <label>Payment Method</label>
            <select
              value={form.paymentMethod}
              disabled={isSaving}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  paymentMethod: Number(event.target.value) as PaymentMethod
                }))
              }
            >
              <option value={1}>Cash</option>
              <option value={2}>EFT</option>
              <option value={3}>Card</option>
              <option value={4}>Bank Deposit</option>
            </select>
          </div>

          <div className="field" style={{ gridColumn: '1 / -1' }}>
            <label>Notes</label>
            <textarea
              rows={3}
              value={form.notes}
              disabled={isSaving}
              onChange={(event) =>
                setForm((current) => ({ ...current, notes: event.target.value }))
              }
            />
            {fieldErrors.notes && <div className="field-error">{fieldErrors.notes}</div>}
          </div>
        </div>

        <div style={{ display: 'flex', gap: 10, marginTop: 16 }}>
          <button className="primary-btn" onClick={saveFinanceEntry} disabled={isSaving}>
            {isSaving ? 'Saving...' : editingId ? 'Save Changes' : 'Save Finance Entry'}
          </button>

          {editingId && (
            <button className="secondary-btn" onClick={cancelEdit} disabled={isSaving}>
              Cancel
            </button>
          )}
        </div>
      </Card>

      <div style={{ height: 18 }} />

      <Card title={`Finance Records — Total: R${totalAmount.toLocaleString()}`}>
        {isLoading ? (
          <Loader message="Loading finance entries..." />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('serviceDate')}>
                      Date{getSortLabel('serviceDate')}
                    </button>
                  </th>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('serviceType')}>
                      Service{getSortLabel('serviceType')}
                    </button>
                  </th>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('category')}>
                      Category{getSortLabel('category')}
                    </button>
                  </th>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('paymentMethod')}>
                      Method{getSortLabel('paymentMethod')}
                    </button>
                  </th>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('amount')}>
                      Amount{getSortLabel('amount')}
                    </button>
                  </th>
                  <th>
                    <button className="table-sort-btn" onClick={() => requestSort('isVerified')}>
                      Status{getSortLabel('isVerified')}
                    </button>
                  </th>
                  <th>Correction Of</th>
                  <th>Actions</th>
                </tr>
              </thead>

              <tbody>
                {sortedItems.map((entry) => (
                  <tr key={entry.id}>
                    <td>{entry.serviceDate}</td>
                    <td>{serviceTypeLabels[entry.serviceType]}</td>
                    <td>{categoryLabels[entry.category]}</td>
                    <td>{paymentMethodLabels[entry.paymentMethod]}</td>
                    <td><strong>R{entry.amount.toLocaleString()}</strong></td>
                    <td>{entry.isVerified ? 'Verified' : 'Pending'}</td>
                    <td>{entry.correctionOfFinanceEntryId ? 'Correction' : '-'}</td>
                    <td>
                      <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                        <button className="secondary-btn" disabled={entry.isVerified} onClick={() => editEntry(entry)}>
                          Edit
                        </button>

                        {!entry.isVerified && (
                          <button className="secondary-btn" onClick={() => setConfirmVerifyId(entry.id)}>
                            Verify
                          </button>
                        )}

                        {entry.isVerified && (
                          <button className="secondary-btn" onClick={() => openCorrection(entry)}>
                            Correction
                          </button>
                        )}

                        <button className="danger-btn" disabled={entry.isVerified} onClick={() => setConfirmDeleteId(entry.id)}>
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}

                {entries.length === 0 && (
                  <tr>
                    <td colSpan={8}>No finance entries found.</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {confirmVerifyId && (
        <ConfirmModal
          title="Verify finance entry"
          message="Verify this finance entry? After verification it cannot be edited or deleted."
          confirmLabel="Verify"
          onCancel={() => setConfirmVerifyId(null)}
          onConfirm={confirmVerify}
        />
      )}

      {confirmDeleteId && (
        <ConfirmModal
          title="Delete finance entry"
          message="Delete this unverified finance entry? Verified finance entries must use correction entries instead."
          confirmLabel="Delete"
          onCancel={() => setConfirmDeleteId(null)}
          onConfirm={confirmDelete}
        />
      )}

      {correctionEntry && (
        <div className="modal-backdrop">
          <div className="confirm-modal">
            <h2>Create Correction</h2>
            <p>
              Original amount:{' '}
              <strong>R{correctionEntry.amount.toLocaleString()}</strong>
            </p>

            <div className="field">
              <label>Category</label>
              <select
                value={correctionForm.category}
                disabled={isSaving}
                onChange={(event) =>
                  setCorrectionForm((current) => ({
                    ...current,
                    category: Number(event.target.value) as FinanceCategory
                  }))
                }
              >
                <option value={1}>Tithe</option>
                <option value={2}>Thanksgiving</option>
                <option value={3}>Normal Offering</option>
                <option value={4}>Special Offering</option>
                <option value={5}>Building Fund</option>
                <option value={6}>Other</option>
              </select>
            </div>

            <div className="field">
              <label>Correction Amount</label>
              <input
                type="number"
                value={correctionForm.amount}
                disabled={isSaving}
                onChange={(event) =>
                  setCorrectionForm((current) => ({ ...current, amount: Number(event.target.value) }))
                }
              />
              {fieldErrors.amount && <div className="field-error">{fieldErrors.amount}</div>}
            </div>

            <div className="field">
              <label>Payment Method</label>
              <select
                value={correctionForm.paymentMethod}
                disabled={isSaving}
                onChange={(event) =>
                  setCorrectionForm((current) => ({
                    ...current,
                    paymentMethod: Number(event.target.value) as PaymentMethod
                  }))
                }
              >
                <option value={1}>Cash</option>
                <option value={2}>EFT</option>
                <option value={3}>Card</option>
                <option value={4}>Bank Deposit</option>
              </select>
            </div>

            <div className="field">
              <label>Reason</label>
              <textarea
                rows={3}
                value={correctionForm.reason}
                disabled={isSaving}
                onChange={(event) =>
                  setCorrectionForm((current) => ({ ...current, reason: event.target.value }))
                }
              />
              {fieldErrors.reason && <div className="field-error">{fieldErrors.reason}</div>}
            </div>

            <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end' }}>
              <button className="secondary-btn" onClick={() => setCorrectionEntry(null)} disabled={isSaving}>
                Cancel
              </button>

              <button className="primary-btn" onClick={submitCorrection} disabled={isSaving}>
                {isSaving ? 'Saving...' : 'Save Correction'}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

function ConfirmModal({
  title,
  message,
  confirmLabel,
  onCancel,
  onConfirm
}: {
  title: string;
  message: string;
  confirmLabel: string;
  onCancel: () => void;
  onConfirm: () => void;
}) {
  return (
    <div className="modal-backdrop">
      <div className="confirm-modal">
        <h2>{title}</h2>
        <p>{message}</p>

        <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end' }}>
          <button className="secondary-btn" onClick={onCancel}>
            Cancel
          </button>

          <button className="danger-btn" onClick={onConfirm}>
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}