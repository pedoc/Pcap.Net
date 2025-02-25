namespace PcapTools.Packets.IpV6.Options
{
    /// <summary>
    /// RFC 5847.
    /// <pre>
    /// +-----+-------------+--------------+
    /// | Bit | 0-7         | 8-15         |
    /// +-----+-------------+--------------+
    /// | 0   | Option Type | Opt Data Len |
    /// +-----+-------------+--------------+
    /// | 16  | Restart Counter            |
    /// |     |                            |
    /// +-----+----------------------------+
    /// </pre>
    /// </summary>
    [IpV6MobilityOptionTypeRegistration(IpV6MobilityOptionType.RestartCounter)]
    public sealed class IpV6MobilityOptionRestartCounter : IpV6MobilityOptionComplex
    {
        /// <summary>
        /// The number of bytes the option data takes.
        /// </summary>
        public const int OptionDataLength = sizeof(uint);

        /// <summary>
        /// Creates an option from the given restart counter.
        /// </summary>
        /// <param name="restartCounter">Indicates the current Restart Counter value.</param>
        public IpV6MobilityOptionRestartCounter(uint restartCounter)
            : base(IpV6MobilityOptionType.RestartCounter)
        {
            RestartCounter = restartCounter;
        }

        /// <summary>
        /// Indicates the current Restart Counter value.
        /// </summary>
        public uint RestartCounter { get; private set; }

        internal override IpV6MobilityOption CreateInstance(DataSegment data)
        {
            if (data.Length != OptionDataLength)
                return null;

            uint restartCounter = data.ReadUInt(0, Endianity.Big);
            return new IpV6MobilityOptionRestartCounter(restartCounter);
        }

        internal override int DataLength
        {
            get { return OptionDataLength; }
        }

        internal override bool EqualsData(IpV6MobilityOption other)
        {
            return EqualsData(other as IpV6MobilityOptionRestartCounter);
        }

        internal override int GetDataHashCode()
        {
            return RestartCounter.GetHashCode();
        }

        internal override void WriteData(byte[] buffer, ref int offset)
        {
            buffer.Write(ref offset, RestartCounter, Endianity.Big);
        }

        private IpV6MobilityOptionRestartCounter()
            : this(0)
        {
        }

        private bool EqualsData(IpV6MobilityOptionRestartCounter other)
        {
            return other != null &&
                   RestartCounter == other.RestartCounter;
        }
    }
}