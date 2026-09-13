## Purpose

This capability provides a repeatable and auditable way to initialize an existing empty VAT database with the complete application schema and database procedures. It also defines the shared connection configuration contract used by the API and migration runner.

## ADDED Requirements

### Requirement: The system uses the shared VAT connection configuration

The API and schema migration command SHALL resolve the target SQL Server connection from `ConnectionStrings:VAT`, and a `ConnectionStrings__VAT` environment value SHALL override the configured default. The current default SHALL target the requested existing `VAT` database on `192.168.25.20`; deployments that should not store credentials in tracked files SHOULD use the environment override or a secret manager.

#### Scenario: Environment configuration selects the remote VAT database

- **WHEN** `ConnectionStrings__VAT` contains a valid connection string for the existing database named `VAT`
- **THEN** both the API and migration command use that connection without requiring a source-controlled credential

#### Scenario: Configured default selects the remote VAT database

- **WHEN** no `ConnectionStrings__VAT` environment override is present
- **THEN** both the API and migration command use the configured `ConnectionStrings:VAT` default for `192.168.25.20` and database `VAT`

#### Scenario: Missing VAT connection configuration is rejected

- **WHEN** neither the configured `VAT` connection string nor its environment override contains a non-blank value
- **THEN** the API and migration command fail with an error identifying `ConnectionStrings:VAT`

### Requirement: The migration command initializes the complete current VAT schema

The migration command SHALL apply all pending versioned schema changes in ascending version order to an existing database named `VAT`. A successful initialization SHALL create the employee, client, and invoice tables; their constraints and indexes; the employee, client, and invoice query/command procedures; and the latest current-tree client and invoice field contracts.

#### Scenario: Empty VAT database receives the complete schema

- **WHEN** the migration command connects to an existing empty database named `VAT`
- **THEN** it records and applies versions `202608220001`, `202608220002`, `202608220003`, `202608220004`, `202609060001`, `202609060002`, `202609060003`, `202609060004`, `202609060005`, `202609120001`, and `202609120002` in ascending order
- **AND** the database contains `dbo.Employees`, `dbo.Clients`, `dbo.Invoices`, `Employee_Query`, `Employee_Command`, `Client_Query`, `Client_Command`, `Invoice_Query`, and `Invoice_Command`

#### Scenario: Re-running after successful initialization is safe

- **WHEN** the migration command is run again after all eleven versions are recorded
- **THEN** it completes without recreating or duplicating schema objects and reports no pending migration work

#### Scenario: A non-VAT database is not migrated

- **WHEN** the read-only connection check opens a database whose name is not `VAT`
- **THEN** the check fails before any schema-changing migration is run

### Requirement: Schema initialization is performed through tracked migrations only

The system SHALL use the repository's versioned migration runner as the only schema-changing path for this initialization. The API SHALL NOT execute migrations during startup, and the initialization process SHALL NOT require manual SQL or a new consolidated migration.

#### Scenario: API startup does not change schema version

- **WHEN** the API is started with a valid `ConnectionStrings:VAT` value
- **THEN** it serves the API without applying pending migrations

#### Scenario: Migration failure remains visible and tracked

- **WHEN** a migration cannot be applied
- **THEN** the command returns a failure status and the migration ledger does not falsely record the failed version as successfully applied
