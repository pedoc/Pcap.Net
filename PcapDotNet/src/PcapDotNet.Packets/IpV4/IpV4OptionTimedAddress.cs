using System;
using PcapTools.Base;

namespace PcapTools.Packets.IpV4
{
    /// <summary>
    /// A pair of address and its time in the day.
    /// </summary>
    public struct IpV4OptionTimedAddress : IEquatable<IpV4OptionTimedAddress>
    {
        /// <summary>
        /// Create a timed address accroding to the given values.
        /// </summary>
        /// <param name="address">The address in the pair.</param>
        /// <param name="timeOfDay">The time passed since midnight UT.</param>
        public IpV4OptionTimedAddress(IpV4Address address, IpV4TimeOfDay timeOfDay)
        {
            _address = address;
            _timeOfDay = timeOfDay;
        }

        /// <summary>
        /// The address.
        /// </summary>
        public IpV4Address Address
        {
            get { return _address; }
        }

        /// <summary>
        /// The time passed since midnight UT.
        /// </summary>
        public IpV4TimeOfDay TimeOfDay
        {
            get { return _timeOfDay; }
        }

        /// <summary>
        /// Two options are equal if they have the same address and time passed since midnight UT.
        /// </summary>
        public bool Equals(IpV4OptionTimedAddress other)
        {
            return Address.Equals(other.Address) &&
                   TimeOfDay.Equals(other.TimeOfDay);
        }

        /// <summary>
        /// Two options are equal if they have the same address and time passed since midnight UT.
        /// </summary>
        public override bool Equals(object obj)
        {
            return (obj is IpV4OptionTimedAddress) &&
                    Equals((IpV4OptionTimedAddress)obj);
        }

        /// <summary>
        /// Two options are equal if they have the same address and time passed since midnight UT.
        /// </summary>
        public static bool operator ==(IpV4OptionTimedAddress value1, IpV4OptionTimedAddress value2)
        {
            return value1.Equals(value2);
        }

        /// <summary>
        /// Two options are different if they have different addresses or time passed since midnight UT.
        /// </summary>
        public static bool operator !=(IpV4OptionTimedAddress value1, IpV4OptionTimedAddress value2)
        {
            return !(value1 == value2);
        }

        /// <summary>
        /// Returns the xor of the address hash code and the time in the day hash code.
        /// </summary>
        public override int GetHashCode()
        {
            return Sequence.GetHashCode(_address, _timeOfDay);
        }

        private readonly IpV4Address _address;
        private readonly IpV4TimeOfDay _timeOfDay;
    }
}