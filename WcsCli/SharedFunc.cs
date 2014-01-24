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

namespace Microsoft.GFS.WCS.WcsCli
{
    using System;
    using System.ServiceModel;
    using System.Text;

    static class SharedFunc
    {

        /// <summary>
        /// Global flat to Signals Serial Session is enabled
        /// </summary>
        private static volatile bool enableSerialSession = false;

        internal static bool ActiveSerialSession
        {
            get { return enableSerialSession; }
            private set { enableSerialSession = value; }
        }

        internal static void SetSerialSession(bool enabled)
        {
            enableSerialSession = enabled;
        }

        /// <summary>
        /// Byte to Hex string representation
        /// </summary>  
        internal static string ByteToHexString(byte bytevalue)
        {
            return string.Format("0x{0:X2}", bytevalue);
        }

        /// <summary>
        /// Byte Array to Hex string representation
        /// </summary>  
        internal static string ByteArrayToHexString(byte[] Bytes)
        {
            string result = string.Empty;
            result += "0x";

            foreach (byte B in Bytes)
            {
                result += string.Format("{0:X2}", B);
            }
            return result;
        }

        /// <summary>
        /// Compare two byte arrays. 
        /// </summary>
        internal static bool CompareByteArray(byte[] arrayA, byte[] arrayB)
        {
            bool response = false;
            if (arrayA.Length == arrayB.Length)
            {
                int i = 0;
                while ((i < arrayA.Length) && (arrayA[i] == arrayB[i]))
                {
                    i += 1;
                }

                if (i == arrayA.Length)
                {
                    response = true;
                }
            }
            return response;
        }

        /// <summary>
        /// Generic Exception Handling method
        /// </summary>
        internal static void ExceptionOutput(Exception ex)
        {
            if(ex is TimeoutException)
            {
                Console.WriteLine("Communication with Chassis Manager timed out.");
                return;
            }
            else if (ex is FaultException)
            {
                Console.WriteLine("Communication fault.");
                return;
            }
            else if (ex is CommunicationException)
            {
                // When http status code is set to 500( internal server error), CommunicationException is thrown.
                // Displaying the ex.message which shows the http error description 
                Console.WriteLine("Command failed : " + ex.Message);
                return;
            }
            else if (ex is Exception)
            {
                Console.WriteLine("Exception: " + ex);
            }
        }
    }
}
