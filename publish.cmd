@echo off
rem Builds a ready-to-run OpenFF folder for people who do not have the .NET SDK: the client
rem (OpenFF.exe), the engine mods reference, and Crystal (crystal.exe) side by side, with the
rem .NET runtime included, in dist\OpenFF - and zips it as dist\OpenFF-<version>-win-x64.zip,
rem which is what a GitHub release carries. The games themselves are not in it: the client
rem reads the Steam installs on the machine it runs on.
rem
rem The version is the <Version> in OpenFF\OpenFF.csproj, Crystal.Editor\Crystal.Editor.csproj
rem and OpenFF.Engine\OpenFF.Engine.csproj; keep the three the same and this line with them.
setlocal
set VERSION=0.1.2
cd /d "%~dp0"
if exist dist\OpenFF rmdir /s /q dist\OpenFF
dotnet publish OpenFF\OpenFF.csproj -c Release -r win-x64 --self-contained true -o dist\OpenFF -nologo -v q || goto fail
dotnet publish Crystal.Editor\Crystal.Editor.csproj -c Release -r win-x64 --self-contained true -o dist\OpenFF -nologo -v q || goto fail
if not exist dist\OpenFF\mods mkdir dist\OpenFF\mods
copy /y README.md dist\OpenFF\README.md >nul
copy /y Docs\Modding.md dist\OpenFF\Modding.md >nul
copy /y Docs\Releases.md dist\OpenFF\Releases.md >nul
copy /y LICENSE dist\OpenFF\LICENSE.txt >nul
rem The sample mods, for Crystal's Sample projects… (Samples\ beside crystal.exe); their build
rem output is left out. The Showcase is also a ready mod: mods\Showcase plays as it stands.
robocopy Samples dist\OpenFF\Samples /e /xd bin obj /njh /njs /ndl /nfl /nc /ns >nul
robocopy Samples\Showcase dist\OpenFF\mods\Showcase /e /xd bin obj /njh /njs /ndl /nfl /nc /ns >nul
rem The guide: the HTML tutorials Crystal opens with Help, readable on their own too.
if exist Docs\Guide robocopy Docs\Guide dist\OpenFF\Guide /e /njh /njs /ndl /nfl /nc /ns >nul
if exist dist\OpenFF-%VERSION%-win-x64.zip del dist\OpenFF-%VERSION%-win-x64.zip
powershell -NoProfile -Command "Compress-Archive -Path dist\OpenFF -DestinationPath dist\OpenFF-%VERSION%-win-x64.zip -CompressionLevel Optimal" || goto fail
echo.
echo dist\OpenFF: OpenFF.exe plays, crystal.exe edits, mods\ is where mods go.
echo dist\OpenFF-%VERSION%-win-x64.zip: the release.
exit /b 0

:fail
echo publish failed
exit /b 1
