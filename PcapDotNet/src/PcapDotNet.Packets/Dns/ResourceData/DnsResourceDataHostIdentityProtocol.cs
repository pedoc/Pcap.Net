﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using PcapTools.Base;
using IListExtensions = PcapTools.Base.IListExtensions;

namespace PcapTools.Packets.Dns
{
    /// <summary>
    /// RFC 5205.
    /// <pre>
    /// +-----+------------+--------------+-----------+
    /// | bit | 0-7        | 8-15         | 16-31     |
    /// +-----+------------+--------------+-----------+
    /// | 0   | HIT Length | PK Algorithm | PK Length |
    /// +-----+------------+--------------+-----------+
    /// | 32  | HIT                                   |
    /// | ... |                                       |
    /// +-----+---------------------------------------+
    /// |     | Public Key                            |
    /// | ... |                                       |
    /// +-----+---------------------------------------+
    /// |     | Rendezvous Servers                    |
    /// | ... |                                       |
    /// +-----+---------------------------------------+
    /// </pre>
    /// </summary>
    [DnsTypeRegistration(Type = DnsType.Hip)]
    public sealed class DnsResourceDataHostIdentityProtocol: DnsResourceDataNoCompression, IEquatable<DnsResourceDataHostIdentityProtocol>
    {
        private static class Offset
        {
            public const int HostIdentityTagLength = 0;
            public const int PublicKeyAlgorithm = HostIdentityTagLength + sizeof(byte);
            public const int PublicKeyLength = PublicKeyAlgorithm + sizeof(byte);
            public const int HostIdentityTag = PublicKeyLength + sizeof(ushort);
        }

        private const int ConstantPartLength = Offset.HostIdentityTag;

        /// <summary>
        /// Constructs an instance out of the host identity tag, public key algorithm, public key and rendezvous servers fields.
        /// </summary>
        /// <param name="hostIdentityTag">Stored as a binary value in network byte order.</param>
        /// <param name="publicKeyAlgorithm">Identifies the public key's cryptographic algorithm and determines the format of the public key field.</param>
        /// <param name="publicKey">Contains the algorithm-specific portion of the KEY RR RDATA.</param>
        /// <param name="rendezvousServers">
        /// Indicates one or more domain names of rendezvous server(s).
        /// Must not be compressed.
        /// The rendezvous server(s) are listed in order of preference (i.e., first rendezvous server(s) are preferred),
        /// defining an implicit order amongst rendezvous servers of a single RR.
        /// When multiple HIP RRs are present at the same owner name,
        /// this implicit order of rendezvous servers within an RR must not be used to infer a preference order between rendezvous servers stored in different RRs.
        /// </param>
        public DnsResourceDataHostIdentityProtocol(DataSegment hostIdentityTag, DnsPublicKeyAlgorithm publicKeyAlgorithm, DataSegment publicKey,
                                                   IEnumerable<DnsDomainName> rendezvousServers)
        {
            if (hostIdentityTag == null)
                throw new ArgumentNullException("hostIdentityTag");
            if (publicKey == null)
                throw new ArgumentNullException("publicKey");

            if (hostIdentityTag.Length > byte.MaxValue)
                throw new ArgumentOutOfRangeException("hostIdentityTag", hostIdentityTag.Length,
                                                      string.Format(CultureInfo.InvariantCulture, "Cannot be bigger than {0}.", byte.MaxValue));
            if (publicKey.Length > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("publicKey", publicKey.Length,
                                                      string.Format(CultureInfo.InvariantCulture, "Cannot be bigger than {0}.", ushort.MaxValue));
            HostIdentityTag = hostIdentityTag;
            PublicKeyAlgorithm = publicKeyAlgorithm;
            PublicKey = publicKey;
            RendezvousServers = IListExtensions.AsReadOnly<DnsDomainName>(rendezvousServers.ToArray());
        }

        /// <summary>
        /// Stored as a binary value in network byte order.
        /// </summary>
        public DataSegment HostIdentityTag { get; private set; }

        /// <summary>
        /// Identifies the public key's cryptographic algorithm and determines the format of the public key field.
        /// </summary>
        public DnsPublicKeyAlgorithm PublicKeyAlgorithm { get; private set; }

        /// <summary>
        /// Contains the algorithm-specific portion of the KEY RR RDATA.
        /// </summary>
        public DataSegment PublicKey { get; private set; }

        /// <summary>
        /// Indicates one or more domain names of rendezvous server(s).
        /// Must not be compressed.
        /// The rendezvous server(s) are listed in order of preference (i.e., first rendezvous server(s) are preferred),
        /// defining an implicit order amongst rendezvous servers of a single RR.
        /// When multiple HIP RRs are present at the same owner name,
        /// this implicit order of rendezvous servers within an RR must not be used to infer a preference order between rendezvous servers stored in different RRs.
        /// </summary>
        public ReadOnlyCollection<DnsDomainName> RendezvousServers { get; private set; }

        /// <summary>
        /// Two DnsResourceDataHostIdentityProtocol are equal iff their host identity tag, public key algorithm, public key and rendezvous servers fields 
        /// are equal.
        /// </summary>
        public bool Equals(DnsResourceDataHostIdentityProtocol other)
        {
            return other != null &&
                   HostIdentityTag.Equals(other.HostIdentityTag) &&
                   PublicKeyAlgorithm.Equals(other.PublicKeyAlgorithm) &&
                   PublicKey.Equals(other.PublicKey) &&
                   RendezvousServers.SequenceEqual(RendezvousServers);
        }

        /// <summary>
        /// Two DnsResourceDataHostIdentityProtocol are equal iff their host identity tag, public key algorithm, public key and rendezvous servers fields 
        /// are equal.
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as DnsResourceDataHostIdentityProtocol);
        }

        /// <summary>
        /// A hash code of the combination of the host identity tag, public key algorithm, public key and rendezvous servers fields.
        /// </summary>
        public override int GetHashCode()
        {
            return Sequence.GetHashCode(HostIdentityTag, PublicKeyAlgorithm, PublicKey) ^ RendezvousServers.SequenceGetHashCode();
        }

        internal DnsResourceDataHostIdentityProtocol()
            : this(DataSegment.Empty, DnsPublicKeyAlgorithm.None, DataSegment.Empty, new DnsDomainName[0])
        {
        }

        internal override int GetLength()
        {
            return ConstantPartLength + HostIdentityTag.Length + PublicKey.Length + RendezvousServers.Sum(rendezvousServer => rendezvousServer.NonCompressedLength);
        }

        internal override int WriteData(byte[] buffer, int offset)
        {
            buffer.Write(offset + Offset.HostIdentityTagLength, (byte)HostIdentityTag.Length);
            buffer.Write(offset + Offset.PublicKeyAlgorithm, (byte)PublicKeyAlgorithm);
            buffer.Write(offset + Offset.PublicKeyLength, (ushort)PublicKey.Length, Endianity.Big);
            HostIdentityTag.Write(buffer, offset + Offset.HostIdentityTag);
            int numBytesWritten = ConstantPartLength + HostIdentityTag.Length;
            PublicKey.Write(buffer, offset + numBytesWritten);
            numBytesWritten += PublicKey.Length;
            foreach (DnsDomainName rendezvousServer in RendezvousServers)
            {
                rendezvousServer.WriteUncompressed(buffer, offset + numBytesWritten);
                numBytesWritten += rendezvousServer.NonCompressedLength;
            }

            return numBytesWritten;
        }
        internal override DnsResourceData CreateInstance(DnsDatagram dns, int offsetInDns, int length)
        {
            if (length < ConstantPartLength)
                return null;

            int hostIdentityTagLength = dns[offsetInDns + Offset.HostIdentityTagLength];
            DnsPublicKeyAlgorithm publicKeyAlgorithm = (DnsPublicKeyAlgorithm)dns[offsetInDns + Offset.PublicKeyAlgorithm];
            int publicKeyLength = dns.ReadUShort(offsetInDns + Offset.PublicKeyLength, Endianity.Big);
            
            if (length < ConstantPartLength + hostIdentityTagLength + publicKeyLength)
                return null;
            DataSegment hostIdentityTag = dns.Subsegment(offsetInDns + Offset.HostIdentityTag, hostIdentityTagLength);
            int publicKeyOffset = offsetInDns + ConstantPartLength + hostIdentityTagLength;
            DataSegment publicKey = dns.Subsegment(publicKeyOffset, publicKeyLength);

            offsetInDns += ConstantPartLength + hostIdentityTagLength + publicKeyLength;
            length -= ConstantPartLength + hostIdentityTagLength + publicKeyLength;

            List<DnsDomainName> rendezvousServers = new List<DnsDomainName>();
            while (length != 0)
            {
                DnsDomainName rendezvousServer;
                int rendezvousServerLength;
                if (!DnsDomainName.TryParse(dns, offsetInDns, length, out rendezvousServer, out rendezvousServerLength))
                    return null;
                rendezvousServers.Add(rendezvousServer);
                offsetInDns += rendezvousServerLength;
                length -= rendezvousServerLength;
            }

            return new DnsResourceDataHostIdentityProtocol(hostIdentityTag, publicKeyAlgorithm, publicKey, rendezvousServers);
        }
    }
}