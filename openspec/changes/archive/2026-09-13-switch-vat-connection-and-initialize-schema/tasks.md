## 1. Connection Configuration

- [x] 1.1 Rename the tracked connection-string entry to `ConnectionStrings:VAT`, set the requested remote `192.168.25.20` default, update `Vat.Api/Program.cs`, all three repositories, and `Vat.Migrations/Program.cs` to use `GetConnectionString("VAT")`, and verify `rg -n "VatDatabase" apps README.md package.json` returns no stale consumer references.
- [x] 1.2 Update `README.md` with the remote default and `ConnectionStrings__VAT` override workflow, and document the private-repository credential risk and secret-manager preference.

## 2. Migration Contract and Build

- [x] 2.1 Add a migration-assembly contract test for the eleven expected versions in ascending order, including `MakeClientDetailsOptional` and `AddClientCode`, and verify the migration test project passes.
- [x] 2.2 Run OpenSpec strict validation, `npm run test:backend`, and `npm run build:backend`; verify all artifacts validate, all backend tests pass, and both backend projects build successfully.

## 3. Remote VAT Schema Initialization

- [x] 3.1 Set the supplied remote connection only in the current PowerShell process as `ConnectionStrings__VAT`, append `TrustServerCertificate=True` while keeping encryption enabled, run `npm run migrate:vat:check`, and verify the opened database name is exactly `VAT` before any schema write.
- [x] 3.2 Run the authorized `npm run migrate:vat` command against the checked database and verify the command exits successfully after applying all pending versions through `202609120002`.
- [x] 3.3 Perform read-only SQL catalog verification for all eleven `VersionInfo` rows, the `Employees`, `Clients`, and `Invoices` tables, all expected columns and constraints, the unique/filtered indexes, and the six query/command procedures; verify the current ClientCode and invoice quantity contracts are present.

## 4. API Smoke Test and Handoff

- [x] 4.1 Start the API directly without `npm start` using the same process-scoped `ConnectionStrings__VAT` value, verify `/VAT_API/health` and empty GET results for employees, clients, and invoices, then stop the API and clear the environment variable.
- [x] 4.2 Run `git diff --check` and `git status --short`; verify no reset, bulk deletion, commit, push, or unrelated worktree cleanup occurred, and mark this OpenSpec change complete only after all preceding checks pass.
