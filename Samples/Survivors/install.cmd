@echo off
rem Builds Goblin Survivors straight into the client's mods folder (the OpenFF build beside
rem this repository), so it shows up in the title's MODS list on the next start.
rem Double-click, or run from anywhere. Rebuild while the client runs and it hot-reloads.
setlocal
set HERE=%~dp0
set CLIENT=%HERE%..\..\OpenFF\bin\Debug\net8.0
if not exist "%CLIENT%\OpenFF.exe" (
  echo The client has not been built: expected %CLIENT%\OpenFF.exe
  echo Build OpenFF first ^(dotnet build OpenFF^), then run this again.
  if /i not "%1"=="-q" pause
  exit /b 1
)
dotnet build "%HERE%Survivors.csproj" -c Debug -nologo -v q -o "%CLIENT%\mods\survivors"
if errorlevel 1 (
  echo Build failed.
  if /i not "%1"=="-q" pause
  exit /b 1
)
echo.
echo Installed to %CLIENT%\mods\survivors - start the client; on any map press T. The hero shoots on their own; 1/2/3 pick a card.
if /i not "%1"=="-q" pause
