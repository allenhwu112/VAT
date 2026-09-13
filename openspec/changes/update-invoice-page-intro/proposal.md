## Why

The invoice-management page currently describes specific invoice issuance methods instead of the concise label requested for the customer-facing invoice type information. The page copy should match the updated wording without changing any invoice-management behavior.

## What Changes

- Replace the invoice-management page intro text with `客戶使用的發票種類`.
- Preserve the existing page structure, controls, data grid, routes, API behavior, and styling.

## Capabilities

### New Capabilities

None. This is a presentation-copy adjustment and does not introduce an application capability.

### Modified Capabilities

None. Existing invoice-management requirements and observable CRUD behavior remain unchanged.

## Impact

- Affected code: `apps/frontend/src/pages/InvoicesPage.jsx`.
- No API, dependency, database, or migration changes are required.
