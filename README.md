# TeamMate

<p align="center">
  <img alt="TeamMate" src="https://raw.githubusercontent.com/microsoft/TeamMate/c03cc824da9359958ae0340eaf6a158d5cb8e9ca/Images/Badge.png?token=AKNCJVZLG2OMQ2H7NLHY5X3BLIPOK">
</p>

TeamMate is an application for managing Azure Dev Ops (ADO) work items and pull requests.

TeamMate was designed from the ground up to manage, search and collaborate on bugs easily. It is built for power users. You can search for or create bugs from anywhere using global keyboard shortcuts. You can track any and all of your ADO queries.

Managing work item queries is simple. You can pin several queries as tiles, filter/group work items on common fields, receive toast notifications when work items change, quickly filter a list of work items using search terms, or do a global search of work items in your local pinned queries.

Pull Requests (PRs) are also first class citizens, and treated almost identically to work items. You get notifications, filtering, search, and quick digests and status of a set of reviews. 

## Installation

Coming Soon.

## Keyboard Shortcuts

* WIN + `: opens the quick search dialog.
* SHIFT + WIN + A: creates a new default work item.
* CTRL + WIN + A: opens the work item type picker to create a new work item.
* WIN + Esc: opens the TeamMate home page.
* ESC: minimizes or closes windows.

## Telemetry

TeamMate does NOT send any telemetry to Microsoft and anywhere else. None.

## Authors

TeamMate was originally created by Ben Amodio.

Authors (in alphabetical order):

* Ben Amodio
* Justin Lam
* Marcus Markiewicz
* Vanya Kashperuk

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Developing

### Pre-Requisites

First, install [Visual Studio Community](https://visualstudio.microsoft.com/vs/community/). Make sure to install .NET Framework 4.8 and C# support.

### Setup

Open a Visual Studio Developer Command Prompt and run:

```bash
git clone https://github.com/microsoft/TeamMate.git
cd Sources
msbuild /restore:true /p:Platform=x64 /p:Configuration=Debug
```

The TeamMate executable can be found under Sources\TeamMate\bin.

MSI installers can be found under Sources\Setup\bin.

## Application Data

Application Data refers to files and settings created and stored by the application at runtime. All of TeamMate's application data is stored in the folder %LOCALAPPDATA%\TeamMate. 

On first run this folder doesn't exist and is created on demand. Deleting this folder and restarting TeamMate is essentially a way of triggering a "first run experience" again. You can also back this directory to save your settings.

Here's an example of the files contained in the TeamMate appdata folder: 

![AppData example state](https://raw.githubusercontent.com/microsoft/TeamMate/main/Images/AppData.png?token=AKNCJVYYGVOZPRJPX3NRFRLBLIQD4)

This might include: 

* Logs, which stores plain text diagnostics logs used for troubleshooting 
* Projects, which contains XML files capture TFS project specific information/configuration. 
* Settings, which contains XML files that store user application settings. 

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
