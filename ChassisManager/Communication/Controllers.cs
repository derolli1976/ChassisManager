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
    internal static class NumberConversionUtil
    {
        static internal int ConvertFromTwosComplement(int numberInTwosComplement, int bitCount)
        {
            int maskForMSB = 1 << (bitCount - 1);
            int maskForAllBits = (1 << bitCount) - 1;
            int convertedNumber;
            if ((numberInTwosComplement & maskForMSB) != 0)
            {
                convertedNumber = -1 * ((numberInTwosComplement ^ maskForAllBits) + 1);
            }
            else
            {
                convertedNumber = numberInTwosComplement;
            }
            return convertedNumber;
        }
    }

    internal static class BitwiseOperationUtil
    {
        /// <summary>
        /// Toggle the target bit
        /// </summary>
        /// <param name="currData"></param>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        static internal byte ToggleSingleBit(byte currData, byte bitPosition)
        {
            byte mask = (byte)(1 << bitPosition);
            return (byte)(currData ^ mask);
        }

        /// <summary>
        /// Set the target bit
        /// </summary>
        /// <param name="currData"></param>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        static internal byte SetSingleBit(byte currData, byte bitPosition)
        {
            byte mask = (byte)(1 << bitPosition);
            return (byte)(currData | mask);
        }

        /// <summary>
        /// Clear the target bit
        /// </summary>
        /// <param name="currData"></param>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        static internal byte ClearSingleBit(byte currData, byte bitPosition)
        {
            byte mask = (byte)(1 << bitPosition);
            mask = (byte)~mask;
            return (byte)(currData & mask);
        }

        /// <summary>
        /// Check if the target bit is set
        /// </summary>
        /// <param name="currData"></param>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        static internal bool IsBitSet(byte currData, byte bitPosition)
        {
            byte mask = (byte)(1 << bitPosition);
            return ((currData & mask) != 0);
        }

        /// <summary>
        /// Check if the target bit is cleared
        /// </summary>
        /// <param name="currData"></param>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        static internal bool IsBitCleared(byte currData, byte bitPosition)
        {
            byte mask = (byte)(1 << bitPosition);
            return ((currData & mask) == 0);
        }

        /// <summary>
        /// Mask off N LSBs
        /// </summary>
        /// <param name="data"></param>
        /// <param name="numLSBsToMaskOff"></param>
        /// <returns></returns>
        static internal byte MaskOffLSBs(byte data, int numLSBsToMaskOff)
        {
            int mask = 1 << numLSBsToMaskOff;
            return (byte)(data & mask);
        }

        /// <summary>
        /// Mask off N MSBs
        /// </summary>
        /// <param name="data"></param>
        /// <param name="numMSBsToMaskOff"></param>
        /// <returns></returns>
        static internal byte MaskOffMSBs(byte data, int numMSBsToMaskOff)
        {
            int mask = (0xFF >> numMSBsToMaskOff);
            return (byte)(data & mask);
        }
    }

    // Based on the the CM block diagram (ver. 0.7.x)
    internal static class I2cAddresses
    {
        internal const byte addrBladeEnable1to16 = 0x40;
        internal const byte addrBladeEnable17to32 = 0x42;
        internal const byte addrBladeEnable33to48 = 0x44;
        internal const byte addrInputDevices = 0x48;
        internal const byte addrOutputDevices = 0x4A;
        internal const byte addrPsusWithOddNumericId = 0xB0;
        internal const byte addrPsusWithEvenNumericId = 0xB2;
        internal const byte addrCmFruEeprom = 0xA0;
        internal const byte addrPdbFruEeprom = 0xA2;
        internal const byte addrFan1to4 = 0x5E;
        internal const byte addrFan5to6 = 0x58;
        internal const byte addrInvalid = 0xFF;

        internal enum addrsOfPCA9535Devices : byte
        {
            bladeEnable1to16 = addrBladeEnable1to16,
            bladeEnable17to32 = addrBladeEnable17to32,
            bladeEnable33to48 = addrBladeEnable33to48,
            inputDevices = addrInputDevices,
            outputDevices = addrOutputDevices,
        };

        internal enum addrsOfOtherDevices : byte
        {
            fan1to4 = addrFan1to4,
            fan5to6 = addrFan5to6,
            cmFruEeprom = addrCmFruEeprom,
            pdbFruEeprom = addrPdbFruEeprom,
            psusWithOddNumericId = addrPsusWithOddNumericId,
            psusWithEvenNumericId = addrPsusWithEvenNumericId,
        };

        internal static bool IsValidAddress(byte address)
        {
            return (
                (Enum.IsDefined(typeof(addrsOfPCA9535Devices), address) ||
                 Enum.IsDefined(typeof(addrsOfOtherDevices), address)) &&
                (address != addrInvalid));
        }
    }

    /// <summary>
    /// This class abstracts the SC18IM700 I2C controller (master)
    /// http://www.nxp.com/documents/data_sheet/SC18IM700.pdf
    /// </summary>
    internal static class SC18IM700
    {
        private enum RwBit
        {
            Write = 0,
            Read = 1,
        }

        private enum AsciiCommand
        {
            S = 0x53,
            P = 0x50,
            R = 0x52,
            W = 0x57,
            I = 0x49,
            O = 0x4F,
            Z = 0x5A,
        }

        internal enum RegisterAddress
        {
            BRG0 = 0x0,
            BRG1 = 0x1,
            PortConf1 = 0x2,
            PortConf2 = 0x3,
            I2CClkL = 0x7,
            I2CClkH = 0x8,
            I2CTO = 0x9,
            I2CStat = 0xA,
        }

        // GPIO is an 8-bit register
        internal const byte gpioBitWidth = 8;

        // COM3
        // WatchDog reset
        internal const byte bitPositionCpldWdt1 = 5;

        // Number of LSBs to encode a server ID
        internal const byte numLSBsToEncodeServerId = 6;

        static private CompletionCode ValidateParametersToGenerateCommand(ref byte[] data)
        {
            if (data == null)
            {
                return CompletionCode.InvalidCommand;
            }
            return CompletionCode.Success;
        }

        static private CompletionCode ValidateParametersToGenerateCommand(ref byte[] address, ref byte[] data)
        {
            if ((address == null) ||
                (data == null) ||
                (address.Length != data.Length))
            {
                return CompletionCode.InvalidCommand;
            }
            return CompletionCode.Success;
        }

        static private CompletionCode ValidateParametersToGenerateCommand(byte numBytesToRead)
        {
            if (numBytesToRead == 0)
            {
                return CompletionCode.InvalidCommand;
            }
            return CompletionCode.Success;
        }

        static private CompletionCode ValidateParametersToGenerateCommand(ref byte[] data, byte numBytesToRead)
        {
            if ((data == null) ||
                (numBytesToRead == 0))
            {
                return CompletionCode.InvalidCommand;
            }
            return CompletionCode.Success;
        }

        /// <summary>
        /// Generate a command to write N bytes to a slave device
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="functionCode"></param>
        /// <param name="data"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        internal static CompletionCode GenerateWriteNBytesToSlaveDeviceCommand(byte deviceType, byte deviceId, byte functionCode, ref byte[] data, out byte[] command)
        {
            byte address;
            CompletionCode completionCode = GetSlaveDeviceAddress(deviceType, deviceId, functionCode, out address);
            if (CompletionCodeChecker.Failed(completionCode))
            {
                command = null;
                return completionCode;
            }
            return GenerateWriteNBytesToSlaveDeviceCommand(address, ref data, out command);
        }

        internal static CompletionCode GenerateWriteNBytesToSlaveDeviceCommand(byte address, ref byte[] data, out byte[] command)
        {
            CompletionCode completionCode;
            command = null;
            if (I2cAddresses.IsValidAddress(address) == false)
            {
                completionCode = CompletionCode.InvalidCommand;
                return completionCode;
            }

            completionCode = ValidateParametersToGenerateCommand(ref data);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }

            // length bytes (data) + 4 bytes (start, stop, address, byte count)
            command = new byte[data.Length + 4];
            command[0] = (byte)AsciiCommand.S;
            command[1] = ConcatenateAddressAndRwBit(address, RwBit.Write);
            command[2] = (byte)data.Length;
            Buffer.BlockCopy(data, 0, command, 3, data.Length);
            command[command.Length - 1] = (byte)AsciiCommand.P;
            completionCode = CompletionCode.Success;
            return completionCode;
        }

        /// <summary>
        /// Generate a command to read N bytes from a slave device
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="numBytesToRead"></param>
        /// <param name="command"></param>
        internal static CompletionCode GenerateReadNBytesFromSlaveDeviceCommand(byte deviceType, byte deviceId, byte functionCode, byte numBytesToRead, out byte[] command)
        {
            byte address;
            command = null;
            CompletionCode completionCode = GetSlaveDeviceAddress(deviceType, deviceId, functionCode, out address);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            completionCode = ValidateParametersToGenerateCommand(numBytesToRead);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            // 4 bytes (start, stop, adddress, byte count)
            command = new byte[4];
            command[0] = (byte)AsciiCommand.S;
            command[1] = ConcatenateAddressAndRwBit(address, RwBit.Read);
            command[2] = numBytesToRead;
            command[3] = (byte)AsciiCommand.P;
            completionCode = CompletionCode.Success;
            return completionCode;
        }

        /// <summary>
        /// Generate a command to write to an internal register
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <param name="command"></param>
        internal static CompletionCode GenerateWriteToInternalRegisterCommand(ref byte[] address, ref byte[] data, out byte[] command)
        {
            CompletionCode completionCode;
            command = null;
            completionCode = ValidateParametersToGenerateCommand(ref address, ref data);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }

            // 2 bytes (start, stop) + 2 * length bytes (pair of address and data)
            command = new byte[address.Length * 2 + 2];
            command[0] = (byte)AsciiCommand.W;
            for (int i = 0; i < address.Length; i++)
            {
                command[2 * i + 1] = address[i];
                command[2 * i + 2] = data[i];
            }
            command[command.Length - 1] = (byte)AsciiCommand.P;
            completionCode = CompletionCode.Success;
            return completionCode;
        }

        /// <summary>
        /// Generate a command to read from an internal register
        /// </summary>
        /// <param name="address"></param>
        /// <param name="command"></param>
        internal static CompletionCode GenerateReadFromInternalRegisterCommand(ref byte[] address, out byte[] command)
        {
            CompletionCode completionCode;
            command = null;
            completionCode = ValidateParametersToGenerateCommand(ref address);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            // 2 bytes (start, stop) + length bytes (the list of reg. addresses)
            command = new byte[address.Length + 2];
            command[0] = (byte)AsciiCommand.R;
            for (int i = 0; i < address.Length; i++)
            {
                command[i + 1] = address[i];
            }
            command[command.Length - 1] = (byte)AsciiCommand.P;
            completionCode = CompletionCode.Success;
            return completionCode;
        }

        /// <summary>
        /// Generate a command to write the GPIO port
        /// </summary>
        /// <param name="command"></param>
        internal static CompletionCode GenerateWriteToGpioPortCommand(byte data, out byte[] command)
        {
            // three bytes (start, stop, data)
            command = new byte[3];
            command[0] = (byte)AsciiCommand.O;
            command[1] = data;
            command[2] = (byte)AsciiCommand.P;
            return CompletionCode.Success;
        }

        /// <summary>
        /// Generate a command to read from the GPIO port
        /// </summary>
        /// <param name="command"></param>
        internal static CompletionCode GenerateReadFromGpioPortCommand(out byte[] command)
        {
            // two bytes (start, stop)
            command = new byte[2];
            command[0] = (byte)AsciiCommand.I;
            command[1] = (byte)AsciiCommand.P;
            return CompletionCode.Success;
        }

        /// <summary>
        /// Generate a read-after-write command
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="data"></param>
        /// <param name="numBytesToRead"></param>
        /// <param name="command"></param>
        internal static CompletionCode GenerateReadAfterWriteCommand(byte deviceType, byte deviceId, byte functionCode, ref byte[] data, byte numBytesToRead, out byte[] command)
        {
            byte address;
            CompletionCode completionCode = GetSlaveDeviceAddress(deviceType, deviceId, functionCode, out address);
            if (CompletionCodeChecker.Failed(completionCode))
            {
                command = null;
                return completionCode;
            }
            return GenerateReadAfterWriteCommand(address, ref data, numBytesToRead, out command);
        }

        internal static CompletionCode GenerateReadAfterWriteCommand(byte address, ref byte[] data, byte numBytesToRead, out byte[] command)
        {
            CompletionCode completionCode;
            command = null;
            if (I2cAddresses.IsValidAddress(address) == false)
            {
                completionCode = CompletionCode.InvalidCommand;
                return completionCode;
            }
            completionCode = ValidateParametersToGenerateCommand(ref data, numBytesToRead);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            // 7 bytes (first start, first address, first byte count, second start, second address,
            // second byte count, stop byte) + length bytes (data)
            command = new byte[data.Length + 7];
            command[0] = (byte)AsciiCommand.S;
            command[1] = ConcatenateAddressAndRwBit(address, RwBit.Write);
            command[2] = (byte)data.Length;
            Buffer.BlockCopy(data, 0, command, 3, data.Length);
            command[data.Length + 3] = (byte)AsciiCommand.S;
            command[data.Length + 4] = ConcatenateAddressAndRwBit(address, RwBit.Read);
            command[data.Length + 5] = numBytesToRead;
            command[command.Length - 1] = (byte)AsciiCommand.P;
            completionCode = CompletionCode.Success;
            return completionCode;
        }

        /// <summary>
        /// Generate a write-after-write command 
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <param name="command"></param>
        internal static CompletionCode GenerateWriteAfterWriteCommand(byte deviceType, byte deviceId, byte functionCode, ref byte[] data1, ref byte[] data2, out byte[] command)
        {
            byte address;
            command = null;
            CompletionCode completionCode = GetSlaveDeviceAddress(deviceType, deviceId, functionCode, out address);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            if ((data1 == null) ||
                (data2 == null))
            {
                completionCode = CompletionCode.InvalidCommand;
                return completionCode;
            }
            // 7 bytes (first start, first address, first byte count, second start, second address,
            // second byte count, stop byte) + length1 bytes (data1) + length2 bytes (data2)
            command = new byte[data1.Length + data2.Length + 7];
            command[0] = (byte)AsciiCommand.S;
            command[1] = ConcatenateAddressAndRwBit(address, RwBit.Write);
            command[2] = (byte)data1.Length;
            Buffer.BlockCopy(data1, 0, command, 3, data1.Length);
            command[data1.Length + 3] = (byte)AsciiCommand.S;
            command[data1.Length + 4] = ConcatenateAddressAndRwBit(address, RwBit.Write);
            command[data1.Length + 5] = (byte)data2.Length;
            Buffer.BlockCopy(data2, 0, command, data1.Length + 6, data2.Length);
            command[command.Length - 1] = (byte)AsciiCommand.P;
            completionCode = CompletionCode.Success;
            return CompletionCode.Success;
        }

        /// <summary>
        /// Generate GPIO port data from Server ID to select a server
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        private static byte GenerateGpioPortDataFromServerId(byte deviceId, byte currGpioReading)
        {
            byte newGpioValue;
            byte bit0to5;
            bit0to5 = BitwiseOperationUtil.MaskOffMSBs((byte)(deviceId - 1), (gpioBitWidth - numLSBsToEncodeServerId));
            newGpioValue = (byte)(BitwiseOperationUtil.MaskOffLSBs(currGpioReading, numLSBsToEncodeServerId) | bit0to5);
            
            return newGpioValue;
        }

        /// <summary>
        /// Generate GPIO port data from PSU ID to select a PMBus
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        private static byte GenerateGpioPortDataFromPsuId(byte deviceId, byte currGpioReading)
        {
            byte newGpioValue = 0;
            byte bit0_2 = 0;
            const byte numLSBsToEncodePsuId = 3;
            switch (deviceId)
            {
                case 0:
                    // This is the case to clear the PM bus hub bits
                    // to work around the I2C signal issue
                    bit0_2 = 0;
                    break;
                case 1:
                case 2:
                    bit0_2 = 1;
                    break;
                case 3:
                case 4:
                    bit0_2 = 2;
                    break;
                case 5:
                case 6:
                    bit0_2 = 4;
                    break;
                default:
                    break;
            }
            newGpioValue = (byte)(BitwiseOperationUtil.MaskOffLSBs(currGpioReading, numLSBsToEncodePsuId) | bit0_2);
            return newGpioValue;
        }

        /// <summary>
        /// Generate a command to write to the GPIO port for demultiplexing based on device type and ID
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="command"></param>
        internal static CompletionCode GenerateWriteToGpioPortCommandForDemultiplexing(byte deviceType, byte deviceId, out byte[] command, byte currGpioReading)
        {
            byte newGpioValue = 0;

            switch (deviceType)
            {
                case (byte)DeviceType.BladeConsole:
                    // Fall through
                case (byte)DeviceType.Server:
                    newGpioValue = GenerateGpioPortDataFromServerId(deviceId, currGpioReading);
                    break;
                case (byte)DeviceType.Psu:
                    newGpioValue = GenerateGpioPortDataFromPsuId(deviceId, currGpioReading);
                    break;
                default:
                    Tracer.WriteError("[SC18IM700] invalid device type ({0})", deviceType);
                    command = null;
                    return CompletionCode.InvalidCommand;
            }
            return GenerateWriteToGpioPortCommand(newGpioValue, out command);
        }

        /// <summary>
        /// Generate a command to initialize internal registers
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        internal static CompletionCode GenerateInternalRegistersInitCommand(out byte[] command)
        {
            const int numRegsToInit = 5;
            byte[] address = new byte[numRegsToInit];
            byte[] data = new byte[numRegsToInit];

            // Program baud rate register
            // Baud rate = 7.3728 * 10^6 / (16 + (BRG1, BRG0))
            // Target baud rate: 115200bps => BRG1 = 0x0, BRG0 = 0x30
            address[0] = (byte)RegisterAddress.BRG0;
            address[1] = (byte)RegisterAddress.BRG1;
            data[0] = 0x30;
            data[1] = 0x0;

            // Program I2CClk registers
            // Bit frequency = 7.3728 * 10^6 / (2 * (I2CClkH + I2CClkL))
            // Target frequency: 369 kHz => I2CClkH = 0x5, I2CClkL = 0x5
            address[2] = (byte)RegisterAddress.I2CClkL;
            address[3] = (byte)RegisterAddress.I2CClkH;
            data[2] = 0x5;
            data[3] = 0x5;

            // Program I2CTO register
            address[4] = (byte)RegisterAddress.I2CTO;
            // bit[7:1] = TO value, bit[0] = enable/disable
            // TO = 256 * bit[7:1] / 57600
            // bit[7:1] = 0x7 (bit[7:0] = 0xF) => TO ~ 30ms
            data[4] = 0xF;

            return GenerateWriteToInternalRegisterCommand(ref address, ref data, out command);
        }

        internal static CompletionCode GenerateGpioInitCommand(string portName, out byte[] command)
        {
            byte data;
            command = null;

            if ((portName != "COM3") && (portName != "COM4"))
            {
                return CompletionCode.InvalidCommand;
            }

            // Initialize GPIO pins
            if (portName == "COM3")
            {
                // COM3
                // GPIO0 (O): PMB_EN_1 (0)
                // GPIO1 (O): PMB_EN_2 (0)
                // GPIO2 (O): PMB_EN_3 (0)
                // GPIO3 (I): PCA9535_INT_N (0)
                // GPIO4 (O): FAN_MAX_CTR (0)
                // GPIO5 (O): CPLD_WDT_1 (0)
                // GPIO6 (O): CPU_RST_N (1)
                // GPIO7 (O): CMC_RESERVE_1 (0)
                // 01000000 => 0x40
                data = 0x40;
            }
            else
            {
                // COM4
                // GPIO0 (O): UART_SW_S0 (0)
                // GPIO1 (O): UART_SW_S1 (0)
                // GPIO2 (O): UART_SW_S2 (0)
                // GPIO3 (O): UART_SW_S3 (0)
                // GPIO4 (O): UART_SW_S4 (0)
                // GPIO5 (O): UART_SW_S5 (0)
                // GPIO6 (O): WDT_EN_N (0)
                // GPIO7 (I): CABLE_DETECT_N (0)
                // 00000000 => 0x0
                data = 0x0;
            }

            return GenerateWriteToGpioPortCommand(data, out command);
        }

        internal static CompletionCode GenerateConfigRegInitCommand(string portName, out byte[] command)
        {
            const int numRegsToInit = 2;
            byte[] address = new byte[numRegsToInit];
            byte[] data = new byte[numRegsToInit];
            command = null;

            if ((portName != "COM3") && (portName != "COM4"))
            {
                return CompletionCode.InvalidCommand;
            }

            // Program PortConf registers
            // Output is set to push-pull configuration
            address[0] = (byte)RegisterAddress.PortConf1;
            address[1] = (byte)RegisterAddress.PortConf2;

            if (portName == "COM3")
            {
                // COM3
                // GPIO0 (O): PMB_EN_1 (0)
                // GPIO1 (O): PMB_EN_2 (0)
                // GPIO2 (O): PMB_EN_3 (0)
                // GPIO3 (I): PCA9535_INT_N (0)
                // GPIO4 (O): FAN_MAX_CTR (0)
                // GPIO5 (O): CPLD_WDT_1 (0)
                // GPIO6 (O): CPU_RST_N (1)
                // GPIO7 (O): CMC_RESERVE_1 (0)
                // data[0] => 01101010 => 0x6A
                // data[1] => 10101010 => 0xAA
                data[0] = 0x6A;
                data[1] = 0xAA;
            }
            else
            {
                // COM4
                // GPIO0 (O): UART_SW_S0 (0)
                // GPIO1 (O): UART_SW_S1 (0)
                // GPIO2 (O): UART_SW_S2 (0)
                // GPIO3 (O): UART_SW_S3 (0)
                // GPIO4 (O): UART_SW_S4 (0)
                // GPIO5 (O): UART_SW_S5 (0)
                // GPIO6 (O): WDT_EN_N (0)
                // GPIO7 (I): CABLE_DETECT_N (0)
                // data[0] => 10101010 => 0xAA
                // data[1] => 01101010 => 0x6A
                data[0] = 0xAA;
                data[1] = 0x6A;
            }
            return GenerateWriteToInternalRegisterCommand(ref address, ref data, out command);
        }

        internal static CompletionCode GenerateGpioInitCommandForPmBus(out byte[] command)
        {
            byte data;
            command = null;

            // COM3
            // Set all PM bus hub bits to 0 to work around PM bus/I2C signal issue
            // GPIO0 (O): PMB_EN_1 (0)
            // GPIO1 (O): PMB_EN_2 (0)
            // GPIO2 (O): PMB_EN_3 (0)
            // Except for PM bus hub bits, set other pins to 1
            // GPIO3 (I): PCA9535_INT_N (1)
            // GPIO4 (O): FAN_MAX_CTR (1)
            // GPIO5 (O): CPLD_WDT_1 (1)
            // GPIO6 (O): CPU_RST_N (1)
            // GPIO7 (O): CMC_RESERVE_1 (1)
            // 11111000 => 0xF8
            data = 0xF8;

            return GenerateWriteToGpioPortCommand(data, out command);
        }

        internal static CompletionCode GenerateConfigRegHubInitCommandForPmBus(out byte[] command)
        {
            const int numRegsToInit = 2;
            byte[] address = new byte[numRegsToInit];
            byte[] data = new byte[numRegsToInit];
            command = null;

            // Program PortConf registers
            // Output is set to push-pull configuration
            address[0] = (byte)RegisterAddress.PortConf1;
            address[1] = (byte)RegisterAddress.PortConf2;
            // COM3
            // GPIO0 (O): PMB_EN_1
            // GPIO1 (O): PMB_EN_2
            // GPIO2 (O): PMB_EN_3
            // Except for PM bus hub pins, the rest of them
            // should be still in the input mode
            // GPIO3 (I): PCA9535_INT_N
            // GPIO4 (I): FAN_MAX_CTR
            // GPIO5 (I): CPLD_WDT_1
            // GPIO6 (I): CPU_RST_N
            // GPIO7 (I): CMC_RESERVE_1
            // data[0] => 01101010 => 0x6A
            // data[1] => 01010101 => 0x55
            data[0] = 0x6A;
            data[1] = 0x55;
            return GenerateWriteToInternalRegisterCommand(ref address, ref data, out command);
        }

        /// <summary>
        /// Generate a command to check I2C bus status
        /// </summary>
        /// <param name="statusRegister"></param>
        /// <returns></returns>
        internal static CompletionCode CheckI2cBusStatusRegister(byte statusRegister)
        {
            // I2C status register
            // 0xF0: I2C_OK
            // 0xF1: I2C_NACK_ON_ADDRESS
            // 0xF2: I2C_NACK_ON_DATA
            // 0xF8: I2C_TIME_OUT
            // Other values: Undefined
            const byte mask = 0xF;

            if ((statusRegister & mask) == 0)
            {
                return CompletionCode.Success;
            }
            else
            {
                Tracer.WriteError("I2C status register: 0x{0:X}", statusRegister);
                return CompletionCode.I2cErrors;
            }
        }

        /// <summary>
        /// Get slave device address for PSU commands
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        private static CompletionCode GetSlaveDeviceAddressForPsuCommands(byte deviceType, byte deviceId, out byte address)
        {
            CompletionCode completionCode;
            address = I2cAddresses.addrInvalid;
            if ((deviceId & 0x1) == 1)
            {
                // PSU 1, 3, 5
                address = I2cAddresses.addrPsusWithOddNumericId;
            }
            else
            {
                // PSU 2, 4, 6
                address = I2cAddresses.addrPsusWithEvenNumericId;
            }
            completionCode = CompletionCode.Success;
            return completionCode;
        }

        /// <summary>
        /// Get slave device address for fan commands
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="functionCode"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        private static CompletionCode GetSlaveDeviceAddressForFanCommands(byte deviceType, byte deviceId, byte functionCode, out byte address)
        {
            CompletionCode completionCode = CompletionCode.Success;
            address = I2cAddresses.addrInvalid;

            if ((deviceId >= 1 && deviceId <= 4) ||
                (functionCode == (byte)FunctionCode.SetFanSpeed))
            {
                address = I2cAddresses.addrFan1to4;
            }
            else if (deviceId >= 5 && deviceId <= ConfigLoaded.NumFans)
            {
                address = I2cAddresses.addrFan5to6;
            }
            else
            {
                completionCode = CompletionCode.InvalidCommand;
                Tracer.WriteError("Invalid device ID: {0}", deviceId);
            }

            return completionCode;
        }

        /// <summary>
        /// Get slave device address for power (blade_enable) commands
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        private static CompletionCode GetSlaveDeviceAddressForPowerCommands(byte deviceType, byte deviceId, out byte address)
        {
            CompletionCode completionCode = CompletionCode.Success;
            address = I2cAddresses.addrInvalid;

            // Based on the diagram in CM block diagram.pdf
            if (deviceId >= 1 && deviceId <= 16)
            {
                address = I2cAddresses.addrBladeEnable1to16;
            }
            else if (deviceId >= 17 && deviceId <= 32)
            {
                address = I2cAddresses.addrBladeEnable17to32;
            }
            else if (deviceId >= 33 && deviceId <= CommunicationDevice.maxNumServersPerChassis)
            {
                address = I2cAddresses.addrBladeEnable33to48;
            }
            else
            {
                completionCode = CompletionCode.InvalidCommand;
                Tracer.WriteError("Invalid device ID: {0}", deviceId);
            }
            return completionCode;
        }

        /// <summary>
        /// Get the slave device address based on the CM block diagram of v0.7 
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        internal static CompletionCode GetSlaveDeviceAddress(byte deviceType, byte deviceId, byte functionCode, out byte address)
        {
            CompletionCode completionCode = CompletionCode.Success;

            switch (deviceType)
            {
                case (byte)DeviceType.Psu:
                    return GetSlaveDeviceAddressForPsuCommands(deviceType, deviceId, out address);
                case (byte)DeviceType.Fan:
                    return GetSlaveDeviceAddressForFanCommands(deviceType, deviceId, functionCode, out address);
                case (byte)DeviceType.Power:
                    return GetSlaveDeviceAddressForPowerCommands(deviceType, deviceId, out address);
                case (byte)DeviceType.StatusLed:
                case (byte)DeviceType.RearAttentionLed:
                    address = I2cAddresses.addrOutputDevices;
                    break;
                case (byte)DeviceType.FanCage:
                    address = I2cAddresses.addrInputDevices;
                    break;
                case (byte)DeviceType.PowerSwitch:
                    address = I2cAddresses.addrOutputDevices;
                    break;
                case (byte)DeviceType.ChassisFruEeprom:
                    address = I2cAddresses.addrCmFruEeprom;
                    break;
                case (byte)DeviceType.WatchDogTimer:
                    address = I2cAddresses.addrOutputDevices;
                    break;
                default:
                    completionCode = CompletionCode.InvalidCommand;
                    address = I2cAddresses.addrInvalid;
                    break;
            }
            return completionCode;
        }

        /// <summary>
        /// Concatenate the address and RW bit
        /// </summary>
        /// <param name="address"></param>
        /// <param name="rwBit"></param>
        /// <returns></returns>
        private static byte ConcatenateAddressAndRwBit(byte address, RwBit rwBit)
        {
            return (byte)(address | (byte)rwBit);
        }

        /// <summary>
        /// Compare the server ID encoded in GPIO and the target device ID
        /// </summary>
        /// <param name="currGpioReading"></param>
        /// <param name="targetServerId"></param>
        /// <returns></returns>
        internal static bool IsServerIdInGpioSameAsTargetServerId(byte currGpioReading, byte targetServerId)
        {
            byte serverIdInGpio = (byte)(BitwiseOperationUtil.MaskOffMSBs(currGpioReading,
                (gpioBitWidth - numLSBsToEncodeServerId)) + 1);
            return (serverIdInGpio == targetServerId);
        }
    }

    /// <summary>
    /// This class abstracts the PCA9535 controller (slave)
    /// http://www.nxp.com/documents/data_sheet/PCA9535_PCA9535C.pdf
    /// </summary>
    internal static class PCA9535
    {
        internal enum Command
        {
            InputPort0 = 0,
            InputPort1 = 1,
            OutputPort0 = 2,
            OutputPort1 = 3,
            PolarityInversionPort0 = 4,
            PolarityInversionPort1 = 5,
            ConfigurationPort0 = 6,
            ConfigurationPort1 = 7,
            Invalid = 8,
        }

        internal enum PortType
        {
            Input = 0,
            Output = 1,
            PolarityInversion = 2,
            Configuration = 3,
        }

        internal const byte numPorts = 2;
        private const byte numBitsInPort = 8;
        internal const byte defaultValueOfConfigReg = 0xFF;

        // Bit position (0 to 7) in an 8-bit register port

        // Input signals
        const byte bitPositionFanCageIntrude = 5;

        // Output signals
        const byte bitPositionRearAttentionLed = 6;
        const byte bitPositionStatusLed = 7;
        const byte bitPositionPowerSwitch1 = 2;
        const byte bitPositionPowerSwitch2 = 3;
        const byte bitPositionPowerSwitch3 = 4;
        // Indicates that WDT enable/disable pin is PCA_CPLD_WDT2
        const byte bitPositionWdtEn = 1;

        // Invalid bit position
        const byte bitPositionInvalid = 0xFF;

        /// <summary>
        /// Get a command based on device type and ID
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="portType"></param>
        /// <returns></returns>
        static internal CompletionCode GetCommand(byte deviceType, byte deviceId, out Command command)
        {
            CompletionCode completionCode = CompletionCode.Success;

            switch (deviceType)
            {
                case (byte)DeviceType.Power:
                    command = GetPowerCommand(deviceId);
                    break;
                case (byte)DeviceType.StatusLed:
                case (byte)DeviceType.RearAttentionLed:
                    command = Command.OutputPort0;
                    break;
                case (byte)DeviceType.PowerSwitch:
                    command = Command.OutputPort1;
                    break;
                case (byte)DeviceType.FanCage:
                    command = Command.InputPort1;
                    break;
                case (byte)DeviceType.WatchDogTimer:
                    command = Command.OutputPort1;
                    break;
                default:
                    completionCode = CompletionCode.InvalidCommand;
                    command = Command.Invalid;
                    break;
            }
            return completionCode;
        }

        /// <summary>
        /// Get command for Power (blade_enable) commands 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        static private Command GetPowerCommand(byte deviceId)
        {
            Command command;
            byte portId = GetPortIdFromServerId(deviceId);
            if (portId == 0)
            {
                command = Command.OutputPort0;
            }
            else
            {
                command = Command.OutputPort1;
            }
            return command;
        }

        /// <summary>
        /// Determine the port ID of the server
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        static private byte GetPortIdFromServerId(byte deviceId)
        {
            byte numTotalIoBits = numPorts * numBitsInPort;
            if (((deviceId - 1) % numTotalIoBits) < numBitsInPort)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Get the bit position in an output register based on device type and ID
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        static private CompletionCode GetBitPositionInPort(byte deviceType, byte deviceId, out byte bitPosition)
        {
            CompletionCode completionCode = CompletionCode.Success;
            bitPosition = bitPositionInvalid;
            switch (deviceType)
            {
                case (byte)DeviceType.Power:
                    bitPosition = GetBitPositionInPortFromServerId(deviceId);
                    break;
                case (byte)DeviceType.RearAttentionLed:
                    bitPosition = bitPositionRearAttentionLed;
                    break;
                case (byte)DeviceType.StatusLed:
                    bitPosition = bitPositionStatusLed;
                    break;
                case (byte)DeviceType.PowerSwitch:
                    bitPosition = GetBitPositionInPortFromPowerSwitchId(deviceId);
                    break;
                case (byte)DeviceType.FanCage:
                    bitPosition = bitPositionFanCageIntrude;
                    break;
                case (byte)DeviceType.WatchDogTimer:
                    bitPosition = bitPositionWdtEn;
                    break;
                default:
                    completionCode = CompletionCode.InvalidCommand;
                    break;
            }
            return completionCode;
        }

        /// <summary>
        /// Get the bit position in an output register based only on device type
        /// (for devices that do not have an ID)
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        static private CompletionCode GetBitPositionInPort(byte deviceType, out byte bitPosition)
        {
            return GetBitPositionInPort(deviceType, 0xFF, out bitPosition);
        }

        /// <summary>
        /// Get bit position from a server ID
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        static private byte GetBitPositionInPortFromServerId(byte deviceId)
        {
            return (byte)((deviceId - 1) % numBitsInPort);
        }

        /// <summary>
        /// Get bit position from a power switch ID
        /// TODO: By default returning switch1. Change to return failure completion code when device id not valid
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        static private byte GetBitPositionInPortFromPowerSwitchId(byte deviceId)
        {
            byte bitPosition;
            if (deviceId == 3)
            {
                bitPosition = bitPositionPowerSwitch3;
            }
            else if(deviceId ==2)
            {
                bitPosition = bitPositionPowerSwitch2;
            }
            else 
            {
                bitPosition = bitPositionPowerSwitch1;
            }
            return bitPosition;
        }

        /// <summary>
        /// Get bit position from the LED device type
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        static private byte GetBitPositionInPortFromLedDeviceType(byte deviceType)
        {
            byte bitPosition;
            if (deviceType == (byte)DeviceType.StatusLed)
            {
                bitPosition = bitPositionStatusLed;
            }
            else
            {
                bitPosition = bitPositionRearAttentionLed;
            }
            return bitPosition;
        }

        /// <summary>
        /// Determine if the server is currently powered on based on the bit vector
        /// information
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="currPowerStateOfServers"> Contains the power state of servers (bit vector)</param>
        /// <returns></returns>
        static internal bool IsServerPoweredOn(byte deviceId, byte currPowerStateOfServers)
        {
            byte bitPosition = GetBitPositionInPortFromServerId(deviceId);
            // Blade_Enable is active high
            bool isPoweredOn = BitwiseOperationUtil.IsBitSet(currPowerStateOfServers, bitPosition);
            return isPoweredOn;
        }

        /// <summary>
        /// Check if the power switch is turn on
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="currRegValue"></param>
        /// <returns></returns>
        static internal bool IsPowerSwitchTurnedOn(byte deviceId, byte currRegValue)
        {
            byte bitPosition = GetBitPositionInPortFromPowerSwitchId(deviceId);
            // Power Switch is active high
            bool isPoweredOn = BitwiseOperationUtil.IsBitSet(currRegValue, bitPosition);
            return isPoweredOn;
        }

        /// <summary>
        /// Check if the FAN_CAGE_INTRUDED_N signal is set or cleared
        /// </summary>
        /// <param name="currGpioReading"></param>
        /// <returns></returns>
        static internal bool IsFanCageIntruded(byte currGpioReading)
        {
            byte bitPosition;
            GetBitPositionInPort((byte)DeviceType.FanCage, out bitPosition);
            // active low
            return BitwiseOperationUtil.IsBitCleared(currGpioReading, bitPosition);
        }

        /// <summary>
        /// Check if the LED is turned on
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="currRegValue"></param>
        /// <returns></returns>
        static internal bool IsLedTurnedOn(byte deviceType, byte currRegValue)
        {
            byte bitPosition = GetBitPositionInPortFromLedDeviceType(deviceType);
            // Active low
            bool isTurnedOn = BitwiseOperationUtil.IsBitCleared(currRegValue, bitPosition);
            return isTurnedOn;
        }

        /// <summary>
        /// Check if the pin is logically set
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="currRegValue"></param>
        /// <param name="isLogicallySet"></param>
        /// <returns></returns>
        static internal CompletionCode IsPinLogicallySet(byte deviceType, byte deviceId, byte currRegValue, out bool isLogicallySet)
        {
            CompletionCode completionCode = CompletionCode.Success;
            isLogicallySet = false;
            switch (deviceType)
            {
                case (byte)DeviceType.Power:
                    isLogicallySet = IsServerPoweredOn(deviceId, currRegValue);
                    break;
                case (byte)DeviceType.PowerSwitch:
                    isLogicallySet = IsPowerSwitchTurnedOn(deviceId, currRegValue);
                    break;
                case (byte)DeviceType.FanCage:
                    isLogicallySet = IsFanCageIntruded(currRegValue);
                    break;
                case (byte)DeviceType.StatusLed:
                case (byte)DeviceType.RearAttentionLed:
                    isLogicallySet = IsLedTurnedOn(deviceType, currRegValue);
                    break;
                default:
                    Tracer.WriteError("Invalid deviceType: 0x{0:X}", deviceType);
                    completionCode = CompletionCode.InvalidCommand;
                    break;
            }
            return completionCode;
        }

        /// <summary>
        /// Generate a command and data to set/clear a single bit in the register
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="currGpioValue"></param>
        /// <param name="isToSet"></param>
        /// <param name="commandAndData"></param>
        /// <returns></returns>
        static internal CompletionCode GenerateCommandAndDataToSetOrClearSingleBit(byte deviceType, byte deviceId, byte currGpioValue, bool isToSet, out byte[] commandAndData)
        {
            CompletionCode completionCode;
            Command command;
            byte bitPosition;
            byte newGpioValue;
            commandAndData = null;

            completionCode = GetCommand(deviceType, deviceId, out command);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }

            completionCode = GetBitPositionInPort(deviceType, deviceId, out bitPosition);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }

            if (isToSet == true)
            {
                newGpioValue = BitwiseOperationUtil.SetSingleBit(currGpioValue, bitPosition);
            }
            else
            {
                newGpioValue = BitwiseOperationUtil.ClearSingleBit(currGpioValue, bitPosition);
            }

            // Success path
            completionCode = CompletionCode.Success;
            commandAndData = new byte[2];
            commandAndData[0] = (byte)command;
            commandAndData[1] = newGpioValue;
            return completionCode;
        }
    }


    /// <summary>
    /// This class abstracts the ADT7470 controller
    /// </summary>
    internal static class ADT7470
    {
        internal enum Command
        {
            GetTachLowByte,
            GetTachHighByte,
            SetPwmDutyCycle,
        }

        private enum RegisterAddress
        {
            Invalid = 0xFF,
            Tach1LowByte = 0x2A,
            Tach1HighByte = 0x2B,
            Tach2LowByte = 0x2C,
            Tach2HighByte = 0x2D,
            Tach3LowByte = 0x2E,
            Tach3HighByte = 0x2F,
            Tach4LowByte = 0x30,
            Tach4HighByte = 0x31,
            Pwm1DutyCycle = 0x32,
        }

        private const byte numRegisterPorts = 4;

        private const int oscillatorFreq = 90000;
        private const double pwmDutyCycleIncreaseStep = 0.39;
        private const ushort tachReadingForSlowOrStalledFan = 0xFFFF;

        /// <summary>
        /// Check if the PWM parameter is in the valid range
        /// </summary>
        /// <param name="pwm"></param>
        /// <returns></returns>
        static internal bool IsValidInputPwmValue(byte pwm)
        {
            return (pwm >= 0 && pwm <= 100);
        }

        /// <summary>
        /// Check if the register is valid
        /// </summary>
        /// <param name="registerAddress"></param>
        /// <returns></returns>
        static internal bool IsValidRegisterAddress(byte registerAddress)
        {
            if ((Enum.IsDefined(typeof(RegisterAddress), (Int32)registerAddress) == false) ||
                (registerAddress == (byte)RegisterAddress.Invalid))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Get the register address based on device ID and command type
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        static internal byte GetRegisterAddress(byte deviceId, Command command)
        {
            byte registerAddress = (byte)RegisterAddress.Invalid;

            switch (command)
            {
                case Command.GetTachLowByte:
                    switch (deviceId)
                    {
                        case 1:
                            registerAddress = (byte)RegisterAddress.Tach1LowByte;
                            break;
                        case 2:
                            registerAddress = (byte)RegisterAddress.Tach2LowByte;
                            break;
                        case 3:
                            registerAddress = (byte)RegisterAddress.Tach3LowByte;
                            break;
                        case 4:
                            registerAddress = (byte)RegisterAddress.Tach4LowByte;
                            break;
                        case 5:
                            registerAddress = (byte)RegisterAddress.Tach1LowByte;
                            break;
                        case 6:
                            registerAddress = (byte)RegisterAddress.Tach2LowByte;
                            break;
                        default:
                            registerAddress = (byte)RegisterAddress.Invalid;
                            Tracer.WriteError("[ADT7470] invalid deviceId ({0})", deviceId);
                            break;
                    }
                    break;
                case Command.GetTachHighByte:
                    switch (deviceId)
                    {
                        case 1:
                            registerAddress = (byte)RegisterAddress.Tach1HighByte;
                            break;
                        case 2:
                            registerAddress = (byte)RegisterAddress.Tach2HighByte;
                            break;
                        case 3:
                            registerAddress = (byte)RegisterAddress.Tach3HighByte;
                            break;
                        case 4:
                            registerAddress = (byte)RegisterAddress.Tach4HighByte;
                            break;
                        case 5:
                            registerAddress = (byte)RegisterAddress.Tach1HighByte;
                            break;
                        case 6:
                            registerAddress = (byte)RegisterAddress.Tach2HighByte;
                            break;
                        default:
                            registerAddress = (byte)RegisterAddress.Invalid;
                            Tracer.WriteError("[ADT7470] invalid deviceId ({0})", deviceId);
                            break;
                    }
                    break;
                case Command.SetPwmDutyCycle:
                    registerAddress = (byte)RegisterAddress.Pwm1DutyCycle;
                    break;
                default:
                    Tracer.WriteError("[ADT7470] invalid Command ({0})", command);
                    registerAddress = (byte)RegisterAddress.Invalid;
                    break;
            }
            return registerAddress;
        }

        /// <summary>
        /// Convert the two byte raw tach reading to RPM (ushort)
        /// </summary>
        /// <param name="tachCombinedBytes"></param>
        /// <returns></returns>
        static internal ushort ConvertTachReadingToRpm(ushort tachCombinedBytes)
        {
            ushort rpm;
            const int secondsPerMin = 60;

            if (tachCombinedBytes == tachReadingForSlowOrStalledFan)
            {
                // If fan is stalled or running too slow, set rpm to 0
                rpm = 0;
            }
            else
            {
                rpm = (ushort)((oscillatorFreq * secondsPerMin) / tachCombinedBytes);
            }
            return rpm;
        }

        /// <summary>
        /// Scale the PWM value using the increase step parameter
        /// </summary>
        /// <param name="pwm"></param>
        /// <returns></returns>
        static internal byte ScalePwmWithIncreaseStepParameter(byte pwm)
        {
            if (pwm == 100)
            {
                // To avoid the rounding issue: floor(100/0.39) = 256
                return 0xFF;
            }
            else
            {
                return (byte)((double)pwm / pwmDutyCycleIncreaseStep);
            }
        }
    }

    /// <summary>
    /// This class abstracts the M24C64 EEPROM
    /// </summary>
    internal static class M24C64
    {
        // 64 Kbits
        private const int sizeInBytes = 8192;

        /// <summary>
        /// Check if the offset and length are in a valid range
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        static internal bool IsValidOffsetAndLength(ushort offset, ushort length)
        {
            return ((offset + length) < sizeInBytes);
        }
    }
}
