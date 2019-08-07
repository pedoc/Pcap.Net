using System;

namespace PcapTools.Packets.IpV6.Options
{
    internal sealed class IpV6FlowIdentificationSubOptionTypeRegistrationAttribute : Attribute
    {
        public IpV6FlowIdentificationSubOptionTypeRegistrationAttribute(IpV6FlowIdentificationSubOptionType optionType)
        {
            OptionType = optionType;
        }

        public IpV6FlowIdentificationSubOptionType OptionType { get; private set; }
    }
}