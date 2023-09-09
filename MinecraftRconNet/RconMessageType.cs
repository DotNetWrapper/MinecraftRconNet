using System;

namespace MinecraftRconNet
{
    /// <summary>
    /// Represents an RCON message type.
    /// </summary>
    public sealed class RconMessageType
    {
        public int Value { get; }

        private RconMessageType(int type)
        {
            Value = type;
        }

        /// <summary>
        /// Represents a Response RCON message type.
        /// </summary>
        public static readonly RconMessageType Response = new RconMessageType(0);

        /// <summary>
        /// Represents a Command RCON message type.
        /// </summary>
        public static readonly RconMessageType Command = new RconMessageType(2);

        /// <summary>
        /// Represents a Login RCON message type.
        /// </summary>
        public static readonly RconMessageType Login = new RconMessageType(3);

        /// <summary>
        /// Represents an Invalid RCON message type.
        /// </summary>
        public static readonly RconMessageType Invalid = new RconMessageType(-1);
    }
}