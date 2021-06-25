@echo off
setlocal
set SN="%~dp0Tools\sn.exe"
set PFXFILE="%~dp0TeamMate.pfx"

%SN% -i %PFXFILE% TEAMMATE
%PFXFILE%
