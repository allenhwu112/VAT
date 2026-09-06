import assert from 'node:assert/strict'
import test from 'node:test'
import { emptyClientForm, toClientRequest, validateClientForm } from './clients.js'

const validForm = {
  taxId: '12345678',
  fullName: '測試客戶股份有限公司',
  shortName: '測試客戶',
  responsiblePerson: '王小明',
  address: '台北市中正區測試路 1 號',
}

test('defines an empty client form with every required field', () => {
  assert.deepEqual(emptyClientForm, {
    taxId: '',
    fullName: '',
    shortName: '',
    responsiblePerson: '',
    address: '',
  })
})

test('accepts a valid client form', () => {
  assert.deepEqual(validateClientForm(validForm), {})
})

test('requires every client field', () => {
  const errors = validateClientForm(emptyClientForm)

  assert.equal(errors.taxId, '統編為必填欄位。')
  assert.equal(errors.fullName, '客戶全稱為必填欄位。')
  assert.equal(errors.shortName, '簡稱為必填欄位。')
  assert.equal(errors.responsiblePerson, '負責人為必填欄位。')
  assert.equal(errors.address, '地址為必填欄位。')
})

test('rejects invalid tax IDs and whitespace-only required fields', () => {
  const errors = validateClientForm({
    ...validForm,
    taxId: '1234567A',
    fullName: '   ',
  })

  assert.equal(errors.taxId, '統編格式不正確，請輸入 8 碼數字。')
  assert.equal(errors.fullName, '客戶全稱為必填欄位。')
})

test('rejects client values over the declared limits', () => {
  const errors = validateClientForm({
    ...validForm,
    fullName: '全'.repeat(101),
    shortName: '簡'.repeat(51),
    responsiblePerson: '人'.repeat(101),
    address: '址'.repeat(256),
  })

  assert.equal(errors.fullName, '客戶全稱不可超過 100 個字元。')
  assert.equal(errors.shortName, '簡稱不可超過 50 個字元。')
  assert.equal(errors.responsiblePerson, '負責人不可超過 100 個字元。')
  assert.equal(errors.address, '地址不可超過 255 個字元。')
})

test('trims all client fields before sending the API request', () => {
  assert.deepEqual(
    toClientRequest({
      taxId: ' 12345678 ',
      fullName: ' 測試客戶股份有限公司 ',
      shortName: ' 測試客戶 ',
      responsiblePerson: ' 王小明 ',
      address: ' 台北市中正區測試路 1 號 ',
    }),
    {
      taxId: '12345678',
      fullName: '測試客戶股份有限公司',
      shortName: '測試客戶',
      responsiblePerson: '王小明',
      address: '台北市中正區測試路 1 號',
    },
  )
})
