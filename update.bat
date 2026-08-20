@echo off
setlocal
cd /d "%~dp0"

rem Prefer the per-user .NET installation when it exists.
if exist "%USERPROFILE%\.dotnet\dotnet.exe" (
    set "PATH=%USERPROFILE%\.dotnet;%PATH%"
    set "DOTNET_ROOT=%USERPROFILE%\.dotnet"
)

set "HAS_DOTNET_SDK="
for /f "delims=" %%i in ('dotnet --list-sdks 2^>nul') do set "HAS_DOTNET_SDK=1"
if not defined HAS_DOTNET_SDK (
    echo .NET 10 SDK not found. Please install the .NET 10 SDK first.
    echo Download: https://dotnet.microsoft.com/download/dotnet/10.0
    exit /b 1
)

echo Generating OnlineRepo.json...
dotnet run --project ScriptParser\ScriptParser.csproj -- "%CD%"
if errorlevel 1 (
    echo Failed to generate OnlineRepo.json.
    exit /b 1
)
echo OnlineRepo.json updated.
