using PcapTools.Packets.Ip;

namespace PcapTools.Packets.Transport
{
    /// <summary>
    /// CC.NEW Option (RFC 1644).
    /// <pre>
    /// +-----+------+--------+
    /// | Bit | 0-7  | 8-15   |
    /// +-----+------+--------+
    /// | 0   | Kind | Length |
    /// +-----+------+--------+
    /// | 16  | Connection    |
    /// |     | Count         |
    /// +-----+---------------+
    /// </pre>
    /// 
    /// <para>
    /// This option may be sent instead of a CC option in an initial &lt;SYN&gt; segment (i.e., SYN but not ACK bit), 
    /// to indicate that the SEG.CC value may not be larger than the previous value.  
    /// Its SEG.CC value is the TCB.CCsend value from the sender's TCB.
    /// </para>
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    [TcpOptionTypeRegistration(TcpOptionType.ConnectionCountNew)]
    public class TcpOptionConnectionCountNew : TcpOptionConnectionCountBase, IOptionComplexFactory
    {
        /// <summary>
        /// Creates the option using the given connection count value.
        /// </summary>
        public TcpOptionConnectionCountNew(uint connectionCount)
            : base(TcpOptionType.ConnectionCountNew, connectionCount)
        {
        }

        /// <summary>
        /// The default connection count value is 0.
        /// </summary>
        public TcpOptionConnectionCountNew()
            : this(0)
        {
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
            uint connectionCount;
            if (!TryRead(out connectionCount, buffer, ref offset, valueLength))
                return null;

            return new TcpOptionConnectionCountNew(connectionCount);
        }
    }
}