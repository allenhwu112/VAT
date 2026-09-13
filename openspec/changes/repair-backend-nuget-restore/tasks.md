## 1. OpenSpec and tooling contract

- [x] 1.1 Update `scripts/dotnet.js` to pass a cloned child environment while preserving caller-provided .NET/NuGet variables, and verify the wrapper tests cover those variables.
- [x] 1.2 Update `scripts/dotnet.test.js` for the explicit environment contract and verify `npm run test:tooling` passes.

## 2. Backend workflow

- [x] 2.1 Make `build:backend` restore all backend projects before `--no-restore` compilation, and verify stale generated assets are replaced by a valid restore.
- [x] 2.2 Make `test:backend` restore all backend projects before tests, and verify `npm run test:backend` passes.

## 3. Verification

- [x] 3.1 Run `npm run build:backend` and verify both `Vat.Api` and `Vat.Migrations` build without missing metadata errors.
- [x] 3.2 Run `git diff --check` and `git status --short`, confirming only scoped OpenSpec/tooling files changed and no migration command was executed.
