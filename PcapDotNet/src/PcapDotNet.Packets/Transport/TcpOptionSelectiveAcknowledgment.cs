using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PcapTools.Base;
using PcapTools.Packets.Ip;

namespace PcapTools.Packets.Transport
{
    /// <summary>
    /// RFC 2018.
    /// The SACK option is to be used to convey extended acknowledgment information from the receiver to the sender over an established TCP connection.
    /// 
    /// <pre>
    /// +-----+------+--------+
    /// | Bit | 0-7  | 8-15   |
    /// +-----+------+--------+
    /// | 0   | Kind | Length |
    /// +-----+------+--------+
    /// | 16  | Left Edge of  |
    /// |     | 1st Block     |
    /// +-----+---------------+
    /// | 48  | Right Edge of |
    /// |     | 1st Block     |
    /// +-----+---------------+
    /// ...
    /// +-----+---------------+
    /// |     | Left Edge of  |
    /// |     | nth Block     |
    /// +-----+---------------+
    /// |     | Right Edge of |
    /// |     | nth Block     |
    /// +-----+---------------+
    /// </pre>
    /// 
    /// <para>
    /// The SACK option is to be sent by a data receiver to inform the data sender of non-contiguous blocks of data that have been received and queued.  
    /// The data receiver awaits the receipt of data (perhaps by means of retransmissions) to fill the gaps in sequence space between received blocks.  
    /// When missing segments are received, the data receiver acknowledges the data normally by advancing 
    /// the left window edge in the Acknowledgement Number Field of the TCP header.  
    /// The SACK option does not change the meaning of the Acknowledgement Number field.
    /// </para>
    /// 
    /// <para>
    /// This option contains a list of some of the blocks of contiguous sequence space occupied by data that has been received and queued within the window.
    /// Each contiguous block of data queued at the data receiver is defined in the SACK option by two 32-bit unsigned integers in network byte order:
    /// <list type="bullet">
    ///   <item>Left Edge of Block - This is the first sequence number of this block.</item>
    ///   <item>Right Edge of Block - This is the sequence number immediately following the last sequence number of this block.</item>
    /// </list>
    /// </para>
    /// Each block represents received bytes of data that are contiguous and isolated; 
    /// that is, the bytes just below the block, (Left Edge of Block - 1), and just above the block, (Right Edge of Block), have not been received.
    /// 
    /// A SACK option that specifies n blocks will have a length of 8*n+2 bytes, so the 40 bytes available for TCP options can specify a maximum of 4 blocks.
    /// It is expected that SACK will often be used in conjunction with the Timestamp option used for RTTM [Jacobson92], 
    /// which takes an additional 10 bytes (plus two bytes of padding); thus a maximum of 3 SACK blocks will be allowed in this case.
    /// 
    /// The SACK option is advisory, in that, while it notifies the data sender that the data receiver has received the indicated segments,
    /// the data receiver is permitted to later discard data which have been reported in a SACK option.  
    /// </summary>
    [TcpOptionTypeRegistration(TcpOptionType.SelectiveAcknowledgment)]
    public sealed class TcpOptionSelectiveAcknowledgment : TcpOptionComplex, IOptionComplexFactory
    {
        /// <summary>
        /// The minimum number of bytes this option take.
        /// </summary>
        public const int OptionMinimumLength = 2;

        /// <summary>
        /// The minimum number of bytes this option's value take.
        /// </summary>
        public const int OptionValueMinimumLength = OptionMinimumLength - OptionHeaderLength;

        /// <summary>
        /// Creates the option from the given list of selective ack blocks.
        /// </summary>
        public TcpOptionSelectiveAcknowledgment(IList<TcpOptionSelectiveAcknowledgmentBlock> blocks)
            : base(TcpOptionType.SelectiveAcknowledgment)
        {
            _blocks = new ReadOnlyCollection<TcpOptionSelectiveAcknowledgmentBlock>(blocks);
        }

        /// <summary>
        /// The default is no blocks.
        /// </summary>
        public TcpOptionSelectiveAcknowledgment()
            : this(new TcpOptionSelectiveAcknowledgmentBlock[]{})
        {
        }

        /// <summary>
        /// The collection of selective ack blocks.
        /// </summary>
        public ReadOnlyCollection<TcpOptionSelectiveAcknowledgmentBlock> Blocks
        {
            get { return _blocks; }
        }

        /// <summary>
        /// The number of bytes this option will take.
        /// </summary>
        public override int Length
        {
            get { return OptionMinimumLength + TcpOptionSelectiveAcknowledgmentBlock.SizeOf * Blocks.Count; }
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
            if (valueLength < OptionValueMinimumLength || valueLength % TcpOptionSelectiveAcknowledgmentBlock.SizeOf != 0)
                return null;

            int numBlocks = valueLength / TcpOptionSelectiveAcknowledgmentBlock.SizeOf;
            TcpOptionSelectiveAcknowledgmentBlock[] blocks = new TcpOptionSelectiveAcknowledgmentBlock[numBlocks];
            for (int i = 0; i != numBlocks; ++i)
            {
                blocks[i] = new TcpOptionSelectiveAcknowledgmentBlock(buffer.ReadUInt(ref offset, Endianity.Big),
                                                                      buffer.ReadUInt(ref offset, Endianity.Big));
            }

            return new TcpOptionSelectiveAcknowledgment(blocks);
        }

        internal override bool EqualsData(TcpOption other)
        {
            return EqualsData(other as TcpOptionSelectiveAcknowledgment);
        }

        internal override int GetDataHashCode()
        {
            return Blocks.SequenceGetHashCode();
        }

        internal override void Write(byte[] buffer, ref int offset)
        {
            base.Write(buffer, ref offset);
            foreach (TcpOptionSelectiveAcknowledgmentBlock block in Blocks)
            {
                buffer.Write(ref offset, block.LeftEdge, Endianity.Big);
                buffer.Write(ref offset, block.RightEdge, Endianity.Big);
            }
        }

        private bool EqualsData(TcpOptionSelectiveAcknowledgment other)
        {
            return other != null &&
                   Blocks.SequenceEqual(other.Blocks);
        }

        private readonly ReadOnlyCollection<TcpOptionSelectiveAcknowledgmentBlock> _blocks;
    }
}