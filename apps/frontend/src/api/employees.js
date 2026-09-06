const apiBaseUrl = (import.meta.env?.VITE_API_BASE_URL || 'http://localhost:5000').replace(
  /\/+$/,
  '',
)

const nationalIdPattern = /^[A-Z][0-9]{9}$/
const isoDatePattern = /^\d{4}-\d{2}-\d{2}$/

function isValidIsoDate(value) {
  if (!value) return true
  if (!isoDatePattern.test(value)) return false

  const [year, month, day] = value.split('-').map(Number)
  const date = new Date(Date.UTC(year, month - 1, day))

  return (
    date.getUTCFullYear() === year &&
    date.getUTCMonth() === month - 1 &&
    date.getUTCDate() === day
  )
}

export const emptyEmployeeForm = {
  name: '',
  shortName: '',
  gender: 'M',
  nationalId: '',
  password: '',
  contactPhone: '',
  address: '',
  birthDate: '',
}

export function validateEmployeeForm(values, mode = 'create') {
  const errors = {}

  if (!values.name?.trim()) {
    errors.name = '姓名為必填欄位。'
  } else if (values.name.trim().length > 100) {
    errors.name = '姓名不可超過 100 個字元。'
  }

  if (!values.shortName?.trim()) {
    errors.shortName = '簡稱為必填欄位。'
  } else if (values.shortName.trim().length > 50) {
    errors.shortName = '簡稱不可超過 50 個字元。'
  }

  if (!['M', 'F'].includes(values.gender)) {
    errors.gender = '性別格式不正確。'
  }

  const nationalId = values.nationalId?.trim().toUpperCase() || ''
  if (!nationalIdPattern.test(nationalId)) {
    errors.nationalId = '身份證字號格式不正確。'
  }

  const contactPhone = values.contactPhone?.trim() || ''
  if (contactPhone.length > 30) {
    errors.contactPhone = '聯絡電話不可超過 30 個字元。'
  }

  const address = values.address?.trim() || ''
  if (address.length > 255) {
    errors.address = '地址不可超過 255 個字元。'
  }

  if (!isValidIsoDate(values.birthDate)) {
    errors.birthDate = '出生年月日格式不正確。'
  }

  if (mode === 'create' && !values.password) {
    errors.password = '密碼為必填欄位。'
  } else if (values.password?.length > 255) {
    errors.password = '密碼不可超過 255 個字元。'
  }

  return errors
}

export function toEmployeeRequest(values, mode = 'create') {
  return {
    name: values.name.trim(),
    shortName: values.shortName.trim(),
    gender: values.gender,
    nationalId: values.nationalId.trim().toUpperCase(),
    password: mode === 'edit' && !values.password ? null : values.password,
    contactPhone: values.contactPhone?.trim() || null,
    address: values.address?.trim() || null,
    birthDate: values.birthDate || null,
  }
}

async function request(path, options = {}) {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...options,
    headers: {
      Accept: 'application/json',
      ...(options.body ? { 'Content-Type': 'application/json' } : {}),
      ...options.headers,
    },
  })

  const text = await response.text()
  const body = text ? JSON.parse(text) : null

  if (!response.ok) {
    const message =
      body?.detail ||
      body?.title ||
      body?.error?.message ||
      `API 回應失敗（HTTP ${response.status}）。`
    throw new Error(message)
  }

  return body
}

export async function fetchEmployees() {
  const body = await request('/VAT_API/employees')
  return body?.data || []
}

export async function createEmployee(employee) {
  const body = await request('/VAT_API/employees', {
    method: 'POST',
    body: JSON.stringify(employee),
  })
  return body.data
}

export async function updateEmployee(employeeId, employee) {
  const body = await request(`/VAT_API/employees/${employeeId}`, {
    method: 'PUT',
    body: JSON.stringify(employee),
  })
  return body.data
}

export async function deleteEmployee(employeeId) {
  await request(`/VAT_API/employees/${employeeId}`, { method: 'DELETE' })
}
