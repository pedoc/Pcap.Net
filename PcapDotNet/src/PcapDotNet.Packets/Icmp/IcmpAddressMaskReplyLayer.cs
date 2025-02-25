namespace PcapTools.Packets.Icmp
{
    /// <summary>
    /// RFC 950.
    /// </summary>
    public sealed class IcmpAddressMaskReplyLayer : IcmpAddressMaskRequestLayer
    {
        /// <summary>
        /// The value of this field determines the format of the remaining data.
        /// </summary>
        public override IcmpMessageType MessageType
        {
            get { return IcmpMessageType.AddressMaskReply; }
        }
    }
}