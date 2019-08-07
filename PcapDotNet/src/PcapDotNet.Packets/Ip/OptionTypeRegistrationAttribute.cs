using System;

namespace PcapTools.Packets.Ip
{
    internal abstract class OptionTypeRegistrationAttribute : Attribute
    {
        public abstract object OptionType { get; }
        public abstract Type OptionTypeType { get; }
    }
}