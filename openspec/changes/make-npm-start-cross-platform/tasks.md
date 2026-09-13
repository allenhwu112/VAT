## 1. Cross-platform .NET launcher

- [x] 1.1 Add a dependency-free `scripts/dotnet.js` launcher that selects a local or PATH .NET executable, forwards arguments with inherited stdio, and propagates child completion; verify its exported resolution behavior with focused unit tests.
- [x] 1.2 Add `node:test` coverage for macOS and Windows local-path selection, PATH fallback, and argument forwarding without invoking .NET or a migration.

## 2. Root npm integration

- [x] 2.1 Replace every root npm script that invokes `scripts\\dotnet.cmd` with the Node launcher while preserving the existing `start` migration-first and `concurrently --kill-others-on-fail` composition; verify the script map has no npm-facing `.cmd` invocation.
- [x] 2.2 Add a tooling test command and update README startup/prerequisite instructions for macOS and Windows; verify the documented commands and the test command are present.

## 3. Verification

- [x] 3.1 Run the launcher tests plus applicable frontend and backend build checks, and verify `git diff --check` succeeds.
- [x] 3.2 With explicit database authorization, run the read-only migration check and a full `npm start` smoke test, verify the Vite page and API health endpoint, then stop both services cleanly unless the user explicitly requests that they remain running.
