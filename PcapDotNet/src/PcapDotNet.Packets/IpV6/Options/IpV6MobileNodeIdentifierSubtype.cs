namespace PcapTools.Packets.IpV6.Options
{
    /// <summary>
    /// RFC 4283.
    /// </summary>
    public enum IpV6MobileNodeIdentifierSubtype : byte
    {
        /// <summary>
        /// Invalid value.
        /// </summary>
        None = 0,

        /// <summary>
        /// RFC 4283.
        /// Uses an identifier of the form user@realm (RFC 4282).
        /// </summary>
        NetworkAccessIdentifier = 1,
    }
}