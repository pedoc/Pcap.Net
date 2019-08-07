using System;

namespace PcapTools.Packets.IpV6.Options
{
    internal sealed class IpV6MobilityOptionTypeRegistrationAttribute : Attribute
    {
        public IpV6MobilityOptionTypeRegistrationAttribute(IpV6MobilityOptionType optionType)
        {
            OptionType = optionType;
        }

        public IpV6MobilityOptionType OptionType { get; private set; }
    }
}