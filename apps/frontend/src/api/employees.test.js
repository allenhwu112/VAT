import assert from 'node:assert/strict'
import test from 'node:test'
import { toEmployeeRequest, validateEmployeeForm } from './employees.js'

const validForm = {
  name: '王小明',
  shortName: '小明',
  gender: 'M',
  nationalId: 'A123456789',
  password: 'temporary-password',
  contactPhone: '02-1234-5678',
  address: '台北市中正區重慶南路一段 1 號',
  birthDate: '1990-01-02',
}

test('accepts a valid create employee form', () => {
  assert.deepEqual(validateEmployeeForm(validForm, 'create'), {})
})

test('requires password when creating an employee', () => {
  const errors = validateEmployeeForm({ ...validForm, password: '' }, 'create')

  assert.equal(errors.password, '密碼為必填欄位。')
})

test('allows a blank password when editing an employee', () => {
  assert.deepEqual(
    validateEmployeeForm({ ...validForm, password: '' }, 'edit'),
    {},
  )
})

test('rejects invalid gender and Taiwan national ID formats', () => {
  const errors = validateEmployeeForm(
    { ...validForm, gender: 'X', nationalId: 'A123' },
    'create',
  )

  assert.equal(errors.gender, '性別格式不正確。')
  assert.equal(errors.nationalId, '身份證字號格式不正確。')
})

test('allows empty optional contact details', () => {
  const errors = validateEmployeeForm(
    { ...validForm, contactPhone: '', address: '', birthDate: '' },
    'create',
  )

  assert.deepEqual(errors, {})
})

test('rejects contact details that exceed limits or use an invalid date', () => {
  const errors = validateEmployeeForm(
    {
      ...validForm,
      contactPhone: '0'.repeat(31),
      address: '台'.repeat(256),
      birthDate: '1990/01/02',
    },
    'create',
  )

  assert.equal(errors.contactPhone, '聯絡電話不可超過 30 個字元。')
  assert.equal(errors.address, '地址不可超過 255 個字元。')
  assert.equal(errors.birthDate, '出生年月日格式不正確。')
})

test('maps empty contact details to null API values', () => {
  assert.deepEqual(
    toEmployeeRequest(
      { ...validForm, contactPhone: '', address: '', birthDate: '' },
      'create',
    ),
    {
      name: '王小明',
      shortName: '小明',
      gender: 'M',
      nationalId: 'A123456789',
      password: 'temporary-password',
      contactPhone: null,
      address: null,
      birthDate: null,
    },
  )
})
