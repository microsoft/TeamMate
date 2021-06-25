using Microsoft.Internal.Tools.TeamMate.Foundation.Threading;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls.Data;
using Microsoft.Internal.Tools.TeamMate.Model;
using Microsoft.Internal.Tools.TeamMate.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows.Data;

namespace Microsoft.Internal.Tools.TeamMate.Resources
{
    public static class SampleData
    {
        public static readonly Uri SampleCollectionUri = new Uri("https://sample.visualstudio.com/DefaultCollection");
        public static readonly string SampleProjectName = "TeamMate";
        public static readonly Uri SampleProjectUri = new Uri("vstfs:///Classification/TeamProject/D31BE5FA-E206-4B4A-B0B8-667071942520");
        public const string SampleQueryPath = @"My Queries\Sandbox Team - Sprint 1 - Backlog";

        private static Random Random = new Random();

        private static WorkItemQueryTileViewModel workItemQueryTileViewModel;
        private static MainWindowViewModel mainWindowViewModel;
        private static TrackingInfo trackingInfo;
        private static ToastViewModel toastViewModel;
        private static List<WorkItemRowViewModel> listOfWorkItems;

        public static CustomDialogViewModel CustomDialogViewModel
        {
            get
            {
                CustomDialogViewModel dialogViewModel = new CustomDialogViewModel();
                dialogViewModel.Title = "We would love to hear from you";
                dialogViewModel.Message =
                    "Looks like you've been using TeamMate quite a bit. We could use your feedback. " +
                    "You can provide it in our Toolbox page by clicking on the Rating section (right-hand side).\n\n" +
                    " Is this a good time to rate the app?";

                var button = new ButtonInfo("Sure");
                button.IsDefault = true;
                dialogViewModel.Buttons.Add(button);

                button = new ButtonInfo("No Thanks");
                dialogViewModel.Buttons.Add(button);

                button = new ButtonInfo("Maybe Later");
                button.IsCancel = true;
                dialogViewModel.Buttons.Add(button);

                dialogViewModel.CheckBoxText = "Do not ask me again";
                dialogViewModel.IsCheckBoxChecked = true;

                return dialogViewModel;
            }
        }

        private static CodeFlowReviewViewModel codeFlowReview;

        public static CodeFlowReviewViewModel CodeFlowReview
        {
            get
            {
                if (codeFlowReview == null)
                {
                    codeFlowReview = new CodeFlowReviewViewModel();
                    codeFlowReview.AuthorDisplayName = "Joe Stevens";
                    codeFlowReview.Name = "Bug 12347: Crashing bug on xyz";
                    codeFlowReview.CreatedOn = DateTime.Now.AddDays(-11);
                    codeFlowReview.IterationCount = 7;
                }

                return codeFlowReview;
            }
        }


        private static SearchPageViewModel searchPageViewModel;

        public static SearchPageViewModel SearchPageViewModel
        {
            get
            {
                if (searchPageViewModel == null)
                {
                    searchPageViewModel = new SearchPageViewModel();
                    searchPageViewModel.SearchText = "crashing bugs";
                }

                return searchPageViewModel;
            }
        }

        public static List<WorkItemRowViewModel> ListOfWorkItems
        {
            get
            {
                if (listOfWorkItems == null)
                {
                    listOfWorkItems = new List<WorkItemRowViewModel>();

                    listOfWorkItems.Add(CreateWorkItemRowViewModel());
                    listOfWorkItems.Add(CreateWorkItemRowViewModel());
                    listOfWorkItems.Add(CreateWorkItemRowViewModel());
                    listOfWorkItems.Add(CreateWorkItemRowViewModel());
                    listOfWorkItems.Add(CreateWorkItemRowViewModel());
                    listOfWorkItems.Add(CreateWorkItemRowViewModel());
                    listOfWorkItems.Add(CreateWorkItemRowViewModel());
                    listOfWorkItems.Add(CreateWorkItemRowViewModel());
                    listOfWorkItems.Add(CreateWorkItemRowViewModel());
                }

                return listOfWorkItems;
            }
        }

        private static WorkItemRowViewModel workItemRowViewModel;

        public static WorkItemRowViewModel WorkItemRowViewModel
        {
            get
            {
                if (workItemRowViewModel == null)
                {
                    workItemRowViewModel = CreateWorkItemRowViewModel();
                }

                return workItemRowViewModel;
            }
        }

        public static ToastViewModel ToastViewModel
        {
            get
            {
                if (toastViewModel == null)
                {
                    toastViewModel = new ToastViewModel();
                    toastViewModel.Title = "Joe Stevens resolved Bug 12345";
                    toastViewModel.Description = "The application crashes when dragging and dropping list elements";
                    toastViewModel.Icon = TeamMateResources.ToastIcon;
                }

                return toastViewModel;
            }
        }


        public static WorkItemQueryTileViewModel WorkItemQueryTileViewModel
        {
            get
            {
                if (workItemQueryTileViewModel == null)
                {
                    workItemQueryTileViewModel = CreateQueryTileViewModel();
                }

                return workItemQueryTileViewModel;
            }
        }

        public static MainWindowViewModel MainWindowViewModel
        {
            get
            {
                if (mainWindowViewModel == null)
                {
                    mainWindowViewModel = new MainWindowViewModel();

                    ICollection<WorkItemQueryTileViewModel> queries = new List<WorkItemQueryTileViewModel>();

                    mainWindowViewModel.HomePage.TileCollection.Tiles.Add(CreateQueryTileViewModel());
                    mainWindowViewModel.HomePage.TileCollection.Tiles.Add(CreateQueryTileViewModel());
                    mainWindowViewModel.HomePage.TileCollection.Tiles.Add(CreateQueryTileViewModel());
                    mainWindowViewModel.HomePage.TileCollection.Tiles.Add(CreateQueryTileViewModel());
                }

                return mainWindowViewModel;
            }
        }

        public static TrackingInfo TrackingInfo
        {
            get
            {
                if (trackingInfo == null)
                {
                    trackingInfo = new TrackingInfo();

                    var reference = new WorkItemReference(SampleCollectionUri, 1234);

                    trackingInfo.RecentlyViewedWorkItems.Add(reference);
                }

                return trackingInfo;
            }
        }

        private static ListViewModel workItemListViewModel;

        public static ListViewModel WorkItemListViewModel
        {
            get
            {
                if (workItemListViewModel == null)
                {
                    workItemListViewModel = new ListViewModel();
                    workItemListViewModel.CollectionView = new ListCollectionView(ListOfWorkItems);

                    /*
                    workItemListViewModel.Filters.Add(new ListViewFilter("All", true, null));
                    workItemListViewModel.Filters.Add(new ListViewFilter("Unread", delegate(object o)
                    {
                        return o is Message && !((Message)o).IsRead;
                    }));
                     */

                    workItemListViewModel.Fields.Add(ListFieldInfo.Create<DateTime>("ChangedDate", "Changed Date"));
                    workItemListViewModel.Fields.Add(ListFieldInfo.Create<string>("State", "State"));

                    workItemListViewModel.OrderBy(workItemListViewModel.Fields[0]);
                    workItemListViewModel.ShowInGroups = true;

                    // After applying the initial ordering/grouping, make sure the current item is still the first first element
                    workItemListViewModel.CollectionView.MoveCurrentToFirst();

                    /*
                    workItemListViewModel.SearchDelegate = delegate(string text, object item)
                    {
                        Message message = item as Message;
                        return (message != null && message.Subject != null && message.Subject.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0);
                    };
                     */
                }

                return workItemListViewModel;
            }
        }

        private static WorkItemQueryTileViewModel CreateQueryTileViewModel()
        {
            WorkItemQueryReference queryReference = new WorkItemQueryReference(SampleCollectionUri, Guid.Empty);

            TileInfo tileInfo = new TileInfo();
            tileInfo.Type = TileType.WorkItemQuery;
            tileInfo.WorkItemQueryReference = queryReference;
            tileInfo.Name = "Product Backlog with a long long name hopefully a 3 liner, maybe, we'll see blah blah blah blah blah";

            WorkItemQueryTileViewModel vm = new WorkItemQueryTileViewModel();
            vm.TileInfo = tileInfo;
            vm.Query.ItemCount = 7;
            vm.Query.UnreadItemCount = 3;

            TaskContext context = new TaskContext();
            // context.Fail("Error");
            // context.Complete();
            vm.Query.ProgressContext = context;
            return vm;
        }

        private static WorkItemRowViewModel CreateWorkItemRowViewModel()
        {
            WorkItemRowViewModel viewModel = new WorkItemRowViewModel();
            viewModel.Reference = new WorkItemReference(SampleCollectionUri, 1234);
            viewModel.AssignedTo = "Joe Stevens";
            viewModel.Type = "Bug";
            viewModel.Title = "Hello World!";
            viewModel.FullTitle = "Bug 1234: Hello World!";
            viewModel.State = "Active";
            viewModel.AttachmentCount = 5;
            viewModel.LinkCount = 7;
            viewModel.Tags = new string[] { "Build Break", "Suggestion", "vNext" };
            viewModel.ChangedDate = DateTime.Now.Subtract(TimeSpan.FromDays(Random.Next(200)));
            return viewModel;
        }

        private static NewWorkItemPageViewModel newWorkItemViewModel;

        public static NewWorkItemPageViewModel NewWorkItemViewModel
        {
            get
            {
                if(newWorkItemViewModel == null)
                {
                    newWorkItemViewModel = new NewWorkItemPageViewModel();
                    newWorkItemViewModel.WorkItemTypes = new WorkItemTypeInfo[]
                    {
                        new WorkItemTypeInfo(new WorkItemTypeReference("Bug", new ProjectReference(SampleCollectionUri, Guid.Empty))),
                        new WorkItemTypeInfo(new WorkItemTypeReference("Epic", new ProjectReference(SampleCollectionUri, Guid.Empty))),
                        new WorkItemTypeInfo(new WorkItemTypeReference("Task", new ProjectReference(SampleCollectionUri, Guid.Empty))),
                    };
                }

                return newWorkItemViewModel;
            }
        }


        private static ConnectionInfo disconnectedConnectionInfo;

        public static ConnectionInfo DisconnectedConnectionInfo
        {
            get
            {
                if (disconnectedConnectionInfo == null)
                {
                    disconnectedConnectionInfo = new ConnectionInfo();
                }

                return disconnectedConnectionInfo;
            }
        }

        private static ConnectionInfo connectingConnectionInfo;

        public static ConnectionInfo ConnectingConnectionInfo
        {
            get
            {
                if (connectingConnectionInfo == null)
                {
                    connectingConnectionInfo = new ConnectionInfo();
                    connectingConnectionInfo.ConnectionState = ConnectionState.Connecting;
                    connectingConnectionInfo.Project = new ProjectInfo(new ProjectReference(SampleCollectionUri, SampleProjectUri), SampleProjectName);
                }

                return connectingConnectionInfo;
            }
        }

        private static ConnectionInfo connectionFailedConnectionInfo;

        public static ConnectionInfo ConnectionFailedConnectionInfo
        {
            get
            {
                if (connectionFailedConnectionInfo == null)
                {
                    connectionFailedConnectionInfo = new ConnectionInfo();
                    connectionFailedConnectionInfo.ConnectionState = ConnectionState.ConnectionFailed;
                    connectionFailedConnectionInfo.ConnectionError = new Exception("TFS123443: Could not find XYZ.\nPlease contact your administrator.");
                }

                return connectionFailedConnectionInfo;
            }
        }

        private static HomePageViewModel homePage;

        public static HomePageViewModel HomePage
        {
            get
            {
                if (homePage == null)
                {
                    homePage = new HomePageViewModel();
                    homePage.Session = new Session();
                }

                return homePage;
            }
        }

        private static ProjectsPageViewModel projectsPage;

        public static ProjectsPageViewModel ProjectsPage
        {
            get
            {
                if (projectsPage == null)
                {
                    projectsPage = new ProjectsPageViewModel();
                    projectsPage.Projects = new List<ProjectInfo>();
                    projectsPage.Projects.Add(new ProjectInfo(new ProjectReference(SampleCollectionUri, SampleProjectUri), SampleProjectName));
                    projectsPage.Projects.Add(new ProjectInfo(new ProjectReference(SampleCollectionUri, SampleProjectUri), "Project 2"));
                    projectsPage.Projects.Add(new ProjectInfo(new ProjectReference(SampleCollectionUri, SampleProjectUri), "Project 3"));
                }

                return projectsPage;
            }
        }


        private static Counter counter;
        private static ItemCountSummary itemCountSummary;

        public static ItemCountSummary ItemCountSummary
        {
            get
            {
                if (itemCountSummary == null)
                {
                    itemCountSummary = new ItemCountSummary();
                    itemCountSummary.ActiveCounter.UpdateCount(11, true);
                    itemCountSummary.ResolvedCounter.UpdateCount(22, false);
                }

                return itemCountSummary;
            }
        }

        public static Counter Counter
        {
            get
            {
                if (counter == null)
                {
                    counter = new Counter();
                    counter.UpdateCount(11, false);
                }

                return counter;
            }
        }

        private static OverviewWindowViewModel overviewWindow;

        public static OverviewWindowViewModel OverviewWindow
        {
            get
            {
                if(overviewWindow == null)
                {
                    overviewWindow = new OverviewWindowViewModel();
                    overviewWindow.ItemCountSummary = ItemCountSummary;

                }

                return overviewWindow;
            }
        }
    }
}
