const apiBaseUrl = (import.meta.env?.VITE_API_BASE_URL || 'http://localhost:5000').replace(
  /\/+$/,
  '',
)

const taxIdPattern = /^[0-9]{8}$/
const clientCodePattern = /^[A-Za-z][0-9]{3}$/

export const emptyClientForm = {
  clientCode: '',
  taxId: '',
  fullName: '',
  shortName: '',
  responsiblePerson: '',
  address: '',
}

export function validateClientForm(values) {
  const errors = {}
  const clientCode = values.clientCode?.trim() || ''
  const taxId = values.taxId?.trim() || ''
  const fullName = values.fullName?.trim() || ''
  const shortName = values.shortName?.trim() || ''
  const responsiblePerson = values.responsiblePerson?.trim() || ''
  const address = values.address?.trim() || ''

  if (clientCode && !clientCodePattern.test(clientCode)) {
    errors.clientCode = '客編格式不正確，請輸入 1 碼英文加 3 碼數字。'
  }

  if (!taxId) {
    errors.taxId = '統編為必填欄位。'
  } else if (!taxIdPattern.test(taxId)) {
    errors.taxId = '統編格式不正確，請輸入 8 碼數字。'
  }

  if (fullName.length > 100) {
    errors.fullName = '客戶全稱不可超過 100 個字元。'
  }

  if (shortName.length > 50) {
    errors.shortName = '簡稱不可超過 50 個字元。'
  }

  if (responsiblePerson.length > 100) {
    errors.responsiblePerson = '負責人不可超過 100 個字元。'
  }

  if (address.length > 255) {
    errors.address = '地址不可超過 255 個字元。'
  }

  return errors
}

export function toClientRequest(values) {
  const optionalText = (value) => {
    const normalized = value?.trim() || ''
    return normalized || null
  }

  const optionalClientCode = (value) => {
    const normalized = value?.trim() || ''
    return normalized ? normalized.toUpperCase() : null
  }

  return {
    clientCode: optionalClientCode(values.clientCode),
    taxId: values.taxId.trim(),
    fullName: optionalText(values.fullName),
    shortName: optionalText(values.shortName),
    responsiblePerson: optionalText(values.responsiblePerson),
    address: optionalText(values.address),
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

export async function fetchClients() {
  const body = await request('/VAT_API/clients')
  return body?.data || []
}

export async function createClient(client) {
  const body = await request('/VAT_API/clients', {
    method: 'POST',
    body: JSON.stringify(client),
  })
  return body.data
}

export async function updateClient(clientId, client) {
  const body = await request(`/VAT_API/clients/${clientId}`, {
    method: 'PUT',
    body: JSON.stringify(client),
  })
  return body.data
}

export async function deleteClient(clientId) {
  await request(`/VAT_API/clients/${clientId}`, { method: 'DELETE' })
}
