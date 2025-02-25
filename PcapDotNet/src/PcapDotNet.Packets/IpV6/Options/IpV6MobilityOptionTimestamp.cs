using System;

namespace PcapTools.Packets.IpV6.Options
{
    /// <summary>
    /// RFC 5213.
    /// <pre>
    /// +-----+-------------+--------------+
    /// | Bit | 0-7         | 8-15         |
    /// +-----+-------------+--------------+
    /// | 0   | Option Type | Opt Data Len |
    /// +-----+-------------+--------------+
    /// | 16  | Timestamp                  |
    /// |     |                            |
    /// |     |                            |
    /// |     |                            |
    /// +-----+----------------------------+
    /// </pre>
    /// </summary>
    [IpV6MobilityOptionTypeRegistration(IpV6MobilityOptionType.Timestamp)]
    public sealed class IpV6MobilityOptionTimestamp : IpV6MobilityOptionULong
    {
        /// <summary>
        /// Creates an instance from timestamp.
        /// </summary>
        /// <param name="timestamp">
        /// Timestamp.  
        /// The value indicates the number of seconds since January 1, 1970, 00:00 UTC, by using a fixed point format.
        /// In this format, the integer number of seconds is contained in the first 48 bits of the field, and the remaining 16 bits indicate the number of 1/65536 fractions of a second.
        /// </param>
        public IpV6MobilityOptionTimestamp(ulong timestamp)
            : base(IpV6MobilityOptionType.Timestamp, timestamp)
        {
        }

        /// <summary>
        /// Timestamp.  
        /// The value indicates the number of seconds since January 1, 1970, 00:00 UTC, by using a fixed point format.
        /// In this format, the integer number of seconds is contained in the first 48 bits of the field, and the remaining 16 bits indicate the number of 1/65536 fractions of a second.
        /// </summary>
        public ulong Timestamp
        {
            get { return Value; }
        }

        /// <summary>
        /// Timestamp.  
        /// The value indicates the number of seconds since January 1, 1970, 00:00 UTC calculated from the ulong Timestamp field.
        /// </summary>
        public double TimestampSeconds
        {
            get { return (Timestamp >> 16) + (Timestamp & 0xFFFF) / 65536.0; }
        }

        /// <summary>
        /// Timestamp.  
        /// The value indicates the Timestamp date calculated from the Timestamp ulong field.
        /// </summary>
        public DateTime TimestampDateTime
        {
            get
            {
                double seconds = TimestampSeconds;
                if (seconds >= MaxSecondsSinceEpcohTimeForDateTime)
                    return DateTime.MaxValue;
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds);
            }
        }

        internal override IpV6MobilityOption CreateInstance(DataSegment data)
        {
            ulong timestamp;
            if (!Read(data, out timestamp))
                return null;

            return new IpV6MobilityOptionTimestamp(timestamp);
        }

        private IpV6MobilityOptionTimestamp()
            : this(0)
        {
        }

        private static readonly double MaxSecondsSinceEpcohTimeForDateTime = (DateTime.MaxValue - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }
}