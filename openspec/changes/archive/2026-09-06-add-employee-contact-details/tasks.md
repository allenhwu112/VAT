## 1. Database migration and Stored Procedures

- [x] 1.1 Add migration contract tests for version `202608220004`, the three nullable columns, the query result fields, and command parameters; verify the tests fail before implementation
- [x] 1.2 Implement `202608220004_CreateEmployeeContactDetails` with `ContactPhone NVARCHAR(30) NULL`, `Address NVARCHAR(255) NULL`, and `BirthDate DATE NULL`; verify `Up()` and `Down()` compile and expose reversible schema changes
- [x] 1.3 Extend `Employee_Query` and `Employee_Command` in the new migration, including NULL handling and password exclusion; verify migration tests cover procedure names and required field tokens

## 2. Backend API

- [x] 2.1 Extend employee DTO contract tests for nullable contact fields, length validation, ISO birth-date handling, and password exclusion; verify the new tests fail before implementation
- [x] 2.2 Update employee request/response models and repository parameter/reader mapping for `contactPhone`, `address`, and nullable `birthDate`; verify API contract tests pass
- [x] 2.3 Update employee CRUD API behavior so create, list, update, and get-by-id preserve or clear contact fields as specified; verify API contract tests and Release backend builds pass; the root Debug build was attempted and is blocked only by the existing `Vat.Api.exe` process lock

## 3. Frontend employee management

- [x] 3.1 Extend employee API mapping and form validation for optional contact phone, address, and `YYYY-MM-DD` birth date; verify frontend unit tests cover empty and invalid values
- [x] 3.2 Add the three fields to the shared MUI Dialog while retaining password behavior and validation; verify the dialog source contains the expected labels and input types
- [x] 3.3 Add contact columns to AG Grid and preserve single-row selection, reload, and blank-value display; verify `npm.cmd --workspace apps/frontend run lint` and `npm.cmd run build:frontend` pass

## 4. End-to-end verification

- [x] 4.1 Run `openspec validate add-employee-contact-details --strict` and verify all change artifacts are valid
- [x] 4.2 Run backend/frontend tests, builds, `git diff --check`, and `git status --short`; verify no migration is applied without explicit authorization and document any SQL Server connectivity blocker. `npm.cmd run migrate:vat:check` remains blocked by the existing SQL Server SSPI error, and no migration was applied.
