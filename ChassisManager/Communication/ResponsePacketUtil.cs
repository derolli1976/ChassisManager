/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Author:  woongkib@Microsoft.com                     *
*                                       	            *
********************************************************/

using System;
using System.Threading;
using System.Collections;
using System.IO.Ports;
using System.Diagnostics;
using System.Collections.Generic;

namespace Microsoft.GFS.WCS.ChassisManager
{
    internal static class ResponsePacketUtil
    {
        /// <summary>
        /// Generate a response packet with an empty payload
        /// </summary>
        /// <param name="completionCode"></param>
        /// <param name="responsePacket"></param>
        internal static void GenerateResponsePacket(CompletionCode completionCode, out byte[] responsePacket)
        {
            byte[] payload = null;
            GenerateResponsePacket(completionCode, 0, ref payload, out responsePacket);
        }

        internal static void GenerateResponsePacket(CompletionCode completionCode, ref byte[] payload, out byte[] responsePacket)
        {
            if (payload == null)
            {
                GenerateResponsePacket(completionCode, out responsePacket);
            }
            else
            {
                GenerateResponsePacket(completionCode, payload.Length, ref payload, out responsePacket);
            }
        }

        /// <summary>
        /// Generate a response packet with a non-empty payload
        /// </summary>
        /// <param name="completionCode"></param>
        /// <param name="payLoadLengthInByte"></param>
        /// <param name="payload"></param>
        /// <param name="responsePacket"></param>
        internal static void GenerateResponsePacket(CompletionCode completionCode, int payLoadLengthInByte, ref byte[] payload, out byte[] responsePacket)
        {
            const int byteCountSegmentLengthInByte = 2;
            if (payLoadLengthInByte == 0)
            {
                responsePacket = new byte[3];
                responsePacket[0] = (byte)completionCode;
                responsePacket[1] = 0;
                responsePacket[2] = 0;
            }
            else
            {
                byte[] byteCountSegment = BitConverter.GetBytes((short)payLoadLengthInByte);
                responsePacket = new byte[payLoadLengthInByte + 3];
                responsePacket[0] = (byte)completionCode;
                Buffer.BlockCopy(byteCountSegment, 0, responsePacket, 1, byteCountSegmentLengthInByte);
                Buffer.BlockCopy(payload, 0, responsePacket, 3, payLoadLengthInByte);
            }
        }
    }
}