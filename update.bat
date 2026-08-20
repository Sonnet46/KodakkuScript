@echo off
setlocal
cd /d "%~dp0"
echo Generating OnlineRepo.json...
dotnet run --project ScriptParser\ScriptParser.csproj -- "%CD%"
if errorlevel 1 (
    echo Failed to generate OnlineRepo.json.
    exit /b 1
)
echo OnlineRepo.json updated.
