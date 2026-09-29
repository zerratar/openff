@echo off
rem Copies the sample mod into the client's mods folder (the OpenFF build beside this repository), so it shows up
rem in the title's MODS list on the next start. No code to build: a layout, a stylesheet and pictures.
setlocal
set HERE=%~dp0
set CLIENT=%HERE%..\..\OpenFF\bin\Debug\net8.0
if not exist "%CLIENT%\OpenFF.exe" (
  echo The client has not been built: expected %CLIENT%\OpenFF.exe
  echo Build OpenFF first ^(dotnet build OpenFF^), then run this again.
  pause
  exit /b 1
)
if exist "%CLIENT%\mods\StarlitMenu" rmdir /s /q "%CLIENT%\mods\StarlitMenu"
xcopy /e /i /q /y "%HERE%." "%CLIENT%\mods\StarlitMenu" >nul
del "%CLIENT%\mods\StarlitMenu\install.cmd" 2>nul
echo Installed to %CLIENT%\mods\StarlitMenu - start the client, the MODS list on the title shows it.
if /i not "%1"=="-q" pause
