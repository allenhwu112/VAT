const apiBaseUrl = (import.meta.env?.VITE_API_BASE_URL || 'http://localhost:5000').replace(
  /\/+$/,
  '',
)

const taxIdPattern = /^[0-9]{8}$/

export const emptyClientForm = {
  taxId: '',
  fullName: '',
  shortName: '',
  responsiblePerson: '',
  address: '',
}

export function validateClientForm(values) {
  const errors = {}
  const taxId = values.taxId?.trim() || ''
  const fullName = values.fullName?.trim() || ''
  const shortName = values.shortName?.trim() || ''
  const responsiblePerson = values.responsiblePerson?.trim() || ''
  const address = values.address?.trim() || ''

  if (!taxId) {
    errors.taxId = '統編為必填欄位。'
  } else if (!taxIdPattern.test(taxId)) {
    errors.taxId = '統編格式不正確，請輸入 8 碼數字。'
  }

  if (!fullName) {
    errors.fullName = '客戶全稱為必填欄位。'
  } else if (fullName.length > 100) {
    errors.fullName = '客戶全稱不可超過 100 個字元。'
  }

  if (!shortName) {
    errors.shortName = '簡稱為必填欄位。'
  } else if (shortName.length > 50) {
    errors.shortName = '簡稱不可超過 50 個字元。'
  }

  if (!responsiblePerson) {
    errors.responsiblePerson = '負責人為必填欄位。'
  } else if (responsiblePerson.length > 100) {
    errors.responsiblePerson = '負責人不可超過 100 個字元。'
  }

  if (!address) {
    errors.address = '地址為必填欄位。'
  } else if (address.length > 255) {
    errors.address = '地址不可超過 255 個字元。'
  }

  return errors
}

export function toClientRequest(values) {
  return {
    taxId: values.taxId.trim(),
    fullName: values.fullName.trim(),
    shortName: values.shortName.trim(),
    responsiblePerson: values.responsiblePerson.trim(),
    address: values.address.trim(),
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
