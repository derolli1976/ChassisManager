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

    /// This file has everything related to Pmbus SPEC
    /// The idea is to decouple the device layer from PmBus specific details
    /// Any changes in PMbus spec (say a command opcode) will only affect this file
    
    /// <summary>
    /// Enumerates the PmBus commands supported
    /// </summary>
    internal enum PmBusCommand
    {
        Invalid = 0x0,
        STATUS_WORD = 0x79,
        READ_POUT = 0x96,
        MFR_SERIAL = 0x9E,
        CLEAR_FAULTS = 0x03,
        MFR_MODEL = 0x9A,
        SET_POWER = 0x01,
    }

    /// <summary>
    /// Enum values supports parameters for PmbusCommand
    /// </summary>
    internal enum PmBusCommandPayload
    {
        /// <summary>
        /// Power On payload for PmbusCommand.SET_POWER
        /// </summary>
        POWER_ON  = 0x80,

        /// <summary>
        /// Power Off payload for PmbusCommand.SET_POWER
        /// </summary>
        POWER_OFF = 0x00
    }

    /// <summary>
    /// Enumerates the PmBus commands response lenght
    /// </summary>
    internal enum PmBusResponseLength
    {
        Invalid = 0,
        STATUS_WORD = 2,
        READ_POUT = 2,
        MFR_SERIAL = 15,
        CLEAR_FAULTS = 0,
        MFR_MODEL = 17,
        SET_POWER = 1
    }


    internal static class PmBus
    {
        /// <summary>
        /// Enumerates the PmBus transaction types 
        /// </summary>
        private enum TransactionType
        {
            ReadByte = 0,
            ReadWord,
            ReadBlock,
            WriteByte,
            WriteWord,
            WriteBlock,
            RwByte,
            RwWord,
            RwBlock,
            ReadSerial,
        }

        /// <summary>
        /// This table contains key (commandCode) / value (numDataBytes) pairs
        /// </summary>
        static private Dictionary<PmBusCommand, TransactionType> commandTable = new Dictionary<PmBusCommand, TransactionType>()
        {
            {PmBusCommand.STATUS_WORD, TransactionType.ReadWord},
            {PmBusCommand.READ_POUT, TransactionType.ReadWord},
            {PmBusCommand.MFR_SERIAL, TransactionType.ReadSerial},
            {PmBusCommand.CLEAR_FAULTS, TransactionType.WriteByte},
            {PmBusCommand.SET_POWER, TransactionType.WriteByte}
        };

        /// <summary>
        /// Get the number of data bytes associated with the command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        static internal CompletionCode GetNumberOfDataBytesForCommand(PmBusCommand command, out byte numDataBytes)
        {
            numDataBytes = 0;
            TransactionType trType;
            if (commandTable.ContainsKey(command))
            {
                trType = commandTable[command];
                numDataBytes = GetNumberfOfDataBytesFromTransactionType(trType);
                return CompletionCode.Success;
            }
            return CompletionCode.InvalidCommand;
        }

        /// <summary>
        /// From the raw PSU status byte array, extract the relevant status information
        /// </summary>
        /// <param name="psuStatus">input raw status byte array</param>
        /// <param name="powerGoodByte">output status byte - 1 indicates good, 0 indicates something is bad with the PSU</param>
        /// <returns>True if PSU status is good, else return false.</returns>
        static internal bool ExtractPowerGoodFromPsuStatus(byte[] psuStatus, out byte powerGoodByte)
        {
            powerGoodByte = psuStatus[0];
            // Get the 4th bit of the high byte which is the power good signal - Pmbus SPEC II search for power good signal
            byte mask = (byte)8;
            powerGoodByte = (byte)(powerGoodByte & mask);
            powerGoodByte = (byte)((powerGoodByte << 4) >> 7);

            if (powerGoodByte == 1) // Power good signal is negated
                powerGoodByte = 0;
            else if (powerGoodByte == 0)
                powerGoodByte = 1;
            
            // If there are any other faults, print the full status word
            if (psuStatus[0] != 0 || psuStatus[1] != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// TODO: If this is Delta PSU specific move it to Delta psu class 
        /// </summary>
        /// <param name="dataPacketReceived"></param>
        /// <param name="interpretedDataPacket"></param>
        static internal void PsuModelNumberParser(ref byte[] dataPacketReceived, out byte[] interpretedDataPacket)
        {
            // Interprete the received data
            // With the Delta PSUs, the first byte contains the packet length.
            // Discard the first byte here.
            interpretedDataPacket = new byte[dataPacketReceived.Length - 1];
            Buffer.BlockCopy(dataPacketReceived, 1, interpretedDataPacket, 0, dataPacketReceived.Length - 1);
        }

        /// <summary>
        /// TODO: If this is Delta PSU specific move it to Delta psu class 
        /// </summary>
        /// <param name="dataPacketReceived"></param>
        /// <param name="interpretedDataPacket"></param>
        static internal void PsuSerialNumberParser(ref byte[] dataPacketReceived, out byte[] interpretedDataPacket)
        {
            // Interprete the received data
            // With the Delta PSUs, the first byte contains the packet length.
            // Discard the first byte here.
            interpretedDataPacket = new byte[dataPacketReceived.Length - 1];
            Buffer.BlockCopy(dataPacketReceived, 1, interpretedDataPacket, 0, dataPacketReceived.Length - 1);
        }        

        static internal void ReadPowerMilliWattConverter(ref byte[] dataPacketReceived, out byte[] interpretedDataPacket)
        {
            // Step 1: Interpret the received data
            // Based on the linear conversion model in the PMBus spec
            byte dataHighByte;
            byte dataLowByte;
            int NinTwosComplement;
            int YinTowsComplement;
            int N;
            int Y;
            int poutReadingInMilliWatts;
            const int bitCountOfN = 5;
            const int bitCountOfY = 11;
            const byte numMSBsToEncodeN = bitCountOfN;

            dataHighByte = dataPacketReceived[1];
            dataLowByte = dataPacketReceived[0];

            NinTwosComplement = dataHighByte >> (8 - numMSBsToEncodeN);
            YinTowsComplement = (BitwiseOperationUtil.MaskOffMSBs(dataHighByte, numMSBsToEncodeN) << 8) | dataLowByte;

            N = NumberConversionUtil.ConvertFromTwosComplement(NinTwosComplement, bitCountOfN);
            Y = NumberConversionUtil.ConvertFromTwosComplement(YinTowsComplement, bitCountOfY);

            // Power (milliwatts) = Y * pow(2, N) * 1000.0
            poutReadingInMilliWatts = (int)((double)Y * Math.Pow(2, N) * 1000.0);
            interpretedDataPacket = BitConverter.GetBytes(poutReadingInMilliWatts);
        }

        /// <summary>
        /// Get the number of data bytes associated with the transaction type
        /// </summary>
        /// <param name="trType"></param>
        /// <returns></returns>
        static private byte GetNumberfOfDataBytesFromTransactionType(TransactionType trType)
        {
            byte numDataBytes = 0;
            switch (trType)
            {
                case TransactionType.ReadWord:
                    numDataBytes = 2;
                    break;
                case TransactionType.ReadSerial:
                    numDataBytes = 15;
                    break;
                case TransactionType.WriteByte:
                    numDataBytes = 0;
                    break;
                default:
                    Tracer.WriteError("Unsupported transaction type: {0}", trType);
                    break;
            }
            return numDataBytes;
        }
    }
}
