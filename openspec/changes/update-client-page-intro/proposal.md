## Why

The client-management page intro currently uses a general description of the VAT client master. Updating the copy to the requested wording makes the page purpose clearer while preserving all client-management behavior.

## What Changes

- Replace the client-management page intro text with `客戶基本資料及稅務資訊`.
- Preserve the existing page structure, controls, data grid, routes, API behavior, and styling.

## Capabilities

### New Capabilities

None. This is a presentation-copy adjustment and does not introduce an application capability.

### Modified Capabilities

None. Existing client-management requirements and observable CRUD behavior remain unchanged.

## Impact

- Affected code: `apps/frontend/src/pages/ClientsPage.jsx`.
- No API, dependency, database, or migration changes are required.
