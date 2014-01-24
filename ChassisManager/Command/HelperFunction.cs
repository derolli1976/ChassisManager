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

namespace Microsoft.GFS.WCS.ChassisManager
{
    using System.Text;

    public static class HelperFunction
    {
        /// <summary>
        /// Stores Max Pwm Requirement.  Used for Data Center AHU integration.
        /// </summary>
        public static volatile byte MaxPwmRequirement;

        /// <summary>
        /// Generates the text representation of an array of bytes
        /// </summary>
        /// <param name="Bytes"></param>
        /// <returns></returns>
        public static string ByteArrayToText(byte[] bytesArray)
        {
            StringBuilder Result = new StringBuilder();
            string HexAlphabet = "0123456789ABCDEF";
            bool needsSeparator = false;
            foreach (byte crtByte in bytesArray)
            {
                if (!needsSeparator)
                {
                    needsSeparator = true;
                }
                else
                {
                    Result.Append(' ');
                }
                Result.Append(HexAlphabet[(int)(crtByte >> 4)]);
                Result.Append(HexAlphabet[(int)(crtByte & 0xF)]);
            }
            return Result.ToString();
        }

        /// <summary>
        /// Byte to Hex string representation
        /// </summary>  
        public static string ByteToHexString(byte Bytes)
        {
            StringBuilder Result = new StringBuilder();
            string HexAlphabet = "0123456789ABCDEF";
            Result.Append("0x");
            Result.Append(HexAlphabet[(int)(Bytes >> 4)]);
            Result.Append(HexAlphabet[(int)(Bytes & 0xF)]);

            return Result.ToString();
        }

        /// <summary>
        /// Byte Array to Hex string representation
        /// </summary>  
        public static string ByteArrayToHexString(byte[] Bytes)
        {
            StringBuilder Result = new StringBuilder();
            string HexAlphabet = "0123456789ABCDEF";

            Result.Append("0x");

            foreach (byte B in Bytes)
            {
                Result.Append(HexAlphabet[(int)(B >> 4)]);
                Result.Append(HexAlphabet[(int)(B & 0xF)]);
            }
            return Result.ToString();
        }
    }
}
