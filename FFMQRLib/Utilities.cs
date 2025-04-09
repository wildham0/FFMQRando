using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using RomUtilities;
using System.Dynamic;
using System.Runtime.CompilerServices;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Serialization;



namespace FFMQLib
{
    public static class Utilities
    {
        public static T PickFrom<T>(this MT19337 rng, IList<T> list)
        {
            return list[rng.Between(0, list.Count - 1)];
        }

        public static T TakeFrom<T>(this MT19337 rng, IList<T> list)
        {
            var value = rng.PickFrom(list);
            list.Remove(value);
            return value;
        }

		public static bool IsEmpty<T>(this IList<T> list) => list.Count == 0;

		public static string GetDescription(this Enum value)
        {
            // Get the Description attribute value for the enum value
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

		[DebuggerStepThrough]
		public static bool TryFind<T>(this IList<T> fromList, Predicate<T> query, out T result)
		{
			int resultIndex = fromList.ToList().FindIndex(query);
			if (resultIndex < 0)
			{
				result = default;
				return false;
			}
			else
			{
				result = fromList[resultIndex];
				return true;
			}
		}
		public static Blob Reverse(this Blob orderedBlob)
        {
            Blob _reversedBlob = new byte[orderedBlob.Length];
            for (int i = 0; i < orderedBlob.Length; i++)
            {
                _reversedBlob[i] = orderedBlob[(orderedBlob.Length - 1) - i];
            }

            return _reversedBlob;
        }

        public class ByteAddress
        {

            private byte _lowbyte;
            private byte _highbyte;
            private byte _bank;
        
            public ByteAddress(byte[] address, bool littleEndian = false)
            {
                GetAddressFromBytes(address, littleEndian);
            }
            public ByteAddress(string stringaddress, bool littleEndian = false)
            {
                var address = Blob.FromHex(stringaddress);
                GetAddressFromBytes(address, littleEndian);
            }
            public ByteAddress(int address, bool littleEndian = false)
            {
                if (address >= 0x100 * 0x100 * 0x100)
                {
                    throw new Exception("Address is too long");
                }
                else if (address >= 0x100 * 0x100)
                {
                    if (littleEndian)
                    {
                        _lowbyte = (byte)(address / (0x100 * 0x100));
                        _highbyte = (byte)((address / 0x100) % 0x100);
                        _bank = (byte)(address % (0x100 * 0x100));
                    }
                    else
                    {
                        _lowbyte = (byte)(address % (0x100 * 0x100));
                        _highbyte = (byte)((address / 0x100) % 0x100);
                        _bank = (byte)(address / (0x100 * 0x100));
                    }
                }
                else if (address >= 0x100)
                {
                    if (littleEndian)
                    {
                        _lowbyte = (byte)(address / 0x100);
                        _highbyte = (byte)(address % 0x100);
                        _bank = 0x00;
                    }
                    else
                    {
                        _lowbyte = (byte)(address % 0x100);
                        _highbyte = (byte)(address / 0x100);
                        _bank = 0x00;
                    }
                }
                else
                {
                    throw new Exception("Address is too short");
                }
            }

            public byte[] ToBytes(bool includeBank, bool littleEndian)
            {
                if (littleEndian)
                {
                    byte[] addressarray = new byte[] { _lowbyte, _highbyte };

                    if (includeBank)
                    {
                        addressarray.Append(_bank);
                    }

                    return addressarray;
                }
                else
                {
                    byte[] addressarray = new byte[] { _highbyte, _lowbyte };

                    if (includeBank)
                    {
                        addressarray.Prepend(_bank);
                    }

                    return addressarray;
                }
            }
            public int ToInt(bool includeBank, bool littleEndian)
            {
                if (littleEndian)
                {
                    int addressarray = (_lowbyte * 0x100) + _highbyte;

                    if (includeBank)
                    {
                        addressarray = (addressarray * 0x100) + _bank;
                    }

                    return addressarray;
                }
                else
                {
                    int addressarray = (_highbyte * 0x100) + _lowbyte;

                    if (includeBank)
                    {
                        addressarray = (_bank * 0x100 * 0x100) + addressarray;
                    }

                    return addressarray;
                }
            }
            public string ToString(bool includeBank, bool littleEndian)
            {
                if (littleEndian)
                {
                    string addressarray = _lowbyte.ToString("X2") + _highbyte.ToString("X2");

                    if (includeBank)
                    {
                        addressarray +=  _bank.ToString("X2");
                    }

                    return addressarray;
                }
                else
                {
                    string addressarray = _highbyte.ToString("X2") + _lowbyte.ToString("X2");

                    if (includeBank)
                    {
                        addressarray = _bank.ToString("X2") + addressarray;
                    }

                    return addressarray;
                }
            }
            private void GetAddressFromBytes(byte[] address, bool littleEndian)
            {
                if (address.Length > 3)
                {
                    throw new Exception("Address is too long");
                }
                else if (address.Length == 3)
                {
                    if (littleEndian)
                    {
                        _lowbyte = address[0];
                        _highbyte = address[1];
                        _bank = address[2];
                    }
                    else
                    {
                        _lowbyte = address[2];
                        _highbyte = address[1];
                        _bank = address[0];
                    }
                }
                else if (address.Length == 2)
                {
                    if (littleEndian)
                    {
                        _lowbyte = address[0];
                        _highbyte = address[1];
                        _bank = 0x00;
                    }
                    else
                    {
                        _lowbyte = address[1];
                        _highbyte = address[0];
                        _bank = 0x00;
                    }
                }
                else
                {
                    throw new Exception("Address is too short");
                }
            }

        }
    }

    class FlowStyleIntegerSequences : ChainedEventEmitter
    {
        public FlowStyleIntegerSequences(IEventEmitter nextEmitter)
            : base(nextEmitter) { }

        public override void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter)
        {
            if (typeof(IEnumerable<int>).IsAssignableFrom(eventInfo.Source.Type))
            {
                eventInfo = new SequenceStartEventInfo(eventInfo.Source)
                {
                    Style = SequenceStyle.Flow
                };
            }

            nextEmitter.Emit(eventInfo, emitter);
        }
    }
}
