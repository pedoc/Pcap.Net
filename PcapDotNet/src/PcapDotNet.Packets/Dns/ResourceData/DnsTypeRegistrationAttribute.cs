using System;

namespace PcapTools.Packets.Dns
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class DnsTypeRegistrationAttribute : Attribute
    {
        public DnsType Type { get; set; }
    }
}