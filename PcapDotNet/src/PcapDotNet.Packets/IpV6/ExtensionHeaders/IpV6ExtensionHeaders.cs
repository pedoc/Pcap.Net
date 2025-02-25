using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using PcapTools.Base;
using PcapTools.Packets.IpV4;

namespace PcapTools.Packets.IpV6.ExtensionHeaders
{
    /// <summary>
    /// List of IPv6 extension headers.
    /// </summary>
    public class IpV6ExtensionHeaders : IEnumerable<IpV6ExtensionHeader>, IEquatable<IpV6ExtensionHeaders>
    {
        /// <summary>
        /// Create an instance from a ReadOnlyCollection of extension headers.
        /// Verifies that there's at most one Encapsulating Security Payload extension header and that it is the last extension header.
        /// Assumes the collection won't be modified.
        /// </summary>
        /// <param name="extensionHeaders">The extension headers.</param>
        public IpV6ExtensionHeaders(ReadOnlyCollection<IpV6ExtensionHeader> extensionHeaders)
        {
            if (extensionHeaders == null) 
                throw new ArgumentNullException("extensionHeaders");
            for (int i = 0; i < extensionHeaders.Count; ++i)
            {
                IpV6ExtensionHeader extensionHeader = extensionHeaders[i];
                if (i != extensionHeaders.Count - 1)
                {
                    if (extensionHeader.Protocol == IpV4Protocol.EncapsulatingSecurityPayload)
                    {
                        throw new ArgumentException(
                            string.Format(CultureInfo.InvariantCulture,
                                          "EncapsulatingSecurityPayload can only be the last extension header. However it is the {0} out of {1}.", (i + 1),
                                          extensionHeaders.Count), "extensionHeaders");
                    }
                }
                BytesLength += extensionHeader.Length;
            }
            Headers = extensionHeaders;
            IsValid = true;
        }

        /// <summary>
        /// Create an instance from an array of extension headers.
        /// Verifies that there's at most one Encapsulating Security Payload extension header and that it is the last extension header.
        /// Assumes the collection won't be modified.
        /// </summary>
        /// <param name="extensionHeaders">The extension headers.</param>
        public IpV6ExtensionHeaders(params IpV6ExtensionHeader[] extensionHeaders)
            : this(extensionHeaders.AsReadOnly())
        {
        }

        /// <summary>
        /// Create an instance from a list of extension headers.
        /// Verifies that there's at most one Encapsulating Security Payload extension header and that it is the last extension header.
        /// Assumes the collection won't be modified.
        /// </summary>
        /// <param name="extensionHeaders">The extension headers.</param>
        public IpV6ExtensionHeaders(IList<IpV6ExtensionHeader> extensionHeaders)
            : this(extensionHeaders.AsReadOnly())
        {
        }

        /// <summary>
        /// Create an instance from an enumerable of extension headers.
        /// Verifies that there's at most one Encapsulating Security Payload extension header and that it is the last extension header.
        /// Assumes the collection won't be modified.
        /// </summary>
        /// <param name="extensionHeaders">The extension headers.</param>
        public IpV6ExtensionHeaders(IEnumerable<IpV6ExtensionHeader> extensionHeaders)
            : this((IpV6ExtensionHeader[])extensionHeaders.ToArray())
        {
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<IpV6ExtensionHeader> GetEnumerator()
        {
            return Headers.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns the extension header in the 'index' place.
        /// </summary>
        /// <param name="index">The index of the extension header returned.</param>
        /// <returns>The extension header in the 'index' place.</returns>
        public IpV6ExtensionHeader this[int index]
        {
            get { return Headers[index]; }
        }

        /// <summary>
        /// The extension headers.
        /// </summary>
        public ReadOnlyCollection<IpV6ExtensionHeader> Headers { get; private set; }

        /// <summary>
        /// The total number of bytes the extension headers take.
        /// </summary>
        public int BytesLength { get; private set; }

        /// <summary>
        /// True iff a parsing issue wasn't encountered when parsing the extension headers.
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// The protocol of the first extension header or null if there are no extension headers.
        /// </summary>
        public IpV4Protocol? FirstHeader
        {
            get
            {
                if (!Headers.Any()) 
                    return null;
                return Headers[0].Protocol;
            }
        }

        /// <summary>
        /// The protocol of the last extension header or null if there are no extension headers.
        /// </summary>
        public IpV4Protocol? LastHeader
        {
            get
            {
                if (!Headers.Any())
                    return null;
                return Headers[Headers.Count - 1].Protocol;
            }
        }

        /// <summary>
        /// The next header of the last extension header or null if there are no extension headers.
        /// </summary>
        public IpV4Protocol? NextHeader
        {
            get
            {
                if (!Headers.Any())
                    return null;
                return Headers[Headers.Count - 1].NextHeader;
            }
        }

        /// <summary>
        /// An empty list of extension headers.
        /// </summary>
        public static IpV6ExtensionHeaders Empty
        {
            get { return _empty; }
        }

        /// <summary>
        /// True iff all the extension headers are equal to the given extension headers instance.
        /// </summary>
        public sealed override bool Equals(object obj)
        {
            return Equals(obj as IpV6ExtensionHeaders);
        }

        /// <summary>
        /// True iff all the extension headers are equal to the given extension headers instance.
        /// </summary>
        public bool Equals(IpV6ExtensionHeaders other)
        {
            return other != null && this.SequenceEqual(other);
        }

        /// <summary>
        /// A hash code based on all the extension headers.
        /// </summary>
        public override int GetHashCode()
        {
            return this.SequenceGetHashCode();
        }

        internal IpV6ExtensionHeaders(DataSegment data, IpV4Protocol firstHeader)
        {
            BytesLength = 0;
            IpV4Protocol? nextHeader = firstHeader;
            List<IpV6ExtensionHeader> headers = new List<IpV6ExtensionHeader>();
            while (data.Length >= 8 && nextHeader.HasValue && IpV6ExtensionHeader.IsExtensionHeader(nextHeader.Value))
            {
                int numBytesRead;
                IpV6ExtensionHeader extensionHeader = IpV6ExtensionHeader.CreateInstance(nextHeader.Value, data, out numBytesRead);
                if (extensionHeader == null)
                    break;
                BytesLength += numBytesRead;
                headers.Add(extensionHeader);
                nextHeader = extensionHeader.NextHeader;
                data = data.Subsegment(numBytesRead, data.Length - numBytesRead);
            }
            Headers = headers.AsReadOnly();
            IsValid = (!nextHeader.HasValue || !IpV6ExtensionHeader.IsExtensionHeader(nextHeader.Value)) && headers.All(header => header.IsValid);
        }

        internal void Write(byte[] buffer, int offset, IpV4Protocol? nextLayerProtocol)
        {
            for (int i = 0; i != Headers.Count; ++i)
            {
                IpV6ExtensionHeader extensionHeader = Headers[i];
                IpV4Protocol nextHeader;
                if (extensionHeader.NextHeader != null)
                    nextHeader = extensionHeader.NextHeader.Value;
                else if (i < Headers.Count - 1)
                    nextHeader = Headers[i + 1].Protocol;
                else if (nextLayerProtocol.HasValue)
                    nextHeader = nextLayerProtocol.Value;
                else if (extensionHeader.Protocol == IpV4Protocol.EncapsulatingSecurityPayload)
                    nextHeader = IpV4Protocol.NoNextHeaderForIpV6;  // Arbitrary.
                else
                    throw new InvalidOperationException("Can't determine extension header next header value. It is the last extension header with no known next layer protocol.");

                extensionHeader.Write(buffer, ref offset, nextHeader);
            }
        }

        private static readonly IpV6ExtensionHeaders _empty = new IpV6ExtensionHeaders();
    }
}