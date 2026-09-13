## Why

The root `npm start` command embeds a Windows path separator for the .NET runner, so macOS shells resolve it as a different command and cannot start the VAT development stack. Developers must currently reconstruct the startup sequence manually, which makes the documented command unreliable across supported development platforms.

## What Changes

- Replace the platform-specific root startup composition with a cross-platform launcher that works from macOS and Windows shells.
- Preserve the existing startup contract: run the VAT migration command first, then start the Vite frontend and .NET API together, and stop sibling processes when either startup command fails.
- Add focused automated coverage for the cross-platform launcher or its command construction, and document the supported invocation if required.

## Capabilities

### New Capabilities

None. This is a developer tooling compatibility change; it does not introduce an application capability.

### Modified Capabilities

None. The API, migration, and application behavior requirements remain unchanged.

## Impact

- Affected files: root `package.json` and a narrowly scoped launcher and/or test under an allowed tooling path.
- Existing commands: `npm start`, `npm run frontend`, `npm run backend`, and `npm run migrate:vat` remain semantically consistent.
- No API contract, database schema, or production deployment behavior changes are intended.
