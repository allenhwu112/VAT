import { useEffect, useState } from 'react'
import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogTitle from '@mui/material/DialogTitle'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import {
  createClient,
  emptyClientForm,
  toClientRequest,
  updateClient,
  validateClientForm,
} from '../api/clients'

function getInitialForm(mode, client) {
  if (mode === 'edit' && client) {
    return {
      clientCode: client.clientCode ?? '',
      taxId: client.taxId,
      fullName: client.fullName ?? '',
      shortName: client.shortName ?? '',
      responsiblePerson: client.responsiblePerson ?? '',
      address: client.address ?? '',
    }
  }

  return { ...emptyClientForm }
}

export function ClientDialog({ mode, client, open, onClose, onSaved }) {
  const [form, setForm] = useState(() => getInitialForm(mode, client))
  const [errors, setErrors] = useState({})
  const [submitError, setSubmitError] = useState('')
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    if (open) {
      setForm(getInitialForm(mode, client))
      setErrors({})
      setSubmitError('')
    }
  }, [client, mode, open])

  const handleChange = (field) => (event) => {
    setForm((current) => ({ ...current, [field]: event.target.value }))
    setErrors((current) => ({ ...current, [field]: undefined }))
    setSubmitError('')
  }

  const handleSubmit = async (event) => {
    event.preventDefault()

    const validationErrors = validateClientForm(form)
    setErrors(validationErrors)
    if (Object.keys(validationErrors).length > 0) {
      return
    }

    setSaving(true)
    setSubmitError('')

    try {
      const request = toClientRequest(form)
      const savedClient =
        mode === 'create'
          ? await createClient(request)
          : await updateClient(client.clientId, request)

      await onSaved(savedClient)
    } catch (error) {
      setSubmitError(error instanceof Error ? error.message : '儲存客戶資料失敗。')
    } finally {
      setSaving(false)
    }
  }

  return (
    <Dialog
      open={open}
      onClose={saving ? undefined : onClose}
      fullWidth
      maxWidth="sm"
      aria-labelledby="client-dialog-title"
    >
      <form onSubmit={handleSubmit} noValidate>
        <DialogTitle id="client-dialog-title">
          {mode === 'create' ? '新增客戶' : '修改客戶'}
        </DialogTitle>

        <DialogContent dividers>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <TextField
              fullWidth
              label="客編"
              value={form.clientCode}
              onChange={handleChange('clientCode')}
              error={Boolean(errors.clientCode)}
              helperText={errors.clientCode || '選填，格式：A001。'}
              inputProps={{ maxLength: 4, inputMode: 'text' }}
            />

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
              label="客戶全稱"
              value={form.fullName}
              onChange={handleChange('fullName')}
              error={Boolean(errors.fullName)}
              helperText={errors.fullName}
              inputProps={{ maxLength: 100 }}
            />

            <TextField
              fullWidth
              label="簡稱"
              value={form.shortName}
              onChange={handleChange('shortName')}
              error={Boolean(errors.shortName)}
              helperText={errors.shortName}
              inputProps={{ maxLength: 50 }}
            />

            <TextField
              fullWidth
              label="負責人"
              value={form.responsiblePerson}
              onChange={handleChange('responsiblePerson')}
              error={Boolean(errors.responsiblePerson)}
              helperText={errors.responsiblePerson}
              inputProps={{ maxLength: 100 }}
            />

            <TextField
              fullWidth
              label="地址"
              value={form.address}
              onChange={handleChange('address')}
              error={Boolean(errors.address)}
              helperText={errors.address}
              inputProps={{ maxLength: 255 }}
              multiline
              minRows={2}
            />

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
