@echo off
setlocal

powershell.exe -command ". %~dp0\%~n0.ps1" %1 %2 %3 %4 %5 %6 %7 %8 %9
