## Context

The repository already has employee and client CRUD backed by SQL Server Stored Procedures, FluentMigrator, ADO.NET repositories, Controllers, MUI dialogs, and a shared AG Grid Enterprise wrapper. The frontend uses pure JavaScript, native History API paths under `/VAT_UI`, and the API uses the `/VAT_API` prefix. Existing employee and client routes and migrations are already applied design constraints; see `proposal.md` and the invoice-management delta spec for the observable contract.

## Goals / Non-Goals

**Goals:**

- Add an independently versioned invoice-settings record with one row per unique tax ID.
- Keep invoice options as explicit booleans so the grid and dialog can represent checked and unchecked states without inventing quantities.
- Mirror the existing client CRUD contract, validation, error mapping, response envelopes, and deletion confirmation.
- Reuse the existing Enterprise grid behavior for single-row selection, cell/range selection, clipboard copying, and double-click editing.
- Preserve the exact employee and client URLs while adding an explicit invoice URL and navigation link.

**Non-Goals:**

- Do not create a foreign-key relationship to `Clients`; the requested invoice record explicitly owns the tax ID and customer short name and does not define synchronization rules.
- Do not add invoice numbering, invoice issuance transactions, period-based history, quantities, accounting calculations, import/export, authentication, or authorization.
- Do not change an existing migration or run a database-changing command during implementation.

## Decisions

### Use independent invoice settings keyed by a unique tax ID

Create `dbo.Invoices` with `InvoiceId INT IDENTITY` as the primary key, `TaxId VARCHAR(8)` as a unique business key, and `ClientShortName NVARCHAR(50)` as required text. A direct `ClientId` foreign key was considered, but the request supplies tax ID and short name as the invoice-management fields and does not define client deletion or rename behavior. Keeping the record independent avoids silently coupling two master-data lifecycles; later synchronization can be added as a separate change.

### Store the seven requested options as non-null BIT values

Use `ElectronicInvoice`, `CashRegister`, `ThreeCashRegister`, `TwoPartInvoice`, `TwoPartInvoiceCopy`, `ThreePartInvoice`, and `ThreePartInvoiceCopy`, each `BIT NOT NULL DEFAULT 0`. The user listed invoice categories rather than counts or amounts, so booleans provide an unambiguous checked/unchecked contract. The UI labels remain the requested Traditional Chinese names, including `三收銀`.

### Add forward-only table and procedure migrations

Use migration `202609060003` for the table, unique index, default constraints, and text/tax-ID checks. Use `202609060004` for `Invoice_Query` and `Invoice_Command`. `Invoice_Query` accepts an optional `@InvoiceId` and orders collections by `InvoiceId DESC`. `Invoice_Command` accepts `CREATE`, `UPDATE`, and `DELETE`, trims text inputs, validates the tax ID and short name, performs one transaction, and returns the created or updated row. Each `Down()` reverses only its own objects in dependency order.

### Mirror the existing backend and frontend contracts

Add `InvoiceDtos.cs`, `InvoiceRepository.cs`, and `InvoicesController.cs`, register `IInvoiceRepository` in `Program.cs`, and map SQL error 52007 to not-found and 2601/2627 to conflict. The public response contains only `invoiceId`, `taxId`, `clientShortName`, and the seven camelCase boolean option fields.

Add `src/api/invoices.js` with `emptyInvoiceForm`, client-side validation, trimming, and CRUD calls. Add `InvoiceDialog.jsx` with text inputs and checkboxes, and `InvoicesPage.jsx` with the same loading/error/selection/delete-confirmation flow as the employee and client pages. Extend `routes.js` and `App.jsx` with `/VAT_UI/invoices`; keep `/VAT_UI/employees` as the root fallback and leave existing employee/client paths unchanged.

The shared `ManagementGrid.jsx` already registers `AllEnterpriseModule`, enables `cellSelection`, configures `singleRow`, and wires row double-click and selection callbacks. Invoice columns will use that wrapper and format booleans as readable `是`／`否` values while preserving clipboard export. No new grid dependency or license secret is needed.

### Validate contracts before database application

Add migration source/contract tests for table columns, constraints, versions, procedures, and reversible `Up`/`Down`; add API DTO/controller contract tests; add frontend invoice form and route tests. Run frontend lint/build and backend tests, then run only the read-only `migrate:vat:check` unless the user separately authorizes `migrate:vat`. Update `README.md` with the three exact UI URLs and `/VAT_API/invoices`.

## Risks / Trade-offs

- **[Risk]** A duplicated tax ID or short name can drift from the client master. → **Mitigation:** enforce unique tax ID within `Invoices`, validate both fields at every layer, and document the lack of synchronization as an intentional non-goal.
- **[Risk]** Users may expect invoice categories to hold quantities rather than yes/no state. → **Mitigation:** expose explicit boolean API properties and checkboxes; changing to quantities would require a new contract and migration rather than silently changing the BIT columns.
- **[Risk]** The new migration can exist in code without being applied to the connected SQL Server. → **Mitigation:** report code/test status separately from database status, run the read-only connection check first, and require explicit authorization before applying or rolling back migrations.
- **[Risk]** Direct refreshes under `/VAT_UI/invoices` depend on the host serving the Vite SPA fallback. → **Mitigation:** retain the configured `/VAT_UI/` base path and document the direct URL alongside the existing employee and client paths.

## Migration Plan

1. Validate OpenSpec artifacts and implement/test the migration, API, UI, and documentation changes without mutating SQL Server.
2. Run `npm.cmd run migrate:vat:check` under the intended Windows identity and confirm the expected `localhost / VAT` connection.
3. After explicit authorization, run `npm.cmd run migrate:vat`, then verify the `Invoices` table, `Invoice_Query`, `Invoice_Command`, and invoice CRUD endpoints.
4. If rollback is explicitly requested, run `npm.cmd run migrate:vat:down` and verify the procedure and table objects are removed in reverse order.
