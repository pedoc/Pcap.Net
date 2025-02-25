﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PcapTools.Base;

namespace PcapTools.Packets.Dns
{
    /// <summary>
    /// Reid.
    /// <pre>
    /// +---------------------+
    /// | One ore more strings|
    /// +---------------------+
    /// </pre>
    /// </summary>
    [DnsTypeRegistration(Type = DnsType.NInfo)]
    public sealed class DnsResourceDataNInfo : DnsResourceDataStrings
    {
        private const int MinNumStrings = 1;

        /// <summary>
        /// Constructs the resource data from strings.
        /// </summary>
        /// <param name="strings">A descriptive text in one or more strings.</param>
        public DnsResourceDataNInfo(ReadOnlyCollection<DataSegment> strings)
            : base(strings)
        {
            if (strings == null)
                throw new ArgumentNullException("strings");

            if (strings.Count < MinNumStrings)
                throw new ArgumentOutOfRangeException("strings", strings.Count, "There must be at least one string.");
        }

        /// <summary>
        /// Constructs the resource data from strings.
        /// </summary>
        /// <param name="strings">A descriptive text in one or more strings.</param>
        public DnsResourceDataNInfo(params DataSegment[] strings)
            : this(strings.AsReadOnly())
        {
        }

        internal DnsResourceDataNInfo()
            : this(DataSegment.Empty)
        {
        }

        internal override DnsResourceData CreateInstance(DataSegment data)
        {
            List<DataSegment> strings = ReadStrings(data, MinNumStrings);
            if (strings == null || strings.Count < MinNumStrings)
                return null;

            return new DnsResourceDataNInfo(strings.AsReadOnly());
        }
    }
}