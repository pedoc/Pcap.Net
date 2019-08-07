using System;

namespace PcapTools.Packets.Icmp
{
    internal sealed class IcmpDatagramRegistrationAttribute : Attribute
    {
        public IcmpDatagramRegistrationAttribute(IcmpMessageType messageType)
        {
            MessageType = messageType;
        }

        public IcmpMessageType MessageType { get; private set; }
    }
}