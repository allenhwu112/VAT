## Purpose

讓發票管理在資料量增加時仍能以清楚的頁面導覽瀏覽資料，同時保留 Enterprise Grid 原有的選取、複製與雙擊編輯操作。

## ADDED Requirements

### Requirement: Invoice management displays client-side pagination

The invoice management page SHALL paginate the records returned by the existing invoice collection request on the client side. The first page SHALL be shown initially, the default page size SHALL be 25 rows, and the user SHALL be able to choose 25, 50, or 100 rows per page and navigate to available pages. The page controls SHALL expose the current page and total record count.

#### Scenario: Open a multi-page invoice list

- **WHEN** the invoice collection contains more than 25 records and the user opens the invoice management page
- **THEN** the grid displays the first 25 records, shows pagination controls, and reports the total record count

#### Scenario: Change invoice page size

- **WHEN** the user chooses 50 or 100 rows per page
- **THEN** the grid updates the visible row count and recalculates the available page count without requesting another API resource

#### Scenario: Navigate between invoice pages

- **WHEN** the user selects a different available page
- **THEN** the grid displays the records for that page and keeps the invoice management page URL unchanged

### Requirement: Pagination preserves Enterprise Grid interactions

Pagination SHALL preserve the invoice grid's single-row selection, cell or range selection, spreadsheet-compatible clipboard copying, and row double-click editing. Edit and delete actions SHALL remain disabled when no row is selected, including after changing pages. A selected row SHALL not be silently replaced by a different row when the user only changes page.

#### Scenario: Select and edit an invoice on a paginated page

- **WHEN** the user selects an invoice row on any page and double-clicks that row
- **THEN** the edit dialog opens for that row's current invoice data

#### Scenario: Copy cells from a paginated invoice page

- **WHEN** the user selects one or more cells or a cell range on any page and presses the platform copy shortcut
- **THEN** the selected values are copied in a spreadsheet-compatible format

#### Scenario: Change page after selecting a row

- **WHEN** the user selects a row and then navigates to another page
- **THEN** the grid does not select a different row automatically and the edit/delete controls reflect the actual current selection

### Requirement: Existing management pages remain compatible

Adding invoice pagination SHALL NOT change the fixed employee and client URLs, their existing CRUD behavior, or their existing Enterprise Grid selection, clipboard, and double-click editing behavior.

#### Scenario: Open the employee and client pages after the pagination change

- **WHEN** a user opens `/VAT_UI/employees` or `/VAT_UI/clients`
- **THEN** the corresponding existing management page loads with its current URL and interactions intact
