import assert from 'node:assert/strict'
import fs from 'node:fs'
import test from 'node:test'

const clientsPageSource = fs.readFileSync(
  new URL('./ClientsPage.jsx', import.meta.url),
  'utf8',
)

test('labels the client code before the tax ID column', () => {
  const clientCodeColumn = clientsPageSource.indexOf(
    "{ field: 'clientCode', headerName: '客編'",
  )
  const taxIdColumn = clientsPageSource.indexOf(
    "{ field: 'taxId', headerName: '統編'",
  )

  assert.notEqual(clientCodeColumn, -1)
  assert.notEqual(taxIdColumn, -1)
  assert.ok(clientCodeColumn < taxIdColumn)
})
