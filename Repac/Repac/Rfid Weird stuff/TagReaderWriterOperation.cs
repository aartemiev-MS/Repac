using System;
using System.Collections.Generic;
using System.Text;

namespace Repac.Rfid_Weird_stuff
{
    using System.Linq;
    using System.Threading.Tasks;
    using TechnologySolutions.Rfid;
    using TechnologySolutions.Rfid.AsciiOperations;
    using TechnologySolutions.Rfid.AsciiProtocol;

    /// <summary>
    /// A class to read and write to transponders
    /// This class demonstrates the use of the <see cref="IReaderOperationTranspondersAccess"/> ReaderOperation
    /// </summary>
    public class TagReaderWriterOperation : ITagReader, ITagWriter
    {
        /// <summary>
        /// Provides access to the ActiveReader to use for reading and writing
        /// </summary>
        private readonly IReaderManager readerManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagReaderWriterOperation"/> class
        /// </summary>
        /// <param name="readerManager">Provides access to the ActiveReader to use for reading and writing</param>
        public TagReaderWriterOperation(IReaderManager readerManager)
        {
            this.readerManager = readerManager;
            this.readerManager.ActiveReaderChanged += this.ReaderManager_ActiveReaderChanged;

            this.MinimumOutputPower = ConfigurationConstants.MinimumAntennaOutputPowerDefault;
            this.MaximumOutputPower = ConfigurationConstants.MaximumAntennaOutputPowerDefault;
        }

        /// <summary>
        /// Raised to report the progress of reading from or writing to transponder(s)
        /// </summary>
        public event EventHandler<MessageEventArgs> ProgressUpdate;

        #region IOutputPowerBounds
        /// <summary>
        /// Gets or sets the maximum output power in dBm
        /// </summary>
        public int MaximumOutputPower { get; set; }

        /// <summary>
        /// Gets or sets the maximum output power in dBm
        /// </summary>
        public int MinimumOutputPower { get; set; }

        #endregion

        /// <summary>
        /// Gets the reader operation from the reader to perform transponder access operations
        /// </summary>
        private IReaderOperationTranspondersAccess TransponderAccessOperation
        {
            get
            {
                var reader = this.readerManager.ActiveReader ?? throw new InvalidOperationException("Not currently connected to a reader");
                var operation = reader.OperationOfType<IReaderOperationTranspondersAccess>() ?? throw new InvalidOperationException("reader does not support transponder access operations");
                this.MaximumOutputPower = operation.MaximumOutputPower;
                this.MinimumOutputPower = operation.MinimumOutputPower;
                return operation;
            }
        }

        /// <summary>
        /// Read tags
        /// </summary>
        /// <param name="hexIdentifier">EPC identifier filter as lower-case hex</param>
        /// <param name="memoryBank">which tag memory bank to read (0-3)</param>
        /// <param name="wordAddress">data offset into the selected bank</param>
        /// <param name="wordCount">words to read from the selected bank</param>
        /// <param name="outputPower">reader power setting</param>
        /// <returns>true if the operation succeeded</returns>
        public async Task<int> ReadTagsAsync(string hexIdentifier, MemoryBank memoryBank, int wordAddress, int wordCount, int outputPower)
        {
            int count = 0;

            var filter = string.IsNullOrEmpty(hexIdentifier) ? TagFilter.All() : TagFilter.ForEpc(hexIdentifier);
            var accessFilterReport = filter
                .AtPower(outputPower).ForTagAccess()
                .ReportDefaults()
                .Read(memoryBank, wordAddress, wordCount);

            var operation = this.TransponderAccessOperation;
            operation.TranspondersReceived += this.Operation_TranspondersReceived;
            operation.Access = accessFilterReport.Access; // what do we want to do to each transponder
            operation.Filter = accessFilterReport.Filter; // which transponders
            operation.Report = accessFilterReport.Report; // what fields do we want from each transponder

            try
            {
                await operation.EnableAsync();
                await operation.StartAsync();
                await operation.StopAsync();
                count = operation.Transponders.Count();
            }
            finally
            {
                operation.TranspondersReceived -= this.Operation_TranspondersReceived;
            }

            return count;
        }

        /// <summary>
        /// Write tags
        /// </summary>
        /// <param name="hexIdentifier">EPC identifier filter as lower-case hex</param>
        /// <param name="memoryBank">which tag memory bank to read (0-3)</param>
        /// <param name="wordAddress">data offset into the selected bank</param>
        /// <param name="wordCount">words to read from the selected bank</param>
        /// <param name="hexData">Word-aligned data to write</param>
        /// <param name="outputPower">reader power setting</param>
        /// <returns>true if the operation succeeded</returns>
        public async Task<int> WriteTagsAsync(string hexIdentifier, MemoryBank memoryBank, int wordAddress, int wordCount, string hexData, int outputPower)
        {
            int count = 0;

            var filter = string.IsNullOrEmpty(hexIdentifier) ? TagFilter.All() : TagFilter.ForEpc(hexIdentifier);
            var accessFiterReport = filter
                .ExpectOne()
                .AtPower(outputPower)
                .ReportDefaults()
                .Write(memoryBank, wordAddress, hexData);

            var operation = this.TransponderAccessOperation;
            operation.TranspondersReceived += this.Operation_TranspondersReceived;
            operation.Configure(accessFiterReport);

            try
            {
                // manually run the operation
                await operation.EnableAsync();  // has to be enabled to start
                await operation.StartAsync();   // Start the operation
                await operation.StopAsync();    // as we awaited the start it will be finished but stop it. Don't have to disable it
                count = operation.Transponders.Count();
            }
            finally
            {
                operation.TranspondersReceived -= this.Operation_TranspondersReceived;
            }

            return count;
        }

        /// <summary>
        /// Notify changes to Progress
        /// </summary>
        /// <param name="message">the message included with the notification</param>
        protected virtual void OnProgressUpdated(string message)
        {
            this.ProgressUpdate?.Invoke(this, new MessageEventArgs(message));
        }

        /// <summary>
        /// Report details about each transponder received from the Read command
        /// </summary>
        /// <param name="sender">the source of the event</param>
        /// <param name="e">the transponder data</param>
        private void Operation_TransponderReceived(object sender, TransponderDataEventArgs e)
        {
            this.ReportTransponder(e.Transponder);
        }

        private void Operation_TranspondersReceived(object sender, TranspondersEventArgs e)
        {
            foreach (var transponder in e.Transponders)
            {
                this.ReportTransponder(transponder);
            }
        }


        private void ReportTransponder(TransponderData transponder)
        {
            this.OnProgressUpdated(string.Format("EPC: {0}", transponder.Epc));

            if (!string.IsNullOrEmpty(transponder.ReadData))
            {
                // show the data that was read
                this.OnProgressUpdated(string.Format("Data: {0}", transponder.ReadData));
            }
            else if (transponder.WordsWritten != null)
            {
                this.OnProgressUpdated(string.Format("Written: {0} words", transponder.WordsWritten));
            }

            if (transponder.TransponderBackscatterErrorCode != null)
            {
                this.OnProgressUpdated(
                    string.Format(
                        "TAG ERROR: {0} {1}",
                        transponder.TransponderBackscatterErrorCode.Parameter(),
                        transponder.TransponderBackscatterErrorCode.Description()));
            }
            else if (transponder.TransponderAccessErrorCode != null)
            {
                this.OnProgressUpdated(
                    string.Format(
                        "READER ERROR: {0} {1}",
                        transponder.TransponderAccessErrorCode.Parameter(),
                        transponder.TransponderAccessErrorCode.Description()));
            }

            this.OnProgressUpdated(string.Empty);
        }

        private void ReaderManager_ActiveReaderChanged(object sender, ReaderEventArgs e)
        {
            if (e.State == ReaderStates.Connected)
            {
                this.MaximumOutputPower = e.Reader.Configuration.MaximumOutputPower;
                this.MinimumOutputPower = e.Reader.Configuration.MinimumOutputPower;
            }
        }
    }
}
