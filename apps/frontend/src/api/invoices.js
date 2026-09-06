const apiBaseUrl = (import.meta.env?.VITE_API_BASE_URL || 'http://localhost:5000').replace(
  /\/+$/,
  '',
)

const taxIdPattern = /^[0-9]{8}$/

export const invoiceQuantityFields = [
  'cashRegister',
  'threeCashRegister',
  'twoPartInvoice',
  'twoPartInvoiceCopy',
  'threePartInvoice',
  'threePartInvoiceCopy',
]

export const emptyInvoiceForm = {
  taxId: '',
  clientShortName: '',
  electronicInvoice: false,
  cashRegister: '',
  threeCashRegister: '',
  twoPartInvoice: '',
  twoPartInvoiceCopy: '',
  threePartInvoice: '',
  threePartInvoiceCopy: '',
}

function parseQuantity(value) {
  if (typeof value === 'number') return value
  if (typeof value !== 'string' || !value.trim()) return null

  return Number(value)
}

export function validateInvoiceForm(values) {
  const errors = {}
  const taxId = values.taxId?.trim() || ''
  const clientShortName = values.clientShortName?.trim() || ''

  if (!taxId) {
    errors.taxId = '統編為必填欄位。'
  } else if (!taxIdPattern.test(taxId)) {
    errors.taxId = '統編格式不正確，請輸入 8 碼數字。'
  }

  if (!clientShortName) {
    errors.clientShortName = '客戶簡稱為必填欄位。'
  } else if (clientShortName.length > 50) {
    errors.clientShortName = '客戶簡稱不可超過 50 個字元。'
  }

  for (const field of invoiceQuantityFields) {
    const quantity = parseQuantity(values[field])
    if (!Number.isInteger(quantity) || quantity < 0 || quantity > 99) {
      errors[field] = '請輸入 0 到 99 的整數。'
    }
  }

  return errors
}

export function toInvoiceRequest(values) {
  return {
    taxId: values.taxId.trim(),
    clientShortName: values.clientShortName.trim(),
    electronicInvoice: Boolean(values.electronicInvoice),
    ...Object.fromEntries(
      invoiceQuantityFields.map((field) => [field, parseQuantity(values[field])]),
    ),
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

export async function fetchInvoices() {
  const body = await request('/VAT_API/invoices')
  return body?.data || []
}

export async function createInvoice(invoice) {
  const body = await request('/VAT_API/invoices', {
    method: 'POST',
    body: JSON.stringify(invoice),
  })
  return body.data
}

export async function updateInvoice(invoiceId, invoice) {
  const body = await request(`/VAT_API/invoices/${invoiceId}`, {
    method: 'PUT',
    body: JSON.stringify(invoice),
  })
  return body.data
}

export async function deleteInvoice(invoiceId) {
  await request(`/VAT_API/invoices/${invoiceId}`, { method: 'DELETE' })
}
