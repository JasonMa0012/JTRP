for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" /latest /property installationPath`) do (
  set VSDIR=%%i
)
call "%VSDIR%\Common7\Tools\VsDevCmd.bat"
cd %~dp0
