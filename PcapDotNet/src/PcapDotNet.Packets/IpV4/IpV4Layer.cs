using System;
using PcapTools.Base;
using PcapTools.Packets.Ethernet;
using PcapTools.Packets.Ip;

namespace PcapTools.Packets.IpV4
{
    /// <summary>
    /// Represents IPv4 layer.
    /// <seealso cref="IpV4Datagram"/>
    /// </summary>
    public sealed class IpV4Layer : Layer, IEthernetNextLayer, IIpNextLayer
    {
        /// <summary>
        /// Creates an IPv4 layer with all zero values.
        /// </summary>
        public IpV4Layer()
        {
            TypeOfService = 0;
            Identification = 0;
            Fragmentation = IpV4Fragmentation.None;
            Ttl = 0;
            Protocol = null;
            HeaderChecksum = null;
            Source = IpV4Address.Zero;
            CurrentDestination = IpV4Address.Zero;
            Options = IpV4Options.None;
        }

        /// <summary>
        /// Type of Service field.
        /// </summary>
        public byte TypeOfService { get; set; }

        /// <summary>
        /// The value of the IPv4 ID field.
        /// </summary>
        public ushort Identification { get; set; }

        /// <summary>
        /// The fragmentation information field.
        /// </summary>
        public IpV4Fragmentation Fragmentation { get; set; }

        /// <summary>
        /// The TTL field.
        /// </summary>
        public byte Ttl { get; set; }

        /// <summary>
        /// The IPv4 (next) protocol field.
        /// null means that this value should be calculated according to the next layer.
        /// </summary>
        public IpV4Protocol? Protocol { get; set; }

        /// <summary>
        /// The header checksum value.
        /// null means that this value should be calculated to be correct according to the data.
        /// </summary>
        public ushort? HeaderChecksum { get; set; }

        /// <summary>
        /// The source address.
        /// </summary>
        public IpV4Address Source { get; set; }

        /// <summary>
        /// The current destination address.
        /// This might not be the final destination when source routing options exist.
        /// </summary>
        public IpV4Address CurrentDestination { get; set; }

        /// <summary>
        /// The final destination address.
        /// Takes into account the current destination and source routing options if they exist.
        /// </summary>
        public IpV4Address Destination
        {
            get { return IpV4Datagram.CalculateDestination(CurrentDestination, Options); }
        }

        /// <summary>
        /// The options field with all the parsed options if any exist.
        /// </summary>
        public IpV4Options Options { get; set; }

        /// <summary>
        /// The Ethernet Type the Ethernet layer should write when this layer is the Ethernet payload.
        /// </summary>
        public EthernetType PreviousLayerEtherType
        {
            get { return EthernetType.IpV4; }
        }

        /// <summary>
        /// The protocol that should be written in the previous (IPv4) layer (in this case: IP).
        /// </summary>
        public IpV4Protocol PreviousLayerProtocol
        {
            get { return IpV4Protocol.Ip; }
        }

        /// <summary>
        /// The default MAC Address value when this layer is the Ethernet payload.
        /// null means there is no default value.
        /// </summary>
        public MacAddress? PreviousLayerDefaultDestination
        {
            get { return null; }
        }

        /// <summary>
        /// The number of bytes this layer will take.
        /// </summary>
        public override int Length
        {
            get { return IpV4Datagram.HeaderMinimumLength + Options.BytesLength; }
        }

        /// <summary>
        /// Writes the layer to the buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write the layer to.</param>
        /// <param name="offset">The offset in the buffer to start writing the layer at.</param>
        /// <param name="payloadLength">The length of the layer's payload (the number of bytes after the layer in the packet).</param>
        /// <param name="previousLayer">The layer that comes before this layer. null if this is the first layer.</param>
        /// <param name="nextLayer">The layer that comes after this layer. null if this is the last layer.</param>
        public override void Write(byte[] buffer, int offset, int payloadLength, ILayer previousLayer, ILayer nextLayer)
        {
            IpV4Protocol protocol;
            if (Protocol == null)
            {
                if (nextLayer == null)
                    throw new ArgumentException("Can't determine protocol automatically from next layer because there is no next layer");
                IIpNextLayer ipNextLayer = nextLayer as IIpNextLayer;
                if (ipNextLayer == null)
                    throw new ArgumentException("Can't determine protocol automatically from next layer (" + nextLayer.GetType() + ")");
                protocol = ipNextLayer.PreviousLayerProtocol;
            }
            else
                protocol = Protocol.Value;

            IpV4Datagram.WriteHeader(buffer, offset,
                                     TypeOfService, Identification, Fragmentation,
                                     Ttl, protocol, HeaderChecksum,
                                     Source, CurrentDestination,
                                     Options, payloadLength);
        }

        /// <summary>
        /// Finalizes the layer data in the buffer.
        /// Used for fields that must be calculated according to the layer's payload (like checksum).
        /// </summary>
        /// <param name="buffer">The buffer to finalize the layer in.</param>
        /// <param name="offset">The offset in the buffer the layer starts.</param>
        /// <param name="payloadLength">The length of the layer's payload (the number of bytes after the layer in the packet).</param>
        /// <param name="nextLayer">The layer that comes after this layer. null if this is the last layer.</param>
        public override void Finalize(byte[] buffer, int offset, int payloadLength, ILayer nextLayer)
        {
            IIpNextTransportLayer nextTransportLayer = nextLayer as IIpNextTransportLayer;
            if (nextTransportLayer == null || !nextTransportLayer.CalculateChecksum)
                return;

            IpV4Datagram.WriteTransportChecksum(buffer, offset, Length, (ushort)payloadLength,
                                                nextTransportLayer.ChecksumOffset, nextTransportLayer.IsChecksumOptional,
                                                nextTransportLayer.Checksum, Destination);
        }

        /// <summary>
        /// The kind of the data link of the layer.
        /// Can be null if this is not the first layer in the packet.
        /// </summary>
        public override DataLinkKind? DataLink
        {
            get { return DataLinkKind.IpV4; }
        }

        /// <summary>
        /// True iff the two IPv4 layers have the same TypeOfService, Identification, Fragmentation, Ttl, Protocol, HeaderChecksum, Source, Destination and Options.
        /// </summary>
        public bool Equals(IpV4Layer other)
        {
            return other != null &&
                   TypeOfService == other.TypeOfService && Identification == other.Identification &&
                   Fragmentation == other.Fragmentation && Ttl == other.Ttl &&
                   Protocol == other.Protocol &&
                   HeaderChecksum == other.HeaderChecksum &&
                   Source == other.Source && CurrentDestination == other.CurrentDestination &&
                   Options.Equals(other.Options);
        }

        /// <summary>
        /// True iff the two IPv4 layers have the same TypeOfService, Identification, Fragmentation, Ttl, Protocol, HeaderChecksum, Source, Destination and Options.
        /// </summary>
        public override bool Equals(Layer other)
        {
            return Equals(other as IpV4Layer);
        }

        /// <summary>
        /// Returns a hash code for the layer.
        /// The hash code is a XOR of the TypeOfService and Identification combined and the hash codes of the layer length, data link, Fragmentation, Protocol, HeaderChecksum, Source, Destination, Options.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^
                   Sequence.GetHashCode(BitSequence.Merge(TypeOfService, Identification, Ttl),
                                        Fragmentation, Source, CurrentDestination, Options) ^ Protocol.GetHashCode() ^ HeaderChecksum.GetHashCode();
        }

        /// <summary>
        /// Contains the Source, Destination and Protocol.
        /// </summary>
        public override string ToString()
        {
            return Source + " -> " + Destination + " (" + Protocol + ")";
        }
    }
}