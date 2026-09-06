import assert from 'node:assert/strict'
import test from 'node:test'
import { UI_PREFIX, getUiPath, resolveUiPage } from './routes.js'

test('defines the prefixed employee, client, and invoice paths', () => {
  assert.equal(UI_PREFIX, '/VAT_UI')
  assert.equal(getUiPath('employees'), '/VAT_UI/employees')
  assert.equal(getUiPath('clients'), '/VAT_UI/clients')
  assert.equal(getUiPath('invoices'), '/VAT_UI/invoices')
})

test('resolves direct management URLs to their corresponding page', () => {
  assert.equal(resolveUiPage('/VAT_UI/employees'), 'employees')
  assert.equal(resolveUiPage('/VAT_UI/clients'), 'clients')
  assert.equal(resolveUiPage('/VAT_UI/invoices'), 'invoices')
})

test('uses the employee page for root, trailing slash, and unknown paths', () => {
  assert.equal(resolveUiPage('/'), 'employees')
  assert.equal(resolveUiPage('/VAT_UI/'), 'employees')
  assert.equal(resolveUiPage('/VAT_UI/unknown'), 'employees')
})

test('falls back to the employee path for an unknown page name', () => {
  assert.equal(getUiPath('unknown'), '/VAT_UI/employees')
})
