using System;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Input
{
    public class RelayCommand : ICommand
    {
        private Action action;
        private Func<bool> canExecute;

        public RelayCommand(Action action) : this(action, null)
        {
        }

        public RelayCommand(Action action, Func<bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return (this.canExecute != null) ? this.canExecute() : true;
        }

        public void Execute(object parameter)
        {
            action?.Invoke();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public class RelayCommand<T> : ICommand where T : class
    {
        private Action<T> action;
        private Predicate<T> canExecute;

        public RelayCommand(Action<T> action)
            : this(action, null)
        {
        }

        public RelayCommand(Action<T> action, Predicate<T> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return (this.canExecute != null) ? this.canExecute(parameter as T) : true;
        }

        public void Execute(object parameter)
        {
            action?.Invoke(parameter as T);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
