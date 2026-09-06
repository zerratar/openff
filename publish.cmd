@echo off
rem Builds a ready-to-run OpenFF folder for people who do not have the .NET SDK: the client
rem (OpenFF.exe), the engine mods reference, and Crystal (crystal.exe) side by side, with the
rem .NET runtime included, in dist\OpenFF. Zip that folder to share it. The games themselves
rem are not in it: the client reads the Steam installs on the machine it runs on.
setlocal
cd /d "%~dp0"
dotnet publish OpenFF\OpenFF.csproj -c Release -r win-x64 --self-contained true -o dist\OpenFF -nologo -v q || goto fail
dotnet publish Crystal.Editor\Crystal.Editor.csproj -c Release -r win-x64 --self-contained true -o dist\OpenFF -nologo -v q || goto fail
if not exist dist\OpenFF\mods mkdir dist\OpenFF\mods
copy /y README.md dist\OpenFF\README.md >nul
copy /y Docs\Modding.md dist\OpenFF\Modding.md >nul
echo.
echo dist\OpenFF: OpenFF.exe plays, crystal.exe edits, mods\ is where mods go.
exit /b 0

:fail
echo publish failed
exit /b 1
