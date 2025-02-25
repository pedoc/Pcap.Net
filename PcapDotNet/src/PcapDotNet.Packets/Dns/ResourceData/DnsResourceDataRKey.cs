﻿using System;
using PcapTools.Base;

namespace PcapTools.Packets.Dns
{
    /// <summary>
    /// Reid.
    /// <pre>
    /// +-----+-------+----------+-----------+
    /// | bit | 0-15  | 16-23    | 24-31     |
    /// +-----+-------+----------+-----------+
    /// | 0   | flags | protocol | algorithm |
    /// +-----+-------+----------+-----------+
    /// | 32  | public key                   |
    /// | ... |                              |
    /// +-----+------------------------------+
    /// </pre>
    /// </summary>
    [DnsTypeRegistration(Type = DnsType.RKey)]
    public sealed class DnsResourceDataRKey : DnsResourceDataSimple, IEquatable<DnsResourceDataRKey>
    {
        private static class Offset
        {
            public const int Flags = 0;
            public const int Protocol = Flags + sizeof(ushort);
            public const int Algorithm = Protocol + sizeof(byte);
            public const int PublicKey = Algorithm + sizeof(byte);
        }

        private const int ConstantPartLength = Offset.PublicKey;

        /// <summary>
        /// Constructs an instance out of the flags, protocol, algorithm and public key fields.
        /// </summary>
        /// <param name="flags">Reserved and must be zero.</param>
        /// <param name="protocol">Must be set to 1.</param>
        /// <param name="algorithm">The key algorithm parallel to the same field for the SIG resource.</param>
        /// <param name="publicKey">The public key value.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags")]
        public DnsResourceDataRKey(ushort flags, byte protocol, DnsAlgorithm algorithm, DataSegment publicKey)
        {
            Flags = flags;
            Protocol = protocol;
            Algorithm = algorithm;
            PublicKey = publicKey;
        }

        /// <summary>
        /// Reserved and must be zero.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        public ushort Flags { get; private set; }

        /// <summary>
        /// Must be set to 1.
        /// </summary>
        public byte Protocol { get; private set; }

        /// <summary>
        /// The key algorithm parallel to the same field for the SIG resource.
        /// </summary>
        public DnsAlgorithm Algorithm { get; private set; }

        /// <summary>
        /// The public key value.
        /// </summary>
        public DataSegment PublicKey { get; private set; }

        /// <summary>
        /// Two DnsResourceDataRKey are equal iff their flags, protocol, algorithm and public key fields are equal.
        /// </summary>
        public bool Equals(DnsResourceDataRKey other)
        {
            return other != null &&
                   Flags.Equals(other.Flags) &&
                   Protocol.Equals(other.Protocol) &&
                   Algorithm.Equals(other.Algorithm) &&
                   PublicKey.Equals(other.PublicKey);
        }

        /// <summary>
        /// Two DnsResourceDataRKey are equal iff their flags, protocol, algorithm and public key fields are equal.
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as DnsResourceDataRKey);
        }

        /// <summary>
        /// A hash code based on the flags, protocol, algorithm and public key fields.
        /// </summary>
        public override int GetHashCode()
        {
            return BitSequence.Merge(Flags, Protocol, (byte)Algorithm).GetHashCode();
        }

        internal DnsResourceDataRKey()
            : this(0, 1, DnsAlgorithm.None, DataSegment.Empty)
        {
        }

        internal override int GetLength()
        {
            return ConstantPartLength + PublicKey.Length;
        }

        internal override void WriteDataSimple(byte[] buffer, int offset)
        {
            buffer.Write(offset + Offset.Flags, Flags, Endianity.Big);
            buffer.Write(offset + Offset.Protocol, Protocol);
            buffer.Write(offset + Offset.Algorithm, (byte)Algorithm);
            PublicKey.Write(buffer, offset + Offset.PublicKey);
        }

        internal override DnsResourceData CreateInstance(DataSegment data)
        {
            if (data.Length < ConstantPartLength)
                return null;

            ushort flags = data.ReadUShort(Offset.Flags, Endianity.Big);
            byte protocol = data[Offset.Protocol];
            DnsAlgorithm algorithm = (DnsAlgorithm)data[Offset.Algorithm];
            DataSegment publicKey = data.Subsegment(Offset.PublicKey, data.Length - ConstantPartLength);

            return new DnsResourceDataRKey(flags, protocol, algorithm, publicKey);
        }
    }
}