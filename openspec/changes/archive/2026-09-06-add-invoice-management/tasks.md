## 1. Database schema and migration contracts

- [x] 1.1 Add the forward-only `Invoices` table migration with a unique eight-digit tax ID, required client short name, seven non-null BIT options defaulting to false, constraints, and a matching reversible `Down()`; verify its version, columns, constraints, and `Up`/`Down` contract with migration tests.
- [x] 1.2 Add `Invoice_Query` and `Invoice_Command` migrations for ordered reads and transactional create/update/delete validation; verify procedure names, parameters, fields, error codes, and reversible `Down()` behavior with migration source tests.

## 2. Backend invoice API

- [x] 2.1 Add invoice request/response DTOs with tax ID, short-name, and boolean-option validation; verify required, length, tax ID, default, and public-response contract tests.
- [x] 2.2 Add the Stored Procedure-backed invoice repository, controller, DI registration, response envelopes, not-found/conflict mapping, and `/VAT_API/invoices` routes; verify API route and controller contract tests plus a successful backend build.

## 3. Frontend invoice management

- [x] 3.1 Add `src/api/invoices.js` with the employee-style empty form, validation, trimming, boolean defaults, and CRUD requests; verify frontend form/request tests.
- [x] 3.2 Add the invoice dialog and management page with the requested Traditional Chinese fields, checkbox editing, loading/error handling, single-row selection, disabled edit/delete actions, double-click edit, delete confirmation, and shared Enterprise grid behavior; verify the frontend test suite and build.

## 4. Explicit management URLs and navigation

- [x] 4.1 Extend the native UI routes and app navigation so the fixed URLs are `/VAT_UI/employees`, `/VAT_UI/clients`, and `/VAT_UI/invoices`, with root and unknown paths falling back to employees; verify direct route resolution and navigation tests for all three paths.
- [x] 4.2 Keep the shared AG Grid Enterprise configuration enabled for employee, client, and invoice pages, including `AllEnterpriseModule`, single-row selection, cell/range selection, clipboard copying, and row double-click callbacks; verify the frontend build and inspect the rendered configuration contract.
- [x] 4.3 Update `README.md` with the three exact frontend URLs and `/VAT_API/invoices`; verify the documented links match the route and controller contracts.

## 5. Verification and database boundary

- [x] 5.1 Run frontend tests, lint, build, backend tests, backend build, `git diff --check`, and `git status --short`; verify failures are resolved without changing unrelated work.
- [x] 5.2 Run the read-only `npm.cmd run migrate:vat:check` preflight and report database connectivity separately from code verification; do not run `migrate:vat` or `migrate:vat:down` without explicit user authorization.
