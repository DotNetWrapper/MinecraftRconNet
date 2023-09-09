using System.Text;

namespace MinecraftRconNet
{

    public sealed class RconMessageAnswer
    {
        private readonly bool success;
        private readonly byte[] data;
        private readonly int responseId;

        /// <summary>
        /// Initializes a new instance of the <see cref="RconMessageAnswer"/> class.
        /// </summary>
        /// <param name="success">A value indicating whether the operation was successful.</param>
        /// <param name="data">The data associated with the answer.</param>
        /// <param name="responseId">The response ID.</param>
        public RconMessageAnswer(bool success, byte[] data, int responseId = -1)
        {
            this.success = success;
            this.data = data;
            this.responseId = responseId;
        }

        /// <summary>
        /// Gets the data associated with the answer.
        /// </summary>
        public byte[] Data
        {
            get
            {
                return this.data;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success
        {
            get
            {
                return this.success;
            }
        }

        /// <summary>
        /// Gets the answer as a UTF-8 encoded string.
        /// </summary>
        public string Answer
        {
            get
            {
                return Encoding.UTF8.GetString(this.data);
            }
        }

        /// <summary>
        /// Gets the response ID.
        /// </summary>
        public int ResponseId
        {
            get
            {
                return this.responseId;
            }
        }
    }
}