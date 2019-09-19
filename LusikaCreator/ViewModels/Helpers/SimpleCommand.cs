using System;
using System.Windows.Input;

namespace TestApp.ViewModels.Helpers
{
    public class SimpleCommand<T> : ICommand
    {
        readonly Action<T> _onExecute;

        public SimpleCommand(Action<T> onExecute)
        {
            _onExecute = onExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _onExecute((T) parameter);
    }
}