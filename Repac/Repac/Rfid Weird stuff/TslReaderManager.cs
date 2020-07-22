using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechnologySolutions.Rfid;
using TechnologySolutions.Rfid.AsciiOperations;
using TechnologySolutions.Rfid.AsciiProtocol.Transports;

namespace Repac.Rfid_Weird_stuff
{
    public class TslReaderManager
        : IReaderManager
    {
        private readonly IAsciiTransportsManager transportsManager;

        private List<AsciiReader> readers;

        public TslReaderManager(IAsciiTransportsManager transportsManager)
        {
            this.Readers = this.readers = new List<AsciiReader>();

            this.transportsManager = transportsManager;
            this.transportsManager.TransportChanged += TransportsManager_TransportChanged;
        }

        /// <summary>
        /// Raised when an <see cref="IReader"/> state changes;
        /// </summary>
        public event EventHandler<ReaderEventArgs> ReaderChanged;

        /// <summary>
        /// Raised when the <see cref="ActiveReader"/> changes
        /// </summary>
        public event EventHandler<ReaderEventArgs> ActiveReaderChanged;

        /// <summary>
        /// Gets the available <see cref="IReader"/>s
        /// </summary>
        public IEnumerable<IReader> Readers { get; private set; }

        #region ActiveReader        
        /// <summary>
        /// Gets the state of the <see cref="ActiveReader"/>
        /// </summary>
        public ReaderStates ActiveReaderState
        {
            get => this.activeReaderState;
            private set
            {
                if (this.activeReaderState != value)
                {
                    this.activeReaderState = value;
                    if (this.activeReader != null)
                    {
                        this.ActiveReaderChanged?.Invoke(this, new ReaderEventArgs(this.activeReader, this.activeReaderState));
                    }
                }
            }
        }

        /// <summary>
        /// Backing field for <see cref="ActiveReaderState"/>
        /// </summary>
        private ReaderStates activeReaderState = ReaderStates.Disconnected;

        /// <summary>
        /// Gets or sets the ActiveReader
        /// </summary>
        public IReader ActiveReader
        {
            get => this.activeReader;
            set
            {
                if (this.activeReader != value)
                {
                    if (this.activeReader != null)
                    {
                        this.ActiveReaderState = ReaderStates.Disconnecting;
                        this.ActiveReaderState = ReaderStates.Disconnected;
                    }

                    this.activeReader = value;

                    this.ActiveReaderState = ReaderStates.Connecting;
                    this.ActiveReaderState = ReaderStates.Connected;
                }
            }
        }

        /// <summary>
        /// Backing field for <see cref="ActiveReader"/>
        /// </summary>
        private IReader activeReader;
        #endregion

        public async Task DisconnectReaderAsync(IReader reader)
        {
            if (reader != null)
            {
                var asciiReader = (reader as AsciiReader);
                if (asciiReader != null)
                {
                    await asciiReader.DisconnectAsync();
                }
            }
        }

        public void AnnounceReaderChange(IReader reader, ReaderStates state)
        {
            this.ReaderChanged.Invoke(this, new ReaderEventArgs(reader, state));
        }

        private async void TransportsManager_TransportChanged(object sender, TransportStateChangedEventArgs e)
        {
            // does a reader have this transport?
            var readerWithTransport = this.readers.Where(r => r.Transports.Any(t => t.Id == e.Transport.Id)).FirstOrDefault();
            if (readerWithTransport == null)
            {
                if (e.Transport.State == ConnectionState.Connected)
                {
                    await this.IdentifyReaderAsync(e.Transport);
                }
            }
            else
            {
                ReaderStates state = ReaderStates.Connecting;
                switch (e.Transport.State)
                {
                    //case ConnectionState.Connecting:
                    case ConnectionState.Connected:
                        await readerWithTransport.RefreshAsync();
                        state = ReaderStates.Connected;

                        // if we don't yet have an active reader we do now
                        if (this.ActiveReader == null)
                        {
                            this.ActiveReader = readerWithTransport;
                        }
                        break;

                    case ConnectionState.Lost:
                        state = ReaderStates.Lost;
                        if (readerWithTransport == this.ActiveReader)
                        {
                            this.ActiveReader = null;
                        }
                        break;

                    case ConnectionState.Disconnecting:
                        state = ReaderStates.Disconnecting;
                        if (readerWithTransport == this.ActiveReader)
                        {
                            this.ActiveReader = null;
                        }
                        break;

                    case ConnectionState.Disconnected:
                        state = ReaderStates.Disconnected;
                        if (readerWithTransport == this.ActiveReader)
                        {
                            this.ActiveReader = null;
                        }
                        break;

                        //case ConnectionState.Interrupted:
                }

                if (state != ReaderStates.Connecting)
                {
                    // TODO: change transport if we can
                    this.AnnounceReaderChange(readerWithTransport, state);
                }
            }
        }

        private async Task IdentifyReaderAsync(IAsciiTransport transport)
        {
            // We have a connected transport not associated with a reader.
            // Lets create a new reader and identify it
            var reader = new AsciiReader();
            reader.AddTransport(transport);
            await reader.RefreshAsync();

            // This, now identified, new transport may belong to an existing reader if so merge it
            var mergeReader = this.readers.Where(r => r.SerialNumber == reader.SerialNumber).FirstOrDefault();
            if (mergeReader == null)
            {
                if (String.IsNullOrEmpty(reader.SerialNumber))
                {
                    // Unable to identify the Reader via its transport
                    // This could be because the transport is Bluetooth and Reader is already connected via USB
                    // To avoid confusing, unnamed Readers in the list disconnect the unknown transport
                    // Note: A future release may implement a different policy
                    transport.Disconnect();
                    return;
                }
                else
                {
                    // New reader has been identified
                    this.readers.Add(reader);
                }
            }
            else
            {
                // merge the transport to the existing reader
                reader.RemoveTransport(transport);
                mergeReader.AddTransport(transport);
                reader.Dispose();
                reader = mergeReader;
                // Transports have changed on the Reader so ensure the appropriate transport is in use
                await reader.RefreshAsync();
            }

            this.AnnounceReaderChange(reader, ReaderStates.Connected);

            // if we don't yet have an active reader we do now
            if (this.ActiveReader == null)
            {
                this.ActiveReader = reader;
            }
        }
    }
}
