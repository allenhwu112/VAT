@echo off
setlocal

set "LOCAL_DOTNET=%USERPROFILE%\.dotnet\dotnet.exe"

if exist "%LOCAL_DOTNET%" (
  "%LOCAL_DOTNET%" %*
  exit /b %ERRORLEVEL%
)

where dotnet >nul 2>&1
if errorlevel 1 (
  echo .NET SDK was not found. Install .NET 10 SDK first.
  exit /b 1
)

dotnet %*
exit /b %ERRORLEVEL%
