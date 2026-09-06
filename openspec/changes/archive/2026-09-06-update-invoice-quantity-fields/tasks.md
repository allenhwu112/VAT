## 1. Contract tests first

- [x] 1.1 Update frontend invoice API tests to require numeric quantity fields, accept 0 and 99, reject missing or out-of-range values, preserve Boolean electronic-invoice behavior, and verify the tests fail before implementation.
- [x] 1.2 Update backend DTO contract tests to require nullable integer quantities with 0-99 validation and an integer response contract; verify the tests fail before implementation.
- [x] 1.3 Extend migration contract tests for migration `202609060005`, integer quantity columns, range constraints, procedure parameters, and reversible rollback guards; verify the tests fail before implementation.

## 2. Database and API implementation

- [x] 2.1 Add `202609060005` with `Up()` and `Down()` to convert the six applied Boolean columns to `INT NOT NULL`, preserve 0/1 values, enforce 0-99 constraints, and update both invoice Stored Procedures without editing prior migrations.
- [x] 2.2 Change invoice request DTOs, response DTOs, repository parameter binding, and reader mapping so electronic invoice remains Boolean and all six quantities are required integers from 0 through 99.
- [x] 2.3 Run backend tests and build, confirming invalid API values are rejected before any database write and valid zero values remain accepted.

## 3. Frontend implementation

- [x] 3.1 Split Boolean and quantity invoice fields in the frontend API helper, use empty initial quantity inputs, validate required integer 0-99 values, and normalize submitted quantities to numbers.
- [x] 3.2 Update `InvoiceDialog` to render one required checkbox plus six required number inputs with min/max 0/99 and field-level errors; update `InvoicesPage` Grid columns to display numeric quantities while retaining Enterprise pagination and interactions.
- [x] 3.3 Run the complete frontend test suite, lint, and build; verify the invoice page still uses `/VAT_UI/invoices` and employee/client pages remain unchanged.

## 4. Verification and migration gate

- [x] 4.1 Run `git diff --check`, inspect `git status --short`, and verify only the intended backend config/docs plus invoice quantity implementation and OpenSpec artifacts changed; do not modify unrelated existing work.
- [x] 4.2 Run OpenSpec strict validation and confirm all tasks are complete; database application was performed only after explicit authorization.
- [x] 4.3 After explicit user authorization only, run `npm.cmd run migrate:vat`, verify `migrate:vat:check`, `GET /VAT_API/invoices`, and a valid create/update round trip; do not run rollback automatically.
