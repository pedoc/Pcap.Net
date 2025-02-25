﻿namespace PcapTools.Packets.Dns
{
    /// <summary>
    /// RFC 5155.
    /// </summary>
    public enum DnsSecNSec3HashAlgorithm : byte
    {
        /// <summary>
        /// Undefined value.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// RFC 5155.
        /// </summary>
        Sha1 = 0x01,
    }
}