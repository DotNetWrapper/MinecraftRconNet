using System;
using System.Net.Sockets;
using System.Text;

namespace MinecraftRconNet
{
    public sealed class RconClient : IDisposable
    {
        private int port = Constants.DefaultPort;
        public static readonly RconClient INSTANCE = new();

        private bool isInit = false;
        private bool isConfigured = false;

        private string server = string.Empty;
        private string password = string.Empty;

        private int messageCounter = 0;
        private NetworkStream? stream = null;

        private TcpClient? tcp = null;
        private BinaryWriter? writer = null;

        private BinaryReader? reader = null;
        private ReaderWriterLockSlim threadLock = new ReaderWriterLockSlim();

        private RconReader rconReader = RconReader.Instance;

        private RconClient()
        {
        }

        /// <summary>
        /// Configures and establishes a connection to the Minecraft server using the specified server address, port, and password.
        /// </summary>
        /// <param name="serverAddress">The IP address or hostname of the Minecraft server.</param>
        /// <param name="serverPort">The port number of the Minecraft server. (Default is 25575)</param>
        /// <param name="serverPassword">The password required to authenticate with the server. (Optional)</param>
        public void SetupStream(string serverAddress, int serverPort = 25575, string serverPassword = "")
        {
            this.threadLock.EnterWriteLock();
            try
            {
                if (this.isConfigured)
                {
                    return;
                }

                this.server = serverAddress;
                this.port = serverPort;

                this.password = serverPassword;
                this.isConfigured = true;

                this.OpenConnection();
            }
            finally
            {
                this.threadLock.ExitWriteLock();
            }
        }

        private void OpenConnection()
        {
            if (this.isInit)
            {
                return;
            }

            try
            {
                this.rconReader = RconReader.Instance;
                this.tcp = new TcpClient(this.server, this.port);

                this.stream = this.tcp.GetStream();
                this.writer = new BinaryWriter(this.stream);

                this.reader = new BinaryReader(this.stream);
                this.rconReader.Setup(this.reader);

                if (!string.IsNullOrEmpty(this.password))
                {
                    RconMessageAnswer answer = this.InternalSendAuth();
                    if (answer == Constants.EmptyAnswer)
                    {
                        this.isInit = false;
                        throw new Exception("IPAddress or Password error!");
                    }
                }

                this.isInit = true;
            }
            catch (SocketException se)
            {
                this.isInit = false;
                this.isConfigured = false;

                throw se;
            }
            catch (IOException ioe)
            {
                this.isInit = false;
                this.isConfigured = false;

                throw ioe;
            }
            finally
            {
                // To prevent huge CPU load if many reconnects happen.
                // Does not affect any normal case ;-)
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }
        }

        /// <summary>
        /// Sends an RCON command message to the Minecraft server and retrieves the response.
        /// </summary>
        /// <param name="type">The type of the RCON message (e.g., Command or Login).</param>
        /// <param name="command">The command to send to the server.</param>
        /// <returns>The response from the server as a string or an empty string if not configured.</returns>
        public string SendMessage(RconMessageType type, string command)
        {
            if (!this.isConfigured)
            {
                return Constants.EmptyAnswer.Answer;
            }

            return this.InternalSendMessage(type, command).Answer;
        }

        /// <summary>
        /// Sends an RCON command message to the Minecraft server without waiting for a response (fire-and-forget).
        /// </summary>
        /// <param name="type">The type of the RCON message (e.g., Command or Login).</param>
        /// <param name="command">The command to send to the server.</param>
        public void FireAndForgetMessage(RconMessageType type, string command)
        {
            if (!this.isConfigured)
            {
                return;
            }

            this.InternalSendMessage(type, command, true);
        }

        private RconMessageAnswer InternalSendAuth()
        {
            string command = this.password;
            RconMessageType type = RconMessageType.Login;

            int messageNumber = ++this.messageCounter;
            List<byte> messageContent = new List<byte>();

            byte[] commandBytes = Encoding.UTF8.GetBytes(command);
            messageContent.AddRange(BitConverter.GetBytes(10 + commandBytes.Length));

            messageContent.AddRange(BitConverter.GetBytes(messageNumber));
            messageContent.AddRange(BitConverter.GetBytes(type.Value));

            messageContent.AddRange(commandBytes);
            messageContent.AddRange(Constants.PADDING);

            this.writer.Write(messageContent.ToArray());
            this.writer.Flush();

            return this.WaitReadMessage(messageNumber);
        }

        private RconMessageAnswer InternalSendMessage(RconMessageType type, string command, bool fireAndForget = false)
        {
            try
            {
                int messageNumber;
                List<byte> msg;

                try
                {
                    this.threadLock.EnterWriteLock();
                    this.CheckAndReconnect();

                    messageNumber = ++this.messageCounter;
                    msg = this.BuildMessage(type, command, messageNumber);

                    this.WriteMessage(msg);
                }
                finally
                {
                    this.threadLock.ExitWriteLock();
                }

                if (fireAndForget && Constants.IsRconServerMultiThreaded)
                {
                    this.FireAndForget(messageNumber);
                    return Constants.EmptyAnswer;
                }

                return this.WaitReadMessage(messageNumber);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while sending: {e.Message}");
                return Constants.EmptyAnswer;
            }
        }

        private void CheckAndReconnect()
        {
            if (!this.isInit || this.tcp == null || !this.tcp.Connected)
            {
                this.InternalDispose();
                this.OpenConnection();
            }
        }

        private List<byte> BuildMessage(RconMessageType type, string command, int messageNumber)
        {
            List<byte> msg = new();
            byte[] commandBytes = Encoding.UTF8.GetBytes(command);

            msg.AddRange(BitConverter.GetBytes(10 + commandBytes.Length));
            msg.AddRange(BitConverter.GetBytes(messageNumber));

            msg.AddRange(BitConverter.GetBytes(type.Value));
            msg.AddRange(commandBytes);

            msg.AddRange(Constants.PADDING);
            return msg;
        }

        private void WriteMessage(List<byte> msg)
        {
            this.writer.Write(msg.ToArray());
            this.writer.Flush();
        }

        private void FireAndForget(int messageNumber)
        {
            int id = messageNumber;
            Task.Factory.StartNew(() =>
            {
                this.WaitReadMessage(id);
            });
        }

        private RconMessageAnswer WaitReadMessage(int messageNo)
        {
            DateTime sendTime = DateTime.UtcNow;
            while (true)
            {
                RconMessageAnswer answer = this.rconReader.GetAnswer(messageNo);
                if (answer == Constants.EmptyAnswer)
                {
                    if ((DateTime.UtcNow - sendTime).TotalSeconds > Constants.TimeoutSeconds)
                    {
                        return Constants.EmptyAnswer;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(0.001));
                    continue;
                }

                return answer;
            }
        }

        public void Dispose()
        {
            this.threadLock.EnterWriteLock();

            try
            {
                this.InternalDispose();
            }
            finally
            {
                this.threadLock.ExitWriteLock();
            }
        }

        private void InternalDispose()
        {
            this.isInit = false;

            try
            {
                this.rconReader.Dispose();
            }
            catch
            {
            }

            this.writer?.Dispose();
            this.reader?.Dispose();
            this.stream?.Dispose();

            if (this.tcp != null)
            {
                try
                {
                    this.tcp.Close();
                }
                catch
                {
                }
            }
        }
    }
}