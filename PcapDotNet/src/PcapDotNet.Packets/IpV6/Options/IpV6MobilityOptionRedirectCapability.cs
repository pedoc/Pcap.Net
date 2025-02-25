namespace PcapTools.Packets.IpV6.Options
{
    /// <summary>
    /// RFC 6463.
    /// <pre>
    /// +-----+-------------+--------------+
    /// | Bit | 0-7         | 8-15         |
    /// +-----+-------------+--------------+
    /// | 0   | Option Type | Opt Data Len |
    /// +-----+-------------+--------------+
    /// | 16  | Reserved                   |
    /// +-----+----------------------------+
    /// </pre>
    /// </summary>
    [IpV6MobilityOptionTypeRegistration(IpV6MobilityOptionType.RedirectCapability)]
    public sealed class IpV6MobilityOptionRedirectCapability : IpV6MobilityOptionComplex
    {
        /// <summary>
        /// The number of bytes this option data takes.
        /// </summary>
        public const int OptionDataLength = sizeof(ushort);

        /// <summary>
        /// Creates an instance.
        /// </summary>
        public IpV6MobilityOptionRedirectCapability()
            : base(IpV6MobilityOptionType.RedirectCapability)
        {
        }

        internal override IpV6MobilityOption CreateInstance(DataSegment data)
        {
            if (data.Length != OptionDataLength)
                return null;

            return new IpV6MobilityOptionRedirectCapability();
        }

        internal override int DataLength
        {
            get { return OptionDataLength; }
        }

        internal override bool EqualsData(IpV6MobilityOption other)
        {
            return true;
        }

        internal override int GetDataHashCode()
        {
            return 0;
        }

        internal override void WriteData(byte[] buffer, ref int offset)
        {
            offset += DataLength;
        }
    }
}