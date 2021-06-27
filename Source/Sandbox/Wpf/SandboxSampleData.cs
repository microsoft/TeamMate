// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media.Imaging;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Resources;
using Microsoft.Tools.TeamMate.Sandbox.Wpf.Board;
using Microsoft.Tools.TeamMate.Sandbox.Wpf.Tiles;
using Microsoft.Tools.TeamMate.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf
{
    public static class SandboxSampleData
    {
        private static ObservableCollection<Message> messages;
        private static Microsoft.Tools.TeamMate.ViewModels.ToastViewModel notification;

        private static TileModel tile;

        private static Random random = new Random();

        private static TileCollection tileCollection;

        private static ImageSource LoadComponent(string relativePath)
        {
            string assemblyName = typeof(SandboxSampleData).Assembly.GetName().Name;
            var stream = System.Windows.Application.GetResourceStream(new Uri(assemblyName + ";component" + relativePath, UriKind.Relative));
            return BitmapUtilities.LoadImage(stream.Stream);
        }

        public static TileCollection TileCollection
        {
            get
            {
                if (tileCollection == null)
                {
                    tileCollection = new TileCollection();
                    TileGroup tg = new TileGroup();

                    var bigTile = new Tiles.Tile();
                    bigTile.Size = Tiles.TileSize.Double;
                    tg.Tiles.Add(bigTile);
                    tg.Tiles.Add(new Tiles.Tile());
                    tg.Tiles.Add(new Tiles.Tile());
                    tg.Tiles.Add(new Tiles.Tile());

                    TileCollection.Groups.Add(tg);

                    TileCollection.Groups.Add(new TileGroup());
                    TileCollection.Groups.Add(new TileGroup());
                    TileCollection.Groups.Add(new TileGroup());
                    TileCollection.Groups.Add(new TileGroup());
                }

                return tileCollection;
            }
        }

        public static TileModel Tile
        {
            get
            {
                if (tile == null)
                {
                    tile = new TileModel();
                    tile.Title = "Product Backlog";
                    tile.Count = 18;

                    List<TileDataModel> tileData = new List<TileDataModel>();
                    tileData.Add(new TileDataModel(1324, 1, "Something really bad is happening to us. I don't know what it is, but we are crashing left and right. We need to figure it out soon."));
                    tileData.Add(new TileDataModel(3424, 3, "I'm really liking this rotating banner, it is probably the best feature yet."));
                    tileData.Add(new TileDataModel(6549, 2, "Talk to Ben and figure out what new features we want to add"));
                    tileData.Add(new TileDataModel(8888, 1, "Fatal Error"));

                    tile.Items = tileData;
                }

                return tile;
            }
        }

        public static Microsoft.Tools.TeamMate.ViewModels.ToastViewModel Notification
        {
            get
            {
                if (notification == null)
                {
                    notification = new Microsoft.Tools.TeamMate.ViewModels.ToastViewModel();
                    notification.Title = "Peter Bright";
                    notification.Description = "Another Test";
                    notification.Icon = (ImageSource)App.Current.FindResource("ApplicationIcon");
                    // notification.Photo = (ImageSource)App.Current.FindResource("ApplicationIcon");
                }

                return notification;
            }
        }

        public static ObservableCollection<Message> Messages
        {
            get
            {
                if (messages == null)
                {
                    messages = new ObservableCollection<Message>();

                    DateTime now = DateTime.Now;

                    messages.Add(new Message("Joe Stevens", "May vacations", now));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-1)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-2)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-3)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-4)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-5)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-6)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-7)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-10)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-15)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-20)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-25)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-30)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-35)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-45)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-60)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-90)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-365)));
                    messages.Add(new Message("Joe Stevens", "May vacations", now.AddDays(-700)));

                    var r = new Random();
                    foreach (var item in messages)
                    {
                        item.IsRead = r.Next(2) == 1;
                    }
                }

                return messages;
            }
        }

        private static ListViewModel listViewModel;

        public static ListViewModel ListViewModel
        {
            get
            {
                if (listViewModel == null)
                {
                    listViewModel = new ListViewModel();
                    listViewModel.CollectionView = new ListCollectionView(Messages);

                    listViewModel.Filters.Add(new ListViewFilter("All"));
                    listViewModel.Filters.Add(new ListViewFilter("Unread", delegate(object o)
                    {
                        return o is Message && !((Message)o).IsRead;
                    }));

                    listViewModel.Fields.Add(ListFieldInfo.Create<DateTime>("Date", "Date"));
                    listViewModel.Fields.Add(ListFieldInfo.Create<string>("From", "From"));
                    listViewModel.OrderBy(listViewModel.Fields[0]);

                    listViewModel.ShowInGroups = true;
                }

                return listViewModel;
            }
        }

        private static List<WorkItemRowViewModel> listOfWorkItems;

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

        private static WorkItemRowViewModel CreateWorkItemRowViewModel()
        {
            WorkItemRowViewModel info = new WorkItemRowViewModel();
            info.Reference = new WorkItemReference(SampleData.SampleCollectionUri, 1234);
            info.AssignedTo = "Joe Stevens";
            info.Type = "Bug";
            info.Title = "Hello World!";
            info.FullTitle = "Bug 1234: Hello World!";
            info.State = "Active";
            info.ChangedDate = DateTime.Now.Subtract(TimeSpan.FromDays(random.Next(200)));
            return info;
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

                    var dateField = ListFieldInfo.Create<DateTime>("ChangedDate", "Changed Date");

                    workItemListViewModel.Fields.Add(dateField);
                    workItemListViewModel.OrderBy(dateField);

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
        public static BoardViewModel Board
        {
            get
            {
                int maxItemsInCell = 10;

                Random r = new Random();

                BoardViewModel board = new BoardViewModel();

                board.Columns.Add(new BoardColumnViewModel("TO DO"));
                board.Columns.Add(new BoardColumnViewModel("IN PROGRESS"));
                board.Columns.Add(new BoardColumnViewModel("DONE"));

                board.Rows.Add(new BoardRowViewModel("Sample Bug"));
                board.Rows.Add(new BoardRowViewModel("Another Story"));

                int id = 1;
                for (int i = 0; i < board.Rows.Count; i++)
                {
                    for (int j = 0; j < board.Columns.Count; j++)
                    {
                        BoardCellViewModel cell = board.GetCellAt(i, j);
                        int itemCount = r.Next(maxItemsInCell);
                        for (int k = 0; k < itemCount; k++)
                        {
                            cell.Items.Add("Task " + id++);
                        }
                    }
                }

                return board;
            }
        }
    }

    public class Message : ObservableObjectBase
    {
        public Message(string from, string subject, DateTime date)
        {
            this.From = from;
            this.Subject = subject;
            this.Date = date;
        }

        private string subject;

        public string Subject
        {
            get { return this.subject; }
            set { SetProperty(ref this.subject, value); }
        }

        private DateTime date;

        public DateTime Date
        {
            get { return this.date; }
            set { SetProperty(ref this.date, value); }
        }

        private string from;

        public string From
        {
            get { return this.from; }
            set { SetProperty(ref this.from, value); }
        }

        private bool isRead;

        public bool IsRead
        {
            get { return this.isRead; }
            set { SetProperty(ref this.isRead, value); }
        }
    }
}
