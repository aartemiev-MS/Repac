using System;
using System.Collections.Generic;
using System.Text;

namespace Repac.Rfid_Weird_stuff
{
    public static class CommandExtensions
    {
        /// <summary>
        /// Assumes command is a <see cref="RelayCommand"/> and calls <see cref="RelayCommand.RaiseCanExecuteChanged"/>
        /// </summary>
        /// <param name="command">The command to raise <see cref="System.Windows.Input.ICommand.CanExecuteChanged"/></param>
        public static void RefreshCanExecute(this System.Windows.Input.ICommand command)
        {
            (command as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
