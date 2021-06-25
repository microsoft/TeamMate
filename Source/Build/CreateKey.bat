@echo off
setlocal
REM http://msdn.microsoft.com/en-us/library/ff699202.aspx

set CWD=%~dp0
SET CWD=%CWD:~0,-1%

makecert -sv "%CWD%\TeamMate.pvk" -r "%CWD%\TeamMate.cer" -n "CN=Ben Amodio"
pvk2pfx -pvk "%CWD%\TeamMate.pvk" -spc "%CWD%\TeamMate.cer" -pfx "%CWD%\TeamMate.pfx"