using System;
using System.Windows.Input;

namespace Dynamo.UI.Commands
{
    /// <summary>
    /// Custom implementation of DelegateCommand which prints to the log.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        //http://wpftutorial.net/DelegateCommand.html

        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public DelegateCommand(Action<object> execute,
                       Predicate<object> canExecute)
        {
            if (execute == null)
            {
                _execute = (o) => { Console.WriteLine($"nameof(this) disabled"); };
            }
            else
            {
                _execute = execute;

            }

            if(canExecute == null)
            {
                _canExecute = (o)=> { return false; };
            }
            else
            {
                _canExecute = canExecute;

            }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            //OnExecute(parameter);
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

    }

}
