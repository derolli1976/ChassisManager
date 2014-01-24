// Copyright © Microsoft Open Technologies, Inc.
// All Rights Reserved
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0 

// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR
// CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 
// See the Apache 2 License for the specific language governing permissions and limitations under the License. 

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    using System;
    using System.Text;
    using System.Collections;
    using System.Reflection;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Security.Cryptography;

    internal static class IpmiSharedFunc
    {

    #region Shared Functions

        /// <summary>
        /// Detect byte array patters in a byte array
        /// </summary>
        internal static int GetInstance(byte[] payload, byte[] pattern)
        {
            for (int i = 0; i < payload.Length; i++)
            {
                if (pattern[0] == payload[i] && pattern[1] == payload[i + 1])
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Detect byte patters in a byte array
        /// </summary>
        internal static int GetInstance(byte[] payload, byte pattern)
        {
            return Array.IndexOf(payload, pattern);
        }
            
        /// <summary>
        /// Detect byte patters in a byte array
        /// </summary>
        internal static int GetInstance(byte[] payload, byte pattern, int index)
        {
            return Array.IndexOf(payload, pattern, index);
        } 

        /// <summary>
        /// Splits a single byte into multiple bytes.  This function
        /// uses bitshifting to extract multiple bytes from a single byte
        /// given the positions to extract.
        /// </summary>  
        internal static byte[] ByteSplit(byte input, int[] positions)
        {
            // return object.
            byte[] response = new byte[positions.Length];

            // bit offset.  default zero.
            int offset = 0;

            // maximum bit per byte.
            int maxbit = 8;

            for (int position = 0; position < response.Length; position++)
            {
                // (A) bit shift the offset to clear the left most bits.
                response[position] = (byte)(input << offset);
                // (B) bit shift the offset to return to (A) position
                response[position] = (byte)(response[position] >> offset);
                // (C) shift right to the position, clearing right most bits.
                response[position] = (byte)(response[position] >> positions[position]);

                // offset should be the maximum bit position, minus the previous position.
                offset = (maxbit - positions[position]);
            }

            return response;
        }

        /// <summary>
        /// Decoder for BCD plus encoding.
        /// BCD PLUS definition:
        ///     0h - 9h = digits 0 through 9
        ///     Ah = space
        ///     Bh = dash '-'
        ///     Ch = period '.'
        ///     Dh = reserved
        ///     Eh = reserved
        ///     Fh = reserved
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static string DecodeBcdPlus(byte[] data)
        {
            // Represent the reserved characters as spaces, they should not show anyway
            const string BCDPlusAlphabet = "01234567890 -.  ";
            StringBuilder decodedString = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                // BUGBUG: The Fru IPMI spec does not say anything about the BCD encoding
                // happening on each byte or on each nibblet. Since they seem focused on compacting, we assume
                // one per nibblet.
                decodedString.Append(BCDPlusAlphabet[(int)(data[i] >> 4)]);
                decodedString.Append(BCDPlusAlphabet[(int)(data[i] & 0xF)]);
            }
            return decodedString.ToString();
        }

        /// <summary>
        /// Packed 6-bit Ascii decoder
        /// 
        /// "IPMI" encoded in 6-bit ASCII is:
        ///     I = 29h (101001b)
        ///     P = 30h (110000b)
        ///     M = 2Dh (101101b)
        ///     I = 29h (101001b)
        /// Which gets packed into bytes as follows:
        /// 
        /// bit    | 7   6   5   4   3   2   1   0 |  hex
        /// =======|=======|=======================|=======
        /// byte 1 | 0   0 | 1   0   1   0   0   1 |  29h
        /// -------|-------|-------|---------------|-------
        /// byte 2 | 1   1   0   1 | 1   1   0   0 |  DCh
        /// -------|---------------|-------|-------|-------
        /// byte 3 | 1   0   1   0   0   1 | 1   0 | A6h
        /// -------|-----------------------|-------|-------
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static string DecodePacked6bitAscii(byte[] data)
        {
            const int PackedByteSize = 6;
            const int Ascii7BitOffset = 0x20;

            List<byte> unpackedAscii = new List<byte>();
            uint pack = 0;
            int packSize = 0;

            for (int i = 0; i < data.Length; i++)
            {
                byte unpackedByte;
                // Put bytes together in the pack
                pack += (uint)((uint)data[i] << packSize);
                packSize += 8;

                //We should have at least a byte in the pack so we start extracting
                while (packSize >= PackedByteSize)
                {
                    unpackedByte = (byte)((pack & 0x3f) + Ascii7BitOffset);
                    packSize -= PackedByteSize;
                    unpackedAscii.Add(unpackedByte);
                    pack = (uint)(pack >> PackedByteSize);
                }
            }

            // BUGBUG: The spec does not mention the need to have multiple of 3
            if (packSize > 0)
            {
                unpackedAscii.Add((byte)(pack & 0x3f + Ascii7BitOffset));
            }
            return Encoding.ASCII.GetString(unpackedAscii.ToArray());
        }

        /// <summary>
        /// Byte to Hex string representation
        /// </summary>  
        internal static string ByteToHexString(byte bytevalue)
        {
            return string.Format("0x{0:X2}", bytevalue);
        }

        /// <summary>
        /// Byte array to hex string representation.  Null or empty array
        /// will return empty string.
        /// </summary>  
        internal static string ByteArrayToHexString(byte[] byteArray)
        {
            string result = string.Empty;

            if (byteArray != null)
            {
                if (byteArray.Length > 0)
                {
                    result += "0x";

                    foreach (byte b in byteArray)
                    {
                        result += string.Format("{0:X2}", b);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Compare two byte arrays. 
        /// </summary>
        internal static bool CompareByteArray(byte[] arrayA, byte[] arrayB)
        {
            bool bEqual = false;
            if (arrayA.Length == arrayB.Length)
            {
                int i = 0;
                while ((i < arrayA.Length) && (arrayA[i] == arrayB[i]))
                {
                    i += 1;
                }
                if (i == arrayA.Length)
                {
                    bEqual = true;
                }
            }
            return bEqual;
        }

        /// <summary>
        /// Appends elements from an array into a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">source array</param>
        /// <param name="offset">offset into array where to read from</param>
        /// <param name="destination">destination list</param>
        /// <param name="count">elements to copy</param>
        internal static bool AppendArrayToList<T>(T[] source, int offset, List<T> destination, int count)
        {
            if (count == -1)
            {
                count = source.Length - offset;
            }

            if (offset + count >= source.Length)
            {
                for (int i = 0; i < count; i++)
                {
                    destination.Add(source[offset + i]);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Appends elements from an array into a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">source array</param>
        /// <param name="offset">offset into array where to read from</param>
        /// <param name="destination">destination list</param>
        internal static void AppendArrayToList<T>(T[] source, int offset, List<T> destination)
        {
            AppendArrayToList<T>(source, offset, destination, -1);
        }

        internal static IpmiResponse ConvertResponse(byte[] data, Type responseType)
        {
            // Create the response based on the provided type.
            ConstructorInfo constructorInfo = responseType.GetConstructor(Type.EmptyTypes);
            IpmiResponse ipmiResponse = (IpmiResponse)constructorInfo.Invoke(new Object[0]);

            foreach (PropertyInfo propertyInfo in responseType.GetProperties())
            {
                IpmiMessageDataAttribute[] attributes =
                    (IpmiMessageDataAttribute[])propertyInfo.GetCustomAttributes(typeof(IpmiMessageDataAttribute), true);

                if (attributes.Length > 0)
                {
                    if (propertyInfo.PropertyType == typeof(byte))
                    {
                        if (attributes[0].Offset < data.Length)
                        {
                            propertyInfo.SetValue(ipmiResponse, data[attributes[0].Offset], null);
                        }
                    }
                    else if (propertyInfo.PropertyType == typeof(ushort))
                    {
                        propertyInfo.SetValue(ipmiResponse, BitConverter.ToUInt16(data, attributes[0].Offset), null);
                    }
                    else if (propertyInfo.PropertyType == typeof(uint))
                    {
                        propertyInfo.SetValue(ipmiResponse, BitConverter.ToUInt32(data, attributes[0].Offset), null);
                    }
                    else if (propertyInfo.PropertyType == typeof(byte[]))
                    {
                        int propertyLength = attributes[0].Length;

                        if (propertyLength == 0)
                        {
                            propertyLength = data.Length - attributes[0].Offset;
                        }

                        if (attributes[0].Offset < data.Length)
                        {
                            byte[] propertyData = new byte[propertyLength];
                            Buffer.BlockCopy(data, attributes[0].Offset, propertyData, 0, propertyData.Length);

                            propertyInfo.SetValue(ipmiResponse, propertyData, null);
                        }
                    }
                    else
                    {
                        // May need to add other types.
                        Debug.Assert(false);
                    }
                }
            }

            return ipmiResponse;
        }

        /// <summary>
        /// Gets two's complement checksum
        /// </summary>
        /// <param name="start">index into array to start from</param>
        /// <param name="end">index into array to stop</param>
        /// <param name="payload">array to index</param>
        /// <returns>two's complement checksum</returns>
        internal static byte TwoComplementChecksum(int start, int end, byte[] payload)
        {
            byte checksum = 0;
            for (int i = start; i < end; i++) // starts at 1 to skip serial start charactor.
            {
                unchecked { checksum = (byte)((checksum + payload[i]) % 256); }
            }

            return (byte)(-checksum);
        }

        /// <summary>
        /// Appends elements from an array into a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">source array</param>
        /// <param name="destination">destination list</param>
        /// <param name="count">elements to copy</param>
        internal static void AppendArrayToList<T>(T[] source, List<T> destination, int count)
        {
            AppendArrayToList<T>(source, 0, destination, count);
        }

        /// <summary>
        /// Separate an word value into its high and low
        /// </summary>
        /// <param name="word"></param>
        /// <param name="lo"></param>
        /// <param name="hi"></param>
        internal static void SplitWord(ushort word, out byte lo, out byte hi)
        {
            lo = (byte)(word & byte.MaxValue);
            hi = (byte)((word >> 8) & byte.MaxValue);
        }

        /// <summary>
        /// Create ushort from two hi & lo bytes
        /// </summary>
        internal static ushort GetShort(byte lo, byte hi)
        {
            return (ushort)(hi << 8 | lo);
        }

        /// <summary>
        /// Get Inital Offset from 1970
        /// </summary>   
        internal static DateTime UnixOffset
        {
            get { return new DateTime(1970, 1, 1); }
        }

        /// <summary>
        /// Get date time from Sel Record time
        /// </summary>   
        internal static DateTime SecondsOffSet(double unixTime)
        {
            return UnixOffset.AddSeconds(unixTime);
        }

        /// <summary>
        /// Helps Set the Date Time for the Sel.
        /// </summary>
        internal static double SecondsFromUnixOffset(DateTime date)
        {
            return date.Subtract(UnixOffset).TotalSeconds;
        }

        /// <summary>
        /// sdr linerization conversion formulas.
        /// </summary>
        /// <param name="linearType">linerization enum type</param>
        /// <param name="val">raw  input value</param>
        /// <returns>converted value</returns>
        internal static Double Linearize(Linearization linearType, Double val)
        {
            Double value = val;

            switch (linearType)
            {
                case Linearization.Linear:
                    value = val;
                    break;
                case Linearization.Ln:
                    // natural logarithm
                    value = Math.Log(val);
                    break;
                case Linearization.Log10:
                    // logarithm with base 10
                    value = Math.Log10(val);
                    break;
                case Linearization.Log2:
                    // logarithm with base 2
                    value = Math.Log(val) / Math.Log(2);
                    break;
                case Linearization.E:
                    // exponential function
                    value = Math.Exp(val);
                    break;
                case Linearization.Exp10:
                    // exponential function 10
                    value = Math.Pow(10.0, val);
                    break;
                case Linearization.Exp2:
                    // exponential function 2
                    value = Math.Pow(2.0, val);
                    break;
                case Linearization.OneX:
                    // reciprocal
                    value = 1 / val;
                    break;
                case Linearization.Sqr:
                    // square
                    value = Math.Pow(val, 2.0);
                    break;
                case Linearization.Cube:
                    // Cube
                    value = Math.Pow(val, 3.0);
                    break;
                case Linearization.Sqrt:
                    // square root 
                    value = Math.Sqrt(val);
                    break;
                case Linearization.OverCube:
                    // cube-1
                    value = (1.0 / Math.Pow(val, 3.0));
                    break;
                default:
                    break;
            }

            return value;

        }

        /// <summary>
        /// Set the bits of the bit array.
        /// </summary>
        /// <param name="address">target bit array</param>
        /// <param name="start">index start point of bit array</param>
        /// <param name="end">index end point of of bit array</param>
        /// <param name="data">input byte</param>
        internal static void UpdateBitArray(ref BitArray address, int start, int end, byte data)
        {
            BitArray bits = new BitArray(new byte[1] { data });

            int index = 0;
            for (int i = start; i <= end; i++)
            {
                address[i] = bits[index++];

                // abort before index out of range
                if (index >= bits.Length)
                    break;
            }
        }

        /// <summary>
        /// Ipmi Auth Code Signature for Single Session Ipmi Channels (Ipmi Basic Mode over Serial)
        /// </summary>
        internal static byte[] AuthCodeSingleSession(uint ipmiSessionId, byte[] ipmiChallengeString, AuthenticationType authenticationType, string password)
        {
            // authentication code
            byte[] authCode = new byte[16];

            // trim password to 16
            if (password.Length > 16)
                password = password.Substring(0, 16);

            // password byte array (user Key)
            byte[] userKey = new byte[16];

            // populdate password byte array (user key).
            if (!string.IsNullOrEmpty(password))
                Array.Copy(ASCIIEncoding.ASCII.GetBytes(password), 0, userKey, 0, password.Length);

            // +16(password) +4(Session Id) +16(challangestring) +16(password) 
            int hashMessageLenght = 52;

            // resize the auth code depending on the integrity algorithm
            switch (authenticationType)
            {
                case AuthenticationType.MD2:
                    break;
                case AuthenticationType.MD5:
                    // message auth code array
                    byte[] hashMessage = new byte[hashMessageLenght];
                    {
                        // copy user password (key) to hash message
                        Buffer.BlockCopy(userKey, 0, hashMessage, 0, 16);

                        // copy ipmi session id to hash message
                        Buffer.BlockCopy(BitConverter.GetBytes(ipmiSessionId), 0, hashMessage, 16, 4);

                        // copy ipmi message data to the hash message
                        Buffer.BlockCopy(ipmiChallengeString, 0, hashMessage, 20, 16);

                        // copy ipmi sequence number to the hash message
                        Buffer.BlockCopy(userKey, 0, hashMessage, 36, 16);
                    }
                    // straight MD5 hash
                    using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                    {
                        // md5 hash the message and copy to authCode
                        Array.Copy(md5.ComputeHash(hashMessage), 0, authCode, 0, 16);
                    }
                    break;
                case AuthenticationType.None:
                    authCode = new byte[16];
                    break;
                case AuthenticationType.Straight:
                    authCode = userKey;
                    break;
                default:
                    break;
            }

            return authCode;
        }

    #endregion

    }
}
