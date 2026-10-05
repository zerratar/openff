@echo off
rem Builds ff4hook's version.dll (32-bit, as FF4.exe is) into Tools\ff4hook\bin with the Visual Studio C++
rem tools found through vswhere. ff4steam.py install copies it beside the Steam FF4.exe.
setlocal
cd /d "%~dp0"
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" (echo no vswhere - install Visual Studio with the C++ tools & exit /b 1)
"%VSWHERE%" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath > "%TEMP%\ff4hook_vs.txt"
set /p VSDIR=<"%TEMP%\ff4hook_vs.txt"
del "%TEMP%\ff4hook_vs.txt"
if not defined VSDIR (echo no Visual Studio C++ tools found & exit /b 1)
call "%VSDIR%\VC\Auxiliary\Build\vcvarsall.bat" x86 >nul || exit /b 1
if not exist bin mkdir bin
cl /nologo /O2 /W3 /LD /MT ff4hook.c /Fobin\ /Febin\version.dll /link /DEF:version.def /NOLOGO || exit /b 1
echo bin\version.dll built
