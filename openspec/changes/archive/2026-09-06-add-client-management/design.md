## Context

The repository is a .NET 10 Controllers Web API and React/Vite JavaScript monorepo. Employee management already uses SQL Server FluentMigrator migrations, Stored Procedures, ADO.NET repositories, MUI dialogs, and AG Grid. Existing employee contact-detail changes are present in the working tree and must remain intact. See `proposal.md` and `specs/client-management/spec.md` for the motivation and observable contract.

## Goals / Non-Goals

**Goals:**

- Add an independently versioned Clients table and a Stored Procedure-backed CRUD API.
- Keep client fields required and enforce the same rules in the UI, API model validation, Stored Procedure, and database constraints.
- Provide `/VAT_UI/employees` and `/VAT_UI/clients` with shared grid behavior and navigation.
- Enable AG Grid Enterprise cell selection and clipboard copy while retaining single-row selection and dialog-based editing.
- Preserve existing employee contact fields and employee password privacy behavior.

**Non-Goals:**

- No accounting, invoicing, tax filing, contact history, or client-to-employee relationship tables.
- No pagination, search, bulk editing, import/export workflow, soft delete, authentication, or authorization changes.
- No data backfill; the new Clients table starts empty.
- No committed AG Grid license key or other secret.

## Decisions

### Use an Identity client key and a unique tax ID

Create `dbo.Clients` with `ClientId INT IDENTITY` as the primary key and `TaxId VARCHAR(8)` as a unique business identifier. This follows the employee table's generated-key pattern while allowing a tax ID correction without changing the row identity. Add `FullName NVARCHAR(100)`, `ShortName NVARCHAR(50)`, `ResponsiblePerson NVARCHAR(100)`, and `Address NVARCHAR(255)`, all `NOT NULL`, plus non-empty and eight-digit check constraints.

### Add two forward-only migration versions

Create `202609060001_CreateClientsTable` after the existing employee migrations and `202609060002_CreateClientProcedures` for `Client_Query` and `Client_Command`. The table migration owns the table, constraints, and unique index. The procedure migration owns the complete query and CREATE/UPDATE/DELETE procedure definitions. Each `Down()` reverses only its own version in dependency order.

### Mirror the existing Stored Procedure repository contract

`Client_Query` accepts an optional `@ClientId` and returns the six client columns ordered by `ClientId DESC`. `Client_Command` accepts `@Action`, optional `@ClientId`, and all client fields, trims inputs, rejects blank or invalid values, performs one transactional action, and returns the persisted row for CREATE and UPDATE. The repository maps SQL error 52007 to not found and SQL duplicate-key errors 2601/2627 to conflict.

### Use a prefixed resource route without changing the response envelope

Employee and health controllers will use `VAT_API` routes, and the new client controller will use `VAT_API/clients`. The existing `{ data: ... }` response envelope and camelCase JSON serialization remain unchanged. OpenAPI remains at `/openapi/v1.json` because it is not a controller resource route.

### Use native History API routing under a Vite base path

Do not add a routing dependency. `App.jsx` will resolve `/VAT_UI/employees` and `/VAT_UI/clients`, use `pushState`/`popstate` for navigation, and redirect `/` or `/VAT_UI/` to the employee page. `vite.config.js` will set `base: '/VAT_UI/'` so built assets resolve from the prefixed deployment path; the hosting server must serve `index.html` for the two SPA paths.

### Share grid behavior, keep entity dialogs separate

Extract the common AG Grid configuration into `ManagementGrid.jsx`; page components retain entity-specific loading, selection, dialogs, and delete messages. Register `AllEnterpriseModule` from `ag-grid-enterprise@36.1.0` once. Use `cellSelection: true`, the existing `singleRow` selection configuration, and `onRowDoubleClicked` to open the entity dialog. Do not set `enableCellTextSelection`, because it disables the grid clipboard behavior needed for shortcut copying. A missing production license key is an environment/deployment concern and must not be committed.

### Validate at every layer

Add migration source/contract tests, API DTO/controller contract tests, client form and route tests, and run frontend lint/build plus backend tests. After code verification, run the read-only database check; apply migrations only after explicit authorization and then perform endpoint smoke tests against the actual VAT database.

## Risks / Trade-offs

- **[Risk]** Changing employee and health routes can break existing callers using `/api/...`. → **Mitigation:** document the exact new URLs, update the frontend and README together, and include route contract tests.
- **[Risk]** Enterprise cell selection can conflict with browser text selection or row selection. → **Mitigation:** configure the shared grid once with `cellSelection: true`, single-row selection, and clipboard shortcut acceptance tests.
- **[Risk]** Direct SPA refreshes can fail when a production server does not fall back to `index.html`. → **Mitigation:** set the Vite base path and document the required fallback; verify direct navigation to both URLs.
- **[Risk]** A migration can exist in code but not in the connected database. → **Mitigation:** never claim database completion from build/tests alone; run `migrate:vat:check`, obtain authorization for `migrate:vat`, then verify table, procedures, and CRUD endpoints.

## Migration Plan

1. Validate the OpenSpec artifacts and run code-level migration/API/frontend tests without changing SQL Server.
2. Run `npm.cmd run migrate:vat:check` using the intended Windows identity and confirm `localhost / VAT`.
3. After explicit authorization, run `npm.cmd run migrate:vat`; verify `Clients`, `Client_Query`, and `Client_Command` and exercise create/list/update/delete plus duplicate and missing-record cases.
4. If the new migration must be reverted, explicitly authorize `npm.cmd run migrate:vat:down`; confirm procedures, table, constraints, and unique index are removed in the expected reverse order.
