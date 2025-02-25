﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using PcapTools.Base;

namespace PcapTools.Packets.Dns
{
    /// <summary>
    /// RFC 5155.
    /// <pre>
    /// +-----+-------------+----------+--------+------------+
    /// | bit | 0-7         | 8-14     | 15     | 16-31      |
    /// +-----+-------------+----------+--------+------------+
    /// | 0   | Hash Alg    | Reserved | OptOut | Iterations |
    /// +-----+-------------+----------+--------+------------+
    /// | 32  | Salt Length | Salt                           |
    /// +-----+-------------+                                |
    /// | ... |                                              |
    /// +-----+-------------+--------------------------------+
    /// |     | Hash Length | Next Hashed Owner Name         |
    /// +-----+-------------+                                |
    /// | ... |                                              |
    /// +-----+----------------------------------------------+
    /// |     | Type Bit Maps                                |
    /// | ... |                                              |
    /// +-----+----------------------------------------------+
    /// </pre>
    /// </summary>
    [DnsTypeRegistration(Type = DnsType.NSec3)]
    public sealed class DnsResourceDataNextDomainSecure3 : DnsResourceDataNextDomainSecure3Base, IEquatable<DnsResourceDataNextDomainSecure3>
    {
        /// <summary>
        /// Constructs an instance out of the hash algorithm, flags, iterations, salt, next hashed owner name and types exist fields.
        /// </summary>
        /// <param name="hashAlgorithm">Identifies the cryptographic hash algorithm used to construct the hash-value.</param>
        /// <param name="flags">Can be used to indicate different processing. All undefined flags must be zero.</param>
        /// <param name="iterations">
        /// Defines the number of additional times the hash function has been performed.
        /// More iterations result in greater resiliency of the hash value against dictionary attacks, 
        /// but at a higher computational cost for both the server and resolver.
        /// </param>
        /// <param name="salt">Appended to the original owner name before hashing in order to defend against pre-calculated dictionary attacks.</param>
        /// <param name="nextHashedOwnerName">
        /// Contains the next hashed owner name in hash order.
        /// This value is in binary format.
        /// Given the ordered set of all hashed owner names, the Next Hashed Owner Name field contains the hash of an owner name that immediately follows the owner name of the given NSEC3 RR.
        /// The value of the Next Hashed Owner Name field in the last NSEC3 RR in the zone is the same as the hashed owner name of the first NSEC3 RR in the zone in hash order.
        /// Note that, unlike the owner name of the NSEC3 RR, the value of this field does not contain the appended zone name.
        /// </param>
        /// <param name="typesExist">Identifies the RRSet types that exist at the original owner name of the NSEC3 RR.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags")]
        public DnsResourceDataNextDomainSecure3(DnsSecNSec3HashAlgorithm hashAlgorithm, DnsSecNSec3Flags flags, ushort iterations, DataSegment salt,
                                                DataSegment nextHashedOwnerName, IEnumerable<DnsType> typesExist)
            : this(hashAlgorithm, flags, iterations, salt, nextHashedOwnerName, new DnsTypeBitmaps(typesExist))
        {
        }

        /// <summary>
        /// Contains the next hashed owner name in hash order.
        /// This value is in binary format.
        /// Given the ordered set of all hashed owner names, the Next Hashed Owner Name field contains the hash of an owner name that immediately follows the owner name of the given NSEC3 RR.
        /// The value of the Next Hashed Owner Name field in the last NSEC3 RR in the zone is the same as the hashed owner name of the first NSEC3 RR in the zone in hash order.
        /// Note that, unlike the owner name of the NSEC3 RR, the value of this field does not contain the appended zone name.
        /// </summary>
        public DataSegment NextHashedOwnerName { get; private set; }

        /// <summary>
        /// Identifies the RRSet types that exist at the original owner name of the NSEC3 RR.
        /// </summary>
        public ReadOnlyCollection<DnsType> TypesExist { get { return _typeBitmaps.TypesExist.AsReadOnly(); } }

        /// <summary>
        /// Two DnsResourceDataNextDomainSecure3 are equal iff their hash algorithm, flags, iterations, salt, next hashed owner name and types exist fields
        /// are equal.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool Equals(DnsResourceDataNextDomainSecure3 other)
        {
            return EqualsParameters(other) &&
                   NextHashedOwnerName.Equals(other.NextHashedOwnerName) &&
                   _typeBitmaps.Equals(other._typeBitmaps);
        }

        /// <summary>
        /// Two DnsResourceDataNextDomainSecure3 are equal iff their hash algorithm, flags, iterations, salt, next hashed owner name and types exist fields
        /// are equal.
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as DnsResourceDataNextDomainSecure3);
        }

        /// <summary>
        /// A hash code of the combination of the hash algorithm, flags, iterations, salt, next hashed owner name and types exist fields.
        /// </summary>
        public override int GetHashCode()
        {
            return GetHashCodeParameters() ^ Sequence.GetHashCode(NextHashedOwnerName, _typeBitmaps);
        }

        internal DnsResourceDataNextDomainSecure3()
            : this(DnsSecNSec3HashAlgorithm.Sha1, DnsSecNSec3Flags.None, 0, DataSegment.Empty, DataSegment.Empty, new DnsType[0])
        {
        }

        internal override int GetLength()
        {
            return ParametersLength + sizeof(byte) + NextHashedOwnerName.Length + _typeBitmaps.GetLength();
        }

        internal override void WriteDataSimple(byte[] buffer, int offset)
        {
            WriteParameters(buffer, offset);
            buffer.Write(offset + NextHashedOwnerNameLengthOffset, (byte)NextHashedOwnerName.Length);
            NextHashedOwnerName.Write(buffer, offset + NextHashedOwnerNameOffset);
            _typeBitmaps.Write(buffer, offset + TypeBitmapsOffset);
        }

        internal override DnsResourceData CreateInstance(DataSegment data)
        {
            DnsSecNSec3HashAlgorithm hashAlgorithm;
            DnsSecNSec3Flags flags;
            ushort iterations;
            DataSegment salt;
            if (!TryReadParameters(data, out hashAlgorithm, out flags, out iterations, out salt))
                return null;

            int nextHashedOwnerNameLengthOffset = GetParametersLength(salt.Length);
            if (data.Length - nextHashedOwnerNameLengthOffset < sizeof(byte))
                return null;
            int nextHashedOwnerNameOffset = nextHashedOwnerNameLengthOffset + sizeof(byte);
            int nextHashedOwnerNameLength = data[nextHashedOwnerNameLengthOffset];
            if (data.Length - nextHashedOwnerNameOffset < nextHashedOwnerNameLength)
                return null;
            DataSegment nextHashedOwnerName = data.Subsegment(nextHashedOwnerNameOffset, nextHashedOwnerNameLength);

            int typeBitmapsOffset = nextHashedOwnerNameOffset + nextHashedOwnerNameLength;
            DnsTypeBitmaps typeBitmaps = DnsTypeBitmaps.CreateInstance(data.Buffer, data.StartOffset + typeBitmapsOffset, data.Length - typeBitmapsOffset);
            if (typeBitmaps == null)
                return null;

            return new DnsResourceDataNextDomainSecure3(hashAlgorithm, flags, iterations, salt, nextHashedOwnerName, typeBitmaps);
        }

        private DnsResourceDataNextDomainSecure3(DnsSecNSec3HashAlgorithm hashAlgorithm, DnsSecNSec3Flags flags, ushort iterations, DataSegment salt,
                                                 DataSegment nextHashedOwnerName, DnsTypeBitmaps typeBitmaps)
            : base(hashAlgorithm, flags, iterations, salt)
        {
            if (nextHashedOwnerName.Length > byte.MaxValue)
                throw new ArgumentOutOfRangeException("nextHashedOwnerName", nextHashedOwnerName.Length,
                                                      string.Format(CultureInfo.InvariantCulture, "Cannot bigger than {0}.", byte.MaxValue));

            NextHashedOwnerName = nextHashedOwnerName;
            _typeBitmaps = typeBitmaps;
        }

        private int NextHashedOwnerNameLengthOffset { get { return ParametersLength; } }

        private int NextHashedOwnerNameOffset { get { return NextHashedOwnerNameLengthOffset + sizeof(byte); } }

        private int TypeBitmapsOffset { get { return NextHashedOwnerNameOffset + NextHashedOwnerName.Length; } }

        private readonly DnsTypeBitmaps _typeBitmaps;
    }
}