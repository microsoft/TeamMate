@echo off

rc.exe /nologo TeamMateExe.rc
rc.exe /nologo /d NO_MANIFEST /fo TeamMateExe_NoManifest.res TeamMateExe.rc