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

using System;
using System.Threading;
using System.Collections;
using System.IO.Ports;
using System.Diagnostics;
using System.Collections.Generic;

namespace Microsoft.GFS.WCS.ChassisManager
{
    internal static class RequestPacketUtil
    {
        internal const int requestPacketHeaderSize = 3;

        /// <summary>
        /// Check if the expected payload length matches the payload buffer length in request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="expectedPayloadLength"></param>
        /// <returns></returns>
        static internal bool IsValidPayloadLength(ref byte[] request, int expectedPayloadLength)
        {
            if (request == null)
            {
                return false;
            }
            if ((request.Length - requestPacketHeaderSize) != expectedPayloadLength)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check if the payload byte count in the request packet matches the actual payload buffer length
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        static internal bool IsValidRequestLength(ref byte[] request)
        {
            ushort payloadByteCountInPacket;
            ushort payloadBufferLength;

            if (request == null)
            {
                return false;
            }
            if (request.Length < requestPacketHeaderSize)
            {
                return false;
            }
            payloadByteCountInPacket = BitConverter.ToUInt16(request, 1);
            payloadBufferLength = (ushort)(request.Length - requestPacketHeaderSize);

            if (payloadByteCountInPacket != payloadBufferLength)
            {
                Tracer.WriteError("Invalid request length: payload byte count in packet ({0}) !=  payload buffer length ({1})",
                    payloadByteCountInPacket, payloadBufferLength);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Return function code from the request packet
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        static internal byte GetFunctionCode(ref byte[] request)
        {
            return request[0];
        }

        /// <summary>
        /// Return the byte payload from the request packet from the specified offset
        /// </summary>
        internal static byte[] GetMultiByteFromPayload(ref byte[] request, int offset)
        {
            List<byte> response = new List<byte>();

            // request lenght -1 as the number of bytes to read is last in the payload.
            for (int i = offset; i < (request.Length -1); i++)
            {
                response.Add(request[i]);
            }

            return response.ToArray();
        }

        /// <summary>
        /// Return the single byte payload from the request packet from the specified offset
        /// </summary>
        static internal byte GetSingleByteFromPayload(ref byte[] request, int offset)
        {
            return request[offset];
        }
    }
}
