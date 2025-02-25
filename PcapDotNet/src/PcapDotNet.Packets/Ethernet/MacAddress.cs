using System;
using System.Globalization;
using PcapTools.Base;

namespace PcapTools.Packets.Ethernet
{
    /// <summary>
    /// Ethernet MacAddress struct.
    /// </summary>
    public struct MacAddress : IEquatable<MacAddress>
    {
        /// <summary>
        /// The number of bytes the struct takes.
        /// </summary>
        public const int SizeOf = UInt48.SizeOf;

        /// <summary>
        /// A MAC Address of all zeros (00:00:00:00:00:00).
        /// </summary>
        public static MacAddress Zero 
        {
            get { return _zero; }
        }

        /// <summary>
        /// Constructs the address from a 48 bit integer.
        /// </summary>
        /// <param name="value">The 48 bit integer to create the address from.</param>
        public MacAddress(UInt48 value)
        {
            _value = value;
        }

        /// <summary>
        /// Create the address from a string in the format XX:XX:XX:XX:XX:XX.
        /// </summary>
        /// <param name="address">The string value in hexadecimal format. Every two digits are separated by a colon.</param>
        public MacAddress(string address)
        {
            if (address == null) 
                throw new ArgumentNullException("address");

            string[] hexes = address.Split(':');
            if (hexes.Length != 6)
                throw new ArgumentException("Failed parsing " + address + " as mac address. Expected 6 hexes and got " + hexes.Length + " hexes", "address");

            _value = BitSequence.Merge(Convert.ToByte(hexes[0], 16), Convert.ToByte(hexes[1], 16), Convert.ToByte(hexes[2], 16),
                                       Convert.ToByte(hexes[3], 16), Convert.ToByte(hexes[4], 16), Convert.ToByte(hexes[5], 16));
        }

        /// <summary>
        /// Converts the address to a 48 bit integer.
        /// </summary>
        /// <returns>A 48 bit integer representing the address.</returns>
        public UInt48 ToValue()
        {
            return _value;
        }

        /// <summary>
        /// Two addresses are equal if they have the exact same value.
        /// </summary>
        public bool Equals(MacAddress other)
        {
            return _value == other._value;
        }

        /// <summary>
        /// Two addresses are equal if they have the exact same value.
        /// </summary>
        public override bool Equals(object obj)
        {
            return (obj is MacAddress &&
                    Equals((MacAddress)obj));
        }

        /// <summary>
        /// Two addresses are equal if they have the exact same value.
        /// </summary>
        public static bool operator ==(MacAddress macAddress1, MacAddress macAddress2)
        {
            return macAddress1.Equals(macAddress2);
        }

        /// <summary>
        /// Two addresses are different if they have different values.
        /// </summary>
        public static bool operator !=(MacAddress macAddress1, MacAddress macAddress2)
        {
            return !(macAddress1 == macAddress2);
        }

        /// <summary>
        /// The hash code of the address is the hash code of its 48 bit integer value.
        /// </summary>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <summary>
        /// Converts the address to a string in the format XX:XX:XX:XX:XX:XX.
        /// </summary>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}",
                                 (byte)(_value >> 40),
                                 (byte)(_value >> 32),
                                 (byte)(_value >> 24),
                                 (byte)(_value >> 16),
                                 (byte)(_value >> 8),
                                 (byte)(_value));
        }

        private static readonly MacAddress _zero = new MacAddress(0);
        private readonly UInt48 _value;
    }
}