## Why

The new `VAT` database on the remote SQL Server is empty, while the application and migration runner still resolve the old `ConnectionStrings:VatDatabase` key. The repository needs one repeatable, version-tracked initialization path so the complete current schema can be created without manual SQL and the application can use the requested remote default.

## What Changes

- Rename the shared .NET connection-string key to `ConnectionStrings:VAT` across the API, repositories, migration runner, configuration, and documentation.
- Use the requested remote SQL login connection as the `ConnectionStrings:VAT` default, while retaining `ConnectionStrings__VAT` as the preferred override for safer secret injection.
- Apply the existing 11 FluentMigrator versions in order to the new database, including the current working-tree client-detail and ClientCode migrations.
- Verify the migration ledger, tables, constraints, indexes, stored procedures, and read-only API queries after initialization.
- Do not add a duplicate all-in-one migration, alter already-applied migration definitions, or change frontend/API route behavior.

## Capabilities

### New Capabilities

- `vat-database-initialization`: Reproducible, version-tracked initialization of the VAT database schema and programmable database objects through the repository migration runner.

### Modified Capabilities

- None.

## Impact

- Backend configuration and connection consumers under `apps/backend/Vat.Api/**` and `apps/backend/Vat.Migrations/**`.
- Existing FluentMigrator schema, constraints, indexes, and stored procedures; no new schema version is introduced.
- `README.md`, backend verification, and the remote SQL Server `VAT` database.
- The current uncommitted working-tree changes remain in place and are not reformatted, reset, committed, or pushed by this change.
