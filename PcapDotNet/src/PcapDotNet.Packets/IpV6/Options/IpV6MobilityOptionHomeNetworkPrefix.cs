namespace PcapTools.Packets.IpV6.Options
{
    /// <summary>
    /// RFC 5213.
    /// <pre>
    /// +-----+--------------+---------------+
    /// | Bit | 0-7          | 8-15          |
    /// +-----+--------------+---------------+
    /// | 0   | Option Type  | Opt Data Len  |
    /// +-----+--------------+---------------+
    /// | 16  | Reserved     | Prefix Length |
    /// +-----+--------------+---------------+
    /// | 32  | Mobile Network Prefix        |
    /// |     |                              |
    /// |     |                              |
    /// |     |                              |
    /// |     |                              |
    /// |     |                              |
    /// |     |                              |
    /// |     |                              |
    /// +-----+------------------------------+
    /// </pre>
    /// </summary>
    [IpV6MobilityOptionTypeRegistration(IpV6MobilityOptionType.HomeNetworkPrefix)]
    public sealed class IpV6MobilityOptionHomeNetworkPrefix : IpV6MobilityOptionNetworkPrefix
    {
        /// <summary>
        /// Creates an instance from prefix length and home network prefix.
        /// </summary>
        /// <param name="prefixLength">Indicates the prefix length of the IPv6 prefix contained in the option.</param>
        /// <param name="homeNetworkPrefix">Contains the Home Network Prefix.</param>
        public IpV6MobilityOptionHomeNetworkPrefix(byte prefixLength, IpV6Address homeNetworkPrefix)
            : base(IpV6MobilityOptionType.HomeNetworkPrefix, prefixLength, homeNetworkPrefix)
        {
        }

        internal override IpV6MobilityOption CreateInstance(DataSegment data)
        {
            byte prefixLength;
            IpV6Address mobileNetworkPrefix;
            if (!Read(data, out prefixLength, out mobileNetworkPrefix))
                return null;

            return new IpV6MobilityOptionHomeNetworkPrefix(prefixLength, mobileNetworkPrefix);
        }

        private IpV6MobilityOptionHomeNetworkPrefix()
            : this(0, IpV6Address.Zero)
        {
        }
    }
}