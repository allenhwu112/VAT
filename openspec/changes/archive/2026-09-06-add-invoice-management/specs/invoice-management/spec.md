## Purpose

提供一個可驗證、可維護的發票設定主檔，讓使用者以統編與客戶簡稱管理每一客戶適用的發票開立方式，並透過清楚分離的 API 與 UI URL 進行操作。

## ADDED Requirements

### Requirement: Invoice records contain a unique customer identity and explicit invoice options

The system SHALL store each invoice-management record with a generated numeric invoice identifier, a unique eight-digit numeric tax ID, a non-empty customer short name limited to 50 characters, and seven non-null boolean options: electronic invoice, cash register, three-cash-register, two-part invoice, two-part invoice copy, three-part invoice, and three-part invoice copy. A newly created record SHALL default any omitted invoice option to false.

#### Scenario: Create a valid invoice-management record

- **WHEN** a caller submits an eight-digit tax ID, a non-empty customer short name, and valid boolean invoice options
- **THEN** the system creates one record, assigns an invoice identifier, and returns all identity and option fields using camelCase names

#### Scenario: Default omitted invoice options

- **WHEN** a caller creates a record without one or more invoice option values
- **THEN** the system stores and returns each omitted option as false

#### Scenario: Reject invalid invoice identity data

- **WHEN** a create or update request has a missing or whitespace-only customer short name, a customer short name longer than 50 characters, or a tax ID that is not exactly eight ASCII digits
- **THEN** the system returns `400 Bad Request` identifying the invalid field and does not create or change a record

#### Scenario: Reject a duplicate invoice tax ID

- **WHEN** a create or update request uses a tax ID already assigned to another invoice-management record
- **THEN** the system returns `409 Conflict` and leaves both existing records unchanged

### Requirement: Invoice records support complete CRUD through the API

The system SHALL expose invoice-management collection and item endpoints under `/VAT_API/invoices`. Collection reads SHALL return a `{ data: [...] }` response, item reads and successful writes SHALL return a `{ data: ... }` response, and delete SHALL return no content. Collection results SHALL be ordered by invoice identifier descending. The API SHALL return `404 Not Found` for a missing record and SHALL not expose fields outside the invoice-management contract.

#### Scenario: List and retrieve invoice-management records

- **WHEN** a caller requests `GET /VAT_API/invoices` or `GET /VAT_API/invoices/{invoiceId}` for existing records
- **THEN** the API returns `200 OK` with the requested records and all seven boolean options

#### Scenario: Create and update an invoice-management record

- **WHEN** a caller sends valid data to `POST /VAT_API/invoices` or `PUT /VAT_API/invoices/{invoiceId}`
- **THEN** the API returns the persisted record, using `201 Created` for creation and `200 OK` for update

#### Scenario: Delete an invoice-management record

- **WHEN** a caller sends `DELETE /VAT_API/invoices/{invoiceId}` for an existing record
- **THEN** the system removes that record and returns `204 No Content`

#### Scenario: Request a missing invoice-management record

- **WHEN** a caller requests, updates, or deletes an invoice identifier that does not exist
- **THEN** the API returns `404 Not Found` without changing any other record

### Requirement: Invoice management has a distinct UI URL and preserves employee and client URLs

The system SHALL provide the invoice-management page at `http://localhost:5173/VAT_UI/invoices`, the employee page at `http://localhost:5173/VAT_UI/employees`, and the client page at `http://localhost:5173/VAT_UI/clients`. Navigation SHALL expose all three links, and the root UI path SHALL continue to resolve to the employee page.

#### Scenario: Open the invoice page directly

- **WHEN** a user opens `/VAT_UI/invoices` directly or refreshes that URL
- **THEN** the system displays the invoice-management page and keeps the invoice URL

#### Scenario: Navigate between employee, client, and invoice pages

- **WHEN** a user selects any of the three management links
- **THEN** the browser URL changes to that page's explicit path and the corresponding table is displayed

### Requirement: Invoice management grids support single-row editing and cell clipboard operations

The invoice-management grid SHALL allow only one row to be selected at a time, allow cell or range selection and spreadsheet-compatible clipboard copying, and open the selected row's edit mode on a double-click. Edit and delete actions SHALL be disabled when no row is selected, and deletion SHALL require confirmation before the request is sent.

#### Scenario: Select one row and edit by double-click

- **WHEN** a user selects an invoice-management row and double-clicks that row
- **THEN** the edit dialog opens with the row's current identity and invoice-option values

#### Scenario: Copy selected invoice cells

- **WHEN** a user selects one or more invoice cells or a cell range and presses the platform copy shortcut
- **THEN** the selected values are copied to the clipboard in a spreadsheet-compatible format

#### Scenario: Prevent invoice actions without a selected row

- **WHEN** no invoice-management row is selected
- **THEN** the edit and delete controls are disabled and no record can be deleted

#### Scenario: Confirm invoice deletion

- **WHEN** a user activates delete for a selected invoice-management record
- **THEN** the system asks for confirmation before issuing the delete request and refreshes the grid after successful deletion
