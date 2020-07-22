using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TechnologySolutions.Rfid;

namespace Repac.Rfid_Weird_stuff
{
    public interface ITagEpcChanger
    {
        /// <summary>
        /// Raised to report progress about the tag EPC changing
        /// </summary>
        event EventHandler<MessageEventArgs> ProgressUpdate;

        /// <summary>
        /// Gets or sets the EPC of the transponder to change the EPC of in HEX
        /// </summary>
        string TargetTagHexIdentifier { get; set; }

        /// <summary>
        /// Returns a task to change the EPC of the TargetTag to hexIdentifier
        /// </summary>
        /// <param name="hexIdentifier">The new EPC for the target transponder</param>
        /// <returns>True if the task was successful</returns>
        Task<bool> ChangeEpcAsync(string hexIdentifier);
    }

    /// <summary>
    /// Provides <see cref="EventArgs"/> to report a message to the user interface
    /// </summary>
    public class MessageEventArgs
        : EventArgs
    {
        /// <summary>
        /// Initialises a new instance of the MessageEventArgs class
        /// </summary>
        /// <param name="message">The message to report</param>
        public MessageEventArgs(string message)
        {
            this.Message = message;
        }

        /// <summary>
        /// Gets the message to report
        /// </summary>
        public string Message { get; private set; }
    }

    /// <summary>
    /// Provides a method to read a transponder
    /// </summary>
    public interface ITagReader : IOutputPowerBounds
    {
        /// <summary>
        /// Raised to report progress about the tag reading
        /// </summary>
        event EventHandler<MessageEventArgs> ProgressUpdate;

        Task<int> ReadTagsAsync(string hexIdentifier, MemoryBank memoryBank, int wordAddress, int wordCount, int outputPower);
    }

    /// <summary>
    /// Provides a method to write a transponder
    /// </summary>
    public interface ITagWriter
        : IOutputPowerBounds
    {
        /// <summary>
        /// Raised to report progress about the tag writing
        /// </summary>
        event EventHandler<MessageEventArgs> ProgressUpdate;

        /// <summary>
        /// Write tags
        /// </summary>
        /// <param name="hexIdentifier">EPC identifier filter as lower-case hex</param>
        /// <param name="memoryBank">which tag memory bank to read (0-3)</param>
        /// <param name="wordAddress">data offset into the selected bank</param>
        /// <param name="wordCount">words to read from the selected bank</param>
        /// <param name="hexData">Word-aligned data to write</param>
        /// <param name="outputPower">reader power setting</param>
        /// <returns>The number of transponders written</returns>
        Task<int> WriteTagsAsync(string hexIdentifier, MemoryBank memoryBank, int wordAddress, int wordCount, string hexData, int outputPower);
    }
}
