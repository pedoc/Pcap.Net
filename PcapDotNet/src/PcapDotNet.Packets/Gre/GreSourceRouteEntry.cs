using System;
using PcapTools.Base;
using PcapTools.Packets.IpV4;

namespace PcapTools.Packets.Gre
{
    /// <summary>
    /// RFC 1701.
    /// SRE.
    /// <pre>
    /// +-----+----------------+------------+------------+
    /// | Bit | 0-15           | 16-23      | 24-31      |
    /// +-----+----------------+------------+------------+
    /// | 0   | Address Family | SRE Offset | SRE Length |
    /// +-----+----------------+------------+------------+
    /// | 32  | Routing Information ...                  |
    /// +-----+------------------------------------------+
    /// </pre>
    /// </summary>
    public abstract class GreSourceRouteEntry : IEquatable<GreSourceRouteEntry>
    {
        /// <summary>
        /// The Address Family field contains a two octet value which indicates the syntax and semantics of the Routing Information field.  
        /// </summary>
        public abstract GreSourceRouteEntryAddressFamily AddressFamily { get; }

        /// <summary>
        /// The number of bytes the entry header takes.
        /// </summary>
        public const int HeaderLength = 4;

        private static class Offset
        {
            public const int AddressFamily = 0;
            public const int SreOffset = 2;
            public const int SreLength = 3;
        }

        /// <summary>
        /// The number of bytes the entry takes.
        /// </summary>
        public int Length
        {
            get { return HeaderLength + PayloadLength; }
        }

        /// <summary>
        /// The SRE Offset field indicates the octet offset from the start of the Routing Information field to the first octet of the active entry in Source Route Entry to be examined.
        /// </summary>
        public abstract byte PayloadOffset { get; }

        /// <summary>
        /// The SRE Length field contains the number of octets in the SRE.  
        /// </summary>
        public abstract byte PayloadLength { get; }

        /// <summary>
        /// Two entries are equal iff they have the same address family, length, payload offset and payload.
        /// </summary>
        public bool Equals(GreSourceRouteEntry other)
        {
            return other != null &&
                   AddressFamily == other.AddressFamily &&
                   Length == other.Length &&
                   PayloadOffset == other.PayloadOffset &&
                   EqualsPayloads(other);
        }

        /// <summary>
        /// Two entries are equal iff they have the same address family, length, payload offset and payload.
        /// </summary>
        public sealed override bool Equals(object obj)
        {
            return Equals(obj as GreSourceRouteEntry);
        }

        /// <summary>
        /// The hash code of an entry is a xor of the hash code of the address family, length, payload offset and payload.
        /// </summary>
        public sealed override int GetHashCode()
        {
            return Sequence.GetHashCode(AddressFamily, Length, PayloadOffset) ^ PayloadHashCode;
        }

        /// <summary>
        /// True iff the payloads a are equal.
        /// </summary>
        protected abstract bool EqualsPayloads(GreSourceRouteEntry other);

        /// <summary>
        /// The hash code of the payload.
        /// </summary>
        protected abstract int PayloadHashCode { get; }

        /// <summary>
        /// Writes the payload to the given buffer in the given offset.
        /// </summary>
        /// <param name="buffer">The buffer to write the payload to.</param>
        /// <param name="offset">The offset in the buffer to start writing.</param>
        protected abstract void WritePayload(byte[] buffer, int offset);

        internal static bool TryReadEntry(byte[] buffer, ref int offset, int length, out GreSourceRouteEntry entry)
        {
            entry = null;
            if (length < HeaderLength)
                return false;

            // Address Family
            GreSourceRouteEntryAddressFamily addressFamily = (GreSourceRouteEntryAddressFamily)buffer.ReadUShort(offset + Offset.AddressFamily, Endianity.Big);

            // SRE Length
            byte sreLength = buffer[offset + Offset.SreLength];
            if (sreLength == 0)
                return addressFamily == GreSourceRouteEntryAddressFamily.None;
            
            if (HeaderLength + sreLength > length)
                return false;

            // SRE Offset
            byte sreOffset = buffer[offset + Offset.SreOffset];
            if (sreOffset > sreLength)
                return false;

            // Entry
            if (!TryReadEntry(buffer, offset + HeaderLength, sreLength, addressFamily, sreOffset, out entry))
                return false;

            // Change offset
            offset += entry.Length;
            return true;
        }

        internal void Write(byte[] buffer, ref int offset)
        {
            buffer.Write(offset + Offset.AddressFamily, (ushort)AddressFamily, Endianity.Big);
            buffer.Write(offset + Offset.SreOffset, PayloadOffset);
            buffer.Write(offset + Offset.SreLength, PayloadLength);
            WritePayload(buffer, offset + HeaderLength);
            offset += Length;
        }

        private static bool TryReadEntry(byte[] buffer, int payloadOffset, int payloadLength, GreSourceRouteEntryAddressFamily addressFamily, int offsetInPayload, out GreSourceRouteEntry entry)
        {
            entry = null;
            switch (addressFamily)
            {
                case GreSourceRouteEntryAddressFamily.IpSourceRoute:
                    if (offsetInPayload % IpV4Address.SizeOf != 0 || payloadLength % IpV4Address.SizeOf != 0)
                        return false;

                    int numAddresses = payloadLength / IpV4Address.SizeOf;
                    IpV4Address[] addresses = new IpV4Address[numAddresses];
                    for (int i = 0; i != numAddresses; ++i)
                        addresses[i] = buffer.ReadIpV4Address(payloadOffset + i * IpV4Address.SizeOf, Endianity.Big);

                    entry = new GreSourceRouteEntryIp(addresses, offsetInPayload / IpV4Address.SizeOf);
                    return true;

                case GreSourceRouteEntryAddressFamily.AsSourceRoute:
                    if (offsetInPayload % sizeof(ushort) != 0 || payloadLength % sizeof(ushort) != 0)
                        return false;

                    int numAsNumbers = payloadLength / sizeof(ushort);
                    ushort[] asNumbers = new ushort[numAsNumbers];
                    for (int i = 0; i != numAsNumbers; ++i)
                        asNumbers[i] = buffer.ReadUShort(payloadOffset + i * sizeof(ushort), Endianity.Big);

                    entry = new GreSourceRouteEntryAs(asNumbers, offsetInPayload / sizeof(ushort));
                    return true;

                default:
                    Datagram data = new Datagram(buffer, payloadOffset, payloadLength);
                    entry = new GreSourceRouteEntryUnknown(addressFamily, data, offsetInPayload);
                    return true;
            }
        }
    }
}