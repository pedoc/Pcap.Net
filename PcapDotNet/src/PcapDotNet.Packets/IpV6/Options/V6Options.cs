using System;
using System.Collections.Generic;
using PcapTools.Packets.Ip;

namespace PcapTools.Packets.IpV6.Options
{
    /// <summary>
    /// Base class for different IPv6 options types.
    /// </summary>
    /// <typeparam name="T">The option concrete type.</typeparam>
    public abstract class V6Options<T> : Options<T> where T : Option, IEquatable<T>
    {
        internal V6Options(IList<T> options, bool isValid)
            : base(options, isValid, SumBytesLength(options))
        {
        }
    }
}