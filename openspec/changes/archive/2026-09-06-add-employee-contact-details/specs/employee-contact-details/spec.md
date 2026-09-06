## Purpose

讓員工資料能保存並維護基本聯絡資訊與出生年月日，同時維持既有員工資料、密碼隱私及 CRUD 流程的相容性。

## ADDED Requirements

### Requirement: Employee contact details are optional and maintainable

The system SHALL allow an employee record to contain an optional contact phone, address, and birth date. Contact phone SHALL be at most 30 characters, address SHALL be at most 255 characters, and birth date SHALL use the `YYYY-MM-DD` date format when provided. Empty values SHALL be stored and returned as `null`.

#### Scenario: Create employee with contact details

- **WHEN** a client creates an employee with valid `contactPhone`, `address`, and `birthDate` values
- **THEN** the employee is created and subsequent employee responses contain the same values using camelCase field names

#### Scenario: Create employee without contact details

- **WHEN** a client creates an employee without one or more contact detail values
- **THEN** the employee is created and the omitted contact detail fields are returned as `null`

#### Scenario: Update employee and clear contact details

- **WHEN** a client updates an employee with an empty or null contact detail value
- **THEN** that contact detail is cleared and returned as `null` without changing unrelated employee fields

#### Scenario: Reject invalid contact detail input

- **WHEN** a client submits a contact phone longer than 30 characters, an address longer than 255 characters, or an invalid birth date
- **THEN** the API returns `400 Bad Request` and does not modify the employee record

### Requirement: Employee contact details are visible in employee management UI

The employee management screen SHALL show contact phone, address, and birth date columns in the employee list and SHALL provide corresponding optional fields in the create and edit dialog. The password SHALL remain excluded from the list and all employee responses.

#### Scenario: Display contact details in the list

- **WHEN** the employee list contains contact details
- **THEN** the grid displays the phone, address, and birth date values in their corresponding columns

#### Scenario: Edit contact details in the dialog

- **WHEN** a user opens the create or edit dialog
- **THEN** the dialog provides phone, address, and date inputs, and saving refreshes the grid with the persisted values

#### Scenario: Show empty contact details

- **WHEN** an employee has no contact phone, address, or birth date
- **THEN** the grid and dialog represent those fields as blank without showing validation errors for being empty
