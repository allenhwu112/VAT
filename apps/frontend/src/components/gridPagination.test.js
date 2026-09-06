import assert from 'node:assert/strict'
import test from 'node:test'
import { getPaginationOptions } from './gridPagination.js'

test('keeps shared management grids non-paginated by default', () => {
  assert.deepEqual(getPaginationOptions(), {})
})

test('configures the invoice grid with the requested client-side page sizes', () => {
  assert.deepEqual(getPaginationOptions(true), {
    pagination: true,
    paginationPageSize: 25,
    paginationPageSizeSelector: [25, 50, 100],
  })
})
