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
set VERSION=0.3.1
cd /d "%~dp0"
if exist dist\OpenFF rmdir /s /q dist\OpenFF
dotnet publish OpenFF\OpenFF.csproj -c Release -r win-x64 --self-contained true -o dist\OpenFF -nologo -v q || goto fail
dotnet publish Crystal.Editor\Crystal.Editor.csproj -c Release -r win-x64 --self-contained true -o dist\OpenFF -nologo -v q || goto fail
rem The updater: a small native program (Native AOT - no .NET needed), which the client runs from a copy in the
rem temp folder to put a new release in place; the linker's tools are found through vswhere, from the VS Installer.
set "PATH=%PATH%;%ProgramFiles(x86)%\Microsoft Visual Studio\Installer"
if exist dist\updater rmdir /s /q dist\updater
dotnet publish OpenFF.Updater\OpenFF.Updater.csproj -c Release -r win-x64 -o dist\updater -nologo -v q || goto fail
copy /y dist\updater\OpenFF.Updater.exe dist\OpenFF\OpenFF.Updater.exe >nul || goto fail
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
if not exist dist\OpenFF-%VERSION%-win-x64.zip goto fail
rem The zip's SHA-256, which the client checks a downloaded update against before it installs it.
powershell -NoProfile -Command "$h = (Get-FileHash -Algorithm SHA256 'dist\OpenFF-%VERSION%-win-x64.zip').Hash.ToLower(); Set-Content -NoNewline -Encoding ascii 'dist\OpenFF-%VERSION%-win-x64.zip.sha256' ($h + '  OpenFF-%VERSION%-win-x64.zip')" || goto fail
echo.
echo dist\OpenFF: OpenFF.exe plays, crystal.exe edits, mods\ is where mods go.
echo dist\OpenFF-%VERSION%-win-x64.zip: the release, and its .sha256 beside it.
exit /b 0

:fail
echo publish failed
exit /b 1
