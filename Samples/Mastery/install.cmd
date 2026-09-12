@echo off
rem Builds the sample mod straight into the client's mods folder (the OpenFF build beside
rem this repository), so it shows up in the title's MODS list on the next start.
rem Double-click, or run from anywhere. Rebuild while the client runs and it hot-reloads.
setlocal
set HERE=%~dp0
set CLIENT=%HERE%..\..\OpenFF\bin\Debug\net8.0
if not exist "%CLIENT%\OpenFF.exe" (
  echo The client has not been built: expected %CLIENT%\OpenFF.exe
  echo Build OpenFF first ^(dotnet build OpenFF^), then run this again.
  pause
  exit /b 1
)
dotnet build "%HERE%Mastery.csproj" -c Debug -nologo -v q -o "%CLIENT%\mods\Mastery"
if errorlevel 1 (
  echo Build failed.
  pause
  exit /b 1
)
echo.
echo Installed to %CLIENT%\mods\Mastery - start the client, the MODS list on the title shows it.
if /i not "%1"=="-q" pause
