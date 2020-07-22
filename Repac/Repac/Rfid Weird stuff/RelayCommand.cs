using System;
using System.Windows.Input;

namespace Repac.Rfid_Weird_stuff
{
    class RelayCommand : ICommand
    {
        /// <summary>
        /// The action to perform to execute the command
        /// </summary>
        private readonly Action<object> execute;

        /// <summary>
        /// The function that returns a value indicating whether the command can execute
        /// </summary>
        private readonly Func<object, bool> canExecute;

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            this.canExecute = canExecute ?? new Func<object, bool>((parameter) => { return true; });
            this.execute = execute;
        }

        /// <summary>
        /// Initializes a new instance of the RelayCommand class
        /// </summary>
        /// <param name="execute">The action to perform to execute the command</param>
        /// <param name="canExecute">A function that returns a bool indicating whether the command can be execute</param>
        public RelayCommand(Action execute, Func<bool> canExecute)
            : this((parameter) => { execute(); }, (parameter) => { return canExecute == null ? true : canExecute(); })
        {
        }

        /// <summary>
        /// Should be raised when CanExecute should be evaluated
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Returns a <see cref="bool"/> indicating if the Command can be executed with the given parameter.
        /// </summary>
        /// <param name="parameter">An System.Object used as parameter to determine if the Command can be executed.</param>
        /// <returns>true if the Command can be executed, false otherwise.</returns>
        /// <remarks>
        /// If no canExecute parameter was passed to the Command constructor, this method always returns true.
        /// If the Command was created with non-generic execute parameter, the parameter of this method is ignored.
        /// </remarks>
        public bool CanExecute(object parameter)
        {
            return this.canExecute(parameter);
        }

        /// <summary>
        /// Sends a <see cref="CanExecuteChanged"/>
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Invokes the execute Action
        /// </summary>
        /// <param name="parameter">An System.Object used as parameter for the execute Action.</param>
        /// <remarks>
        /// If the Command was created with non-generic execute parameter, the parameter of this method is ignored.
        /// </remarks>
        public void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
            {
                this.execute(parameter);
            }
        }
    }
}
