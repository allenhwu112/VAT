## Context

`build:backend` and `test:backend` currently invoke .NET with `--no-restore`. Their generated `obj` files can retain an absolute NuGet package root under `/private/tmp`, which becomes invalid when the temporary cache is removed. The wrapper already launches the user-local .NET SDK and should explicitly pass through the caller's environment.

## Goals / Non-Goals

**Goals:**

- Make normal backend build and test entry points refresh NuGet assets before compilation.
- Keep caller-provided NuGet and .NET cache variables intact.
- Add regression coverage for environment forwarding without coupling tests to a machine-specific cache path.

**Non-Goals:**

- Do not commit packages, change package versions, or choose a hard-coded cache directory.
- Do not change migration execution, database schema, API behavior, or runtime startup.

## Decisions

- Run the existing `restore:backend` script before backend build and test. This reuses the four project-specific restore commands and the checked-in `NuGet.Config`, and it repairs stale ignored `obj` metadata before `--no-restore` compilation.
- Make `scripts/dotnet.js` construct an explicit child environment by cloning the caller environment. This preserves `NUGET_PACKAGES`, `DOTNET_CLI_HOME`, `NUGET_HTTP_CACHE_PATH`, connection overrides, and PATH while making the wrapper contract testable. A hard-coded fallback cache was rejected because cache ownership and writable locations differ across machines.
- Keep compilation commands `--no-restore` after the explicit restore so build/test steps remain deterministic and do not restore repeatedly within a single invocation.

## Risks / Trade-offs

- [Restore requires network or a complete configured cache] → report the restore error directly and allow callers to provide the existing cache environment variables.
- [Build and test become slower on a cold cache] → NuGet restore remains incremental and is performed once per top-level command.
