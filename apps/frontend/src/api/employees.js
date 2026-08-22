const apiBaseUrl = (import.meta.env?.VITE_API_BASE_URL || 'http://localhost:5000').replace(
  /\/+$/,
  '',
)

const nationalIdPattern = /^[A-Z][0-9]{9}$/

export const emptyEmployeeForm = {
  name: '',
  shortName: '',
  gender: 'M',
  nationalId: '',
  password: '',
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
  const body = await request('/api/employees')
  return body?.data || []
}

export async function createEmployee(employee) {
  const body = await request('/api/employees', {
    method: 'POST',
    body: JSON.stringify(employee),
  })
  return body.data
}

export async function updateEmployee(employeeId, employee) {
  const body = await request(`/api/employees/${employeeId}`, {
    method: 'PUT',
    body: JSON.stringify(employee),
  })
  return body.data
}

export async function deleteEmployee(employeeId) {
  await request(`/api/employees/${employeeId}`, { method: 'DELETE' })
}
