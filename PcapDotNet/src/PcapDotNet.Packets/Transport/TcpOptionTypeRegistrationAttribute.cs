using System;
using PcapTools.Packets.Ip;

namespace PcapTools.Packets.Transport
{
    internal sealed class TcpOptionTypeRegistrationAttribute : OptionTypeRegistrationAttribute
    {
        public TcpOptionTypeRegistrationAttribute(TcpOptionType optionType)
        {
            TcpOptionType = optionType;
        }

        public TcpOptionType TcpOptionType { get; private set; }

        public override object OptionType
        {
            get { return TcpOptionType; }
        }

        public override Type OptionTypeType
        {
            get { return typeof(TcpOptionType); }
        }
    }
}