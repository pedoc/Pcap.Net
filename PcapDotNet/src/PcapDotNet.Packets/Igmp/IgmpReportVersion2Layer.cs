namespace PcapTools.Packets.Igmp
{
    /// <summary>
    /// RFC 2236.
    /// </summary>
    public sealed class IgmpReportVersion2Layer : IgmpVersion2Layer
    {
        /// <summary>
        /// The type of the IGMP message of concern to the host-router interaction.
        /// </summary>
        public override IgmpMessageType MessageTypeValue
        {
            get { return IgmpMessageType.MembershipReportVersion2; }
        }
    }
}