## 1. Database schema and Stored Procedures

- [X] 1.1 Add migration contract tests for the two client migration versions, ClientId identity key, required client columns, tax ID constraint, unique index, and reversible Up/Down methods; verify the new tests fail before the implementation exists
- [X] 1.2 Implement `202609060001_CreateClientsTable` with the Clients table, required-field constraints, eight-digit tax ID check, and unique tax ID index; verify migration tests pass for the table contract
- [X] 1.3 Implement `202609060002_CreateClientProcedures` with `Client_Query` and transactional `Client_Command` CREATE/UPDATE/DELETE behavior, validation errors, duplicate handling, and reversible Down SQL; verify migration tests cover every procedure parameter and returned field

## 2. Backend client API and route prefix

- [X] 2.1 Add API contract tests for ClientDtos, required/whitespace validation, eight-digit tax ID validation, length limits, response shape, client route, and the `/VAT_API` employee and health routes; verify the tests fail before implementation
- [X] 2.2 Implement client request/response models with camelCase-compatible properties and field validation; verify all DTO contract tests pass
- [X] 2.3 Implement `ClientRepository` and `ClientsController` for list, get-by-id, create, update, delete, 404, 409, 400, 201, 200, and 204 behavior; register the repository and verify backend tests pass
- [X] 2.4 Update EmployeesController, HealthController, frontend API paths, README API examples, and OpenAPI route documentation to use the agreed `/VAT_API` prefix; verify route contract tests and documentation search show no stale resource URLs

## 3. Frontend routing, Enterprise Grid, and employee compatibility

- [X] 3.1 Add `ag-grid-enterprise@36.1.0`, register `AllEnterpriseModule`, and create the shared grid configuration with cell selection, clipboard copy, single-row selection, and row double-click callbacks; verify dependency resolution and frontend build succeed
- [X] 3.2 Add pure `/VAT_UI` route helpers and tests for employees, clients, root redirect, and unknown-path fallback; verify route tests pass
- [X] 3.3 Refactor the existing employee screen into the routed management layout without dropping contact fields or password behavior, add employee double-click editing, and add navigation links; verify the employee page remains functional at `/VAT_UI/employees`
- [X] 3.4 Add `ClientDialog`, client form state, required-field validation, trim/mapping logic, and client API helpers; verify frontend tests cover valid data, missing fields, invalid tax IDs, length limits, and API payload shape
- [X] 3.5 Add the client management page at `/VAT_UI/clients` with the required columns, complete CRUD toolbar, selection state, double-click editing, delete confirmation, reload/error handling, and shared Enterprise Grid; verify direct navigation, refresh, and frontend build succeed
- [X] 3.6 Update Vite base path, favicon/title, shared management styles, and README startup URLs for `/VAT_UI`; verify built asset paths use the VAT_UI prefix and lint passes

## 4. Integrated verification and migration safety

- [X] 4.1 Run `openspec validate add-client-management --strict` and verify every required artifact is valid
- [X] 4.2 Run frontend tests, frontend lint, frontend build, backend tests, `npm.cmd run build`, `git diff --check`, and `git status --short`; verify existing unrelated employee changes remain preserved
- [X] 4.3 Run the read-only `npm.cmd run migrate:vat:check` under the intended Windows identity and record `localhost / VAT`; do not run `migrate:vat` or `migrate:vat:down` without explicit authorization
- [X] 4.4 After separate explicit database authorization, apply the migrations and manually verify Clients CRUD, duplicate tax ID rejection, missing-record responses, both prefixed UI URLs, cell-range copying, and double-click editing against the actual VAT database
