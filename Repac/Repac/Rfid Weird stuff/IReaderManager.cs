using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TechnologySolutions.Rfid;

namespace Repac.Rfid_Weird_stuff
{
    public interface IReaderManager
    {
        /// <summary>
        /// Raised when an <see cref="IReader"/> is Connecting, Connected, Disconnecting, Disconnected
        /// </summary>
        event EventHandler<ReaderEventArgs> ReaderChanged;

        /// <summary>
        /// Raised when the <see cref="ActiveReader"/> changes
        /// </summary>
        /// <remarks>
        /// This behaves as an ActiveReader changing/changed event.
        /// <list type="bullet">
        /// <item>Changing - Disconnecting. The current reader is going away, do any configuration then release it</item>
        /// <item>Changed - Connected. This is a new reader to use</item>
        /// <item>Lost = Disconnected. The reader is no longer available release reference but it is not there to communicate with</item>
        /// </list>
        /// </remarks>
        event EventHandler<ReaderEventArgs> ActiveReaderChanged;

        /// <summary>
        /// Gets or sets a reference to the Active reader
        /// </summary>
        IReader ActiveReader { get; set; }

        ReaderStates ActiveReaderState { get; }

        /// <summary>
        /// Disconnect the specified reader
        /// </summary>
        /// <param name="reader">The reader to disconnect</param>
        /// <returns>A task to disconnect the specified reader</returns>
        Task DisconnectReaderAsync(IReader reader);
    }

    /// <summary>
    /// <see cref="EventArgs"/> for a <see cref="IReader"/> change event
    /// </summary>
    public class ReaderEventArgs
        : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the ReaderEventArgs class
        /// </summary>
        /// <param name="reader">The reader that changed</param>
        /// <param name="state">The new reader state</param>
        public ReaderEventArgs(IReader reader, ReaderStates state)
        {
            this.Reader = reader;
            this.State = state;
        }

        /// <summary>
        /// Gets the connection state of the reader
        /// </summary>
        public ReaderStates State { get; private set; }

        /// <summary>
        /// Gets the reader
        /// </summary>
        public IReader Reader { get; private set; }
    }

    /// <summary>
    /// Provides connection state for the IReader
    /// </summary>
    public enum ReaderStates
    {
        //Available = TechnologySolutions.Rfid.AsciiProtocol.Transports.ConnectionState.Available,
        Connected = TechnologySolutions.Rfid.AsciiProtocol.Transports.ConnectionState.Connected,
        Connecting = TechnologySolutions.Rfid.AsciiProtocol.Transports.ConnectionState.Connecting,
        Disconnected = TechnologySolutions.Rfid.AsciiProtocol.Transports.ConnectionState.Disconnected,
        Disconnecting = TechnologySolutions.Rfid.AsciiProtocol.Transports.ConnectionState.Disconnecting,
        Interrupted = TechnologySolutions.Rfid.AsciiProtocol.Transports.ConnectionState.Interrupted,
        Lost = TechnologySolutions.Rfid.AsciiProtocol.Transports.ConnectionState.Lost,
        //NotAvailable = TechnologySolutions.Rfid.AsciiProtocol.Transports.ConnectionState.NotAvailable
    }
}
