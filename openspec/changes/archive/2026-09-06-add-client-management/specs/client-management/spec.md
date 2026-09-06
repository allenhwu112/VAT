## Purpose

提供 VAT 可驗證、可搜尋與可維護的客戶主檔，讓客戶資料能透過獨立管理頁與 API 被一致地建立、修改、查詢及刪除。

## ADDED Requirements

### Requirement: Client master records require valid identifying and contact data

The system SHALL store every client with a generated numeric client identifier, a unique eight-digit numeric tax ID, a full name, a short name, a responsible person, and an address. The four text fields SHALL be non-empty after trimming and SHALL be limited to 100, 50, 100, and 255 characters respectively.

#### Scenario: Create a client with valid required data

- **WHEN** a client is submitted with an eight-digit tax ID and non-empty values for full name, short name, responsible person, and address within the declared limits
- **THEN** the system creates one client record, assigns a client identifier, and returns all five submitted fields using camelCase names

#### Scenario: Reject missing or whitespace-only required data

- **WHEN** a client create or update request omits or supplies only whitespace for any required field
- **THEN** the system returns `400 Bad Request` identifying the invalid field and does not create or change a record

#### Scenario: Reject an invalid tax ID

- **WHEN** a client create or update request contains a tax ID that is not exactly eight ASCII digits
- **THEN** the system returns `400 Bad Request` and does not create or change a record

#### Scenario: Reject a duplicate tax ID

- **WHEN** a client create or update request uses a tax ID already assigned to another client
- **THEN** the system returns `409 Conflict` and leaves both existing records unchanged

### Requirement: Client records support complete CRUD through the API

The system SHALL expose client collection and item endpoints under `/VAT_API/clients`. Collection reads SHALL return a `{ data: [...] }` response, item reads and successful writes SHALL return a `{ data: ... }` response, and delete SHALL return no content. The API SHALL return `404 Not Found` for a missing client and SHALL never expose unrelated secret fields.

#### Scenario: List and retrieve clients

- **WHEN** a caller requests `GET /VAT_API/clients` or `GET /VAT_API/clients/{clientId}` for existing records
- **THEN** the API returns `200 OK` with the client data ordered by client identifier descending for the collection

#### Scenario: Create and update a client

- **WHEN** a caller sends valid data to `POST /VAT_API/clients` or `PUT /VAT_API/clients/{clientId}`
- **THEN** the API returns the persisted client data, using `201 Created` for creation and `200 OK` for update

#### Scenario: Delete a client

- **WHEN** a caller sends `DELETE /VAT_API/clients/{clientId}` for an existing client
- **THEN** the API removes that client and returns `204 No Content`

#### Scenario: Request a missing client

- **WHEN** a caller requests, updates, or deletes a client identifier that does not exist
- **THEN** the API returns `404 Not Found` without changing any other client record

### Requirement: Employee and client management pages have distinct URLs

The system SHALL provide the employee management page at `http://localhost:5173/VAT_UI/employees` and the client management page at `http://localhost:5173/VAT_UI/clients`. The navigation SHALL expose both links, and the root UI path SHALL resolve to the employee page.

#### Scenario: Open the client page directly

- **WHEN** a user opens `/VAT_UI/clients` directly or refreshes that URL
- **THEN** the system displays the client management page instead of the employee page

#### Scenario: Navigate between management pages

- **WHEN** a user selects the employee or client navigation link
- **THEN** the browser URL changes to the selected page path and the corresponding table is displayed

#### Scenario: Use the prefixed health endpoint

- **WHEN** a caller requests `GET /VAT_API/health`
- **THEN** the API returns the existing health response with `200 OK`

### Requirement: Management grids support Enterprise selection, copying, and double-click editing

The employee and client management grids SHALL use Enterprise grid capabilities, allow a user to select a single row, allow Excel-like cell or range selection and clipboard copying, and open that row's edit mode when the user double-clicks it. Edit and delete actions SHALL be disabled when no row is selected, and deletion SHALL require confirmation.

#### Scenario: Select one row and edit by double-click

- **WHEN** a user selects a row and double-clicks that row in either management grid
- **THEN** the corresponding edit dialog opens with the selected record's current values

#### Scenario: Copy selected cells

- **WHEN** a user selects one or more cells or a cell range and presses the platform copy shortcut
- **THEN** the selected grid values are copied to the clipboard in a spreadsheet-compatible format

#### Scenario: Prevent actions without a selected row

- **WHEN** no row is selected
- **THEN** the edit and delete controls are disabled and no record can be deleted

#### Scenario: Confirm deletion

- **WHEN** a user activates delete for a selected client or employee
- **THEN** the system asks for confirmation before issuing the delete request and refreshes the grid after a successful deletion
