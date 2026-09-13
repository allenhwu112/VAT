## Context

The .NET 10 API and the standalone FluentMigrator runner currently read the same linked `appsettings.json` value named `ConnectionStrings:VatDatabase`. The migration assembly already contains eleven ordered versions: a no-op baseline, employee schema and procedures, client schema and procedures, invoice schema and procedures, invoice quantity changes, and the current working-tree client optional-detail and ClientCode changes. The target SQL Server database already exists and is expected to be empty.

## Goals / Non-Goals

**Goals:**

- Make `ConnectionStrings:VAT` the single configuration key used by all backend consumers.
- Use the requested remote SQL login default in the linked `appsettings.json`; keep environment-variable override support for deployments that should not store credentials in tracked files.
- Initialize and verify the complete current schema through FluentMigrator in version order.
- Preserve all unrelated uncommitted work and leave the API startup path migration-free.

**Non-Goals:**

- Do not create, rename, or drop the SQL Server database itself.
- Do not squash or rewrite historical migrations, add a duplicate bootstrap migration, or modify frontend routes and behavior.
- Keep encryption enabled for the remote connection and use `TrustServerCertificate=True` for this demonstrated internal-server certificate-trust failure; do not disable encryption.
- Do not migrate application data; the target database is new and empty.

## Decisions

### Use `ConnectionStrings:VAT` with environment precedence

Update the API, all repositories, the migration runner, the linked `appsettings.json`, and README examples to use `VAT`. The requested default is the complete remote SQL login connection to `192.168.25.20`; `ConnectionStrings__VAT` remains available and takes precedence for environments that use secret injection. The remote connection explicitly keeps encryption enabled and uses `TrustServerCertificate=True` to match the successful SSMS trust behavior. Changing every consumer is preferred over maintaining aliases because it makes a wrong or stale key fail visibly.

### Keep the existing eleven migrations as the schema source of truth

Do not add a consolidated migration. FluentMigrator already scans the migration assembly and records applied versions in `VersionInfo`, so an empty target receives the exact same ordered history as any other environment. The two current untracked migrations are included because the user selected the current working tree as the schema boundary; they must remain present and compile before execution.

### Preflight, apply, then inspect

Run the read-only connection/database-name check first. Only after it confirms database `VAT` should the authorized `migrate:vat` command run. Verify the ledger and SQL catalog objects afterward, then query the health and three list endpoints through a separately started API process if available. Do not use `npm start`, because it chains migration into startup.

### Preserve the current checkout

Work in the shared checkout because the selected schema includes uncommitted files. Before edits, capture `git status --short`; never reset, stash, bulk-delete, format, commit, or push unrelated changes. Final checks must show only the intended connection/docs/OpenSpec edits in addition to the pre-existing user changes.

## Risks / Trade-offs

- **[Risk]** The two latest migrations are untracked and could be absent from a later checkout. → **Mitigation:** verify their files, versions, compilation, and expected eleven-version list before connecting to the target database; stop if either is missing.
- **[Risk]** The SQL login password is present in the requested private-repository default and could leak through configuration history or access changes. → **Mitigation:** keep the repository private, prefer `ConnectionStrings__VAT` or a secret manager for deployment, do not echo credentials, and rotate the exposed credential.
- **[Risk]** `TrustServerCertificate=True` accepts the internal server's untrusted certificate without validating its issuing authority. → **Mitigation:** keep encryption enabled, scope the setting to the configured remote connection, and replace it with a certificate trusted by the machine when the SQL Server PKI is available.
- **[Risk]** A migration could fail partway through due to SQL Server DDL batching or permissions. → **Mitigation:** rely on the existing independent migration batches and `VersionInfo`, capture the failing version/error, and do not mark the run complete without catalog verification.

## Migration Plan

1. Create and validate this OpenSpec change, then update the shared connection-key consumers and documentation.
2. Run backend tests and build; confirm all eleven migration versions are discoverable and the current working-tree migrations are compiled.
3. Set `ConnectionStrings__VAT` in the current PowerShell process with `TrustServerCertificate=True` and run `npm run migrate:vat:check`.
4. After the check confirms `VAT`, run the authorized `npm run migrate:vat` command once.
5. Query `VersionInfo` and SQL Server catalogs to verify every version, table, procedure, index, constraint, and expected column/type/range.
6. Run API health/list smoke tests without automatic migration, clear the environment variable, run final diff/status checks, and leave commit/push/archive for a separate explicit request.
