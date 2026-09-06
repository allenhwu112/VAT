## Context

The completed invoice-management change already loads `GET /VAT_API/invoices` into the React page and renders it through a shared AG Grid Enterprise wrapper. The wrapper owns the Enterprise module registration, single-row selection, cell selection, clipboard behavior, and row double-click callback. The pagination request changes only frontend presentation; it does not require a new API contract or SQL migration.

## Goals / Non-Goals

**Goals:**

- Enable AG Grid client-side pagination only for `InvoicesPage`.
- Use a default page size of 25 with selectable sizes 25, 50, and 100.
- Keep the built-in AG Grid pagination panel so current page, page count, and navigation remain consistent with the installed Enterprise version.
- Clear the invoice row selection when the page or page size changes, preventing edit/delete actions from operating on a row that is no longer visible.
- Leave employee and client pages unchanged by keeping pagination opt-in.

**Non-Goals:**

- Do not add server-side pagination, API query parameters, database paging, caching, sorting endpoints, or new dependencies.
- Do not alter the employee/client URL paths or their existing grid behavior.
- Do not add a custom MUI pagination component that duplicates AG Grid's native pagination panel.

## Decisions

### Use opt-in client-side pagination in the shared wrapper

Add an `enablePagination` prop to `ManagementGrid`, defaulting to `false`. When enabled, pass `pagination: true`, `paginationPageSize: 25`, and `paginationPageSizeSelector: [25, 50, 100]` to `AgGridReact`. This reuses the installed Enterprise Grid and avoids changing existing pages. Server-side pagination was considered, but it would require changing the invoice Stored Procedure, repository, API response contract, loading state, and database tests for a requirement that only asks for Grid pagination.

### Reset invoice selection on pagination changes

Expose an optional `onPaginationChanged` callback from `ManagementGrid` and call it with the Grid API. `InvoicesPage` will deselect all rows and clear `selectedInvoice` when the user changes page or page size. This avoids a selected row remaining hidden while the edit/delete buttons stay enabled, and it cannot silently substitute a different row.

### Keep pagination configuration independently testable

Place the enabled/disabled pagination prop construction in a small pure JavaScript helper. Unit tests will assert the default page size and selector values and that the shared wrapper remains opt-in. The React build and source contract will additionally verify the Enterprise module, cell selection, row selection, clipboard-capable wrapper, and pagination callback are wired together.

## Risks / Trade-offs

- **[Risk]** Client-side pagination still loads the complete invoice list into the browser. → **Mitigation:** retain the existing API for this change and document server-side paging as a future scale improvement.
- **[Risk]** Clearing selection on page changes may require the user to reselect a row after navigating. → **Mitigation:** this prevents actions against an invisible row and keeps the single-row selection contract explicit.
- **[Risk]** AG Grid Enterprise pagination controls can change with a library upgrade. → **Mitigation:** use the installed v36.1.0 native properties and test the exact page-size configuration rather than duplicating its UI.

## Migration Plan

1. Add frontend pagination configuration tests before changing the Grid wrapper.
2. Add opt-in pagination to `ManagementGrid` and enable it for `InvoicesPage`.
3. Run frontend tests, lint, and build; verify employee/client routes and existing grid usage remain unchanged.
4. No API, database, FluentMigrator, or migration-runner operation is required.
