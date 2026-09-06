## Why

The invoice-management record currently models every invoice option as a boolean, but six of those fields represent quantities that must accept values from 0 through 99. The electronic-invoice field should remain a checkbox while the remaining invoice fields need required numeric input and persistence.

## What Changes

- Keep `electronicInvoice` as an optional Boolean checkbox.
- Change `cashRegister`, `threeCashRegister`, `twoPartInvoice`, `twoPartInvoiceCopy`, `threePartInvoice`, and `threePartInvoiceCopy` to required integers in the inclusive range 0-99 across the API, database, Stored Procedures, and UI.
- Add a new forward migration after the applied invoice migrations that converts existing bit values, removes obsolete defaults, adds range constraints, and updates both invoice Stored Procedures with a reversible rollback guarded against lossy values.
- Replace the six invoice-option checkboxes with required numeric inputs while retaining the electronic-invoice checkbox and Enterprise Grid display.
- Update backend, migration, and frontend contract tests and documentation without changing invoice URLs or pagination behavior.

## Capabilities

### New Capabilities

- `invoice-quantity-fields`: Required 0-99 invoice quantity fields with a Boolean electronic-invoice flag across persistence, API, and UI.

### Modified Capabilities

- None.

## Impact

- Backend API models, repository parameter mapping, and validation.
- FluentMigrator schema and Stored Procedure migration under `apps/backend`.
- Invoice form, API normalization, Grid column formatting, and frontend tests under `apps/frontend`.
- Existing applied migrations remain unchanged; no new dependency is required.
