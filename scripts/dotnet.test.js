"use strict";

const assert = require("node:assert/strict");
const { EventEmitter } = require("node:events");
const test = require("node:test");

const {
  getLocalDotnetPath,
  resolveDotnetCommand,
  runDotnet,
} = require("./dotnet");

test("selects the macOS user-local .NET SDK", () => {
  const expected = "/Users/allen/.dotnet/dotnet";

  assert.equal(
    getLocalDotnetPath({
      platform: "darwin",
      environment: { HOME: "/Users/allen" },
      isFile: (candidate) => candidate === expected,
    }),
    expected,
  );
});

test("selects the Windows user-local .NET SDK", () => {
  const expected = "C:\\Users\\Jane Doe\\.dotnet\\dotnet.exe";

  assert.equal(
    getLocalDotnetPath({
      platform: "win32",
      environment: { USERPROFILE: "C:\\Users\\Jane Doe" },
      isFile: (candidate) => candidate === expected,
    }),
    expected,
  );
});

test("uses the home-directory fallback on Windows when USERPROFILE is missing", () => {
  const expected = "C:\\Users\\Fallback\\.dotnet\\dotnet.exe";

  assert.equal(
    getLocalDotnetPath({
      platform: "win32",
      environment: {},
      userHome: "C:\\Users\\Fallback",
      isFile: (candidate) => candidate === expected,
    }),
    expected,
  );
});

test("falls back to dotnet on PATH when no local SDK file exists", () => {
  assert.equal(
    resolveDotnetCommand({
      platform: "darwin",
      environment: { HOME: "/Users/allen" },
      isFile: () => false,
    }),
    "dotnet",
  );
});

test("forwards arguments to a non-shell child process with inherited stdio", () => {
  const child = new EventEmitter();
  let invocation;
  let completion;

  const result = runDotnet(
    [
      "run",
      "--project",
      "apps/backend/Vat.Migrations/Vat.Migrations.csproj",
      "--no-restore",
      "--",
      "--check",
    ],
    {
      resolveCommand: () => "dotnet",
      spawnProcess: (command, args, options) => {
        invocation = { args, command, options };
        return child;
      },
      onError: () => {},
      onExit: (code, signal) => {
        completion = { code, signal };
      },
    },
  );

  assert.equal(result, child);
  assert.deepEqual(invocation, {
    args: [
      "run",
      "--project",
      "apps/backend/Vat.Migrations/Vat.Migrations.csproj",
      "--no-restore",
      "--",
      "--check",
    ],
    command: "dotnet",
    options: {
      shell: false,
      stdio: "inherit",
    },
  });

  child.emit("exit", 0, null);
  assert.deepEqual(completion, { code: 0, signal: null });
});
