## Why

The migration project can fail before compilation when generated NuGet assets point to a removed `/private/tmp/vat-nuget-packages` directory. The repository's backend build and test commands should restore dependencies before consuming generated assets so transient cache paths do not leave the workflow broken.

## What Changes

- Make backend build and test commands perform restore as part of their normal workflow.
- Preserve explicit `NUGET_PACKAGES`, `DOTNET_CLI_HOME`, and `NUGET_HTTP_CACHE_PATH` overrides in the .NET command wrapper.
- Add tooling regression coverage for restore-aware invocation and missing-cache recovery.
- Do not change migration schema, API contracts, database state, or committed dependency binaries.

## Capabilities

### New Capabilities

None. This is a tooling and build-workflow change, so specs are intentionally skipped.

### Modified Capabilities

None.

## Impact

The root npm scripts, `scripts/dotnet.js`, and its Node test file will change. Backend builds and tests may perform an incremental NuGet restore before compilation; no runtime or database behavior changes.
