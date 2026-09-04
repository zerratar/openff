@echo off
rem Builds the sample mod straight into the client's mods folder (the FF3.Game build beside
rem this repository), so it shows up in the title's MODS list on the next start.
rem Double-click, or run from anywhere. Rebuild while the client runs and it hot-reloads.
setlocal
set HERE=%~dp0
set CLIENT=%HERE%..\..\FF3.Game\bin\Debug\net8.0
if not exist "%CLIENT%\FF3.exe" (
  echo The client has not been built: expected %CLIENT%\FF3.exe
  echo Build FF3.Game first ^(dotnet build FF3.Game^), then run this again.
  pause
  exit /b 1
)
dotnet build "%HERE%HelloMod.csproj" -c Debug -nologo -v q -o "%CLIENT%\mods\hello"
if errorlevel 1 (
  echo Build failed.
  pause
  exit /b 1
)
echo.
echo Installed to %CLIENT%\mods\hello - start the client, the MODS list on the title shows it.
if /i not "%1"=="-q" pause
