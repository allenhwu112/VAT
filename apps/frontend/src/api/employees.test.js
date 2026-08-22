import assert from 'node:assert/strict'
import test from 'node:test'
import { validateEmployeeForm } from './employees.js'

const validForm = {
  name: '王小明',
  shortName: '小明',
  gender: 'M',
  nationalId: 'A123456789',
  password: 'temporary-password',
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
