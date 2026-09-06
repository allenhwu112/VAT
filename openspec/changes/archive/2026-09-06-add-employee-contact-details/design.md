## Context

The existing employee feature uses FluentMigrator, SQL Server Stored Procedures, an ADO.NET repository, ASP.NET Core DTOs/controllers, and a React screen backed by AG Grid and MUI. The current employee table and procedures already exist in migrations, so this change must be additive and reversible without editing an already-applied migration.

## Goals / Non-Goals

**Goals:**

- Add nullable contact phone, address, and birth date fields across the database, Stored Procedures, API, and UI.
- Preserve existing employee rows and allow empty contact details.
- Keep password excluded from query results, API responses, grid columns, and logs.
- Provide contract and UI validation coverage for the new fields.

**Non-Goals:**

- No authentication, authorization, employee search, or new employee workflow.
- No phone-number normalization or country-specific phone validation.
- No change to the existing plaintext-password behavior in this follow-up.

## Decisions

### Add a new migration instead of changing existing migrations

Create migration `202608220004_CreateEmployeeContactDetails` after the existing employee table and procedure migrations. Add `ContactPhone NVARCHAR(30) NULL`, `Address NVARCHAR(255) NULL`, and `BirthDate DATE NULL`. Nullable columns keep existing rows valid without fabricated personal data.

### Version the Stored Procedure contract in the same migration

The new migration will recreate the query and command procedure definitions with the three new parameters and result columns. `Down()` will restore the prior procedure definitions before dropping the new columns, so rollback remains executable and version-tracked.

### Use nullable API fields and ISO dates

The API request and response models will expose `contactPhone`, `address`, and nullable `birthDate`. ASP.NET Core will serialize `DateOnly?` as `YYYY-MM-DD`; the repository will bind absent values as `DBNull` and map database NULL back to nullable response values. Empty text from the UI will be converted to NULL.

### Keep the existing single-row management flow

The AG Grid will add three read-only display columns while retaining single-row selection. The existing MUI Dialog will add optional phone, address, and date inputs for both create and edit modes. Existing reload, error, confirmation, and password-handling behavior remains unchanged.

## Risks / Trade-offs

- **[Risk]** Existing employee rows have no historical contact data. → **Mitigation:** Use nullable columns and display blank values; do not invent defaults.
- **[Risk]** Birth date is personal information. → **Mitigation:** Keep it out of logs and password responses, and retain the current API security scope without adding new exposure paths.
- **[Risk]** A procedure rollback can become inconsistent if only the columns are removed. → **Mitigation:** Put the complete prior procedure definitions in the new migration's `Down()` before dropping columns.
- **[Risk]** Large address values or malformed dates can reach the API. → **Mitigation:** Enforce the specified length/date validation in DTOs, procedures, and database types.

## Migration Plan

1. Run migration tests and backend/frontend builds without applying the database migration.
2. After the SQL Server connection is available, run the read-only VAT database check.
3. Obtain explicit authorization before running `npm.cmd run migrate:vat`.
4. Verify existing employee rows, new CRUD fields, and password exclusion.
5. If rollback is required, run `npm.cmd run migrate:vat:down`; the migration restores the previous procedures and then removes the three columns.
