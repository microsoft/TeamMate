using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Web;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Model.Actions;
using Microsoft.Tools.TeamMate.Utilities;
using System;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Services
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class CommandLineService
    {
        private const string Create = "create";
        private const string Search = "search";
        private const string Open = "open";
        private const string Connect = "connect";
        private const string Exit = "exit";
        private const string OpenUri = "openuri";
        private const string Autostart = "autostart";
        private const string Uninstall = "uninstall";

        private const string Default = "default";
        private const string Id = "id";
        private const string Collection = "collection";
        private const string Project = "project";
        private const string Type = "type";
        private const string Command = "command";

        [Import]
        public WindowService WindowService { get; set; }

        [Import]
        public MessageBoxService MessageBoxService { get; set; }

        [Import]
        public GlobalCommandService GlobalCommandService { get; set; }

        [Import]
        public SessionService SessionService { get; set; }

        [Import]
        public ConfigurationService ConfigurationService { get; set; }

        [Import]
        public SettingsService SettingsService { get; set; }

        [Import]
        public VstsConnectionService VstsConnectionService { get; set; }


        public async void Execute(string[] args)
        {
            Assert.ParamIsNotNull(args, "args");

            try
            {
                if (args.Length == 0)
                {
                    this.WindowService.ShowMainWindow(true);
                }
                else
                {
                    string arg = args[0];

                    if (String.Equals(Path.GetExtension(arg), TeamMateApplicationInfo.TeamMateFileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        OpenFile(arg);
                    }
                    else if (String.Equals(arg, OpenUri, StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length != 2)
                        {
                            throw new Exception("Unexpected number of arguments");
                        }

                        string uri = args[1];
                        await ExecuteUriAsync(uri);
                    }
                    else
                    {
                        CommandLineArgs parsedArgs = CommandLineArgs.Parse(args.Skip(1).ToArray());
                        await ExecuteCommandAsync(arg, parsedArgs);
                    }
                }
            }
            catch (Exception e)
            {
                this.MessageBoxService.ShowError(e);
            }
        }

        private async Task ExecuteUriAsync(string activationUriString)
        {
            Uri activationUri = new Uri(activationUriString);
            NameValueCollection queryParams = HttpUtility.ParseQueryString(activationUri.Query);
            if (queryParams.Count > 0)
            {
                CommandLineArgs args = ExtractQueryStringParams(queryParams);
                if (!args.HasValue(Command))
                {
                    throw new Exception("Please specify a valid command value");
                }

                string command = args.GetValue<string>(Command);
                args.RemoveValue(Command);

                await ExecuteCommandAsync(command, args);
            }
        }

        private static CommandLineArgs ExtractQueryStringParams(NameValueCollection queryString)
        {
            CommandLineArgs args = new CommandLineArgs();

            foreach (var key in queryString.AllKeys)
            {
                if (key == null)
                {
                    var switches = queryString.Get(key).Split(",".ToArray());
                    foreach (var value in switches)
                    {
                        args.SetSwitch(value);
                    }
                }
                else
                {
                    var value = queryString.Get(key);
                    if (String.IsNullOrEmpty(value))
                    {
                        args.SetSwitch(key);
                    }
                    else
                    {
                        args.SetValue(key, value);
                    }
                }
            }

            return args;
        }


        public async void ExecuteCommand(string command, CommandLineArgs args)
        {
            Assert.ParamIsNotNull(command, "command");
            Assert.ParamIsNotNull(args, "args");

            try
            {
                await ExecuteCommandAsync(command, args);
            }
            catch (Exception e)
            {
                this.MessageBoxService.ShowError(e);
            }
        }

        private async Task ExecuteCommandAsync(string command, CommandLineArgs args)
        {
            switch (NormalizeCommand(command))
            {
                case Create:
                    await CreateWorkItemAsync(args);
                    break;

                case Search:
                    this.GlobalCommandService.QuickSearch();
                    break;

                case Exit:
                    this.WindowService.RequestShutdown();
                    break;

                case Open:
                    OpenWorkItem(args);
                    break;

                case Connect:
                    await ConnectToProjectAsync(args);
                    break;

                case Autostart:
                    // Do nothing, the caller already takes care of starting the main window minimized
                    break;

                case Uninstall:
                    PerformUninstall();
                    break;

                default:
                    Debug.Fail("Received command line args: " + String.Join(" ", args.ToString()));
                    throw new Exception(String.Format("Unrecognized command {0}", command));
            }
        }

        private async Task ConnectToProjectAsync(CommandLineArgs args)
        {
            Uri collectionUri = GetCollectionUri(args);
            string projectName = args.GetValue<string>(Project);

            var reference = await this.VstsConnectionService.ResolveProjectReferenceAsync(collectionUri, projectName);
            var projectInfo = new Model.ProjectInfo(reference, projectName);

            var settings = this.SettingsService.Settings;
            if (!settings.Projects.Contains(projectInfo))
            {
                settings.Projects.Add(projectInfo);
                this.VstsConnectionService.BeginConnect(projectInfo);
            }
        }

        private async Task CreateWorkItemAsync(CommandLineArgs args)
        {
            bool createDefault = args.HasSwitch(Default);
            if (createDefault)
            {
                this.GlobalCommandService.QuickCreateDefault();
            }
            else if (!args.Any())
            {
                this.GlobalCommandService.QuickCreate();
            }
            else
            {
                Uri collectionUri = GetCollectionUri(args);
                string projectName = args.GetValue<string>(Project);
                string workitemType = args.GetValue<string>(Type);

                var projectReference = await this.VstsConnectionService.ResolveProjectReferenceAsync(collectionUri, projectName);

                // TODO: Eliminate project lookup? Or at least do it async? Or reuse existing project context if it matches?
                WorkItemTypeReference reference = new WorkItemTypeReference(workitemType, projectReference);
                this.WindowService.ShowNewWorkItemWindow(reference);
            }
        }

        private void OpenWorkItem(CommandLineArgs args)
        {
            int id = args.GetValue<int>(Id);
            if (id <= 0)
            {
                throw new Exception("Invalid id: " + id);
            }

            Uri collectionUri = GetCollectionUri(args);
            WorkItemReference reference = new WorkItemReference(collectionUri, id);
            this.WindowService.ShowWorkItemWindow(reference);
        }

        private static Uri GetCollectionUri(CommandLineArgs args)
        {
            Uri collectionUri = args.GetValue<Uri>(Collection);
            if (!UriUtilities.IsHttpUri(collectionUri))
            {
                throw new Exception("Invalid uri: " + collectionUri);
            }
            return collectionUri;
        }

        private static string NormalizeCommand(string command)
        {
            Assert.ParamIsNotNull(command, "command");

            return command.ToLowerInvariant();
        }

        private void OpenFile(string inputFile)
        {
            Assert.ParamIsNotNull(inputFile, "inputFile");

            ActionSerializer actionSerializer = new ActionSerializer();
            TeamMateAction action = actionSerializer.ReadAction(inputFile);
            if (action.DeleteOnLoad)
            {
                File.Delete(inputFile);
            }

            switch (action.Type)
            {
                case ActionType.CreateWorkItem:
                    CreateWorkItemAction cwia = (CreateWorkItemAction)action;
                    this.GlobalCommandService.QuickCreateDefault(cwia.UpdateInfo);
                    break;

                default:
                    throw new Exception("Unsupported action");
            }
        }

        private void PerformUninstall()
        {
            this.ConfigurationService.Uninstall();
        }

        public static string GetArgsForOpening(WorkItemReference reference)
        {
            return String.Format("{0} \"{1}{2}:{3}\" \"{4}{5}:{6}\"",
                Open,
                CommandLineArgs.SwitchChar, Id, reference.Id,
                CommandLineArgs.SwitchChar, Collection, reference.ProjectCollectionUri);
        }

        public static string GetArgsForCreateDefault()
        {
            return String.Format("{0} \"{1}{2}\"",
                Create,
                CommandLineArgs.SwitchChar, Default);
        }

        public static string GeatArgsForCreate()
        {
            return Create;
        }

        public static string GetArgsForSearch()
        {
            return Search;
        }

        public static string GetArgsForExit()
        {
            return Exit;
        }

        public static string GetArgsForAutostart()
        {
            return Autostart;
        }

        public static string[] GetArgListForUri(Uri activationUri)
        {
            return new string[] { OpenUri, activationUri.AbsoluteUri };
        }
    }
}
