#!/usr/bin/env node

"use strict";

const { spawn } = require("node:child_process");
const { statSync } = require("node:fs");
const { homedir } = require("node:os");
const path = require("node:path");

function isRegularFile(filePath) {
  try {
    return statSync(filePath).isFile();
  } catch {
    return false;
  }
}

function getLocalDotnetPath({
  platform = process.platform,
  environment = process.env,
  userHome = homedir(),
  isFile = isRegularFile,
} = {}) {
  const isWindows = platform === "win32";
  const pathApi = isWindows ? path.win32 : path.posix;
  const home = isWindows
    ? environment.USERPROFILE || userHome
    : environment.HOME || userHome;
  const executable = isWindows ? "dotnet.exe" : "dotnet";
  const localDotnet = pathApi.join(home, ".dotnet", executable);

  return isFile(localDotnet) ? localDotnet : null;
}

function resolveDotnetCommand(options) {
  return getLocalDotnetPath(options) || "dotnet";
}

function runDotnet(args, {
  spawnProcess = spawn,
  resolveCommand = resolveDotnetCommand,
  onError = (error) => {
    console.error(`Unable to start .NET SDK: ${error.message}`);
    process.exitCode = 1;
  },
  onExit = (code, signal) => {
    if (signal) {
      console.error(`.NET SDK process stopped by signal ${signal}.`);
      process.exitCode = 1;
      return;
    }

    process.exitCode = code ?? 1;
  },
} = {}) {
  const command = resolveCommand();
  const child = spawnProcess(command, args, {
    shell: false,
    stdio: "inherit",
  });

  child.once("error", onError);
  child.once("exit", onExit);
  return child;
}

if (require.main === module) {
  runDotnet(process.argv.slice(2));
}

module.exports = {
  getLocalDotnetPath,
  resolveDotnetCommand,
  runDotnet,
};
