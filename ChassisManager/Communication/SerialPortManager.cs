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
using System.Text;

namespace Microsoft.GFS.WCS.ChassisManager
{
    /// <summary>
    /// Abstracts a serial port
    /// </summary>
    class SerialPortManager : PortManager
    {
        // An instance of SerialPort (.NET) class that abstracts a physical serial port
        // Each SerialPortManager manages its dedictated physical serial port
        SerialPort serialPort;

        private const byte startByte = 0xA0;
        private const byte stopByte = 0xA5;
        private const byte handshake = 0xA6;

        // Hack to capture setserialmux command after dequeuing work item
        // Planning to remove this after introducing a separate function code for all IPMI requests from CM
        private const byte setSerialMuxFuncCode = 0x30;
        private const byte setSerialMuxCommandNo = 0x12;

        // SerialPort timeout, in ms. Set in App.Config
        private readonly int timeout = ConfigLoaded.SerialTimeout;

        // number of consecutive gpio read timeouts before SC18IM700 reset
        private readonly int gpioErrorLimit = ConfigLoaded.GpioErrorLimit;

        // counter used to track consecutive gpio read/write errors.
        private int gpioErrorCount;

        private bool disposed = false;

        // Time delay to add when using CPLD as a switch
        // sriramg TODO move it to config
        const int timeDelayInMsToStabilizeCpldSwitch = 1;

        // Time delay to add before changing the baud rate
        const int timeDelayInMsBeforeChangingBaudRate = 500;

        // Byte array that holds per-server IPMI sequence ID
        // Array index is a physical device ID minus one
        private byte[] perServerIpmiSequenceId;

        internal SerialPortManager(int lpId, int numPriorityLevels)
            : base(lpId, numPriorityLevels)
        {
            serialPort = new SerialPort();
        }

        ~SerialPortManager()
        {
            TerminateWorkerThread();
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (serialPort != null)
                    {
                        serialPort.Dispose();
                        serialPort = null;
                    }
                }
                disposed = true;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Release all the resource
        /// </summary>
        public override void Release()
        {
            TerminateWorkerThread();
            Dispose();
            base.Release();
        }

        /// <summary>
        /// Initalizes the port manager
        /// </summary>
        internal override CompletionCode Init()
        {
            CompletionCode completionCode;
            const int timeToSleepInMsAfterOpenSerialPort = 100;

            serialPort.PortName = GetPhysicalPortNameFromLogicalId(logicalPortId);

            if (serialPort.PortName == null)
            {
                Tracer.WriteError("Serial port name is null (logicalPortId: {0})", logicalPortId);
                completionCode = CompletionCode.CommDevFailedToInit;
                return completionCode;
            }

            // Other ports (e.g., COM1/2) are initialized on demand
            if ((serialPort.PortName == "COM3") ||
                (serialPort.PortName == "COM4"))
            {
                serialPort.BaudRate = 9600;
                serialPort.Parity = Parity.None;
                serialPort.StopBits = StopBits.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = Handshake.None;
                serialPort.RtsEnable = false;
                serialPort.DtrEnable = false;

                serialPort.ReadTimeout = timeout;
                serialPort.WriteTimeout = timeout;

                try
                {
                    serialPort.Open();
                }
                catch (Exception e)
                {
                    Tracer.WriteError(e);
                    completionCode = CompletionCode.CommDevFailedToInit;
                    return completionCode;
                }

                // BUG FIX: UART 3 SC18IM700 is initialized in quick succession casuing intemittent
                // initialization failures.  When serialPort.Open() is called for the first time 
                // since the OS loads an RTS pulse is detected after the serialport.Open() method.
                Thread.Sleep(timeToSleepInMsAfterOpenSerialPort);

                // Step 1: Do the phase 1 of SC18IM700 initialization
                completionCode = InitializeSC18IM700Phase1();
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    Tracer.WriteError("InitializeSC18IM700Phase1 failed (code: 0x{0:X})", (byte)completionCode);
                    return completionCode;
                }

                // Step 2: Set the baud rate to 115200 bps. 
                // The baud rate of SC18IM700 has been set to 115200 in step 1
                serialPort.BaudRate = 115200;

                // Step 2.5: Set COM3 SC18IM700 GPIO[2:0] to "000" to work around
                // the PM bus/I2C signal issue
                if (serialPort.PortName == "COM3")
                {
                    completionCode = InitializeSC18IM700ForPmBusHub();
                    if (CompletionCodeChecker.Failed(completionCode) == true)
                    {
                        Tracer.WriteError("InitializeSC18IM700ForPmBusHub failed (code: 0x{0:X})", (byte)completionCode);
                        return completionCode;
                    }
                }

                // Step 3: If COM3, initialize ADT7470s
                // Set PWM to max
                if (serialPort.PortName == "COM3")
                {
                    completionCode = InitializeADT7470s();
                    if (CompletionCodeChecker.Failed(completionCode) == true)
                    {
                        Tracer.WriteError("InitializeADT7470s failed (code: 0x{0:X})", (byte)completionCode);
                        return completionCode;
                    }
                }

                // Step 4: Do the phase 2 of SC18IM700 initialization
                // PWM has been set to max in Step 3, so FAN_MAX_CTL can be cleared
                completionCode = InitializeSC18IM700Phase2();
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    Tracer.WriteError("InitializeSC18IM700Phase2 failed (code: 0x{0:X})", (byte)completionCode);
                    return completionCode;
                }

                // Step 5: If COM3, Initialize PCA9535s
                if (serialPort.PortName == "COM3")
                {
                    completionCode = InitializePCA9535s();
                    if (CompletionCodeChecker.Failed(completionCode) == true)
                    {
                        Tracer.WriteError("InitializePCA9535s failed (code: 0x{0:X})", (byte)completionCode);
                        return completionCode;
                    }
                }

                // Step 6: If COM4, allocate and initialize the per-server IPMI sequence ID array.
                if (serialPort.PortName == "COM4")
                {
                    perServerIpmiSequenceId = new byte[CommunicationDevice.maxNumServersPerChassis];
                    for (int i = 0; i < CommunicationDevice.maxNumServersPerChassis; i++)
                    {
                        perServerIpmiSequenceId[i] = 0;
                    }
                }
            }

            completionCode = base.Init();
            return completionCode;
        }

        /// <summary>
        /// Send and receive data over physical serial port
        /// </summary>
        /// <param name="workItem"></param>
        protected override void SendReceive(ref WorkItem workItem)
        {
            byte deviceType = workItem.deviceType;
            byte deviceId = workItem.deviceId;
            byte[] request = workItem.GetRequest();
            byte[] response;

            Tracer.WriteInfo("[Worker] sessionId: {0}, deviceType: {1}, physical deviceId: {2}",
                workItem.sessionId, deviceType, deviceId);

            // If currently in safe mode, reject all the requests routed to COM4 except for BladeConsole commands
            if ((isSafeModeEnabled == true) &&
                (serialPort.PortName == "COM4") &&
                (deviceType != (byte)DeviceType.BladeConsole))
            {
                CompletionCode completionCode = CompletionCode.CannotExecuteRequestInSafeMode;
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                Tracer.WriteInfo("[Worker] Cannot execute the request in safe mode (sessionId: {0})", workItem.sessionId);
                goto FallThroughPath;
            }

            if (DeviceTypeChecker.IsConsoleDeviceType(deviceType) == false)
            {
                // If the device type is not a console device type, clear any garbage data 
                // in buffer (left from the previous (failed) transaction)
                ClearInOutBuffers();
            }

            switch (deviceType)
            {
                case (byte)DeviceType.Fan:
                    SendReceiveFan(deviceType, deviceId, ref request, out response);
                    break;
                case (byte)DeviceType.Psu:
                    SendReceivePsu(deviceType, deviceId, ref request, out response);
                    break;
                case (byte)DeviceType.Power:
                    SendReceivePower(deviceType, deviceId, ref request, out response);
                    break;
                case (byte)DeviceType.Server:
                    {
                        // ensure packet format conforms to IPMI payload
                        if (request[0] == startByte && request.Length > 5)
                        {
                            // set the sequence Id for the request
                            SetPerServerSequenceId(deviceId, request[5]);
                        }
                        else
                        {
                            ResponsePacketUtil.GenerateResponsePacket(CompletionCode.InvalidRequestDataLength, out response);
                            break;
                        }

                        // Step 1: track the request with the actual sequence Id
                        // to handle out-of-order sequence issue.
                        byte ipmiSequenceId = (byte)(perServerIpmiSequenceId[deviceId - 1]);

                        // check if the packet is serial mux switching.
                        if (request[2] == setSerialMuxFuncCode && request[6] == setSerialMuxCommandNo)
                        {
                            // enable safe mode when entering serial console mode.
                            CommunicationDevice.EnableSafeMode();
                            Tracer.WriteInfo("SerialPortManager.SendReceive: SetSerialMux detected, implicitly assuming safe mode");

                            // MuxSwtich.BlockRequeststoSystem identifies JBOD.
                            // no payload is required to mux the JBOD.
                            if (request[8] == 0x05)
                            {
                                // we will return a false payload.
                                request = new byte[0];
                            }
                        }

                        // Send/Receive the payload
                        SendReceiveServer(deviceType, deviceId, ipmiSequenceId, ref request, out response);
                    }
                    break;
                case (byte)DeviceType.WatchDogTimer:
                    SendReceiveWatchDogTimer(ref request, out response);
                    break;
                case (byte)DeviceType.StatusLed:
                case (byte)DeviceType.RearAttentionLed:
                    SendReceiveLed(deviceType, deviceId, ref request, out response);
                    break;
                case (byte)DeviceType.PowerSwitch:
                    SendReceivePowerSwitch(deviceType, deviceId, ref request, out response);
                    break;
                case (byte)DeviceType.FanCage:
                    SendReceiveFanCage(deviceType, deviceId, ref request, out response);
                    break;
                case (byte)DeviceType.ChassisFruEeprom:
                    SendReceiveCmFruEeprom(deviceType, deviceId, ref request, out response);
                    break;
                case (byte)DeviceType.BladeConsole:
                    // Fall through
                case (byte)DeviceType.SerialPortConsole:
                    SendReceiveConsole(deviceType, deviceId, ref request, out response);
                    break;
                default:
                    CompletionCode completionCode = CompletionCode.InvalidCommand;
                    ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                    Tracer.WriteError("Invalid device type: {0}", deviceType);
                    break;
            }

            // Fall through
            FallThroughPath:

            // Copy the response packet
            workItem.SetResponse(response);

            // Signal the waiting user-level thread
            workItem.Signal();
        }

        #region PSU Functions

        /// <summary>
        /// The common code that is used to receive the raw data from a slave device
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="dataPacketReceived"></param>
        /// <returns></returns>
        private CompletionCode SendReceivePsuReceiveData(byte deviceType, byte deviceId, ref byte[] request, out byte[] dataPacketReceived)
        {
            CompletionCode completionCode;
            byte[] commandPacket = null;
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);
            byte[] commandPayload = null;
            byte numDataBytesToRead;
            byte currGpioReading;

            dataPacketReceived = null;

            // Step 1: Read the current GPIO value
            completionCode = DoSerialTxToReadSC18IM700Gpio(out currGpioReading);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            // Step 2: Program GPIO for demultiplexing (to a target PMBus channel)
            completionCode = SC18IM700.GenerateWriteToGpioPortCommandForDemultiplexing(deviceType, deviceId, out commandPacket, currGpioReading);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            completionCode = TryToWrite(ref commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("Demultiplexing failed");
                goto CleanUp;
            }

            // Step 3: Send the command
            // The first byte of the payload indicates the PSU pmbus command op code
            commandPayload = RequestPacketUtil.GetMultiByteFromPayload(ref request, 3);

            // Get the expected number of bytes to be read back from the PSU
            numDataBytesToRead = RequestPacketUtil.GetSingleByteFromPayload(ref request,(request.Length - 1));

            if(numDataBytesToRead > 0)
            completionCode = SC18IM700.GenerateReadAfterWriteCommand(deviceType, deviceId, functionCode, ref commandPayload,
                numDataBytesToRead, out commandPacket);
            else
            completionCode = SC18IM700.GenerateWriteNBytesToSlaveDeviceCommand(deviceType, deviceId, functionCode, ref commandPayload, out commandPacket);

            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                goto CleanUp;
            }
            
            completionCode = TryToWrite(ref commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                goto CleanUp;
            }

            // Step 4: Receive the data if required - indicated using a non-zero numDataBytesToRead value
            if (numDataBytesToRead > 0)
            {
                completionCode = TryToRead(out dataPacketReceived, numDataBytesToRead);
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    goto CleanUp;
                }
            }

            // Step 5: Check the I2C bus status
            completionCode = DoSerialTxToCheckSC18IM700I2cBusStatus();
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                goto CleanUp;
            }

            // Success path
            completionCode = CompletionCode.Success;
            
            // fall through
            CleanUp:
            
            // Step 6: Set PM bus hub bits to 000 to work around the signal issue
            CompletionCode cleanUpCode;
            const byte deviceIdToCleanUpPmBus = 0;
            cleanUpCode = SC18IM700.GenerateWriteToGpioPortCommandForDemultiplexing(deviceType, 
                deviceIdToCleanUpPmBus, out commandPacket, currGpioReading);
            if (CompletionCodeChecker.Failed(cleanUpCode) == true)
            {
                Tracer.WriteError("GenerateWriteToGpioPortCommandForDemultiplexing in Cleanup failed");
            }
            cleanUpCode = TryToWrite(ref commandPacket);
            if (CompletionCodeChecker.Failed(cleanUpCode) == true)
            {
                Tracer.WriteError("TryToWrite in Cleanup failed");
            }
            return completionCode;
        }

        /// <summary>
        /// Top-level method that processes PSU commands
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceivePsu(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            byte[] dataPacketReceived = null;
            CompletionCode completionCode = SendReceivePsuReceiveData(deviceType, deviceId, ref request, out dataPacketReceived);
            ResponsePacketUtil.GenerateResponsePacket(completionCode, ref dataPacketReceived, out response);
        }
        #endregion

        #region Fan Functions

        /// <summary>
        /// Implements the GetFanSpeed command
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private CompletionCode SendReceiveGetFanSpeed(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            CompletionCode completionCode;
            byte tachLowByte;
            byte tachHighByte;
            ushort tachCombinedBytes;
            ushort rpm;
            byte[] rpmInTwoByteArray;
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);

            // Step 1: Read the tach low byte
            completionCode = DoSerialTxToGetADT7470TachByte(deviceType, deviceId, functionCode, ADT7470.Command.GetTachLowByte, out tachLowByte);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                return completionCode;
            }

            // Step 2: Read the tach high byte
            completionCode = DoSerialTxToGetADT7470TachByte(deviceType, deviceId, functionCode, ADT7470.Command.GetTachHighByte, out tachHighByte);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                return completionCode;
            }

            // Step 3: Convert tach to RPM
            tachCombinedBytes = (ushort)((tachHighByte << 8) | tachLowByte);

            if (tachCombinedBytes == 0)
            {
                completionCode = CompletionCode.ResponseNotProvided;
                Tracer.WriteWarning("Tach high/low bytes are 0");
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                return completionCode;
            }

            rpm = ADT7470.ConvertTachReadingToRpm(tachCombinedBytes);
            rpmInTwoByteArray = BitConverter.GetBytes(rpm);
            completionCode = CompletionCode.Success;
            ResponsePacketUtil.GenerateResponsePacket(completionCode, ref rpmInTwoByteArray, out response);
            return completionCode;
        }

        /// <summary>
        /// Implements the SetFanSpeed command
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private void SendReceiveSetFanSpeed(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            CompletionCode completionCode;
            // PWM is 1 byte (0 to 100%)
            const int pwmPayloadLength = 1;

            // Step 1: Check if the request packet is valid
            if (RequestPacketUtil.IsValidPayloadLength(ref request, pwmPayloadLength) == false)
            {
                completionCode = CompletionCode.InvalidCommand;
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                return;
            }

            // Step 2: Send the request to update the PWM register
            byte pwm = RequestPacketUtil.GetSingleByteFromPayload(ref request,3);
            if (ADT7470.IsValidInputPwmValue(pwm) == false)
            {
                Tracer.WriteError("Invalid PWM: {0}", pwm);
                completionCode = CompletionCode.InvalidDataFieldInRequest;
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                return;
            }

            completionCode = DoSerialTxToWriteADT7470PwmRegister(deviceType, deviceId, pwm);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("SendReceiveSetFanSpeed failed");
            }
            ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
        }

        /// <summary>
        /// Top-level method that processes Fan commands
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveFan(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);

            switch (functionCode)
            {
                case (byte)FunctionCode.GetFanSpeed:
                    SendReceiveGetFanSpeed(deviceType, deviceId, ref request, out response);
                    break;
                case (byte)FunctionCode.SetFanSpeed:
                    SendReceiveSetFanSpeed(deviceType, deviceId, ref request, out response);
                    break;
                default:
                    Tracer.WriteError("Invalid functionCode: 0x{0:X}", functionCode);
                    ResponsePacketUtil.GenerateResponsePacket(CompletionCode.InvalidCommand, out response);
                    break;
            }
        }

        /// <summary>
        /// SendReceive data for FanCage command
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="response"></param>
        private void SendReceiveFanCage(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            CompletionCode completionCode;
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);

            if (functionCode != (byte)FunctionCode.ReadFanCageIntrude)
            {
                Tracer.WriteError("Invalid functionCode: 0x{0:X}", functionCode);
                completionCode = CompletionCode.InvalidCommand;
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                return;
            }
            SendReceiveToCheckIfPCA9535PinIsLogicallySet(deviceType, deviceId, ref request, out response);
        }

        #endregion

        #region Power Functions

        private void SendReceivePowerGetStatus(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            SendReceiveToCheckIfPCA9535PinIsLogicallySet(deviceType, deviceId, ref request, out response);
        }

        private void SendReceivePowerTurnOnOrOffSingleServer(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            CompletionCode completionCode;
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);
            // Blade_Enable is active high
            bool isToSet = (functionCode == (byte)FunctionCode.TurnOnServer);
            completionCode = DoSerialTxToSetOrClearPCA9535OutputRegBit(deviceType, deviceId, functionCode, isToSet);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("SendReceivePowerTurnOnOrOffSingleServer failed");
            }
            ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
        }

        /// <summary>
        /// Implements commands related to blade_enable
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceivePower(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);
            switch (functionCode)
            {
                case (byte)FunctionCode.GetServerPowerStatus:
                    SendReceivePowerGetStatus(deviceType, deviceId, ref request, out response);
                    break;
                case (byte)FunctionCode.TurnOffServer:
                    // Fall through
                case (byte)FunctionCode.TurnOnServer:
                    SendReceivePowerTurnOnOrOffSingleServer(deviceType, deviceId, ref request, out response);
                    break;
                default:
                    Tracer.WriteError("Invalid functionCode: 0x{0:X}", functionCode);
                    ResponsePacketUtil.GenerateResponsePacket(CompletionCode.InvalidCommand, out response);
                    break;
            }
        }

        #endregion

        #region Server Functions

        /// <summary>
        /// Implements server commands
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveServer(byte deviceType, byte deviceId, byte ipmiSequenceId, ref byte[] request, out byte[] response)
        {
            CompletionCode completionCode;
            byte[] dataPacketReceived;

            // Steps 2-4: Select a server (connection will be pointed to servers (i.e., not SC18IM700)
            completionCode = DoSerialTxToSelectServer(deviceType, deviceId);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("DoSerialTxToSelectServer failed");
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                return;
            }

            if (request.Length > 0)
            {
                // Step 5: Send the request to the server
                completionCode = TryToWrite(ref request);
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    Tracer.WriteError("TryToWrite failed");
                    ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                    return;
                }

                // Step 6: Receive data from the server
                dataPacketReceived = null;
                completionCode = TryToReadWithStartStopBytes(out dataPacketReceived, startByte, stopByte, ipmiSequenceId);
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    Tracer.WriteError("TryToReadWithStartStopBytes failed (physical device ID: {0})", deviceId);
                    ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                    return;
                }

                // Step 7: Generate the response packet
                completionCode = CompletionCode.Success;
                ResponsePacketUtil.GenerateResponsePacket(completionCode, ref dataPacketReceived, out response);
            }
            else
            {
                completionCode = CompletionCode.InvalidRequestDataLength;
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                return;
            }
        }

        private void SetPerServerSequenceId(byte deviceId, byte sequenceId)
        {
            perServerIpmiSequenceId[deviceId - 1] = sequenceId;
        }

        /// <summary>
        /// 1. Toggle DTR to establish a connection to COM4 SC18IM700
        /// 2. Program COM4 SC18IM700 GPIO to select the target server
        /// 3. Toggle DTR to establish a connection to the target server
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        private CompletionCode DoSerialTxToSelectServer(byte deviceType, byte deviceId)
        {
            CompletionCode completionCode;
            byte[] commandPacket = null;
            byte currGpioReading = 0;

            // Step 1: Program CPLD to connect CM to I2C bus controller
            ToggleDtrValueToConnectToSC18IM700();

            // Step 1.5 Once the DTR has been toggled, flush the read/write buffers as the Gpio is
            // grabage sensative.
            ClearInOutBuffers();

            // Step 2: Send the request to the controller to program GPIO
            // Note that even after introducing a delay in ToggleDtrValueToConnectToSC18IM700(), actual toggling to the master chip need not have committed
            // In the delay was not sufficient, the GPIO read below may read some junk (perhaps from the blade we were pointing to earlier)
            // This should not really matter since all it does is introduce compulsory write to the GPIOs
            // The GPIO write below is highly likely to suceed in writing to the SC18IM master chip since we have introduced two delays in the form of
            // (1) explicity delay inside ToggleDtrValueToConnectToSC18IM700() and (2) implicit delay in the form of read to the GPIO below
            completionCode = DoSerialTxToReadSC18IM700Gpio(out currGpioReading);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("DoSerialTxToReadSC18IM700Gpio failed (code: 0x{0:X})", completionCode);

                // if consecutive read/write errors occur the gpioErrorCount will get incremented.  If the error count 
                // exceeds the error limit the master chip will be re-initialized.  consecutive failures indicate the
                // master chip reset it's baud rate.
                if (gpioErrorCount >= gpioErrorLimit)
                {
                    // reinitialize the SC18IM700
                    CompletionCode initializeCode = DoInitializeSC18IM700Reset();
                    if (CompletionCodeChecker.Failed(initializeCode) == true)
                    {
                        Tracer.WriteError("DoInitializeSC18IM700Reset UART re-initialization Failed (code: 0x{0:X})", initializeCode);
                        return completionCode;
                    }

                    Tracer.WriteWarning("DoInitializeSC18IM700Reset UART re-initialization Completed (code: 0x{0:X})", initializeCode);

                    // reset the error count
                    gpioErrorCount = 0;
                }
                else
                {
                    gpioErrorCount++;
                    Tracer.WriteWarning("DoSerialTxToReadSC18IM700Gpio Read Error (Count: 0x{0:X})", gpioErrorCount);
                }

                return completionCode;
            }
            else
            {
                // reset the error count
                gpioErrorCount = 0;
            }
            
            // Optimization: If the current GPIO reading is already same as the target server ID,
            // no need to issue a serial write to update the GPIO. Only issue a write request,
            // if the GPIO does not match with the target server ID
            if (SC18IM700.IsServerIdInGpioSameAsTargetServerId(currGpioReading, deviceId) == false)
            {                
                completionCode = SC18IM700.GenerateWriteToGpioPortCommandForDemultiplexing(deviceType, deviceId,
                    out commandPacket, currGpioReading);
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    return completionCode;
                }
                // The TryToWrite method is non-blocking, and does not wait on a response from the GPIO.  
                // Therefore we do a readAndCompare-after-write to ensure write commit
                completionCode = TryToWrite(ref commandPacket);
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    Tracer.WriteError("SelectServer failed");
                    return completionCode;
                }

                // Read-after-write to Gpio 
                completionCode = DoSerialTxToReadSC18IM700Gpio(out currGpioReading);
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    Tracer.WriteError("DoSerialTxToReadSC18IM700Gpio ReadAfterWrite failed (code: 0x{0:X})", completionCode);
                    return completionCode;
                }

                // Compare whether the GPIO value read is same as what was written 
                // Testing if the switch to a particular server suceeded
                if (SC18IM700.IsServerIdInGpioSameAsTargetServerId(currGpioReading, deviceId) == false)
                {
                    Tracer.WriteError("DoSerialTxToSelectServer FAILURE: Written Gpio value is different from what is read");
                    // TODO: Do we create a new error category
                    completionCode = CompletionCode.SerialPortOtherErrors;
                    return completionCode;
                }
            }

            // Step 3: Program CPLD to connect CM to the server
            ToggleDtrValueToConnectToServer();

            return completionCode;
        }

        /// <summary>
        /// Toggle DTR value to connect CM to the target server (COM4)
        /// Code calling this function should ensure that all previous writes to SC18IM chip has committed 
        /// This can be done by doing read-after-write on SC18IM chip GPIO registers 
        /// </summary>
        private void ToggleDtrValueToConnectToServer()
        {
            serialPort.DtrEnable = true;
            // Add a delay of 1 MS after DTR enable.. just to make sure that the switch operation has been committed
            // DtrEnable synchronization using Read-after-write of DtrEnable pin may not work since .NET implementation might be using local state variables for these pins
            // TODO: Do we really need this delay?
            Thread.Sleep(timeDelayInMsToStabilizeCpldSwitch);
        }

        /// <summary>
        /// Toggle DTR value to connect CM to SC18IM700 (COM4)
        /// Assumption: Code calling this function has succesfully committed all previous read/writes to the blades
        /// This may be a reasonable assumption since all blade operations are synchronous 
        /// </summary>
        private void ToggleDtrValueToConnectToSC18IM700()
        {
            serialPort.DtrEnable = false;
            // Add a delay of 1 MS after DTR enable.. just to make sure that the switch operation has been committed
            // DtrEnable synchronization using Read-after-write of DtrEnable pin may not work since .NET implementation might be using local state variables for these pins
            // TODO: Do we really need this delay?
            Thread.Sleep(timeDelayInMsToStabilizeCpldSwitch);
        }

        #endregion

        #region Watchdog Functions

        /// <summary>
        /// SendReceive for WatchDogTimer enable/disable commands
        /// PCA_CPLD_WDT2 pin is used for enable/disable watchdog timer - it is enabled by default
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="response"></param>
        /// <param name="isToEnable"></param>
        private void SendReceiveWatchDogTimerEnableOrDisable(ref byte[] request, out byte[] response, bool isToEnable)
        {
            CompletionCode completionCode;
            
            // Updated after PCA_CPLD_WDT2 pin changed from active low to active high
            bool isToSet = isToEnable;
            
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);
            // WatchDog timer does not have a device ID.
            const byte deviceIdForWatchDog = 1;
            completionCode = DoSerialTxToSetOrClearPCA9535OutputRegBit((byte)DeviceType.WatchDogTimer, 
                deviceIdForWatchDog, functionCode, isToSet);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("SendReceiveWatchDogTimerEnableOrDisable failed");
            }
            ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
        }

        /// <summary>
        /// SendReceive for WatchDogTimer reset command
        /// </summary>
        /// <param name="response"></param>
        private void SendReceiveWatchDogTimerReset(out byte[] response)
        {
            CompletionCode completionCode;
            byte currGpioValue;
            byte newGpioValue;
            const int timeDelayInMsToGeneratePulse = 30;

            // Step 1: Read the current GPIO value
            completionCode = DoSerialTxToReadSC18IM700Gpio(out currGpioValue);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                return;
            }

            newGpioValue = BitwiseOperationUtil.ClearSingleBit(currGpioValue, SC18IM700.bitPositionCpldWdt1);

            // Step 2: Write to GPIO to generate a pulse through the WatchDog Reset bit
            // Pulse: low -> delay -> High -> delay -> low
            for (int i = 0; i < 2; i++)
            {
                completionCode = DoSerialTxToWriteSC18IM700Gpio(newGpioValue);
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    Tracer.WriteError("SendReceiveWatchDogTimerDisableOrEnable failed");
                    ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                    return;
                }
                // Add a delay to generate a pulse
                Thread.Sleep(timeDelayInMsToGeneratePulse);
                newGpioValue = BitwiseOperationUtil.ToggleSingleBit(newGpioValue, SC18IM700.bitPositionCpldWdt1);
            }

            // Success path
            completionCode = CompletionCode.Success;
            ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
        }

        /// <summary>
        /// SendReceive for WatchDogTimer
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveWatchDogTimer(ref byte[] request, out byte[] response)
        {
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);
            switch (functionCode)
            {
                case (byte)FunctionCode.DisableWatchDogTimer:
                    SendReceiveWatchDogTimerEnableOrDisable(ref request, out response, false);
                    break;
                case (byte)FunctionCode.EnableWatchDogTimer:
                    SendReceiveWatchDogTimerEnableOrDisable(ref request, out response, true);
                    break;
                case (byte)FunctionCode.ResetWatchDogTimer:
                    SendReceiveWatchDogTimerReset(out response);
                    break;
                default:
                    Tracer.WriteError("Invalid functionCode: 0x{0:X}", functionCode);
                    ResponsePacketUtil.GenerateResponsePacket(CompletionCode.InvalidCommand, out response);
                    break;
            }
        }

        #endregion

        #region LED Functions

        /// <summary>
        /// SendReceive data to turn on or off the LED
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveLedToTurnOnOrOff(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);
            CompletionCode completionCode;
            bool isToSet;
            // LED signals are active low
            isToSet = (functionCode == (byte)FunctionCode.TurnOffLed);
            completionCode = DoSerialTxToSetOrClearPCA9535OutputRegBit(deviceType, deviceId, functionCode, isToSet);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("SendReceiveLedToTurnOnOrOff failed");
            }
            ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
        }

        /// <summary>
        /// SendReceive to check the LED status
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveGetLedStatus(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            SendReceiveToCheckIfPCA9535PinIsLogicallySet(deviceType, deviceId, ref request, out response);
        }

        /// <summary>
        /// SendReceive for LEDs
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveLed(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);
            CompletionCode completionCode;

            switch (functionCode)
            {
                case (byte)FunctionCode.TurnOnLed:
                case (byte)FunctionCode.TurnOffLed:
                    SendReceiveLedToTurnOnOrOff(deviceType, deviceId, ref request, out response);
                    break;
                case (byte)FunctionCode.GetLedStatus:
                    SendReceiveGetLedStatus(deviceType, deviceId, ref request, out response);
                    break;
                default:
                    Tracer.WriteError("Invalid functionCode: 0x{0:X}", functionCode);
                    completionCode = CompletionCode.InvalidCommand;
                    ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                    break;
            }
        }

        #endregion

        #region Power Switch Functions

        /// <summary>
        /// Turn on or off the switch
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceivePowerSwitchToTurnOnOrOff(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);
            CompletionCode completionCode;

            // PowerSwitch is active high
            bool isToTurnOn = (functionCode == (byte)FunctionCode.TurnOnPowerSwitch);
            completionCode = DoSerialTxToSetOrClearPCA9535OutputRegBit(deviceType, deviceId, functionCode, isToTurnOn);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("SendReceivePowerSwitchToTurnOnOrOff failed");
            }
            ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
        }

        /// <summary>
        /// Check the status of the power switch
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveGetPowerSwitchStatus(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            SendReceiveToCheckIfPCA9535PinIsLogicallySet(deviceType, deviceId, ref request, out response);
        }
        
        /// <summary>
        /// SendReceive data for PowerSwitch commands
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceivePowerSwitch(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);
            CompletionCode completionCode;

            switch (functionCode)
            {
                case (byte)FunctionCode.TurnOnPowerSwitch:
                case (byte)FunctionCode.TurnOffPowerSwitch:
                    SendReceivePowerSwitchToTurnOnOrOff(deviceType, deviceId, ref request, out response);
                    break;
                case (byte)FunctionCode.GetPowerSwitchStatus:
                    SendReceiveGetPowerSwitchStatus(deviceType, deviceId, ref request, out response);
                    break;
                default:
                    Tracer.WriteError("Invalid functionCode: 0x{0:X}", functionCode);
                    completionCode = CompletionCode.InvalidCommand;
                    ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                    break;
            }
        }

        #endregion

        #region FRU Functions

        /// <summary>
        /// SendReceive data to read from CM FRU EEPROM
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveCmFruEepromRead(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            CompletionCode completionCode;
            const int payLoadSize = 4;
            const byte maxSingleReadByteSize = 16;

            if (request.Length != (RequestPacketUtil.requestPacketHeaderSize + payLoadSize))
            {
                completionCode = CompletionCode.InvalidCommand;
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                return;
            }

            ushort offset = BitConverter.ToUInt16(request, 3);
            ushort length = BitConverter.ToUInt16(request, 5);
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);

            if (M24C64.IsValidOffsetAndLength(offset, length) == false)
            {
                completionCode = CompletionCode.InvalidCommand;
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                return;
            }

            ushort startEepromAddress = offset;
            ushort stopEepromAddressPlusOne = (ushort)(offset + length);
            byte[] eepromAddressInTwoBytes = new byte[2];
            ushort currAddress = startEepromAddress;
            byte[] bufferForDataFromEeprom = new byte[length];
            ushort currBufferIndex = 0;

            for (ushort i = startEepromAddress; i < stopEepromAddressPlusOne; i += maxSingleReadByteSize)
            {
                byte numBytesToRead;
                byte[] command;
                byte[] receivedData;

                if ((i + maxSingleReadByteSize) < stopEepromAddressPlusOne)
                {
                    numBytesToRead = maxSingleReadByteSize;
                }
                else
                {
                    numBytesToRead = (byte)(stopEepromAddressPlusOne - i);
                }

                // High address byte is sent first, then low address byte
                eepromAddressInTwoBytes[0] = (byte)((currAddress & 0xFF00) >> 8);
                eepromAddressInTwoBytes[1] = (byte)(currAddress & 0xFF);

                completionCode = SC18IM700.GenerateReadAfterWriteCommand(deviceType, deviceId,
                    functionCode, ref eepromAddressInTwoBytes, numBytesToRead, out command);
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                    return;
                }

                // Send the read request to EEPROM
                completionCode = TryToWrite(ref command);
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                    return;
                }

                // Receive the data from EEPROM
                completionCode = TryToRead(out receivedData, numBytesToRead);
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                    return;
                }

                // Check the I2C bus status
                completionCode = DoSerialTxToCheckSC18IM700I2cBusStatus();
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                    return;
                }

                // Store the received data into the buffer
                Buffer.BlockCopy(receivedData, 0, bufferForDataFromEeprom, currBufferIndex, numBytesToRead);

                currAddress += maxSingleReadByteSize;
                currBufferIndex += maxSingleReadByteSize;
            }

            // Success path
            completionCode = CompletionCode.Success;
            ResponsePacketUtil.GenerateResponsePacket(completionCode, ref bufferForDataFromEeprom, out response);
        }

        /// <summary>
        /// SendReceive data to write to CM FRU EEPROM
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveCmFruEepromWrite(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            CompletionCode completionCode;
            const int offsetAndLengthByteCount = 4;
            const byte maxSingleWriteByteSize = 8;
            // A single page write should not roll over 32 byte boundary
            const int pageWriteBoundaryInByte = 32;
            const int timeToSleepAfterPageWriteInMs = 20;

            if (request.Length <= (RequestPacketUtil.requestPacketHeaderSize + offsetAndLengthByteCount))
            {
                completionCode = CompletionCode.InvalidCommand;
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                return;
            }

            ushort offset = BitConverter.ToUInt16(request, 3);
            ushort length = BitConverter.ToUInt16(request, 5);
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);

            if ((request.Length != (RequestPacketUtil.requestPacketHeaderSize + offsetAndLengthByteCount + length)) ||
                (M24C64.IsValidOffsetAndLength(offset, length) == false))
            {
                completionCode = CompletionCode.InvalidCommand;
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                return;
            }

            ushort startEepromAddress = offset;
            ushort stopEepromAddressPlusOne = (ushort)(offset + length);
            ushort currAddress;
            ushort currIndexInRequestPacket = RequestPacketUtil.requestPacketHeaderSize + offsetAndLengthByteCount;
            byte numBytesToWrite = maxSingleWriteByteSize;

            for (ushort i = startEepromAddress; i < stopEepromAddressPlusOne; i += numBytesToWrite)
            {
                byte[] command;
                const int eepromAddressSizeInBytes = 2;
                int currLocationWithinPageWriteBoundary;
                currAddress = i;

                if ((currAddress + maxSingleWriteByteSize) < stopEepromAddressPlusOne)
                {
                    numBytesToWrite = maxSingleWriteByteSize;
                }
                else
                {
                    numBytesToWrite = (byte)(stopEepromAddressPlusOne - currAddress);
                }

                currLocationWithinPageWriteBoundary = currAddress % pageWriteBoundaryInByte;

                // If the page write is going to roll over the page write boundary, 
                // adjust numBytesToWrite
                if ((currLocationWithinPageWriteBoundary + numBytesToWrite) > pageWriteBoundaryInByte)
                {
                    numBytesToWrite = (byte)(pageWriteBoundaryInByte - currLocationWithinPageWriteBoundary);
                }

                // Address: 2 bytes
                byte[] eepromAddressAndData = new byte[numBytesToWrite + eepromAddressSizeInBytes];
                Buffer.BlockCopy(request, currIndexInRequestPacket, eepromAddressAndData, eepromAddressSizeInBytes, numBytesToWrite);

                // High address byte is sent first, then low address byte
                eepromAddressAndData[0] = (byte)((currAddress & 0xFF00) >> 8);
                eepromAddressAndData[1] = (byte)(currAddress & 0xFF);

                completionCode = SC18IM700.GenerateWriteNBytesToSlaveDeviceCommand(deviceType, deviceId,
                    functionCode, ref eepromAddressAndData, out command);

                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                    return;
                }

                // Send the write request with the data
                completionCode = TryToWrite(ref command);
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                    return;
                }

                // Wait until the page write gets propagated to EEPROM
                Thread.Sleep(timeToSleepAfterPageWriteInMs);

                // Check the I2C bus status
                completionCode = DoSerialTxToCheckSC18IM700I2cBusStatus();
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                    return;
                }

                currIndexInRequestPacket += numBytesToWrite;
            }

            // Success path
            completionCode = CompletionCode.Success;
            ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
        }

        /// <summary>
        /// SendReceive data to/from CM FRU EEPROM
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveCmFruEeprom(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            CompletionCode completionCode;
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);
            switch (functionCode)
            {
                case (byte)FunctionCode.ReadEeprom:
                    SendReceiveCmFruEepromRead(deviceType, deviceId, ref request, out response);
                    break;
                case (byte)FunctionCode.WriteEeprom:
                    SendReceiveCmFruEepromWrite(deviceType, deviceId, ref request, out response);
                    break;
                default:
                    Tracer.WriteError("Invalid functionCode: 0x{0:X}", functionCode);
                    completionCode = CompletionCode.InvalidCommand;
                    ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                    break;
            }
        }

        #endregion

        #region Console Functions

        /// <summary>
        /// Handle the console open request
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveOpenConsole(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            CompletionCode completionCode;
   
            if (deviceType == (byte)DeviceType.SerialPortConsole)
            {
                int commTimeoutIn1ms;
                int baudrate;

                try
                {
                    if (request == null)
                    {
                        // should never occur.  request should always be populated with baud rate and timeout values.
                        Tracer.WriteError("Invalid Open Serial Console Port request packet - Baud rate and timeout values not specified");
                        completionCode = CompletionCode.InvalidCommand;
                    }
                    else if (request.Length < 3+8)
                    {
                        // should never occur.  request should always be populated with baud rate and timeout values.
                        Tracer.WriteError("Invalid Open Serial Console Port request packet - Baud rate or timeout value not specified");
                        completionCode = CompletionCode.InvalidCommand;
                    }
                    else
                    {
                        // The first 3 bytes have default packet info including function code and packet length
                        // The next 4 bytes correspond to integer baud rate
                        baudrate = BitConverter.ToInt32(request, 3);
                        // The last 4 bytes correspond to integer baud timeout value
                        commTimeoutIn1ms = BitConverter.ToInt32(request, 3+4);

                        serialPort.BaudRate = baudrate;
                        serialPort.Parity = Parity.None;
                        serialPort.StopBits = StopBits.One;
                        serialPort.DataBits = 8;
                        serialPort.Handshake = Handshake.None;
                        serialPort.RtsEnable = false;

                        if (commTimeoutIn1ms <= 0)
                        {
                            serialPort.ReadTimeout = SerialPort.InfiniteTimeout;
                            serialPort.WriteTimeout = SerialPort.InfiniteTimeout;
                        }
                        else
                        {
                            serialPort.ReadTimeout = commTimeoutIn1ms;
                            serialPort.WriteTimeout = commTimeoutIn1ms;
                        }

                        serialPort.Open();
                        completionCode = CompletionCode.Success;
                    }
                }
                catch (Exception e)
                {
                    Tracer.WriteError(e);
                    completionCode = CompletionCode.FailToOpenSerialPort;
                    Tracer.WriteError("SendReceiveOpenConsole: Failed to open {0}", serialPort.PortName);
                }
            }
            else if (deviceType == (byte)DeviceType.BladeConsole)
            {
                // No-op
                completionCode = CompletionCode.Success;
            }
            else
            {
                Tracer.WriteError("Invalid deviceType: 0x{0:X}", deviceType);
                completionCode = CompletionCode.InvalidCommand;
            }

            // Fall through for all execution paths
            ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
        }

        /// <summary>
        /// Handle the console close request
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveCloseConsole(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            CompletionCode completionCode;

            if (deviceType == (byte)DeviceType.SerialPortConsole)
            {
                try
                {
                    serialPort.Close();
                    completionCode = CompletionCode.Success;
                }
                catch (Exception e)
                {
                    Tracer.WriteError(e);
                    completionCode = CompletionCode.FailToCloseSerialPort;
                    Tracer.WriteError("SendReceiveCloseConsole: Failed to close {0}", serialPort.PortName);
                }
            }
            else if (deviceType == (byte)DeviceType.BladeConsole)
            {
                // No-op
                completionCode = CompletionCode.Success;
            }
            else
            {
                Tracer.WriteError("Invalid deviceType: 0x{0:X}", deviceType);
                completionCode = CompletionCode.InvalidCommand;
            }

            // Fall through for all execution paths
            ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
        }

        /// <summary>
        /// Handle the console send request
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveSendConsole(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            CompletionCode completionCode;
            if (request.Length > 3)
            {
                completionCode = TryToWrite(ref request, 3, request.Length - 3);
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    Tracer.WriteError("SendReceiveSendConsole failed (completionCode: 0x{0:X})", completionCode);
                }
            }
            else
            {
                Tracer.WriteError("SendReceiveSendConsole failed due to the invalid request length ({0})", request.Length);
                completionCode = CompletionCode.InvalidCommand;
            }
            ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
        }

        /// <summary>
        /// Handle the console receive request
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveReceiveConsole(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            CompletionCode completionCode;
            byte[] dataReceivedFromConsole = null;
            // TODO: this parameter should be defined in the configuration file
            const int maxDataFromSingleRead = 1024;
            completionCode = TryToRead(out dataReceivedFromConsole, maxDataFromSingleRead, false);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
            }
            else
            {
                completionCode = CompletionCode.Success;
                ResponsePacketUtil.GenerateResponsePacket(completionCode, ref dataReceivedFromConsole, out response);
            }
        }

        /// <summary>
        /// Handle console (blade, serial port) requests
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveConsole(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            CompletionCode completionCode;
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);
            switch (functionCode)
            {
                case (byte)FunctionCode.OpenConsole:
                    SendReceiveOpenConsole(deviceType, deviceId, ref request, out response);
                    break;
                case (byte)FunctionCode.CloseConsole:
                    SendReceiveCloseConsole(deviceType, deviceId, ref request, out response);
                    break;
                case (byte)FunctionCode.SendConsole:
                    SendReceiveSendConsole(deviceType, deviceId, ref request, out response);
                    break;
                case (byte)FunctionCode.ReceiveConsole:
                    SendReceiveReceiveConsole(deviceType, deviceId, ref request, out response);
                    break;
                default:
                    Tracer.WriteError("Invalid functionCode: 0x{0:X}", functionCode);
                    completionCode = CompletionCode.InvalidCommand;
                    ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                    break;
            }
        }

        #endregion

        #region SC18IM700

        /// <summary>
        /// Reset SC18IM700 Master Chip
        /// </summary>
        private CompletionCode DoInitializeSC18IM700Reset()
        {

            Tracer.WriteError("DoInitializeSC18IM700Reset Called");

            CompletionCode completionCode;
            //const int timeToSleepInMsAfterClearingBuffers = 100;

            if (serialPort.PortName == null)
            {
                Tracer.WriteError("Serial port name is null (logicalPortId: {0})", logicalPortId);
                completionCode = CompletionCode.CommDevFailedToInit;
                return completionCode;
            }

            // Only reinitialize COM4
            if (serialPort.PortName == "COM4")
            {
                // initialize new port.
                serialPort.BaudRate = 9600;
                serialPort.RtsEnable = false;

                ToggleDtrValueToConnectToSC18IM700();

                // Step 1: Do the phase 1 of SC18IM700 initialization
                // this step will reset the SC18IM700 with RtsEnable.
                completionCode = InitializeSC18IM700Phase1();
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    Tracer.WriteError("ResetSC18IM700Phase1 failed (code: 0x{0:X})", (byte)completionCode);
                    return completionCode;
                }

                // Step 2: Set the baud rate to 115200 bps. 
                // The baud rate of SC18IM700 has been set to 115200 in step 1
                serialPort.BaudRate = 115200;

                // Step 3: Do the phase 2 of SC18IM700 initialization
                // PWM has been set to max in Step 3, so FAN_MAX_CTL can be cleared
                completionCode = InitializeSC18IM700Phase2();
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    Tracer.WriteError("ResetSC18IM700Phase2 failed (code: 0x{0:X})", (byte)completionCode);
                    return completionCode;
                }

                // Step 4: If COM4, allocate and initialize the per-server IPMI sequence ID array
                perServerIpmiSequenceId = new byte[CommunicationDevice.maxNumServersPerChassis];
                for (int i = 0; i < CommunicationDevice.maxNumServersPerChassis; i++)
                {
                    perServerIpmiSequenceId[i] = 0;
                }

                Tracer.WriteWarning("DoInitializeSC18IM700Reset Complete (code: 0x{0:X})", (byte)completionCode);

                return completionCode;
            }
            else
            {
                Tracer.WriteError("DoInitializeSC18IM700Reset Invalid Port {0}", serialPort.PortName);
                return CompletionCode.UnspecifiedError;
            }
        }

        /// <summary>
        /// Do serial transactions to check I2C bus status
        /// </summary>
        /// <returns></returns>
        private CompletionCode DoSerialTxToCheckSC18IM700I2cBusStatus()
        {
            CompletionCode completionCode;
            byte[] commandPacket;
            byte[] registerAddress = new byte[1];
            registerAddress[0] = (byte)SC18IM700.RegisterAddress.I2CStat;
            completionCode = SC18IM700.GenerateReadFromInternalRegisterCommand(ref registerAddress, out commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            completionCode = TryToWrite(ref commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            byte[] singeByteBuffer;
            completionCode = TryToRead(out singeByteBuffer, 1);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            completionCode = SC18IM700.CheckI2cBusStatusRegister(singeByteBuffer[0]);
            return completionCode;
        }

        /// <summary>
        /// Do serial transactions to read from SC18IM700 GPIO
        /// </summary>
        /// <param name="gpioReading"></param>
        /// <returns></returns>
        private CompletionCode DoSerialTxToReadSC18IM700Gpio(out byte gpioReading)
        {
            CompletionCode completionCode;
            byte[] commandPacket;
            byte[] singleByteData;
            gpioReading = 0;

            completionCode = SC18IM700.GenerateReadFromGpioPortCommand(out commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            completionCode = TryToWrite(ref commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            completionCode = TryToRead(out singleByteData, 1);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }

            // Success path
            completionCode = CompletionCode.Success;
            gpioReading = singleByteData[0];
            return completionCode;
        }

        private CompletionCode DoSerialTxToWriteSC18IM700Gpio(byte newGpioValue)
        {
            CompletionCode completionCode;
            byte[] commandPacket;

            completionCode = SC18IM700.GenerateWriteToGpioPortCommand(newGpioValue, out commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            completionCode = TryToWrite(ref commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            return completionCode;
        }

        /// <summary>
        /// Do serial transactions to initialize SC18IM700 internal registers
        /// </summary>
        /// <returns></returns>
        private CompletionCode DoSerialTxToInitSC18IM700InternalRegisters()
        {
            CompletionCode completionCode;
            byte[] commandPacket;

            completionCode = SC18IM700.GenerateInternalRegistersInitCommand(out commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("Failed to GenerateInternalRegistersInitCommand (code: 0x{0:X})", (byte)completionCode);
                return completionCode;
            }
            completionCode = TryToWrite(ref commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("Failed to TryToWrite (code: 0x{0:X})", (byte)completionCode);
                return completionCode;
            }
            return completionCode;
        }

        /// <summary>
        /// Initialize GPIO and configuration registers
        /// For all output pins, GPIO should be initialized first before changing the pin
        /// into output mode
        /// </summary>
        /// <returns></returns>
        private CompletionCode DoSerialTxToInitSC18IM700GpioAndConfigRegisters()
        {
            CompletionCode completionCode;
            byte[] commandPacket;

            completionCode = SC18IM700.GenerateGpioInitCommand(serialPort.PortName, out commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            completionCode = TryToWrite(ref commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }

            completionCode = SC18IM700.GenerateConfigRegInitCommand(serialPort.PortName, out commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            completionCode = TryToWrite(ref commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            return completionCode;
        }

        /// <summary>
        /// Initialize GPIO and configuration registers for PM bus hub bits
        /// to work around PM bus/I2C signal issue
        /// </summary>
        /// <returns></returns>
        private CompletionCode DoSerialTxToInitSC18IM700PmBusHub()
        {
            CompletionCode completionCode;
            byte[] commandPacket;

            completionCode = SC18IM700.GenerateGpioInitCommandForPmBus(out commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("GenerateGpioInitCommandForPmBus failed (code: 0x{0:X})", completionCode);
                return completionCode;
            }
            completionCode = TryToWrite(ref commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("TryToWrite failed (code: 0x{0:X})", completionCode);
                return completionCode;
            }

            completionCode = SC18IM700.GenerateConfigRegHubInitCommandForPmBus(out commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("GenerateConfigRegHubInitCommandForPmBus failed (code: 0x{0:X})", completionCode);
                return completionCode;
            }
            completionCode = TryToWrite(ref commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("TryToWrite failed (code: 0x{0:X})", completionCode);
                return completionCode;
            }
            return completionCode;
        }

        /// <summary>
        /// Initialize SC18IM700 (Phase 1)
        /// 1. Reset SC18IM700
        /// 2. Initialize internal registers (BRGs, ICClks, ICTO)
        /// </summary>
        /// <returns></returns>
        private CompletionCode InitializeSC18IM700Phase1()
        {
            CompletionCode completionCode;
            const byte asciiO = 0x4F;
            const byte asciiK = 0x4B;
            byte[] twoByteData;
            const double timeDelayInMsToResetSC18IM700 = 2.0;
            const int timeDelayInMsBeforeAfterResettingSC18IM700 = 100;
            const int numItersForDelay = 5000000;
            Stopwatch sw = new Stopwatch();

            // Clear buffers to discard any garbage data
            ClearInOutBuffers();

            // Step 1: Reset SC18IM700
            serialPort.RtsEnable = false;

            // sleep after clearing buffers and setting RTS.
            Thread.Sleep(timeDelayInMsBeforeAfterResettingSC18IM700);

            serialPort.RtsEnable = true;
            sw.Start();
            for (int i = 0; i < numItersForDelay; i++)
            {
                if (sw.Elapsed.TotalMilliseconds >= timeDelayInMsToResetSC18IM700)
                {
                    break;
                }
            }
            sw.Stop();
            serialPort.RtsEnable = false;

            Thread.Sleep(timeDelayInMsBeforeAfterResettingSC18IM700);

            // Step 2: Receive two ASCII chars ('O', 'K') from SC18IM700
            completionCode = TryToRead(out twoByteData, 2);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("UART: {0} Failed to reset SC18IM700. Completion Code: 0x{1:X}", 
                    serialPort.PortName, completionCode);
                return completionCode;
            }

            if ((twoByteData[0] != asciiO) ||
                (twoByteData[1] != asciiK))
            {
                Tracer.WriteError("InitializeSC18IM700Phase1 Failed: The two bytes received from SC18IM700 are not 'OK': 0x{0:X} 0x{1:X}", 
                    twoByteData[0], twoByteData[1]);
                completionCode = CompletionCode.CommDevFailedToInit;
                return completionCode;
            }

            // Step 3: Inialize internal registers
            completionCode = DoSerialTxToInitSC18IM700InternalRegisters();
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("Failed to intialize SC18IM700 internal registers");
                return completionCode;
            }

            // Wait until SC18IM700 changes its baud rate
            Thread.Sleep(timeDelayInMsBeforeChangingBaudRate);

            return completionCode;
        }

        /// <summary>
        /// Initialize SC18IM700 (phase 2)
        /// GPIO and configuration registers
        /// </summary>
        /// <returns></returns>
        private CompletionCode InitializeSC18IM700Phase2()
        {
            CompletionCode completionCode;

            // Clear buffers to discard any garbage data
            ClearInOutBuffers();

            // Step 1: Inialize GPIO and configuration registers
            completionCode = DoSerialTxToInitSC18IM700GpioAndConfigRegisters();
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("COM: {0} Failed to intialize SC18IM700 internal registers", serialPort.PortName);
                return completionCode;
            }
            return completionCode;
        }

        /// <summary>
        /// Initialize SC18IM700 PM bus hub bits to "000"
        /// to work around PM bus/I2C signal issue
        /// </summary>
        /// <returns></returns>
        private CompletionCode InitializeSC18IM700ForPmBusHub()
        {
            CompletionCode completionCode;

            completionCode = DoSerialTxToInitSC18IM700PmBusHub();
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("DoSerialTxToInitSC18IM700PmBusHub failed");
                return completionCode;
            }
            return completionCode;
        }

        #endregion

        #region PCA9535

        /// <summary>
        /// Check if the pin is logically set
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void SendReceiveToCheckIfPCA9535PinIsLogicallySet(byte deviceType, byte deviceId, ref byte[] request, out byte[] response)
        {
            CompletionCode completionCode;
            byte currRegValue;
            byte[] singleByteData = new byte[1];
            byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);
            bool isLogicallySet;

            // Step 1: Read the register
            completionCode = DoSerialTxToReadPCA9535Register(deviceType, deviceId, functionCode, out currRegValue);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("DoSerialTxToReadPCA9535Register failed (code: 0x{0:X})", completionCode);
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                return;
            }

            // Step 2: Interpret the data
            completionCode = PCA9535.IsPinLogicallySet(deviceType, deviceId, currRegValue, out isLogicallySet);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("PCA9535.IsPinLogicallySet failed (code: 0x{0:X})", completionCode);
                ResponsePacketUtil.GenerateResponsePacket(completionCode, out response);
                return;
            }

            if (isLogicallySet == true)
            {
                // The pin is logically set
                singleByteData[0] = 1;
            }
            else
            {
                // The pin is logically clear
                singleByteData[0] = 0;
            }

            // Step 3: Generate the response packet (success path)
            completionCode = CompletionCode.Success;
            ResponsePacketUtil.GenerateResponsePacket(completionCode, ref singleByteData, out response);
        }

        /// <summary>
        /// Do serial transactions to read from a PCA9535 register
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="regReading"></param>
        /// <returns></returns>
        private CompletionCode DoSerialTxToReadPCA9535Register(byte deviceType, byte deviceId, byte functionCode, out byte regReading)
        {
            byte address;
            PCA9535.Command pca9535Command;
            regReading = 0;
            CompletionCode completionCode; 
            completionCode = SC18IM700.GetSlaveDeviceAddress(deviceType, deviceId, functionCode, out address);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("GetSlaveDeviceAddress failed");
                return completionCode;
            }
            completionCode = PCA9535.GetCommand(deviceType, deviceId, out pca9535Command);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("GetCommand failed");
                return completionCode;
            }
            return DoSerialTxToReadPCA9535Register(address, pca9535Command, out regReading);
        }

        /// <summary>
        /// Do serial transactions to read from a PCA9535 register
        /// </summary>
        /// <param name="address"></param>
        /// <param name="pca9535Command"></param>
        /// <param name="regReading"></param>
        /// <returns></returns>
        private CompletionCode DoSerialTxToReadPCA9535Register(byte address, PCA9535.Command pca9535Command, out byte regReading)
        {
            byte[] commandPacket;
            byte[] readRegCommand = new byte[1];
            byte[] regDataReceived;
            CompletionCode completionCode;

            regReading = 0;
            commandPacket = null;
            readRegCommand[0] = (byte)pca9535Command;
            completionCode = SC18IM700.GenerateReadAfterWriteCommand(address, ref readRegCommand, 1, out commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("SC18IM700.GenerateReadAfterWriteCommand failed (code: 0x{0:X})", (byte)completionCode);
                return completionCode;
            }

            // Step 1: Send the read-after-write command
            completionCode = TryToWrite(ref commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("TryToWrite failed (code: 0x{0:X})", (byte)completionCode);
                return completionCode;
            }

            // Step 2: Receive the register data 
            completionCode = TryToRead(out regDataReceived, 1);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("TryToRead failed (code: 0x{0:X})", (byte)completionCode);
                return completionCode;
            }

            // Step 3: Check I2C bus status
            completionCode = DoSerialTxToCheckSC18IM700I2cBusStatus();
            if (CompletionCodeChecker.Failed(completionCode))
            {
                Tracer.WriteError("I2C bus status failed");
            }

            // Success path 
            completionCode = CompletionCode.Success;
            regReading = regDataReceived[0];
            return completionCode;
        }

        /// <summary>
        /// Do serial transactions to write to a PCA9535 register
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="pca9535CommandAndData"></param>
        /// <returns></returns>
        private CompletionCode DoSerialTxToWritePCA9535Register(byte deviceType, byte deviceId, byte functionCode, ref byte[] pca9535CommandAndData)
        {
            byte address;
            CompletionCode completionCode = SC18IM700.GetSlaveDeviceAddress(deviceType, deviceId, functionCode, out address);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            return DoSerialTxToWritePCA9535Register(address, ref pca9535CommandAndData);
        }

        /// <summary>
        /// Do serial transactions to write to a PCA9535 register
        /// </summary>
        /// <param name="address"></param>
        /// <param name="pca9535CommandAndData"></param>
        /// <returns></returns>
        private CompletionCode DoSerialTxToWritePCA9535Register(byte address, ref byte[] pca9535CommandAndData)
        {
            CompletionCode completionCode;
            byte[] commandPacket;

            // Step 1: Write to the PCA9535 register
            completionCode = SC18IM700.GenerateWriteNBytesToSlaveDeviceCommand(address, ref pca9535CommandAndData, out commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            completionCode = TryToWrite(ref commandPacket);

            // Step 2: Check I2C bus status
            completionCode = DoSerialTxToCheckSC18IM700I2cBusStatus();
            if (CompletionCodeChecker.Failed(completionCode))
            {
                Tracer.WriteError("I2C bus status failed");
            }

            return completionCode;
        }

        /// <summary>
        /// Do serial transactions to set/clear a single bit in a PCA9535 output register
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="isToSet"></param>
        /// <returns></returns>
        private CompletionCode DoSerialTxToSetOrClearPCA9535OutputRegBit(byte deviceType, byte deviceId, byte functionCode, bool isToSet)
        {
            CompletionCode completionCode;
            byte[] pca9535CommandAndData;
            byte currOutputRegValue;

            // Step 1: Get current 8-bit output register value
            completionCode = DoSerialTxToReadPCA9535Register(deviceType, deviceId, functionCode, out currOutputRegValue);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                Tracer.WriteError("DoSerialTxToReadPCA9535Register failed");
                return completionCode;
            }

            // Step 2: Write to the output port register
            completionCode = PCA9535.GenerateCommandAndDataToSetOrClearSingleBit(deviceType, deviceId, currOutputRegValue, isToSet, out pca9535CommandAndData);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            completionCode = DoSerialTxToWritePCA9535Register(deviceType, deviceId, functionCode, ref pca9535CommandAndData);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }

            return completionCode;
        }

        /// <summary>
        /// Do serial transactions to initialize PCA9535 registers
        /// </summary>
        /// <returns></returns>
        private CompletionCode DoSerialTxToInitPCA9535Registers()
        {
            CompletionCode completionCode = CompletionCode.CommDevFailedToInit;

            foreach (byte address in Enum.GetValues(typeof(I2cAddresses.addrsOfPCA9535Devices)))
            {
                for (int regPort = 0; regPort < PCA9535.numPorts; regPort++)
                {
                    PCA9535.Command pca9535Command = PCA9535.Command.ConfigurationPort1;
                    if (regPort == 0)
                    {
                        pca9535Command = PCA9535.Command.ConfigurationPort0;
                    }

                    // Step 1: Read the configuration register to see if it has been already initialized
                    byte currConfigRegReading;

                    completionCode = DoSerialTxToReadPCA9535Register(address, pca9535Command, out currConfigRegReading);
                    if (CompletionCodeChecker.Failed(completionCode) == true)
                    {
                        Tracer.WriteError("DoSerialTxToReadPCA9535Register failed (code: 0x{0:X})", (byte)completionCode);
                        return completionCode;
                    }
                    if (currConfigRegReading != PCA9535.defaultValueOfConfigReg)
                    {
                        // This configuration register has been already initialized, so skip this
                        continue;
                    }

                    // Step 2: For all PCA9535s that are not an input-only device,
                    // initialize configuration registers to put all the pins to the output mode
                    if (address != I2cAddresses.addrInputDevices)
                    {
                        byte[] commandAndDataToWriteRegister = new byte[2];
                        commandAndDataToWriteRegister[0] = (byte)pca9535Command;
                        commandAndDataToWriteRegister[1] = 0x0;
                        completionCode = DoSerialTxToWritePCA9535Register(address, ref commandAndDataToWriteRegister);
                        if (CompletionCodeChecker.Failed(completionCode) == true)
                        {
                            Tracer.WriteError("DoSerialTxToWritePCA9535Register failed (code: 0x{0:X})", (byte)completionCode);
                            return completionCode;
                        }
                    }
                }
            }

            return completionCode;
        }

        private CompletionCode InitializePCA9535s()
        {
            CompletionCode completionCode;
            completionCode = DoSerialTxToInitPCA9535Registers();
            return completionCode;
        }

        #endregion

        #region ADT7470

        /// <summary>
        /// Do serial transactions to get a tach byte from ADT7470
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="adt7470command"></param>
        /// <param name="tachByte"></param>
        /// <returns></returns>
        private CompletionCode DoSerialTxToGetADT7470TachByte(byte deviceType, byte deviceId, byte functionCode, ADT7470.Command adt7470command, out byte tachByte)
        {
            CompletionCode completionCode;
            byte[] singleByteIoBuffer = new byte[1];
            byte[] commandPacket;
            tachByte = 0;

            if ((adt7470command != ADT7470.Command.GetTachLowByte) &&
                (adt7470command != ADT7470.Command.GetTachHighByte))
            {
                completionCode = CompletionCode.InvalidCommand;
                return completionCode;
            }

            singleByteIoBuffer[0] = ADT7470.GetRegisterAddress(deviceId, adt7470command);
            if (ADT7470.IsValidRegisterAddress(singleByteIoBuffer[0]) == false)
            {
                completionCode = CompletionCode.InvalidCommand;
                return completionCode;
            }

            completionCode = SC18IM700.GenerateReadAfterWriteCommand(deviceType, deviceId, functionCode, ref singleByteIoBuffer, 1, out commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }

            completionCode = TryToWrite(ref commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            completionCode = TryToRead(out singleByteIoBuffer, 1);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }
            tachByte = singleByteIoBuffer[0];

            // Check the I2C bus status
            completionCode = DoSerialTxToCheckSC18IM700I2cBusStatus();
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }

            return completionCode;
        }

        /// <summary>
        /// Do serial transactions to write to the PWM register
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="pwm">0 to 100 (%)</param>
        /// <returns></returns>
        private CompletionCode DoSerialTxToWriteADT7470PwmRegister(byte deviceType, byte deviceId, byte pwm)
        {
            CompletionCode completionCode;
            byte[] commandPacket;
            byte[] registerAddressAndData = new byte[2];

            if (ADT7470.IsValidInputPwmValue(pwm) == false)
            {
                Tracer.WriteError("Invalid PWM: {0}", pwm);
                completionCode = CompletionCode.InvalidDataFieldInRequest;
                return completionCode;
            }

            // Step 1: Send data to set the fan speed
            registerAddressAndData[0] = ADT7470.GetRegisterAddress(deviceId, ADT7470.Command.SetPwmDutyCycle);
            if (ADT7470.IsValidRegisterAddress(registerAddressAndData[0]) == false)
            {
                completionCode = CompletionCode.InvalidCommand;
                return completionCode;
            }
            registerAddressAndData[1] = (byte)ADT7470.ScalePwmWithIncreaseStepParameter(pwm);
            completionCode = SC18IM700.GenerateWriteNBytesToSlaveDeviceCommand(deviceType, deviceId, 
                (byte)FunctionCode.SetFanSpeed, ref registerAddressAndData, out commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }

            completionCode = TryToWrite(ref commandPacket);
            if (CompletionCodeChecker.Failed(completionCode) == true)
            {
                return completionCode;
            }

            // Step 2: Check I2C stat to see if the transaction has been successful
            completionCode = DoSerialTxToCheckSC18IM700I2cBusStatus();
            return completionCode;
        }

        private CompletionCode InitializeADT7470s()
        {
            CompletionCode completionCode;
            // The speed of all fans is controlled by a single PWM register.
            // Thus the device ID does not matter and is set to 1
            const byte deviceId = 1;
            // pwm is 0 to 100 scale
            const byte maxPwm = 100;
            // Clear buffers to discard any garbage data
            ClearInOutBuffers();
            completionCode = DoSerialTxToWriteADT7470PwmRegister((byte)DeviceType.Fan, deviceId, maxPwm);
            return completionCode;
        }

        #endregion

        /// <summary>
        /// Clear in/out (software) buffers to ensure that there is no garbage data
        /// due to the previous (failed) serial transaction
        /// </summary>
        private void ClearInOutBuffers()
        {
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
        }

        #region Serial Read/Write

        /// <summary>
        /// Write N bytes over serial.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private CompletionCode TryToWrite(ref byte[] packet, int offset, int length)
        {
            CompletionCode completionCode;

            if (packet == null)
            {
                completionCode = CompletionCode.InvalidRequestDataLength;
                Tracer.WriteError("[SerialPortManager] null data packet in TryToWrite");
            }
            else if (packet.Length < (offset + length))
            {
                completionCode = CompletionCode.InvalidRequestDataLength;
                Tracer.WriteError("[SerialPortManager] invalid offset/length in TryToWrite");
            }
            else
            {
                try
                {
                    serialPort.Write(packet, offset, length);
                    completionCode = CompletionCode.Success;
                }
                catch (TimeoutException te)
                {
                    completionCode = CompletionCode.Timeout;
                    Tracer.WriteError(te);
                }
                catch (Exception e)
                {
                    completionCode = CompletionCode.SerialPortOtherErrors;
                    Tracer.WriteError(e);
                }
            }

            return completionCode;
        }

        private CompletionCode TryToWrite(ref byte[] packet)
        {
            return TryToWrite(ref packet, 0, packet.Length);
        }

        /// <summary>
        /// Read maxLength bytes over serial if isFixedLength set to true (or serial time out).
        /// Otherwise, it returns with the output buffer that contains the bytes
        /// that have been received over the serial so far (if serial times out)
        /// </summary>
        /// <param name="output"></param>
        /// <param name="maxLength"></param>
        /// <param name="isFixedLength"></param>
        /// <returns></returns>
        private CompletionCode TryToRead(out byte[] output, int maxLength, bool isFixedLength = true)
        {
            CompletionCode completionCode;
            int readByteCount = 0;
            int dataInInt;

            output = null;
            output = new byte[maxLength];

            try
            {
                while (readByteCount < maxLength)
                {
                    dataInInt = serialPort.ReadByte();

                    if (dataInInt == -1)
                    {
                        completionCode = CompletionCode.SerialPortOtherErrors;
                        Tracer.WriteError("[SerialPortManager] End of stream has been read in TryToRead");
                        break;
                    }
                    else
                    {
                        output[readByteCount] = (byte)dataInInt;
                        readByteCount++;
                    }
                }

                completionCode = CompletionCode.Success;
            }
            catch (TimeoutException te)
            {
                if (isFixedLength == true)
                {
                    // If the flag is set, consider the time-out exception as an error
                    completionCode = CompletionCode.Timeout;
                    Tracer.WriteError(te);
                }
                else
                {
                    if (readByteCount == 0)
                    {
                        // If the flag is reset but no data has been received,
                        // return with the time-out completion code
                        completionCode = CompletionCode.Timeout;
                        output = null;
                    }
                    else
                    {
                        // If any data has been received, resize the array to the
                        // actual size and return with the success completion code
                        completionCode = CompletionCode.Success;
                        Array.Resize(ref output, readByteCount);
                    }
                }
            }
            catch (Exception e)
            {
                completionCode = CompletionCode.SerialPortOtherErrors;
                Tracer.WriteError(e);
            }

            return completionCode;
        }

        /// <summary>
        /// Read with start/stop stop bytes (or timeout)
        /// </summary>
        /// <param name="output">[startByte][data1]...[dataN][stopByte]</param>
        /// <returns></returns>
        private CompletionCode TryToReadWithStartStopBytes(out byte[] output, byte startByte, byte stopByte, byte ipmiSequenceId)
        {
            CompletionCode completionCode;
            List<byte> receivedBytes = new List<byte>();
            const int maxDataPacketByteCount = 128;
            const int maxGarbageDataByteCount = 128;
            int garbageDataByteCount = 0;
            // Once the start byte has been received, this flag variable is set
            bool hasStartByteBeenReceived = false;
            // indicates the Bmc responded with buffers clear, ready for command.
            bool hasHandShake = false;
            output = null;

            while (true)
            {
                try
                {
                    int receivedData = serialPort.ReadByte();
                    if (receivedData == -1)
                    {
                        // The end of the stream has been read
                        completionCode = CompletionCode.SerialPortOtherErrors;
                        Tracer.WriteError("End of stream has been read");
                        return completionCode;
                    }
                    else
                    {
                        byte receivedDataInByte = (byte)receivedData;
                        if (hasStartByteBeenReceived == false)
                        {
                            if (receivedDataInByte == startByte)
                            {
                                hasStartByteBeenReceived = true;
                                receivedBytes.Add(receivedDataInByte);
                            }
                            else if (receivedDataInByte == handshake)
                            {
                                hasHandShake = true;
                            }
                            else
                            {
                                // Discard all the incoming garbage bytes until receiving the start byte.
                                // If receiving too many garbage data bytes, return with error code to ensure
                                // forward progress
                                garbageDataByteCount++;
                                if (garbageDataByteCount > maxGarbageDataByteCount)
                                {
                                    completionCode = CompletionCode.SerialPortOtherErrors;
                                    Tracer.WriteError("Start byte has not been received");
                                    return completionCode;
                                }
                            }
                        }
                        else
                        {
                            receivedBytes.Add(receivedDataInByte);
                            if (receivedBytes.Count > maxDataPacketByteCount)
                            {
                                // If the data packet size is too large, return with an error code
                                completionCode = CompletionCode.SerialPortOtherErrors;
                                Tracer.WriteError("Received data packet size is too big");
                                return completionCode;
                            }
                            if (receivedDataInByte == stopByte)
                            {
                                // If the stop byte has been received, validate the sequence Id 
                                // serialize the packet and return it with the success code
                                // Received data packet: [startByte][data1]...[dataN][stopByte]
                                if (receivedBytes.Count > 5)
                                {
                                    // ensure packet sequence matches. it is possible late ipmi responses
                                    // from previous commands can enter the UART buffer, provided the M700
                                    // selection is directed at the same server.
                                    if (receivedBytes[5] == ipmiSequenceId)
                                    {
                                        output = receivedBytes.ToArray();
                                        completionCode = CompletionCode.Success;
                                        return completionCode;
                                    }
                                    // Wrong ipmi payload has been received. Mismatched SequenceId
                                    else
                                    {
                                        // Flush the list of collected bytes.
                                        receivedBytes.Clear();

                                        // Set the start by to false to begin collecting bytes again.
                                        hasStartByteBeenReceived = false;
                                    }
                                }
                                else
                                {
                                    // if stop byte is received and packet lenght is malformed,
                                    // return payload with failed response.
                                    output = receivedBytes.ToArray();
                                    completionCode = CompletionCode.ResponseNotProvided;
                                    return completionCode;
                                }
                            }
                        }
                    }
                }
                catch (TimeoutException te)
                {
                    if (hasHandShake)
                    {
                        // If serial time out occured, but BMC sent handshake charactor, 
                        // return with the timeout error code.
                        completionCode = CompletionCode.IpmiTimeOutHandShake;
                    }
                    else
                    {
                        // If serial time out occurs, return with the timeout error code
                        completionCode = CompletionCode.Timeout;
                    }
                    Tracer.WriteError(te);
                    return completionCode;
                }
                catch (Exception e)
                {
                    completionCode = CompletionCode.SerialPortOtherErrors;
                    Tracer.WriteError(e);
                    return completionCode;
                }
            }
        }

        #endregion
    }
}
