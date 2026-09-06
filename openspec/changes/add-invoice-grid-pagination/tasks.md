## 1. Pagination contract tests

- [x] 1.1 Add a pure frontend pagination-options helper and tests proving the enabled configuration uses a 25-row default with 25／50／100 selectors while the default shared-grid configuration remains disabled; verify the new tests fail before implementation.

## 2. Shared Enterprise Grid integration

- [x] 2.1 Add opt-in pagination props to `ManagementGrid`, preserve `AllEnterpriseModule`, cell selection, clipboard copying, single-row selection, and double-click callbacks, and expose pagination-change callbacks; verify the pagination helper tests and frontend lint pass.
- [x] 2.2 Enable pagination only for `InvoicesPage` and clear the invoice row selection when the user changes page or page size; verify employee/client grid usages remain opt-in and the frontend build succeeds.

## 3. Verification

- [x] 3.1 Run the complete frontend test suite, lint, build, `git diff --check`, and `git status --short`; verify the invoice page has native Enterprise pagination and no API or database files changed.
- [x] 3.2 Run OpenSpec strict validation and report the client-side pagination limitation and the existing database migration boundary separately; verify no migration command is executed.
