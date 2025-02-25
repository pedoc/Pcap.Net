using System;
using System.Collections.ObjectModel;
using System.Linq;
using PcapTools.Base;
using PcapTools.Packets.IpV4;

namespace PcapTools.Packets.Igmp
{
    /// <summary>
    /// RFC 3376.
    /// Represents an IGMP Query version 3 layer.
    /// <seealso cref="IgmpDatagram"/>
    /// </summary>
    public sealed class IgmpQueryVersion3Layer : IgmpVersion1PlusLayer, IIgmpLayerWithGroupAddress
    {
        /// <summary>
        /// Creates an instance of an IGMP Query Version 3 layer.
        /// Default SourceAddresses is no addresses.
        /// </summary>
        public IgmpQueryVersion3Layer()
        {
            SourceAddresses = new IpV4Address[0].AsReadOnly();
        }
        /// <summary>
        /// The actual time allowed, called the Max Resp Time.
        /// </summary>
        public TimeSpan MaxResponseTime { get; set; }

        /// <summary>
        /// The Group Address field is set to zero when sending a General Query, 
        /// and set to the IP multicast address being queried when sending a Group-Specific Query or Group-and-Source-Specific Query.
        /// In a Membership Report of version 1 or 2 or Leave Group message, the group address field holds the IP multicast group address of the group being reported or left.
        /// In a Membership Report of version 3 this field is meaningless.
        /// </summary>
        public IpV4Address GroupAddress { get; set; }

        /// <summary>
        /// When set to one, the S Flag indicates to any receiving multicast routers that they are to suppress the normal timer updates they perform upon hearing a Query.  
        /// It does not, however, suppress the querier election or the normal "host-side" processing of a Query 
        /// that a router may be required to perform as a consequence of itself being a group member.
        /// </summary>
        public bool IsSuppressRouterSideProcessing { get; set; }

        /// <summary>
        /// If non-zero, the QRV field contains the [Robustness Variable] value used by the querier, i.e., the sender of the Query.  
        /// If the querier's [Robustness Variable] exceeds 7, the maximum value of the QRV field, the QRV is set to zero.  
        /// Routers adopt the QRV value from the most recently received Query as their own [Robustness Variable] value, 
        /// unless that most recently received QRV was zero, in which case the receivers use the default [Robustness Variable] value or a statically configured value.
        /// </summary>
        public byte QueryRobustnessVariable { get; set; }

        /// <summary>
        /// The actual interval, called the Querier's Query Interval (QQI).
        /// </summary>
        public TimeSpan QueryInterval { get; set; }

        /// <summary>
        /// The Source Address [i] fields are a vector of n IP unicast addresses,
        /// where n is the value in the Number of Sources (N) field.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ReadOnlyCollection<IpV4Address> SourceAddresses { get; set; }

        /// <summary>
        /// The number of bytes this layer will take.
        /// </summary>
        public override int Length
        {
            get { return IgmpDatagram.GetQueryVersion3Length(SourceAddresses.Count); }
        }

        /// <summary>
        /// Writes the layer to the buffer.
        /// This method ignores the payload length, and the previous and next layers.
        /// </summary>
        /// <param name="buffer">The buffer to write the layer to.</param>
        /// <param name="offset">The offset in the buffer to start writing the layer at.</param>
        protected override void Write(byte[] buffer, int offset)
        {
            IgmpDatagram.WriteQueryVersion3(buffer, offset,
                                            MaxResponseTime, GroupAddress, IsSuppressRouterSideProcessing, QueryRobustnessVariable,
                                            QueryInterval, SourceAddresses);
        }

        /// <summary>
        /// The type of the IGMP message of concern to the host-router interaction.
        /// </summary>
        public override IgmpMessageType MessageTypeValue
        {
            get { return IgmpMessageType.MembershipQuery; }
        }

        /// <summary>
        /// The IGMP version of a Membership Query message.
        /// If the type is not a query, None will be returned.
        /// </summary>
        public override IgmpQueryVersion QueryVersion
        {
            get { return IgmpQueryVersion.Version3; }
        }

        /// <summary>
        /// The actual time allowed, called the Max Resp Time.
        /// </summary>
        public override TimeSpan MaxResponseTimeValue
        {
            get { return MaxResponseTime; }
        }

        /// <summary>
        /// Xor of the combination of the IsSuppressRouterSideProcessing and QueryRobustnessVariable fields with
        /// the hash codes of the layer length, datalink, message type, query version, group address and all the source addresses.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^
                   Sequence.GetHashCode(GroupAddress, BitSequence.Merge(IsSuppressRouterSideProcessing.ToByte(), QueryRobustnessVariable)) ^
                   SourceAddresses.SequenceGetHashCode();
        }

        /// <summary>
        /// true iff the GroupAddress, IsSuppressRouterSideProcessing, QueryRobustnessVariable and SourceAddresses fields are equal
        /// and the QueryInterval is similar.
        /// </summary>
        protected override bool EqualFields(IgmpVersion1PlusLayer other)
        {
            return EqualFields(other as IgmpQueryVersion3Layer);
        }

        /// <summary>
        /// true iff the GroupAddress, IsSuppressRouterSideProcessing, QueryRobustnessVariable and SourceAddresses fields are equal
        /// and the QueryInterval is similar.
        /// </summary>
        private bool EqualFields(IgmpQueryVersion3Layer other)
        {
            return other != null &&
                   GroupAddress == other.GroupAddress &&
                   IsSuppressRouterSideProcessing == other.IsSuppressRouterSideProcessing &&
                   QueryRobustnessVariable == other.QueryRobustnessVariable &&
                   QueryInterval.Divide(2) <= other.QueryInterval && QueryInterval.Multiply(2) >= other.QueryInterval &&
                   SourceAddresses.SequenceEqual(other.SourceAddresses);
        }
    }
}