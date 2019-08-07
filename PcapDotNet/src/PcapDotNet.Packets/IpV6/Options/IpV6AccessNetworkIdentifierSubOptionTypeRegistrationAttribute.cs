using System;

namespace PcapTools.Packets.IpV6.Options
{
    /// <summary>
    /// RFC 6757.
    /// </summary>
    internal sealed class IpV6AccessNetworkIdentifierSubOptionTypeRegistrationAttribute : Attribute
    {
        public IpV6AccessNetworkIdentifierSubOptionTypeRegistrationAttribute(IpV6AccessNetworkIdentifierSubOptionType optionType)
        {
            OptionType = optionType;
        }

        public IpV6AccessNetworkIdentifierSubOptionType OptionType { get; private set; }
    }
}