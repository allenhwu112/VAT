## Context

See `proposal.md` for motivation. Root npm scripts currently delegate .NET work to `scripts\\dotnet.cmd`. npm executes those scripts through a Unix shell on macOS, where the backslash is treated as an escape and the batch file is not executable. Replacing only the path separator would still leave a Windows batch file that macOS cannot run.

## Goals / Non-Goals

**Goals:**

- Provide one dependency-free launcher that starts the .NET CLI on both macOS and Windows.
- Preserve arguments, standard input/output/error, exit status, and the existing migration-before-concurrent-start sequence.
- Make every documented root npm command that currently reaches `dotnet.cmd` use the same cross-platform path.
- Cover runner selection and argument forwarding without contacting the database or running migrations.

**Non-Goals:**

- Change database connection configuration, migration ordering, or the rule that the API does not migrate its schema on startup.
- Replace the existing Windows batch runner for direct Windows use.
- Add a process-management dependency or claim Windows runtime validation from a macOS-only test environment.

## Decisions

### Use a Node.js .NET launcher under `scripts/`

Add a small JavaScript launcher that resolves a user-local .NET executable for the current platform when present, then falls back to `dotnet` on `PATH`. It will call the executable through `child_process.spawn` with a non-shell invocation and inherited stdio, forwarding all received arguments and mirroring the child exit result.

Node.js is already a prerequisite for npm scripts, so this avoids a new package dependency and handles paths safely on both platforms. `spawn` avoids shell quoting differences between zsh and cmd.exe.

Alternatives considered:

- Use conditional shell syntax in `package.json`: rejected because shell syntax and executable discovery differ between macOS and Windows.
- Replace the runner with `cross-env` or another package: rejected because it does not solve invoking a platform-specific `.cmd` file and adds an unnecessary dependency.
- Change only `npm start`: rejected because its migration prerequisite and every other root .NET command would remain broken on macOS.

### Route all root .NET npm scripts through the launcher

Update `backend`, backend build/restore/test, and all migration scripts to invoke `node scripts/dotnet.js` rather than `scripts\\dotnet.cmd`. `start` will keep its existing composition: migration first, then `concurrently` launches the frontend and backend, stopping the sibling process if either fails.

Keeping `scripts/dotnet.cmd` unchanged preserves direct compatibility for existing Windows users while npm scripts gain a single cross-platform execution path.

### Test the launcher as a pure Node module

Expose narrowly scoped resolution and process-construction logic so a `node:test` test can simulate macOS and Windows path candidates, validate fallback behavior, and assert exact argument forwarding. Tests must not invoke a real .NET executable, migration, API, or database.

## Risks / Trade-offs

- [A developer's .NET location is nonstandard] → Fall back to `dotnet` on `PATH` and emit a clear command-not-found failure if neither candidate is available.
- [A child process exits from a signal or nonzero status] → Propagate that result so npm and `concurrently` retain their existing failure behavior.
- [Windows behavior cannot be exercised in this macOS environment] → Keep the batch runner intact and require a Windows smoke test before reporting Windows runtime verification.
- [A real `npm start` applies migrations] → Use unit and command-shape validation by default; obtain explicit authorization before any migration-backed smoke test.

## Migration Plan

1. Add the launcher and its focused tests.
2. Redirect the affected root npm scripts and update the startup documentation for macOS and Windows.
3. Run the tooling tests and platform-appropriate build/lint checks.
4. Roll back by restoring the previous npm script references and removing the new launcher/test; the existing `dotnet.cmd` remains available throughout.
