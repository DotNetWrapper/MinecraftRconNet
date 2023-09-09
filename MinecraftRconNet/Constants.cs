using System;
using System.Linq;

namespace MinecraftRconNet
{
    internal static class Constants
    {

        public const bool IsRconServerMultiThreaded = false;
        public const int TimeoutSeconds = 3;
        public const int DefaultPort = 25575;
        public static readonly byte[] PADDING = new byte[] { 0x0, 0x0 };

        public const int MinimumDataLength = 10;
        public const int InvalidRequestId = -1;

        public static readonly RconMessageAnswer EmptyAnswer = new RconMessageAnswer(false, Array.Empty<byte>());

    }
}
