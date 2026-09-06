import assert from 'node:assert/strict'
import test from 'node:test'
import {
  emptyInvoiceForm,
  invoiceQuantityFields,
  toInvoiceRequest,
  validateInvoiceForm,
} from './invoices.js'

const quantityNames = [
  'cashRegister',
  'threeCashRegister',
  'twoPartInvoice',
  'twoPartInvoiceCopy',
  'threePartInvoice',
  'threePartInvoiceCopy',
]

const validForm = {
  taxId: '12345678',
  clientShortName: '測試客戶',
  electronicInvoice: true,
  cashRegister: '0',
  threeCashRegister: '99',
  twoPartInvoice: '1',
  twoPartInvoiceCopy: '2',
  threePartInvoice: '3',
  threePartInvoiceCopy: '4',
}

test('defines an empty invoice form with a Boolean flag and blank quantity inputs', () => {
  assert.deepEqual(emptyInvoiceForm, {
    taxId: '',
    clientShortName: '',
    electronicInvoice: false,
    cashRegister: '',
    threeCashRegister: '',
    twoPartInvoice: '',
    twoPartInvoiceCopy: '',
    threePartInvoice: '',
    threePartInvoiceCopy: '',
  })
})

test('accepts valid invoice quantities including zero and 99', () => {
  assert.deepEqual(validateInvoiceForm(validForm), {})
})

test('requires the invoice tax ID, client short name, and every quantity', () => {
  const errors = validateInvoiceForm(emptyInvoiceForm)

  assert.equal(errors.taxId, '統編為必填欄位。')
  assert.equal(errors.clientShortName, '客戶簡稱為必填欄位。')
  for (const field of quantityNames) {
    assert.equal(errors[field], '請輸入 0 到 99 的整數。')
  }
})

test('rejects invalid tax IDs, short names, and invoice quantities', () => {
  const errors = validateInvoiceForm({
    ...validForm,
    taxId: '1234567A',
    clientShortName: '簡'.repeat(51),
    cashRegister: '-1',
    threeCashRegister: '100',
    twoPartInvoice: '1.5',
    twoPartInvoiceCopy: 'abc',
  })

  assert.equal(errors.taxId, '統編格式不正確，請輸入 8 碼數字。')
  assert.equal(errors.clientShortName, '客戶簡稱不可超過 50 個字元。')
  assert.equal(errors.cashRegister, '請輸入 0 到 99 的整數。')
  assert.equal(errors.threeCashRegister, '請輸入 0 到 99 的整數。')
  assert.equal(errors.twoPartInvoice, '請輸入 0 到 99 的整數。')
  assert.equal(errors.twoPartInvoiceCopy, '請輸入 0 到 99 的整數。')
})

test('trims identity fields and normalizes quantity inputs to numbers', () => {
  const request = toInvoiceRequest({
    taxId: ' 12345678 ',
    clientShortName: ' 測試客戶 ',
    electronicInvoice: false,
    cashRegister: ' 0 ',
    threeCashRegister: '99',
    twoPartInvoice: '1',
    twoPartInvoiceCopy: '2',
    threePartInvoice: '3',
    threePartInvoiceCopy: '4',
  })

  assert.equal(request.taxId, '12345678')
  assert.equal(request.clientShortName, '測試客戶')
  assert.equal(request.electronicInvoice, false)
  for (const field of invoiceQuantityFields) {
    assert.equal(typeof request[field], 'number')
    assert.equal(Number.isInteger(request[field]), true)
  }
})
