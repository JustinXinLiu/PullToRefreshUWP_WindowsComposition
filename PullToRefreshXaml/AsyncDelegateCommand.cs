using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PullToRefreshXaml
{
    public class AsyncDelegateCommand : ICommand
    {
        #region Fields

        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        private bool _isExecuting;

        #endregion

        public AsyncDelegateCommand(Func<Task> execute, Func<bool> canExcute = null)
        {
            _execute = execute;
            _canExecute = canExcute ?? (() => true);
        }

        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion

        #region Methods

        public bool CanExecute(object parameter) => !_isExecuting && _canExecute();

        public bool CanExecute(object parameter, bool ignoreIsExecutingFlag)
        {
            if (ignoreIsExecutingFlag)
            {
                return _canExecute();
            }

            return !_isExecuting && _canExecute();
        }

        public async void Execute(object parameter)
        {
            _isExecuting = true;

            try
            {
                RaiseCanExecuteChanged();
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public async Task ExecuteAsync(object parameter)
        {
            _isExecuting = true;

            try
            {
                RaiseCanExecuteChanged();
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        #endregion
    }

    public class AsyncDelegateCommand<T> : ICommand
    {
        #region Fields

        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool> _canExecute;

        private bool _isExecuting;

        #endregion

        public AsyncDelegateCommand(Func<T, Task> execute, Func<T, bool> canExcute = null)
        {
            _execute = execute;
            _canExecute = canExcute ?? (x => true);
        }

        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion

        #region Methods

        public bool CanExecute(object parameter) => !_isExecuting && _canExecute((T)parameter);

        public bool CanExecute(object parameter, bool ignoreIsExecutingFlag)
        {
            if (ignoreIsExecutingFlag)
            {
                return _canExecute((T)parameter);
            }

            return !_isExecuting && _canExecute((T)parameter);
        }

        public async void Execute(object parameter)
        {
            _isExecuting = true;

            try
            {
                RaiseCanExecuteChanged();
                await _execute((T)parameter);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public async Task ExecuteAsync(object parameter)
        {
            _isExecuting = true;

            try
            {
                RaiseCanExecuteChanged();
                await _execute((T)parameter);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        #endregion
    }
}