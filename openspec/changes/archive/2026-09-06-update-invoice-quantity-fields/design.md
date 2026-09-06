## Context

The applied invoice migrations currently store all seven invoice options as `BIT` columns and expose them as Boolean API fields. The shared frontend API helper and dialog also normalize all options to checkboxes, while the invoice Grid is already Enterprise-enabled and paginated. Applied migrations are immutable, so the type and procedure changes require a new FluentMigrator version.

## Goals / Non-Goals

**Goals:**

- Preserve the electronic-invoice Boolean field and convert the other six fields to required integer quantities in the inclusive range 0-99.
- Keep existing invoice records compatible by converting previous bit values to integer 0 or 1.
- Enforce the contract in the API model, Stored Procedures, database constraints, and frontend form.
- Preserve the existing invoice URL, Enterprise Grid interactions, and client-side pagination.

**Non-Goals:**

- Do not edit the already-applied invoice migrations.
- Do not add server-side pagination, new dependencies, or a new API route.
- Do not run the new migration automatically during implementation; applying it remains an explicit database operation.

## Decisions

### Add a forward schema-and-procedure migration

Create migration `202609060005` with `Up()` and `Down()`. `Up()` drops the six old default constraints, alters the existing `BIT` columns to `INT NOT NULL`, adds one 0-99 check constraint per quantity, and recreates both invoice procedures with integer quantity parameters and validation. The conversion preserves existing 0/1 values. `Down()` refuses to narrow any quantity greater than 1, then removes range checks, converts the columns back to `BIT`, restores the old defaults, and restores the previous procedure contract.

This keeps applied migrations immutable and makes the schema transition auditable. A direct manual SQL change was rejected because it would bypass FluentMigrator version tracking.

### Use nullable request integers with explicit validation

Request DTO quantity properties will be nullable `int?` with `Required` and `Range(0, 99)` attributes. This distinguishes an omitted value from the valid value 0 and produces a 400 response for missing or invalid quantities. Response properties remain non-null `int`. The repository will bind quantity parameters as `SqlDbType.Int` and read them with `GetInt32`.

### Split frontend Boolean and quantity option handling

The frontend API module will keep separate field lists. The form starts quantity values as empty strings, validates integer 0-99 values before submission, and converts valid strings to numbers in the request. `InvoiceDialog` will render one checkbox and six number inputs. `InvoicesPage` will format the six Grid columns as numeric values and retain the existing pagination callback and selection behavior.

### Test the contract before production changes

Update backend DTO and migration contract tests and frontend API tests to describe the new behavior first. Run the affected tests in the expected failing state, then implement the migration, API mapping, and UI changes until the tests, lint, builds, and live read-only endpoint checks pass.

## Risks / Trade-offs

- **[Risk]** Existing quantity values greater than 1 cannot be represented by the old Boolean schema during rollback. → **Mitigation:** the Down migration checks for values greater than 1 and fails before narrowing, preventing silent data loss.
- **[Risk]** Existing callers sending Boolean values for the six changed fields will no longer satisfy the API contract. → **Mitigation:** return field-level 400 validation errors and update the frontend in the same change.
- **[Risk]** The database may contain no invoice rows while the migration is developed. → **Mitigation:** test conversion and constraints through migration source contracts and apply the migration only after the explicit database authorization.

## Migration Plan

1. Add failing backend, migration, and frontend contract tests.
2. Implement migration `202609060005`, API numeric types and mappings, and frontend numeric inputs/grid formatting.
3. Run tests, lint, builds, `git diff --check`, and read-only connection checks.
4. After explicit user authorization, run `npm.cmd run migrate:vat` and verify `GET /VAT_API/invoices` plus a valid create/update request.
5. If rollback is required, run `npm.cmd run migrate:vat:down` only after confirming all quantity values are 0 or 1.
