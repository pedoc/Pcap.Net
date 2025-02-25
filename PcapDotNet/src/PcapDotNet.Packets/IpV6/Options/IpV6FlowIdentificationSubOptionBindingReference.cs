using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PcapTools.Base;

namespace PcapTools.Packets.IpV6.Options
{
    /// <summary>
    /// RFC 6089.
    /// <pre>
    /// +-----+--------------+-------------+
    /// | Bit | 0-7          | 8-15        |
    /// +-----+--------------+-------------+
    /// | 0   | Sub-Opt Type | Sub-Opt Len | 
    /// +-----+--------------+-------------+
    /// | 16  | BIDs                       |
    /// | ... |                            |
    /// +-----+----------------------------+
    /// </pre>
    /// </summary>
    [IpV6FlowIdentificationSubOptionTypeRegistration(IpV6FlowIdentificationSubOptionType.BindingReference)]
    public sealed class IpV6FlowIdentificationSubOptionBindingReference : IpV6FlowIdentificationSubOptionComplex
    {
        /// <summary>
        /// Creates an instance from a list of binding ids.
        /// </summary>
        /// <param name="bindingIds">
        /// Indicates the BIDs that the mobile node wants to associate with the flow identification option.
        /// One or more BID fields can be included in this sub-option.
        /// </param>
        public IpV6FlowIdentificationSubOptionBindingReference(IList<ushort> bindingIds)
            : this((ReadOnlyCollection<ushort>)bindingIds.AsReadOnly())
        {
        }

        /// <summary>
        /// Creates an instance from an array of binding ids.
        /// </summary>
        /// <param name="bindingIds">
        /// Indicates the BIDs that the mobile node wants to associate with the flow identification option.
        /// One or more BID fields can be included in this sub-option.
        /// </param>
        public IpV6FlowIdentificationSubOptionBindingReference(params ushort[] bindingIds)
            : this(bindingIds.AsReadOnly())
        {
        }

        /// <summary>
        /// Creates an instance from an enumerable of binding ids.
        /// </summary>
        /// <param name="bindingIds">
        /// Indicates the BIDs that the mobile node wants to associate with the flow identification option.
        /// One or more BID fields can be included in this sub-option.
        /// </param>
        public IpV6FlowIdentificationSubOptionBindingReference(IEnumerable<ushort> bindingIds)
            : this((IList<ushort>)bindingIds.ToList())
        {
        }

        /// <summary>
        /// Creates an instance from a collection of binding ids.
        /// </summary>
        /// <param name="bindingIds">
        /// Indicates the BIDs that the mobile node wants to associate with the flow identification option.
        /// One or more BID fields can be included in this sub-option.
        /// </param>
        public IpV6FlowIdentificationSubOptionBindingReference(ReadOnlyCollection<ushort> bindingIds)
            : base(IpV6FlowIdentificationSubOptionType.BindingReference)
        {
            BindingIds = bindingIds;
        }

        /// <summary>
        /// Indicates the BIDs that the mobile node wants to associate with the flow identification option.
        /// One or more BID fields can be included in this sub-option.
        /// </summary>
        public ReadOnlyCollection<ushort> BindingIds { get; private set; }

        internal override IpV6FlowIdentificationSubOption CreateInstance(DataSegment data)
        {
            if (data.Length % sizeof(ushort) != 0)
                return null;

            ushort[] bindingIds = new ushort[data.Length / sizeof(ushort)];
            for (int i = 0; i != bindingIds.Length; ++i)
                bindingIds[i] = data.ReadUShort(i * sizeof(ushort), Endianity.Big);
            return new IpV6FlowIdentificationSubOptionBindingReference(bindingIds);
        }

        internal override int DataLength
        {
            get { return BindingIds.Count * sizeof(ushort); }
        }

        internal override bool EqualsData(IpV6FlowIdentificationSubOption other)
        {
            return EqualsData(other as IpV6FlowIdentificationSubOptionBindingReference);
        }

        internal override object GetDataHashCode()
        {
            return BindingIds.UShortsSequenceGetHashCode();
        }

        internal override void WriteData(byte[] buffer, ref int offset)
        {
            foreach (ushort bindingId in BindingIds)
                buffer.Write(ref offset, bindingId, Endianity.Big);
        }

        private IpV6FlowIdentificationSubOptionBindingReference()
            : this(new ushort[0])
        {
        }

        private bool EqualsData(IpV6FlowIdentificationSubOptionBindingReference other)
        {
            return other != null &&
                   BindingIds.SequenceEqual(other.BindingIds);
        }
    }
}