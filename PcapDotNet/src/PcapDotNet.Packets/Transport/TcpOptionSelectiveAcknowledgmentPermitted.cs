using PcapTools.Packets.Ip;

namespace PcapTools.Packets.Transport
{
    /// <summary>
    /// Sack-Permitted Option (RFC 2018)
    /// This two-byte option may be sent in a SYN by a TCP that has been extended to receive (and presumably process) 
    /// the SACK option once the connection has opened.  
    /// It MUST NOT be sent on non-SYN segments.
    /// 
    /// <pre>
    /// +-----+------+--------+
    /// | Bit | 0-7  | 8-15   |
    /// +-----+------+--------+
    /// | 0   | Kind | Length |
    /// +-----+------+--------+
    /// </pre>
    /// </summary>
    [TcpOptionTypeRegistration(TcpOptionType.SelectiveAcknowledgmentPermitted)]
    public sealed class TcpOptionSelectiveAcknowledgmentPermitted : TcpOptionComplex, IOptionComplexFactory
    {
        /// <summary>
        /// The number of bytes this option take.
        /// </summary>
        public const int OptionLength = 2;

        /// <summary>
        /// The number of bytes this option value take.
        /// </summary>
        public const int OptionValueLength = OptionLength - OptionHeaderLength;

        /// <summary>
        /// Creates a selective ack permitted option.
        /// </summary>
        public TcpOptionSelectiveAcknowledgmentPermitted()
            : base(TcpOptionType.SelectiveAcknowledgmentPermitted)
        {
        }

        /// <summary>
        /// The number of bytes this option will take.
        /// </summary>
        public override int Length
        {
            get { return OptionLength; }
        }

        /// <summary>
        /// True iff this option may appear at most once in a datagram.
        /// </summary>
        public override bool IsAppearsAtMostOnce
        {
            get { return true; }
        }

        /// <summary>
        /// Tries to read the option from a buffer starting from the option value (after the type and length).
        /// </summary>
        /// <param name="buffer">The buffer to read the option from.</param>
        /// <param name="offset">The offset to the first byte to read the buffer. Will be incremented by the number of bytes read.</param>
        /// <param name="valueLength">The number of bytes the option value should take according to the length field that was already read.</param>
        /// <returns>On success - the complex option read. On failure - null.</returns>
        Option IOptionComplexFactory.CreateInstance(byte[] buffer, ref int offset, byte valueLength)
        {
            if (valueLength != OptionValueLength)
                return null;

            return _instance;
        }

        internal override bool EqualsData(TcpOption other)
        {
            return other is TcpOptionSelectiveAcknowledgmentPermitted;
        }

        internal override int GetDataHashCode()
        {
            return 0;
        }

        private static readonly TcpOptionSelectiveAcknowledgmentPermitted _instance = new TcpOptionSelectiveAcknowledgmentPermitted();
    }
}