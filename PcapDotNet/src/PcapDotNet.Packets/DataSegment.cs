﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using PcapTools.Base;
using PcapTools.Packets.Ethernet;
using PcapTools.Packets.IpV4;
using PcapTools.Packets.IpV6;

namespace PcapTools.Packets
{
    /// <summary>
    /// Represents segement of a byte array.
    /// Never copies the given buffer.
    /// </summary>
    public class DataSegment : IEquatable<DataSegment>, IEnumerable<byte>
    {
        /// <summary>
        /// Take all the bytes as a segment.
        /// </summary>
        /// <param name="buffer">The buffer to take as a segment.</param>
        public DataSegment(byte[] buffer) : this(buffer, 0 , buffer == null ? 0 : buffer.Length) {}

        /// <summary>
        /// Take only part of the bytes as a segment.
        /// </summary>
        /// <param name="buffer">The bytes to take the segment from.</param>
        /// <param name="offset">The offset in the buffer to start taking the bytes from.</param>
        /// <param name="length">The number of bytes to take.</param>
        public DataSegment(byte[] buffer, int offset, int length)
        {
            if (buffer == null) 
                throw new ArgumentNullException("buffer");

            Buffer = buffer;
            StartOffset = offset;
            Length = length;
        }

        /// <summary>
        /// The number of bytes in this segment.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// The value of the byte in the given offset in the segment.
        /// </summary>
        /// <param name="offset">The offset in the segment to take the byte from.</param>
        public byte this[int offset]
        {
            get { return Buffer[StartOffset + offset]; }
        }

        /// <summary>
        /// Returns the last byte of the segment.
        /// </summary>
        public byte Last { get { return this[Length - 1]; } }

        /// <summary>
        /// Creates a subsegment starting from a given offset in the segment taking a given number of bytes.
        /// </summary>
        /// <param name="offset">The offset in the segment to start taking.</param>
        /// <param name="length">The number of bytes to take from the segment.</param>
        /// <returns>A new DataSegment that is part of the given DataSegment.</returns>
        public DataSegment Subsegment(int offset, int length)
        {
            return Subsegment(ref offset, length);
        }

        /// <summary>
        /// Returns the Segment's bytes as a read only MemoryStream with a non-public buffer.
        /// </summary>
        /// <returns>A read only MemoryStream containing the bytes of the segment.</returns>
        public MemoryStream ToMemoryStream()
        {
            return new MemoryStream(Buffer, StartOffset, Length, false, false);
        }

        /// <summary>
        /// Iterate through all the bytes in the segment.
        /// </summary>
        public IEnumerator<byte> GetEnumerator()
        {
            for (int i = 0; i != Length; ++i)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Two segments are equal if they have the same data.
        /// </summary>
        public bool Equals(DataSegment other)
        {
            if (other == null || Length != other.Length)
                return false;

            for (int i = 0; i != Length; ++i)
            {
                if (this[i] != other[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Two segments are equal if they have the same data.
        /// </summary>
        public sealed override bool Equals(object obj)
        {
            return Equals(obj as DataSegment);
        }

        /// <summary>
        /// The hash code of a segment is the hash code of its length xored with all its bytes (each byte is xored with the next byte in the integer).
        /// </summary>
        public sealed override int GetHashCode()
        {
            return Length.GetHashCode() ^ this.BytesSequenceGetHashCode();
        }

        /// <summary>
        /// Creates a string starting with the number of bytes the data segment contains 
        /// and ending with the first 10 bytes of the data segment as hexadecimal digits.
        /// </summary>
        /// <returns>
        /// A string starting with the number of bytes the data segment contains 
        /// and ending with the first 10 bytes of the data segment as hexadecimal digits.
        /// </returns>
        public sealed override string ToString()
        {
            const int MaxNumBytesToUse = 10;
            return string.Format(CultureInfo.InvariantCulture, "{0} bytes: {1}{2}", Length,
                                 Buffer.Range(StartOffset, Math.Min(Length, MaxNumBytesToUse)).BytesSequenceToHexadecimalString(),
                                 (Length > MaxNumBytesToUse ? "..." : ""));
        }

        /// <summary>
        /// Converts the segment to a hexadecimal string representing every byte as two hexadecimal digits.
        /// </summary>
        /// <returns>A hexadecimal string representing every byte as two hexadecimal digits.</returns>
        public string ToHexadecimalString()
        {
            return Buffer.Range(StartOffset, Length).BytesSequenceToHexadecimalString();
        }

        /// <summary>
        /// Converts the segment to a string using the given encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use to convert the bytes sequence in the segment to a string.</param>
        /// <returns>A string of the bytes in the segment converted using the given encoding.</returns>
        public string Decode(Encoding encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            return encoding.GetString(Buffer, StartOffset, Length);
        }

        /// <summary>
        /// An empty DataSegment.
        /// </summary>
        public static DataSegment Empty { get { return _empty; } }

        internal void Write(byte[] buffer, ref int offset)
        {
            Buffer.BlockCopy(StartOffset, buffer, offset, Length);
            offset += Length;
        }

        internal void Write(byte[] buffer, int offset)
        {
            Write(buffer, ref offset);
        }

        /// <summary>
        /// The original buffer that holds all the data for the segment.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        internal byte[] Buffer { get; private set; }

        /// <summary>
        /// The offset of the first byte of the segment in the buffer.
        /// </summary>
        internal int StartOffset { get; private set; }

        /// <summary>
        /// Reads a requested number of bytes from a specific offset in the segment.
        /// </summary>
        /// <param name="offset">The offset in the segment to start reading.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The bytes read from the segment starting from the given offset and in the given length.</returns>
        internal byte[] ReadBytes(int offset, int length)
        {
            return Buffer.ReadBytes(StartOffset + offset, length);
        }

        internal DataSegment Subsegment(ref int offset, int length)
        {
            DataSegment subSegemnt = new DataSegment(Buffer, StartOffset + offset, length);
            offset += length;
            return subSegemnt;
        }

        internal bool ReadBool(int offset, byte mask)
        {
            return (this[offset] & mask) == mask;
        }

        /// <summary>
        /// Reads 2 bytes from a specific offset in the segment as a ushort with a given endianity.
        /// </summary>
        /// <param name="offset">The offset in the segment to start reading.</param>
        /// <param name="endianity">The endianity to use to translate the bytes to the value.</param>
        /// <returns>The value converted from the read bytes according to the endianity.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "ushort")]
        internal ushort ReadUShort(int offset, Endianity endianity)
        {
            return Buffer.ReadUShort(StartOffset + offset, endianity);
        }

        /// <summary>
        /// Reads 3 bytes from a specific offset in the segment as a UInt24 with a given endianity.
        /// </summary>
        /// <param name="offset">The offset in the segment to start reading.</param>
        /// <param name="endianity">The endianity to use to translate the bytes to the value.</param>
        /// <returns>The value converted from the read bytes according to the endianity.</returns>
        internal UInt24 ReadUInt24(int offset, Endianity endianity)
        {
            return Buffer.ReadUInt24(StartOffset + offset, endianity);
        }

        /// <summary>
        /// Reads 4 bytes from a specific offset in the segment as an int with a given endianity.
        /// </summary>
        /// <param name="offset">The offset in the segment to start reading.</param>
        /// <param name="endianity">The endianity to use to translate the bytes to the value.</param>
        /// <returns>The value converted from the read bytes according to the endianity.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int")]
        internal int ReadInt(int offset, Endianity endianity)
        {
            return Buffer.ReadInt(StartOffset + offset, endianity);
        }

        /// <summary>
        /// Reads 4 bytes from a specific offset in the segment as a uint with a given endianity.
        /// </summary>
        /// <param name="offset">The offset in the segment to start reading.</param>
        /// <param name="endianity">The endianity to use to translate the bytes to the value.</param>
        /// <returns>The value converted from the read bytes according to the endianity.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "uint")]
        internal uint ReadUInt(int offset, Endianity endianity)
        {
            return Buffer.ReadUInt(StartOffset + offset, endianity);
        }

        /// <summary>
        /// Reads 6 bytes from a specific offset in the segment as a UInt48 with a given endianity.
        /// </summary>
        /// <param name="offset">The offset in the segment to start reading.</param>
        /// <param name="endianity">The endianity to use to translate the bytes to the value.</param>
        /// <returns>The value converted from the read bytes according to the endianity.</returns>
        internal UInt48 ReadUInt48(int offset, Endianity endianity)
        {
            return Buffer.ReadUInt48(StartOffset + offset, endianity);
        }

        internal ulong ReadULong(int offset, Endianity endianity)
        {
            return Buffer.ReadULong(StartOffset + offset, endianity);
        }

        internal BigInteger ReadUnsignedBigInteger(int offset, int length, Endianity endianity)
        {
            return Buffer.ReadUnsignedBigInteger(StartOffset + offset, length, endianity);
        }

        /// <summary>
        /// Reads 6 bytes from a specific offset in the segment as a MacAddress with a given endianity.
        /// </summary>
        /// <param name="offset">The offset in the segment to start reading.</param>
        /// <param name="endianity">The endianity to use to translate the bytes to the value.</param>
        /// <returns>The value converted from the read bytes according to the endianity.</returns>
        protected MacAddress ReadMacAddress(int offset, Endianity endianity)
        {
            return Buffer.ReadMacAddress(StartOffset + offset, endianity);
        }

        /// <summary>
        /// Reads 4 bytes from a specific offset in the segment as an IpV4Address with a given endianity.
        /// </summary>
        /// <param name="offset">The offset in the segment to start reading.</param>
        /// <param name="endianity">The endianity to use to translate the bytes to the value.</param>
        /// <returns>The value converted from the read bytes according to the endianity.</returns>
        internal IpV4Address ReadIpV4Address(int offset, Endianity endianity)
        {
            return Buffer.ReadIpV4Address(StartOffset + offset, endianity);
        }

        /// <summary>
        /// Reads 4 bytes from a specific offset in the segment as an IpV4TimeOfDay with a given endianity.
        /// </summary>
        /// <param name="offset">The offset in the segment to start reading.</param>
        /// <param name="endianity">The endianity to use to translate the bytes to the value.</param>
        /// <returns>The value converted from the read bytes according to the endianity.</returns>
        protected IpV4TimeOfDay ReadIpV4TimeOfDay(int offset, Endianity endianity)
        {
            return Buffer.ReadIpV4TimeOfDay(StartOffset + offset, endianity);
        }

        /// <summary>
        /// Reads 4 bytes from a specific offset in the segment as an IpV4Address with a given endianity.
        /// </summary>
        /// <param name="offset">The offset in the segment to start reading.</param>
        /// <param name="endianity">The endianity to use to translate the bytes to the value.</param>
        /// <returns>The value converted from the read bytes according to the endianity.</returns>
        internal IpV6Address ReadIpV6Address(int offset, Endianity endianity)
        {
            return Buffer.ReadIpV6Address(StartOffset + offset, endianity);
        }

        internal uint Sum16Bits()
        {
            return Sum16Bits(Buffer, StartOffset, Length);
        }

        /// <summary>
        /// Converts the given 16 bits sum to a checksum.
        /// Sums the two 16 bits in the 32 bits value and if the result is bigger than a 16 bits value repeat.
        /// The result is one's complemented and the least significant 16 bits are taken.
        /// </summary>
        /// <param name="sum"></param>
        /// <returns></returns>
        internal static ushort Sum16BitsToChecksum(uint sum)
        {
            // Take only 16 bits out of the 32 bit sum and add up the carrier.
            // if the results overflows - do it again.
            while (sum > 0xFFFF)
                sum = (sum & 0xFFFF) + (sum >> 16);

            // one's complement the result
            sum = ~sum;

            return (ushort)sum;
        }

        /// <summary>
        /// Sums bytes in a buffer as 16 bits big endian values.
        /// If the number of bytes is odd then a 0x00 value is assumed after the last byte.
        /// Used to calculate checksum.
        /// </summary>
        /// <param name="buffer">The buffer to take the bytes from.</param>
        /// <param name="offset">The offset in the buffer to start reading the bytes.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>A value equals to the sum of all 16 bits big endian values of the given bytes.</returns>
        protected static uint Sum16Bits(byte[] buffer, int offset, int length)
        {
            if (buffer == null) 
                throw new ArgumentNullException("buffer");

            int endOffset = offset + length;
            uint sum = 0;
            while (offset < endOffset - 1)
                sum += buffer.ReadUShort(ref offset, Endianity.Big);
            if (offset < endOffset)
                sum += (ushort)(buffer[offset] << 8);
            return sum;
        }

        internal static uint Sum16Bits(IpV6Address address)
        {
            return Sum16Bits(address.ToValue());
        }

        internal static uint Sum16Bits(IpV4Address address)
        {
            return Sum16Bits(address.ToValue());
        }


        internal static uint Sum16Bits(UInt128 value)
        {
            return Sum16Bits((ulong)(value >> 64)) + Sum16Bits((ulong)value);
        }

        internal static uint Sum16Bits(ulong value)
        {
            return Sum16Bits((uint)(value >> 32)) + Sum16Bits((uint)value);
        }

        internal static uint Sum16Bits(uint value)
        {
            return (value >> 16) + (value & 0xFFFF);
        }

        private static readonly DataSegment _empty = new DataSegment(new byte[0]);
    }
}
