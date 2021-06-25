using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Internal.Tools.TeamMate.Sandbox.Wpf.Board
{
    public class BoardViewModel
    {
        private Dictionary<CellKey, BoardCellViewModel> cells = new Dictionary<CellKey, BoardCellViewModel>();
        private ObservableCollection<BoardColumnViewModel> columns = new ObservableCollection<BoardColumnViewModel>();
        private ObservableCollection<BoardRowViewModel> rows = new ObservableCollection<BoardRowViewModel>();

        public event EventHandler<ItemMoveStartedEventArgs> ItemMoveStarted;
        public event EventHandler<ItemMovingEventArgs> ItemMoving;
        public event EventHandler<ItemMovedEventArgs> ItemMoved;

        public IList<BoardColumnViewModel> Columns
        {
            get { return this.columns; }
        }

        public IList<BoardRowViewModel> Rows
        {
            get { return this.rows; }
        }

        public IEnumerable<BoardCellViewModel> Cells
        {
            get { return cells.Values; }
        }

        public IEnumerable<object> Items
        {
            get
            {
                foreach (var cell in Cells)
                {
                    foreach (var item in cell.Items)
                    {
                        yield return item;
                    }
                }
            }
        }

        public BoardCellViewModel FindCell(object item)
        {
            return Cells.FirstOrDefault(cell => cell.Items.Contains(item));
        }

        public BoardCellViewModel GetCellAt(BoardRowViewModel row, BoardColumnViewModel col)
        {
            BoardCellViewModel result;
            CellKey cellKey = new CellKey(row, col);
            if(!cells.TryGetValue(cellKey, out result))
            {
                result = new BoardCellViewModel(row, col);
                cells[cellKey] = result;
            }

            return result;
        }

        public BoardCellViewModel GetCellAt(int row, int col)
        {
            return GetCellAt(Rows[row], Columns[col]);
        }

        public bool CanMoveItem(object item, BoardCellViewModel source)
        {
            bool cancelled = false;
            if (ItemMoveStarted != null)
            {
                var args = new ItemMoveStartedEventArgs(item, source);
                ItemMoveStarted(this, args);
                cancelled = args.Cancel;
            }

            return !cancelled;
        }

        public bool CanMoveItemTo(object item, BoardCellViewModel source, BoardCellViewModel target)
        {
            bool cancelled = false;
            if (ItemMoving != null)
            {
                var args = new ItemMovingEventArgs(item, source, target);
                ItemMoving(this, args);
                cancelled = args.Cancel;
            }

            return !cancelled;
        }

        public void MoveItemTo(object item, BoardCellViewModel source, BoardCellViewModel target)
        {
            source.Items.Remove(item);
            target.Items.Add(item);

            if (ItemMoved != null)
            {
                var args = new ItemMovedEventArgs(item, source, target);
                ItemMoved(this, args);
            }
        }

        private class CellKey : Tuple<BoardRowViewModel, BoardColumnViewModel>
        {
            public CellKey(BoardRowViewModel row, BoardColumnViewModel col)
                : base(row, col)
            {
            }
        }
    }

    public class ItemMoveStartedEventArgs : CancelEventArgs
    {
        public ItemMoveStartedEventArgs(object item, BoardCellViewModel source)
        {
            this.Item = item;
            this.Source = source;
        }

        public object Item { get; private set; }
        public BoardCellViewModel Source { get; private set; }
    }

    public class ItemMovingEventArgs : CancelEventArgs
    {
        public ItemMovingEventArgs(object item, BoardCellViewModel source, BoardCellViewModel target)
        {
            this.Item = item;
            this.Source = source;
            this.Target = target;
        }

        public object Item { get; private set; }
        public BoardCellViewModel Source { get; private set; }
        public BoardCellViewModel Target { get; private set; }
    }

    public class ItemMovedEventArgs : EventArgs
    {
        public ItemMovedEventArgs(object item, BoardCellViewModel source, BoardCellViewModel target)
        {
            this.Item = item;
            this.Source = source;
            this.Target = target;
        }

        public object Item { get; private set; }
        public BoardCellViewModel Source { get; private set; }
        public BoardCellViewModel Target { get; private set; }
    }

    public class BoardColumnViewModel
    {
        public BoardColumnViewModel()
        {
        }

        public BoardColumnViewModel(object item)
        {
            this.Item = item;
        }

        public object Item { get; set; }
    }

    public class BoardRowViewModel
    {
        public BoardRowViewModel()
        {
        }

        public BoardRowViewModel(object item)
        {
            this.Item = item;
        }

        public object Item { get; set; }
    }

    public class BoardCellViewModel
    {
        public BoardCellViewModel(BoardRowViewModel row, BoardColumnViewModel column)
        {
            this.Items = new ObservableCollection<object>();
            this.Row = row;
            this.Column = column;
        }

        public BoardRowViewModel Row { get; private set; }
        public BoardColumnViewModel Column { get; private set; }
        public IList<object> Items { get; private set; }
    }
}
