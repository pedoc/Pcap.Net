using System;

namespace PcapTools.Packets.IpV6.Options
{
    internal sealed class IpV6OptionTypeRegistrationAttribute : Attribute
    {
        public IpV6OptionTypeRegistrationAttribute(IpV6OptionType optionType)
        {
            OptionType = optionType;
        }

        public IpV6OptionType OptionType { get; private set; }
    }
}