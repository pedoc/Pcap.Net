using PcapTools.Packets.IpV4;

namespace PcapTools.Packets.IpV6.Options
{
    /// <summary>
    /// RFC 5844.
    /// </summary>
    public interface IIpV6MobilityOptionIpV4HomeAddress
    {
        /// <summary>
        /// The prefix length of the address.
        /// </summary>
        byte PrefixLength { get; }

        /// <summary>
        /// Contains the IPv4 home address.
        /// </summary>
        IpV4Address HomeAddress { get; }
    }
}