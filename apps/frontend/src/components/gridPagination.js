const paginationPageSizes = Object.freeze([25, 50, 100])

const enabledPaginationOptions = Object.freeze({
  pagination: true,
  paginationPageSize: 25,
  paginationPageSizeSelector: paginationPageSizes,
})

export function getPaginationOptions(enabled = false) {
  if (!enabled) return {}

  return {
    ...enabledPaginationOptions,
    paginationPageSizeSelector: [...paginationPageSizes],
  }
}
