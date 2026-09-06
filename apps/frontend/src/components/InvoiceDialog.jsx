import { useEffect, useState } from 'react'
import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import Checkbox from '@mui/material/Checkbox'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogTitle from '@mui/material/DialogTitle'
import FormControlLabel from '@mui/material/FormControlLabel'
import FormGroup from '@mui/material/FormGroup'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import {
  createInvoice,
  emptyInvoiceForm,
  invoiceQuantityFields,
  toInvoiceRequest,
  updateInvoice,
  validateInvoiceForm,
} from '../api/invoices'

const optionLabels = {
  electronicInvoice: '電子發票',
  cashRegister: '收銀機',
  threeCashRegister: '三收銀',
  twoPartInvoice: '二聯式',
  twoPartInvoiceCopy: '二聯式副聯',
  threePartInvoice: '三聯式',
  threePartInvoiceCopy: '三聯式副聯',
}

const quantityFields = invoiceQuantityFields.map((field) => ({
  field,
  label: optionLabels[field],
}))

function getInitialForm(mode, invoice) {
  if (mode === 'edit' && invoice) {
    return {
      taxId: invoice.taxId,
      clientShortName: invoice.clientShortName,
      electronicInvoice: Boolean(invoice.electronicInvoice),
      ...Object.fromEntries(
        invoiceQuantityFields.map((field) => [field, invoice[field] ?? '']),
      ),
    }
  }

  return { ...emptyInvoiceForm }
}

export function InvoiceDialog({ mode, invoice, open, onClose, onSaved }) {
  const [form, setForm] = useState(() => getInitialForm(mode, invoice))
  const [errors, setErrors] = useState({})
  const [submitError, setSubmitError] = useState('')
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    if (open) {
      setForm(getInitialForm(mode, invoice))
      setErrors({})
      setSubmitError('')
    }
  }, [invoice, mode, open])

  const handleChange = (field) => (event) => {
    const value = event.target.type === 'checkbox' ? event.target.checked : event.target.value
    setForm((current) => ({ ...current, [field]: value }))
    setErrors((current) => ({ ...current, [field]: undefined }))
    setSubmitError('')
  }

  const handleSubmit = async (event) => {
    event.preventDefault()

    const validationErrors = validateInvoiceForm(form)
    setErrors(validationErrors)
    if (Object.keys(validationErrors).length > 0) {
      return
    }

    setSaving(true)
    setSubmitError('')

    try {
      const request = toInvoiceRequest(form)
      const savedInvoice =
        mode === 'create'
          ? await createInvoice(request)
          : await updateInvoice(invoice.invoiceId, request)

      await onSaved(savedInvoice)
    } catch (error) {
      setSubmitError(error instanceof Error ? error.message : '儲存發票管理資料失敗。')
    } finally {
      setSaving(false)
    }
  }

  return (
    <Dialog
      open={open}
      onClose={saving ? undefined : onClose}
      fullWidth
      maxWidth="md"
      aria-labelledby="invoice-dialog-title"
    >
      <form onSubmit={handleSubmit} noValidate>
        <DialogTitle id="invoice-dialog-title">
          {mode === 'create' ? '新增發票管理' : '修改發票管理'}
        </DialogTitle>

        <DialogContent dividers>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <TextField
              autoFocus
              fullWidth
              label="統編"
              value={form.taxId}
              onChange={handleChange('taxId')}
              error={Boolean(errors.taxId)}
              helperText={errors.taxId || '請輸入 8 碼數字。'}
              inputProps={{ maxLength: 8, inputMode: 'numeric' }}
              required
            />

            <TextField
              fullWidth
              label="客戶簡稱"
              value={form.clientShortName}
              onChange={handleChange('clientShortName')}
              error={Boolean(errors.clientShortName)}
              helperText={errors.clientShortName}
              inputProps={{ maxLength: 50 }}
              required
            />

            <FormGroup row>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={Boolean(form.electronicInvoice)}
                    onChange={handleChange('electronicInvoice')}
                  />
                }
                label={optionLabels.electronicInvoice}
              />
            </FormGroup>

            <Stack direction="row" flexWrap="wrap" gap={2}>
              {quantityFields.map(({ field, label }) => (
                <TextField
                  key={field}
                  label={label}
                  type="number"
                  value={form[field]}
                  onChange={handleChange(field)}
                  error={Boolean(errors[field])}
                  helperText={errors[field] || '請輸入 0 到 99 的整數。'}
                  inputProps={{ min: 0, max: 99, step: 1, inputMode: 'numeric' }}
                  required
                  sx={{ flex: '1 1 180px', minWidth: 180 }}
                />
              ))}
            </Stack>

            {submitError && <Alert severity="error">{submitError}</Alert>}
          </Stack>
        </DialogContent>

        <DialogActions>
          <Button type="button" onClick={onClose} disabled={saving}>
            取消
          </Button>
          <Button type="submit" variant="contained" disabled={saving}>
            {saving ? '儲存中…' : '儲存'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}
