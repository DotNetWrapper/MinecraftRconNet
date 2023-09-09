using System;
using System.Collections.Concurrent;
using System.Linq;

namespace MinecraftRconNet
{

    public class RconReader : IDisposable
    {
        public static readonly RconReader Instance = new RconReader();
        private readonly ConcurrentBag<RconMessageAnswer> answers = new ConcurrentBag<RconMessageAnswer>();

        private bool IsInitialized { get; set; } = false;
        private BinaryReader? Reader { get; set; } = null;

        private RconReader()
        {
            this.IsInitialized = false;
        }

        /// <summary>
        /// Sets up the RconReader with the provided BinaryReader.
        /// </summary>
        /// <param name="reader">The BinaryReader to use for reading messages.</param>
        public void Setup(BinaryReader reader)
        {
            this.Reader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.IsInitialized = true;

            this.StartReaderThread();
        }

        /// <summary>
        /// Gets the RCON message answer with the specified message ID.
        /// </summary>
        /// <param name="messageId">The message ID to search for.</param>
        /// <returns>The RCON message answer, or RconMessageAnswer.EMPTY if not found.</returns>
        public RconMessageAnswer GetAnswer(int messageId)
        {
            List<RconMessageAnswer> matching = new();
            foreach (RconMessageAnswer answer in this.answers)
            {
                if (answer.ResponseId == messageId)
                {
                    matching.Add(answer);
                }
            }

            List<byte> data = matching.SelectMany(n => n.Data).ToList();
            foreach (RconMessageAnswer answer in matching)
            {
                this.answers.TryTake(out _);
            }

            if (matching.Count > 0)
            {
                return new RconMessageAnswer(true, data.ToArray(), messageId);
            }
            else
            {
                return Constants.EmptyAnswer;
            }
        }

        private void ReadData()
        {
            while (this.IsInitialized)
            {
                try
                {
                    int len = this.Reader.ReadInt32();
                    int reqId = this.Reader.ReadInt32();

                    int type = this.Reader.ReadInt32();
                    byte[] data = len > 10 ? this.Reader.ReadBytes(len - Constants.MinimumDataLength) : Array.Empty<byte>();

                    byte[] pad = this.Reader.ReadBytes(2);
                    RconMessageAnswer msg = new RconMessageAnswer(reqId > Constants.InvalidRequestId, data, reqId);

                    this.answers.Add(msg);
                }
                catch (EndOfStreamException)
                {
                    return;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch
                {
                    return;
                }

                Thread.Sleep(1);
            }
        }

        private void StartReaderThread()
        {
            Task.Factory.StartNew(ReadData, TaskCreationOptions.LongRunning);
        }

        #region IDisposable implementation

        /// <summary>
        /// Disposes of the RconReader and stops the reader thread.
        /// </summary>
        public void Dispose()
        {
            this.IsInitialized = false;
        }

        #endregion
    }
}