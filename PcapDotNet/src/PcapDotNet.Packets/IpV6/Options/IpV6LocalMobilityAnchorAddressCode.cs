namespace PcapTools.Packets.IpV6.Options
{
    /// <summary>
    /// RFC 5949.
    /// </summary>
    public enum IpV6LocalMobilityAnchorAddressCode : byte
    {
        /// <summary>
        /// Invalid value.
        /// </summary>
        None = 0,

        /// <summary>
        /// IPv6 address of the local mobility anchor (LMAA).
        /// </summary>
        IpV6 = 1,

        /// <summary>
        /// IPv4 address of the local mobility anchor (IPv4-LMAA).
        /// </summary>
        IpV4 = 2,
    }
}