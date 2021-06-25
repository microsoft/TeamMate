This project wraps access to Windows Runtime (WinRT) APIs, for Desktop Application consumption.

See http://www.hanselman.com/blog/HowToCallWinRTAPIsInWindows8FromCDesktopApplicationsWinRTDiagram.aspx for more info.

In theory, we don't need a library to wrap access to this. However, I found that adding these references to Windows
causes ClickOnce manifest creation to fail, since the DLLs cannot be scanned as part of the

<GenerateManifests>true</GenerateManifests>

step. Wrapping these references in a project reference seems to workaround the problem. I also found a discussion on MSDN
that talks about the same workaround.