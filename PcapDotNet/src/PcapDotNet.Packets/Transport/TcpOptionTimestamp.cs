using PcapTools.Packets.Ip;

namespace PcapTools.Packets.Transport
{
    /// <summary>
    /// RFC 1323.
    /// TCP Timestamps Option (TSopt):
    /// <pre>
    /// +-----+------+--------+
    /// | Bit | 0-7  | 8-15   |
    /// +-----+------+--------+
    /// | 0   | Kind | Length |
    /// +-----+------+--------+
    /// | 16  | TS Value      |
    /// |     | (TSval)       |
    /// +-----+---------------+
    /// | 48  | TS Echo Reply |
    /// |     | (TSecr)       |
    /// +-----+---------------+
    /// </pre>
    /// 
    /// <para>
    /// The Timestamps option carries two four-byte timestamp fields.
    /// The Timestamp Value field (TSval) contains the current value of the timestamp clock of the TCP sending the option.
    /// </para>
    /// 
    /// <para>
    /// The Timestamp Echo Reply field (TSecr) is only valid if the ACK bit is set in the TCP header; 
    /// if it is valid, it echos a timestamp value that was sent by the remote TCP in the TSval field of a Timestamps option.  
    /// When TSecr is not valid, its value must be zero.  
    /// The TSecr value will generally be from the most recent Timestamp option that was received; however, there are exceptions that are explained below.
    /// </para>
    /// 
    /// <para>
    /// A TCP may send the Timestamps option (TSopt) in an initial &lt;SYN&gt; segment (i.e., segment containing a SYN bit and no ACK bit), 
    /// and may send a TSopt in other segments only if it received a TSopt in the initial &lt;SYN&gt; segment for the connection.
    /// </para>
    /// </summary>
    [TcpOptionTypeRegistration(TcpOptionType.Timestamp)]
    public sealed class TcpOptionTimestamp : TcpOptionComplex, IOptionComplexFactory
    {
        /// <summary>
        /// The number of bytes this option take.
        /// </summary>
        public const int OptionLength = 10;

        /// <summary>
        /// The number of bytes this option value take.
        /// </summary>
        public const int OptionValueLength = OptionLength - OptionHeaderLength;

        /// <summary>
        /// Creates the option from the given timestamp value and echo reply.
        /// </summary>
        public TcpOptionTimestamp(uint timestampValue, uint timestampEchoReply)
            : base(TcpOptionType.Timestamp)
        {
            TimestampValue = timestampValue;
            TimestampEchoReply = timestampEchoReply;
        }

        /// <summary>
        /// The default values for the timestamp value and echo reply are 0.
        /// </summary>
        public TcpOptionTimestamp()
            : this(0, 0)
        {
        }

        /// <summary>
        /// The timestamp value.
        /// </summary>
        public uint TimestampValue { get; private set; }

        /// <summary>
        /// The echo reply value.
        /// </summary>
        public uint TimestampEchoReply { get; private set; }

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

            uint timeStampValue = buffer.ReadUInt(ref offset, Endianity.Big);
            uint timeStampEchoReply = buffer.ReadUInt(ref offset, Endianity.Big);
            return new TcpOptionTimestamp(timeStampValue, timeStampEchoReply);
        }

        internal override bool EqualsData(TcpOption other)
        {
            return EqualsData(other as TcpOptionTimestamp);
        }

        internal override int GetDataHashCode()
        {
            return TimestampEchoReply.GetHashCode();
        }

        internal override void Write(byte[] buffer, ref int offset)
        {
            base.Write(buffer, ref offset);
            buffer.Write(ref offset, TimestampValue, Endianity.Big);
            buffer.Write(ref offset, TimestampEchoReply, Endianity.Big);
        }

        private bool EqualsData(TcpOptionTimestamp other)
        {
            return other != null &&
                   TimestampValue == other.TimestampValue && TimestampEchoReply == other.TimestampEchoReply;
        }
    }
}