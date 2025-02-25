using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PcapTools.Base;
using PcapTools.Packets.Ip;

namespace PcapTools.Packets.Transport
{
    /// <summary>
    /// <pre>
    /// +-----+------+--------+
    /// | Bit | 0-7  | 8-15   |
    /// +-----+------+--------+
    /// | 0   | Kind | Length |
    /// +-----+------+--------+
    /// | 16  | MD5 digest    |
    /// |     |               |
    /// |     |               |
    /// |     |               |
    /// |     |               |
    /// |     |               |
    /// |     |               |
    /// |     |               |
    /// +-----+---------------+
    /// </pre>
    /// 
    /// <para>
    /// The MD5 digest is always 16 bytes in length, and the option would appear in every segment of a connection.
    /// </para>
    /// </summary>
    [TcpOptionTypeRegistration(TcpOptionType.Md5Signature)]
    public sealed class TcpOptionMd5Signature: TcpOptionComplex, IOptionComplexFactory
    {
        /// <summary>
        /// The number of bytes this option take.
        /// </summary>
        public const int OptionLength = OptionHeaderLength + 16;

        /// <summary>
        /// The number of bytes this option value take.
        /// </summary>
        public const int OptionValueLength = OptionLength - OptionHeaderLength;

        /// <summary>
        /// Creates the option using the given signature data.
        /// </summary>
        public TcpOptionMd5Signature(IList<byte> data)
            : base(TcpOptionType.Md5Signature)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (data.Count != OptionValueLength)
                throw new ArgumentException("data must be " + OptionValueLength + " bytes and not " + data.Count + " bytes", "data");

            Data = new ReadOnlyCollection<byte>(data);
        }

        /// <summary>
        /// The default signature is all zeroes.
        /// </summary>
        public TcpOptionMd5Signature()
            : this(new byte[OptionValueLength])
        {
        }

        /// <summary>
        /// The signature value.
        /// </summary>
        public ReadOnlyCollection<byte> Data { get; private set; }

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

            byte[] data = buffer.ReadBytes(ref offset, OptionValueLength);
            return new TcpOptionMd5Signature(data);
        }

        internal override bool EqualsData(TcpOption other)
        {
            return EqualsData(other as TcpOptionMd5Signature);
        }

        internal override int GetDataHashCode()
        {
            return Data.BytesSequenceGetHashCode();
        }

        internal override void Write(byte[] buffer, ref int offset)
        {
            base.Write(buffer, ref offset);
            buffer.Write(ref offset, Data);
        }

        private bool EqualsData(TcpOptionMd5Signature other)
        {
            return other != null &&
                   Data.SequenceEqual(other.Data);
        }
    }
}