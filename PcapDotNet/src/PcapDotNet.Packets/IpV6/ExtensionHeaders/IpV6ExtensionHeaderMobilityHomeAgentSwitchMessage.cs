using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PcapTools.Base;
using PcapTools.Packets.IpV4;
using PcapTools.Packets.IpV6.Options;

namespace PcapTools.Packets.IpV6.ExtensionHeaders
{
    /// <summary>
    /// RFC 5142.
    /// <pre>
    /// +-----+----------------+-------------------------+
    /// | Bit | 0-7            | 8-15                    |
    /// +-----+----------------+-------------------------+
    /// | 0   | Next Header    | Header Extension Length |
    /// +-----+----------------+-------------------------+
    /// | 16  | MH Type        | Reserved                |
    /// +-----+----------------+-------------------------+
    /// | 32  | Checksum                                 |
    /// +-----+----------------+-------------------------+
    /// | 48  | # of Addresses | Reserved                |
    /// +-----+----------------+-------------------------+
    /// | 64  | Home Agent Addresses                     |
    /// | ... |                                          |
    /// +-----+------------------------------------------+
    /// |     | Mobility Options                         |
    /// | ... |                                          |
    /// +-----+------------------------------------------+
    /// </pre>
    /// </summary>
    public sealed class IpV6ExtensionHeaderMobilityHomeAgentSwitchMessage : IpV6ExtensionHeaderMobility
    {
        private static class MessageDataOffset
        {
            public const int NumberOfAddresses = 0;
            public const int HomeAgentAddresses = NumberOfAddresses + sizeof(byte) + sizeof(byte);
        }

        /// <summary>
        /// The minimum number of bytes the message data takes.
        /// </summary>
        public const int MinimumMessageDataLength = MessageDataOffset.HomeAgentAddresses;

        /// <summary>
        /// Creates an instance from next header, checksum, home agent addresses and options.
        /// </summary>
        /// <param name="nextHeader">Identifies the type of header immediately following this extension header.</param>
        /// <param name="checksum">
        /// Contains the checksum of the Mobility Header.
        /// The checksum is calculated from the octet string consisting of a "pseudo-header"
        /// followed by the entire Mobility Header starting with the Payload Proto field.
        /// The checksum is the 16-bit one's complement of the one's complement sum of this string.
        /// </param>
        /// <param name="homeAgentAddresses">A list of alternate home agent addresses for the mobile node.</param>
        /// <param name="options">Zero or more TLV-encoded mobility options.</param>
        public IpV6ExtensionHeaderMobilityHomeAgentSwitchMessage(IpV4Protocol? nextHeader, ushort checksum, ReadOnlyCollection<IpV6Address> homeAgentAddresses,
                                                                 IpV6MobilityOptions options)
            : base(nextHeader, checksum, options,
                   MessageDataOffset.HomeAgentAddresses + (homeAgentAddresses == null ? 0 : homeAgentAddresses.Count) * IpV6Address.SizeOf)
        {
            if (homeAgentAddresses == null)
                throw new ArgumentNullException("homeAgentAddresses");
            HomeAgentAddresses = homeAgentAddresses;
        }

        /// <summary>
        /// Creates an instance from next header, checksum, home agent addresses and options.
        /// </summary>
        /// <param name="nextHeader">Identifies the type of header immediately following this extension header.</param>
        /// <param name="checksum">
        /// Contains the checksum of the Mobility Header.
        /// The checksum is calculated from the octet string consisting of a "pseudo-header"
        /// followed by the entire Mobility Header starting with the Payload Proto field.
        /// The checksum is the 16-bit one's complement of the one's complement sum of this string.
        /// </param>
        /// <param name="homeAgentAddresses">A list of alternate home agent addresses for the mobile node.</param>
        /// <param name="options">Zero or more TLV-encoded mobility options.</param>
        public IpV6ExtensionHeaderMobilityHomeAgentSwitchMessage(IpV4Protocol? nextHeader, ushort checksum, IList<IpV6Address> homeAgentAddresses, IpV6MobilityOptions options)
            : this(nextHeader, checksum, (ReadOnlyCollection<IpV6Address>)homeAgentAddresses.AsReadOnly(), options)
        {
        }

        /// <summary>
        /// Identifies the particular mobility message in question.
        /// An unrecognized MH Type field causes an error indication to be sent.
        /// </summary>
        public override IpV6MobilityHeaderType MobilityHeaderType
        {
            get { return IpV6MobilityHeaderType.HomeAgentSwitchMessage; }
        }

        /// <summary>
        /// A list of alternate home agent addresses for the mobile node.
        /// </summary>
        public ReadOnlyCollection<IpV6Address> HomeAgentAddresses { get; private set; }

        internal override int MessageDataLength
        {
            get { return MinimumMessageDataLength + HomeAgentAddresses.Count * IpV6Address.SizeOf + MobilityOptions.BytesLength; }
        }

        internal override bool EqualsMessageData(IpV6ExtensionHeaderMobility other)
        {
            return EqualsMessageData(other as IpV6ExtensionHeaderMobilityHomeAgentSwitchMessage);
        }

        internal override int GetMessageDataHashCode()
        {
            return HomeAgentAddresses.SequenceGetHashCode();
        }

        internal static IpV6ExtensionHeaderMobilityHomeAgentSwitchMessage ParseMessageData(IpV4Protocol nextHeader, ushort checksum, DataSegment messageData)
        {
            if (messageData.Length < MinimumMessageDataLength)
                return null;

            byte numberOfAddresses = messageData[MessageDataOffset.NumberOfAddresses];
            int homeAgentAddressesSize = numberOfAddresses * IpV6Address.SizeOf;
            if (messageData.Length < MinimumMessageDataLength + homeAgentAddressesSize)
                return null;

            IpV6Address[] homeAgentAddresses = new IpV6Address[numberOfAddresses];
            for (int i = 0; i != numberOfAddresses; ++i)
                homeAgentAddresses[i] = messageData.ReadIpV6Address(MessageDataOffset.HomeAgentAddresses + i * IpV6Address.SizeOf, Endianity.Big);

            int optionsOffset = MessageDataOffset.HomeAgentAddresses + homeAgentAddressesSize;
            IpV6MobilityOptions options = new IpV6MobilityOptions(messageData.Subsegment(optionsOffset, messageData.Length - optionsOffset));
            return new IpV6ExtensionHeaderMobilityHomeAgentSwitchMessage(nextHeader, checksum, homeAgentAddresses, options);
        }

        internal override void WriteMessageData(byte[] buffer, int offset)
        {
            buffer.Write(offset + MessageDataOffset.NumberOfAddresses, (byte)HomeAgentAddresses.Count);
            for (int i = 0; i != HomeAgentAddresses.Count; ++i)
                buffer.Write(offset + MessageDataOffset.HomeAgentAddresses + i * IpV6Address.SizeOf, HomeAgentAddresses[i], Endianity.Big);
            MobilityOptions.Write(buffer, offset + MessageDataOffset.HomeAgentAddresses + HomeAgentAddresses.Count * IpV6Address.SizeOf);
        }

        private bool EqualsMessageData(IpV6ExtensionHeaderMobilityHomeAgentSwitchMessage other)
        {
            return other != null &&
                   HomeAgentAddresses.SequenceEqual(other.HomeAgentAddresses);
        }
    }
}