export const UI_PREFIX = '/VAT_UI'

const uiPaths = Object.freeze({
  employees: `${UI_PREFIX}/employees`,
  clients: `${UI_PREFIX}/clients`,
  invoices: `${UI_PREFIX}/invoices`,
})

function normalizePath(pathname) {
  if (!pathname || pathname === '/') return '/'
  return pathname.length > 1 ? pathname.replace(/\/+$/, '') : pathname
}

export function getUiPath(page) {
  return uiPaths[page] || uiPaths.employees
}

export function resolveUiPage(pathname) {
  const normalizedPath = normalizePath(pathname)

  if (normalizedPath === uiPaths.clients) return 'clients'
  if (normalizedPath === uiPaths.invoices) return 'invoices'
  return 'employees'
}
