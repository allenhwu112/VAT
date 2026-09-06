import { useEffect, useState } from 'react'
import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogTitle from '@mui/material/DialogTitle'
import FormControl from '@mui/material/FormControl'
import FormHelperText from '@mui/material/FormHelperText'
import InputLabel from '@mui/material/InputLabel'
import MenuItem from '@mui/material/MenuItem'
import Select from '@mui/material/Select'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import {
  createEmployee,
  emptyEmployeeForm,
  toEmployeeRequest,
  updateEmployee,
  validateEmployeeForm,
} from '../api/employees'

function getInitialForm(mode, employee) {
  if (mode === 'edit' && employee) {
    return {
      name: employee.name,
      shortName: employee.shortName,
      gender: employee.gender,
      nationalId: employee.nationalId,
      password: '',
      contactPhone: employee.contactPhone || '',
      address: employee.address || '',
      birthDate: employee.birthDate || '',
    }
  }

  return { ...emptyEmployeeForm }
}

export function EmployeeDialog({ mode, employee, open, onClose, onSaved }) {
  const [form, setForm] = useState(() => getInitialForm(mode, employee))
  const [errors, setErrors] = useState({})
  const [submitError, setSubmitError] = useState('')
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    if (open) {
      setForm(getInitialForm(mode, employee))
      setErrors({})
      setSubmitError('')
    }
  }, [employee, mode, open])

  const handleChange = (field) => (event) => {
    setForm((current) => ({ ...current, [field]: event.target.value }))
    setErrors((current) => ({ ...current, [field]: undefined }))
    setSubmitError('')
  }

  const handleSubmit = async (event) => {
    event.preventDefault()

    const validationErrors = validateEmployeeForm(form, mode)
    setErrors(validationErrors)
    if (Object.keys(validationErrors).length > 0) {
      return
    }

    setSaving(true)
    setSubmitError('')

    try {
      const request = toEmployeeRequest(form, mode)
      const savedEmployee =
        mode === 'create'
          ? await createEmployee(request)
          : await updateEmployee(employee.employeeId, request)

      await onSaved(savedEmployee)
    } catch (error) {
      setSubmitError(error instanceof Error ? error.message : '儲存員工資料失敗。')
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
      aria-labelledby="employee-dialog-title"
    >
      <form onSubmit={handleSubmit} noValidate>
        <DialogTitle id="employee-dialog-title">
          {mode === 'create' ? '新增員工' : '修改員工'}
        </DialogTitle>

        <DialogContent dividers>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <TextField
              autoFocus
              fullWidth
              label="姓名"
              value={form.name}
              onChange={handleChange('name')}
              error={Boolean(errors.name)}
              helperText={errors.name}
              inputProps={{ maxLength: 100 }}
              required
            />

            <TextField
              fullWidth
              label="簡稱"
              value={form.shortName}
              onChange={handleChange('shortName')}
              error={Boolean(errors.shortName)}
              helperText={errors.shortName}
              inputProps={{ maxLength: 50 }}
              required
            />

            <FormControl fullWidth error={Boolean(errors.gender)} required>
              <InputLabel id="employee-gender-label">性別</InputLabel>
              <Select
                labelId="employee-gender-label"
                value={form.gender}
                label="性別"
                onChange={handleChange('gender')}
              >
                <MenuItem value="M">男</MenuItem>
                <MenuItem value="F">女</MenuItem>
              </Select>
              <FormHelperText>{errors.gender}</FormHelperText>
            </FormControl>

            <TextField
              fullWidth
              label="身份證字號"
              value={form.nationalId}
              onChange={handleChange('nationalId')}
              error={Boolean(errors.nationalId)}
              helperText={errors.nationalId || '格式：一碼英文字母加九碼數字。'}
              inputProps={{ maxLength: 10, style: { textTransform: 'uppercase' } }}
              required
            />

            <TextField
              fullWidth
              label="聯絡電話"
              value={form.contactPhone}
              onChange={handleChange('contactPhone')}
              error={Boolean(errors.contactPhone)}
              helperText={errors.contactPhone || '選填，最多 30 個字元。'}
              inputProps={{ maxLength: 30 }}
            />

            <TextField
              fullWidth
              label="地址"
              value={form.address}
              onChange={handleChange('address')}
              error={Boolean(errors.address)}
              helperText={errors.address || '選填，最多 255 個字元。'}
              inputProps={{ maxLength: 255 }}
              multiline
              minRows={2}
            />

            <TextField
              fullWidth
              label="出生年月日"
              type="date"
              value={form.birthDate}
              onChange={handleChange('birthDate')}
              error={Boolean(errors.birthDate)}
              helperText={errors.birthDate || '選填，格式：YYYY-MM-DD。'}
              InputLabelProps={{ shrink: true }}
            />

            <TextField
              fullWidth
              label={mode === 'create' ? '密碼' : '密碼（留白表示不變更）'}
              type="password"
              value={form.password}
              onChange={handleChange('password')}
              error={Boolean(errors.password)}
              helperText={errors.password}
              inputProps={{ maxLength: 255 }}
              required={mode === 'create'}
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
