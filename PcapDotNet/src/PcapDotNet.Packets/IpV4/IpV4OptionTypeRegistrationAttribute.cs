﻿using System;
using PcapTools.Packets.Ip;

namespace PcapTools.Packets.IpV4
{
    internal sealed class IpV4OptionTypeRegistrationAttribute : OptionTypeRegistrationAttribute
    {
        public IpV4OptionTypeRegistrationAttribute(IpV4OptionType optionType)
        {
            IpV4OptionType = optionType;
        }

        public IpV4OptionType IpV4OptionType { get; private set; }

        public override object OptionType
        {
            get { return IpV4OptionType; }
        }

        public override Type OptionTypeType
        {
            get { return typeof(IpV4OptionType); }
        }
    }
}