## Purpose

讓發票管理能以電子發票勾選搭配六個可驗證的發票數量欄位，確保使用者與 API 都能輸入並保存 0 到 99 的整數。

## ADDED Requirements

### Requirement: Invoice option fields distinguish a Boolean flag from required quantities

The invoice-management contract SHALL keep `electronicInvoice` as a Boolean field and SHALL represent `cashRegister`, `threeCashRegister`, `twoPartInvoice`, `twoPartInvoiceCopy`, `threePartInvoice`, and `threePartInvoiceCopy` as required integer fields. Each quantity SHALL accept every integer from 0 through 99 inclusive and SHALL reject missing, fractional, non-numeric, negative, or greater-than-99 values.

#### Scenario: Accept valid invoice option values

- **WHEN** a caller submits an invoice with a Boolean `electronicInvoice` value and integer quantities from 0 through 99
- **THEN** the system accepts the request and returns the same option values using Boolean and integer JSON types respectively

#### Scenario: Reject an omitted or out-of-range quantity

- **WHEN** a create or update request omits a quantity, supplies a non-integer value, or supplies a value outside 0 through 99
- **THEN** the API returns `400 Bad Request` identifying the invalid quantity field and does not create or change the record

#### Scenario: Preserve the electronic-invoice checkbox contract

- **WHEN** a caller submits an invoice with `electronicInvoice` omitted or set to `false`
- **THEN** the system treats the field as `false` and does not require a numeric value for it

### Requirement: Invoice quantities are persisted with database range enforcement

The invoice database SHALL store the six quantity fields as non-null integer columns with a check constraint restricting each value to 0 through 99. The existing Boolean values SHALL be preserved as numeric 0 or 1 when the schema is upgraded, and the invoice query and command procedures SHALL read and write the updated numeric contract.

#### Scenario: Upgrade an existing invoice record

- **WHEN** the quantity-field migration is applied to an existing invoice record
- **THEN** each previous Boolean quantity value is preserved as integer 0 or 1 and remains queryable through the invoice API

#### Scenario: Reject an invalid direct database quantity

- **WHEN** a database write attempts to store a quantity below 0 or above 99
- **THEN** the database constraint or invoice command procedure rejects the write

### Requirement: Invoice form and grid expose numeric quantities

The invoice-management form SHALL render only `electronicInvoice` as a checkbox and SHALL render the six quantity fields as required numeric inputs constrained to 0 through 99. The invoice Grid SHALL display the six quantity values as numbers, while preserving the existing invoice URL, Enterprise pagination, single-row selection, cell selection, clipboard copying, and double-click editing behavior.

#### Scenario: Create an invoice with numeric quantities

- **WHEN** a user enters valid integer values for all six quantity inputs and submits the form
- **THEN** the form sends numeric JSON values, creates the invoice, and refreshes the Grid with those quantities

#### Scenario: Prevent submission without all quantities

- **WHEN** a user leaves any quantity input blank or enters a value outside 0 through 99
- **THEN** the form shows a field-level validation error and does not send the API request

#### Scenario: Edit an existing invoice quantity

- **WHEN** a user double-clicks an invoice row and changes one or more quantity inputs
- **THEN** the edit form shows the persisted numeric values and saves the updated integers without changing the page URL or pagination behavior
