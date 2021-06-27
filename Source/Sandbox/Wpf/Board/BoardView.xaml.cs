// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.DragAndDrop;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf.Board
{
    /// <summary>
    /// Interaction logic for BoardView.xaml
    /// </summary>
    public partial class BoardView : UserControl
    {
        public static readonly DependencyProperty ItemContainerHotStyleProperty = DependencyProperty.Register(
            "ItemContainerHotStyle", typeof(Style), typeof(BoardView)
        );

        public Style ItemContainerHotStyle
        {
            get { return (Style)GetValue(ItemContainerHotStyleProperty); }
            set { SetValue(ItemContainerHotStyleProperty, value); }
        }

        internal static void OnGridLineSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            BoardView board = (BoardView)obj;
            board.TopPseudoGridLineBorder = new Thickness(0, board.GridLineSize, 0, 0);
            board.LeftPseudoGridLineBorder = new Thickness(board.GridLineSize, 0, 0, 0);
            board.TopLeftPseudoGridLineBorder = new Thickness(board.GridLineSize, board.GridLineSize, 0, 0);
        }

        public static readonly DependencyProperty CellPaddingProperty = DependencyProperty.Register(
            "CellPadding", typeof(Thickness), typeof(BoardView)
        );

        public Thickness CellPadding
        {
            get { return (Thickness)GetValue(CellPaddingProperty); }
            set { SetValue(CellPaddingProperty, value); }
        }

        public static readonly DependencyProperty GridLineColorProperty = DependencyProperty.Register(
            "GridLineColor", typeof(Brush), typeof(BoardView)
        );

        public Brush GridLineColor
        {
            get { return (Brush)GetValue(GridLineColorProperty); }
            set { SetValue(GridLineColorProperty, value); }
        }

        public static readonly DependencyProperty GridLineSizeProperty = DependencyProperty.Register(
            "GridLineSize", typeof(int), typeof(BoardView), new PropertyMetadata(0, OnGridLineSizeChanged)
        );

        public static readonly DependencyProperty TopLeftPseudoGridLineBorderProperty = DependencyProperty.Register(
            "TopLeftPseudoGridLineBorder", typeof(Thickness), typeof(BoardView)
        );

        public Thickness TopLeftPseudoGridLineBorder
        {
            get { return (Thickness)GetValue(TopLeftPseudoGridLineBorderProperty); }
            set { SetValue(TopLeftPseudoGridLineBorderProperty, value); }
        }

        public static readonly DependencyProperty LeftPseudoGridLineBorderProperty = DependencyProperty.Register(
            "LeftPseudoGridLineBorder", typeof(Thickness), typeof(BoardView)
        );

        public Thickness LeftPseudoGridLineBorder
        {
            get { return (Thickness)GetValue(LeftPseudoGridLineBorderProperty); }
            set { SetValue(LeftPseudoGridLineBorderProperty, value); }
        }

        public static readonly DependencyProperty TopPseudoGridLineBorderProperty = DependencyProperty.Register(
            "TopPseudoGridLineBorder", typeof(Thickness), typeof(BoardView)
        );

        public Thickness TopPseudoGridLineBorder
        {
            get { return (Thickness)GetValue(TopPseudoGridLineBorderProperty); }
            set { SetValue(TopPseudoGridLineBorderProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register(
            "ItemTemplateSelector", typeof(DataTemplateSelector), typeof(BoardView)
        );

        public static readonly DependencyProperty MinColumnWidthProperty = DependencyProperty.Register(
            "MinColumnWidth", typeof(double), typeof(BoardView)
        );

        public double MinColumnWidth
        {
            get { return (double)GetValue(MinColumnWidthProperty); }
            set { SetValue(MinColumnWidthProperty, value); }
        }

        public static readonly DependencyProperty MinRowHeightProperty = DependencyProperty.Register(
            "MinRowHeight", typeof(double), typeof(BoardView)
        );

        public double MinRowHeight
        {
            get { return (double)GetValue(MinRowHeightProperty); }
            set { SetValue(MinRowHeightProperty, value); }
        }

        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        public int GridLineSize
        {
            get { return (int)GetValue(GridLineSizeProperty); }
            set { SetValue(GridLineSizeProperty, value); }
        }

        public BoardView()
        {
            this.DataContextChanged += HandleDataContextChanged;
            InitializeComponent();

            this.PreviewMouseLeftButtonDown += HandleLeftMouseDown;
            this.PreviewMouseLeftButtonUp += HandleLeftMouseUp;

            this.MouseEnter += HandleMouseEnter;

            UI.SetZoomWithWheel(grid, true);
        }

        private bool isDragging;
        private object draggedItem;
        private FrameworkElement draggedItemUIElement;
        private BoardItemsControl draggedItemContainer;
        private Point dragInitialMousePosition;
        private Cursor dragOriginalCursor;

        private BoardItemsControl dragTargetItemContainer;
        private bool dragTargetItemContainerCanDrop;
        private Style dragTargetItemContainerOriginalStyle;
        private DraggedAdorner draggedAdorner;
        private BoardCellViewModel dragSourceCell;
        private BoardCellViewModel dragTargetCell;

        private void HandleLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            BoardItemsControl itemsControl;
            DependencyObject itemContainer;
            object item;

            if (TryGetItem(e, out itemsControl, out itemContainer, out item))
            {
                // Shift cells by 1 (remember row and column headers)
                int sourceRow = Grid.GetRow(itemsControl) - 1;
                int sourceColumn = Grid.GetColumn(itemsControl) - 1;
                var sourceCell = ViewModel.GetCellAt(sourceRow, sourceColumn);

                if (ViewModel.CanMoveItem(item, sourceCell))
                {
                    this.dragInitialMousePosition = e.GetPosition(this);
                    this.draggedItem = item;
                    this.draggedItemUIElement = (FrameworkElement)itemContainer;
                    this.draggedItemContainer = itemsControl;
                    this.dragSourceCell = sourceCell;
                    this.PreviewMouseMove += HandleMouseMove;
                }
            }
        }

        private static bool TryGetItem(MouseButtonEventArgs e, out BoardItemsControl itemsControl, out DependencyObject itemContainer, out object item)
        {
            itemsControl = GetBoardItemContainer(e);
            item = null;
            itemContainer = null;

            if (itemsControl != null)
            {
                DependencyObject current = (DependencyObject)e.OriginalSource;
                while (item == null && current != itemsControl)
                {
                    object testItem = itemsControl.ItemContainerGenerator.ItemFromContainer(current);
                    if (testItem != DependencyProperty.UnsetValue)
                    {
                        itemContainer = current;
                        item = testItem;
                    }

                    current = VisualTreeHelper.GetParent(current);
                }
            }

            return (item != null);
        }

        private static BoardItemsControl GetBoardItemContainer(MouseEventArgs e)
        {
            return VisualTreeUtilities.TryFindAncestor<BoardItemsControl>((DependencyObject)e.OriginalSource);
        }

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            Point currentPosition = e.GetPosition(this);

            if (isDragging)
            {
                ShowDraggedAdorner(currentPosition);

                BoardItemsControl targetContainer = GetBoardItemContainer(e);
                if (targetContainer != dragTargetItemContainer)
                {
                    RevertLastDragTargetStyle();

                    dragTargetItemContainer = targetContainer;

                    if (dragTargetItemContainer != null)
                    {
                        // Shift cells by 1 (remember row and column headers)
                        int dragTargetRow = Grid.GetRow(dragTargetItemContainer) - 1;
                        int dragTargetColumn = Grid.GetColumn(dragTargetItemContainer) - 1;
                        dragTargetCell = ViewModel.GetCellAt(dragTargetRow, dragTargetColumn);

                        dragTargetItemContainerCanDrop = (dragTargetItemContainer != draggedItemContainer)
                            && ViewModel.CanMoveItemTo(draggedItem, dragSourceCell, dragTargetCell);

                        ApplyDragTargetStyle(targetContainer);
                    }
                }

                draggedAdorner.Content.Opacity = (dragTargetItemContainerCanDrop) ? 1 : 0.3;

                /*
                 * TODO: Add support for autoscrolling
                var scrolled = DragDropUtilities.AutoScrollIfNeeded(grid, e.GetPosition(grid));
                if (scrolled.X != 0 || scrolled.Y != 0)
                {
                    // TODO: This is not working still, not doing what I need it to be doing...
                    // this.draggedAdorner.SetPosition(currentPosition.X - this.initialMousePosition.X + scrolled.X, currentPosition.Y - this.initialMousePosition.Y + scrolled.Y);
                }
                 */

            }
            else if (this.draggedItem != null)
            {
                // Only drag when user moved the mouse by a reasonable amount.
                if (DragDropUtilities.ShouldBeginDrag(this.dragInitialMousePosition, currentPosition))
                {
                    isDragging = true;
                    dragOriginalCursor = this.Cursor;
                    this.Cursor = Cursors.Hand;
                    ShowDraggedAdorner(currentPosition);
                    draggedAdorner.Content.Opacity = 0.3;
                }
            }
        }

        private void RevertLastDragTargetStyle()
        {
            if (dragTargetItemContainer != null)
            {
                if (ItemContainerHotStyle != null)
                {
                    dragTargetItemContainer.Style = dragTargetItemContainerOriginalStyle;
                    dragTargetItemContainerOriginalStyle = null;
                }
            }
        }

        private void ApplyDragTargetStyle(BoardItemsControl targetContainer)
        {
            if (dragTargetItemContainerCanDrop)
            {
                dragTargetItemContainerOriginalStyle = targetContainer.Style;
                if (ItemContainerHotStyle != null)
                {
                    targetContainer.Style = ItemContainerHotStyle;
                }
            }
        }

        private void HandleLeftMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                if (dragTargetItemContainerCanDrop)
                {
                    var item = draggedItem;
                    var sourceCell = dragSourceCell;
                    var targetCell = dragTargetCell;

                    StopDragging();

                    ViewModel.MoveItemTo(item, sourceCell, targetCell);

                    var uiElement = VisualTreeUtilities.Descendants(this).OfType<Control>().FirstOrDefault(f => f.DataContext == item);
                    if (uiElement != null)
                    {
                        uiElement.BringIntoView();
                    }

                    return;
                }
            }

            StopDragging();
        }

        private void StopDragging()
        {
            if (isDragging)
            {
                Cursor = this.dragOriginalCursor;
                RevertLastDragTargetStyle();
                RemoveDraggedAdorner();

                this.isDragging = false;
                this.dragOriginalCursor = null;
                this.dragTargetItemContainerCanDrop = false;
                this.dragTargetItemContainerOriginalStyle = null;
                this.dragTargetCell = null;
                this.PreviewMouseMove -= HandleMouseMove;
            }

            this.draggedItem = null;
            this.draggedItemUIElement = null;
            this.draggedItemContainer = null;
            this.dragTargetItemContainer = null;
            this.dragSourceCell = null;
        }

        private void HandleMouseEnter(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                if (e.LeftButton == MouseButtonState.Released)
                {
                    StopDragging();
                }
            }
        }

        private void ShowDraggedAdorner(Point currentPosition)
        {
            if (this.draggedAdorner == null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(this);

                /*
                DataTemplate template = DragDropItemTemplate;
                if(template == null)
                {
                    if (ItemTemplateSelector != null)
                    {
                        template = ItemTemplateSelector.SelectTemplate(draggedItem, draggedItemUIElement);
                    }
                    else
                    {
                        template = ItemTemplate;
                    }
                }

                // TODO: If we use the default image constructor/rendering engine, we end up with a blurrier image
                // which also includes the margin. Need to fix that.
                // this.draggedAdorner = new DraggedAdorner(
                //     this.draggedItemUIElement, adornerLayer, this.draggedItemUIElement.DataContext, template);
                */

                this.draggedAdorner = new DraggedAdorner(this.draggedItemUIElement, adornerLayer);
            }

            this.draggedAdorner.SetPosition(currentPosition.X - this.dragInitialMousePosition.X, currentPosition.Y - this.dragInitialMousePosition.Y);
        }

        private void RemoveDraggedAdorner()
        {
            if (this.draggedAdorner != null)
            {
                this.draggedAdorner.Detach();
                this.draggedAdorner = null;
            }
        }

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate", typeof(DataTemplate), typeof(BoardView)
        );

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty DragDropItemTemplateProperty = DependencyProperty.Register(
            "DragDropItemTemplate", typeof(DataTemplate), typeof(BoardView)
        );

        public DataTemplate DragDropItemTemplate
        {
            get { return (DataTemplate)GetValue(DragDropItemTemplateProperty); }
            set { SetValue(DragDropItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemContainerTemplateProperty = DependencyProperty.Register(
            "ItemContainerTemplate", typeof(ControlTemplate), typeof(BoardView)
        );

        public ControlTemplate ItemContainerTemplate
        {
            get { return (ControlTemplate)GetValue(ItemContainerTemplateProperty); }
            set { SetValue(ItemContainerTemplateProperty, value); }
        }

        public static readonly DependencyProperty ColumnHeaderTemplateProperty = DependencyProperty.Register(
            "ColumnHeaderTemplate", typeof(DataTemplate), typeof(BoardView)
        );

        public DataTemplate ColumnHeaderTemplate
        {
            get { return (DataTemplate)GetValue(ColumnHeaderTemplateProperty); }
            set { SetValue(ColumnHeaderTemplateProperty, value); }
        }

        public static readonly DependencyProperty RowHeaderTemplateProperty = DependencyProperty.Register(
            "RowHeaderTemplate", typeof(DataTemplate), typeof(BoardView)
        );

        public DataTemplate RowHeaderTemplate
        {
            get { return (DataTemplate)GetValue(RowHeaderTemplateProperty); }
            set { SetValue(RowHeaderTemplateProperty, value); }
        }

        public static readonly DependencyProperty TopLeftContentProperty = DependencyProperty.Register(
            "TopLeftContent", typeof(object), typeof(BoardView)
        );

        public object TopLeftContent
        {
            get { return (object)GetValue(TopLeftContentProperty); }
            set { SetValue(TopLeftContentProperty, value); }
        }

        private BoardViewModel ViewModel
        {
            get { return DataContext as BoardViewModel; }
        }

        private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();

            if (ViewModel != null)
            {
                GridLength autoSize = new GridLength(0, GridUnitType.Auto);
                GridLength starSize = new GridLength(1, GridUnitType.Star);

                // COLUMN/ROW DEFNITIONS

                // Top Left Cell & Column Header Row
                AddColumnDefinition(autoSize, false);
                AddRowDefinition(autoSize, false);

                // Columns
                foreach (var columns in ViewModel.Columns)
                {
                    AddColumnDefinition(starSize);
                }

                // Rows
                int rowCount = ViewModel.Rows.Count;
                int rowIndex = 0;
                foreach (var rows in ViewModel.Rows)
                {
                    bool isLastRow = (++rowIndex == rowCount);
                    AddRowDefinition((isLastRow) ? starSize : autoSize);
                }

                // SHARED STYLES AND TEMPLATES
                Style topLeftCellStyle = (Style)FindResource("TopLeftCellStyle");
                Style columnHeaderStyle = (Style)FindResource("ColumnHeaderStyle");
                Style rowHeaderStyle = (Style)FindResource("RowHeaderStyle");
                ControlTemplate cellTemplate = (ControlTemplate)FindResource("BoardItemsControlTemplate");

                // CONTROLS

                // Top Left Cell
                ContentControl topLeftControl = new ContentControl();
                topLeftControl.Style = topLeftCellStyle;
                Bind(topLeftControl, ContentControl.ContentProperty, "TopLeftContent");
                AddToGrid(topLeftControl, 0, 0);

                // Column Headers
                int columnIndex = 0;
                foreach (var column in ViewModel.Columns)
                {
                    CreateHeaderCell(0, ++columnIndex, column, columnHeaderStyle, "ColumnHeaderTemplate");
                }

                // Row Headers
                rowIndex = 0;
                foreach (var row in ViewModel.Rows)
                {
                    CreateHeaderCell(++rowIndex, 0, row, rowHeaderStyle, "RowHeaderTemplate");
                }

                // Cells
                for (int i = 0; i < ViewModel.Rows.Count; i++)
                {
                    for (int j = 0; j < ViewModel.Columns.Count; j++)
                    {
                        BoardItemsControl itemContainer = new BoardItemsControl();
                        itemContainer.Template = cellTemplate;
                        Bind(itemContainer, ItemsControl.ItemTemplateProperty, "ItemTemplate");
                        Bind(itemContainer, ItemsControl.ItemTemplateSelectorProperty, "ItemTemplateSelector");

                        var cell = ViewModel.GetCellAt(i, j);
                        itemContainer.DataContext = cell;
                        itemContainer.ItemsSource = cell.Items;

                        AddToGrid(itemContainer, i + 1, j + 1);
                    }
                }
            }
        }

        private ContentControl CreateHeaderCell(int row, int column, object content, Style style, string templatePath)
        {
            ContentControl headerCell = new ContentControl();
            headerCell.Content = content;
            AddToGrid(headerCell, row, column);

            headerCell.Style = style;
            Bind(headerCell, ContentControl.ContentTemplateProperty, templatePath);
            return headerCell;
        }

        private void AddToGrid(UIElement e, int row, int column)
        {
            Grid.SetRow(e, row);
            Grid.SetColumn(e, column);
            grid.Children.Add(e);
        }

        private RowDefinition AddRowDefinition(GridLength height, bool useMinSize = true)
        {
            RowDefinition row = new RowDefinition();
            row.Height = height;

            if(useMinSize)
                row.MinHeight = MinRowHeight + GridLineSize;

            grid.RowDefinitions.Add(row);
            return row;
        }

        private ColumnDefinition AddColumnDefinition(GridLength width, bool useMinSize = true)
        {
            ColumnDefinition column = new ColumnDefinition();
            column.Width = width;

            if (useMinSize)
                column.MinWidth = MinColumnWidth + GridLineSize;

            grid.ColumnDefinitions.Add(column);
            return column;
        }

        private void Bind(FrameworkElement e, DependencyProperty p, string propertyPath)
        {
            Binding binding = new Binding();
            binding.Source = this;
            binding.Path = new PropertyPath(propertyPath);
            e.SetBinding(p, binding);
        }
    }

    public class BoardItemsControl : ItemsControl
    {
        public BoardItemsControl()
        {
            this.Focusable = false;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new BoardItem();
        }
    }

    public class BoardItem : ContentControl
    {
    }
}
