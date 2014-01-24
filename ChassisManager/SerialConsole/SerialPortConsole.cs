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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.GFS.WCS.ChassisManager
{
    public class SerialPortConsole : ChassisSendReceive
    {
        /// <summary>
        ///  Port id
        /// </summary>
        protected byte portId;

        /// <summary>
        ///  Define Device Type
        /// </summary>
        protected static DeviceType serialPortConsoleDeviceType;

        public SerialPortConsole(byte id)
        {
            this.portId = id;
            serialPortConsoleDeviceType = DeviceType.SerialPortConsole;
        }

        public byte PortId
        {
            get
            {
                return this.portId;
            }
            set
            {
                if (value > 0 && value < ConfigLoaded.MaxSerialConsolePorts)
                    this.portId = value;
            }
        }

        public DeviceType SerialPortConsoleDeviceType
        {
            get
            {
                return serialPortConsoleDeviceType;
            }
        }

        // TODO: Avoid hard coding of COM ports and make it 
        // Mapping of COM actual port number to CommunicationDevice logical port number
        public byte translateSerialPortId(byte portId)
        {
            if (portId == 5)
                return 3;
            else if (portId == 6)
                return 4;
            return portId;
        }

        #region Function Classes

        public SerialStatusPacket openSerialPortConsole(int communicationDeviceTimeoutIn1ms, BaudRate baudrate)
        {
            return openSerialPortConsole(this.portId, communicationDeviceTimeoutIn1ms, baudrate);
        }

        private SerialStatusPacket openSerialPortConsole(byte id, int communicationDeviceTimeoutIn1ms, BaudRate baudrate)
        {
            // Initialize return packet 
            SerialStatusPacket returnPacket = new SerialStatusPacket();
            returnPacket.completionCode = CompletionCode.UnspecifiedError;
            Tracer.WriteInfo("Invoked SerialPortConsole.openSerialPortConsole({0})", id);

            try
            {
                // Call device layer below
                SerialConsolePortOpenResponse serialResponse = (SerialConsolePortOpenResponse)this.SendReceive(this.SerialPortConsoleDeviceType,
                    translateSerialPortId(this.PortId), new SerialConsolePortOpenRequest(communicationDeviceTimeoutIn1ms, baudrate),
                    typeof(SerialConsolePortOpenResponse), (byte)PriorityLevel.User);

                // check for completion code 
                if (serialResponse.CompletionCode != 0)
                {
                    Tracer.WriteError("SerialPortConsole.openSerialPortConsole({0}) error in commdev.sendreceive", id);
                    returnPacket.completionCode = (CompletionCode)serialResponse.CompletionCode;
                }
                else
                {
                    returnPacket.completionCode = CompletionCode.Success;
                }
            }
            catch (System.Exception ex)
            {
                returnPacket.completionCode = CompletionCode.UnspecifiedError;
                Tracer.WriteError(ex);
            }

            return returnPacket;
        }

        public SerialStatusPacket closeSerialPortConsole()
        {
            return closeSerialPortConsole(this.portId);
        }

        private SerialStatusPacket closeSerialPortConsole(byte id)
        {
            // Initialize return packet 
            SerialStatusPacket returnPacket = new SerialStatusPacket();
            returnPacket.completionCode = CompletionCode.UnspecifiedError;

            try
            {
                // Call device layer below
                SerialConsolePortCloseResponse serialResponse =
                    (SerialConsolePortCloseResponse)this.SendReceive(this.SerialPortConsoleDeviceType, translateSerialPortId(this.PortId),
                    new SerialConsolePortCloseRequest(), typeof(SerialConsolePortCloseResponse), (byte)PriorityLevel.User);

                // check for completion code 
                if (serialResponse.CompletionCode != 0)
                {
                    returnPacket.completionCode = (CompletionCode)serialResponse.CompletionCode;
                }
                else
                {
                    returnPacket.completionCode = CompletionCode.Success;
                }
            }
            catch (System.Exception ex)
            {
                returnPacket.completionCode = CompletionCode.UnspecifiedError;
                Tracer.WriteError(ex);
            }

            return returnPacket;
        }

        public SerialStatusPacket sendSerialData(byte[] data)
        {
            return sendSerialData(this.portId, data);
        }

        private SerialStatusPacket sendSerialData(byte id, byte[] data)
        {
            // Initialize return packet 
            SerialStatusPacket returnPacket = new SerialStatusPacket();
            returnPacket.completionCode = CompletionCode.UnspecifiedError;

            try
            {
                // Call device layer below
                SerialConsolePortSendResponse serialResponse =
                    (SerialConsolePortSendResponse)this.SendReceive(this.SerialPortConsoleDeviceType, translateSerialPortId(this.PortId),
                    new SerialConsolePortSendRequest(data), typeof(SerialConsolePortSendResponse), (byte)PriorityLevel.User);

                // check for completion code 
                if (serialResponse.CompletionCode != 0)
                {
                    returnPacket.completionCode = (CompletionCode)serialResponse.CompletionCode;
                }
                else
                {
                    returnPacket.completionCode = CompletionCode.Success;
                }
            }
            catch (System.Exception ex)
            {
                returnPacket.completionCode = CompletionCode.UnspecifiedError;
                Tracer.WriteError(ex);
            }

            return returnPacket;
        }

        public SerialDataPacket receiveSerialData()
        {
            return receiveSerialData(this.PortId);
        }

        private SerialDataPacket receiveSerialData(byte id)
        {
            // Initialize return packet 
            SerialDataPacket returnPacket = new SerialDataPacket();
            returnPacket.completionCode = CompletionCode.UnspecifiedError;
            returnPacket.data = null;

            try
            {
                // Call device layer below
                SerialConsolePortReceiveResponse serialResponse =
                    (SerialConsolePortReceiveResponse)this.SendReceive(this.SerialPortConsoleDeviceType, translateSerialPortId(this.PortId),
                    new SerialConsolePortReceiveRequest(), typeof(SerialConsolePortReceiveResponse), (byte)PriorityLevel.User);

                // check for completion code 
                if (serialResponse.CompletionCode != 0)
                {
                    returnPacket.completionCode = (CompletionCode)serialResponse.CompletionCode;
                }
                else
                {
                    returnPacket.completionCode = CompletionCode.Success;
                    if (serialResponse.ReceiveData == null)
                    {
                        Tracer.WriteWarning("Data is empty in SerialPortConsole.receiveSerialData");
                    }
                    returnPacket.data = serialResponse.ReceiveData;
                }
            }
            catch (System.Exception ex)
            {
                returnPacket.completionCode = CompletionCode.UnspecifiedError;
                returnPacket.data = null;
                Tracer.WriteError(ex);
            }

            return returnPacket;
        }

        #endregion

        #region Command Classes

        [ChassisMessageRequest(FunctionCode.OpenConsole)]
        internal class SerialConsolePortOpenRequest : ChassisRequest
        {
            private byte[] baudrate;
            private byte[] deviceTimeoutIn1ms;

            internal SerialConsolePortOpenRequest(int deviceTimeoutIn1ms, BaudRate baudrate)
            {
                this.DeviceTimeoutIn1Ms = BitConverter.GetBytes(deviceTimeoutIn1ms);

                switch (baudrate)
                {
                    case BaudRate.Rate_75:
                        this.Baudrate = BitConverter.GetBytes(75);
                        break;
                    case BaudRate.Rate_110:
                        this.Baudrate = BitConverter.GetBytes(110);
                        break;
                    case BaudRate.Rate_300:
                        this.Baudrate = BitConverter.GetBytes(300);
                        break;
                    case BaudRate.Rate_1200:
                        this.Baudrate = BitConverter.GetBytes(1200);
                        break;
                    case BaudRate.Rate_2400:
                        this.Baudrate = BitConverter.GetBytes(2400);
                        break;
                    case BaudRate.Rate_4800:
                        this.Baudrate = BitConverter.GetBytes(4800);
                        break;
                    case BaudRate.Rate_9600:
                        this.Baudrate = BitConverter.GetBytes(9600);
                        break;
                    case BaudRate.Rate_19200:
                        this.Baudrate = BitConverter.GetBytes(19200);
                        break;
                    case BaudRate.Rate_38400:
                        this.Baudrate = BitConverter.GetBytes(38400);
                        break;
                    case BaudRate.Rate_57600:
                        this.Baudrate = BitConverter.GetBytes(57600);
                        break;
                    case BaudRate.Rate_115200:
                        this.Baudrate = BitConverter.GetBytes(115200);
                        break;
                    default:
                        this.Baudrate = BitConverter.GetBytes(9600);
                        break;
                }
            }

            [ChassisMessageData(0)]
            public byte[] Baudrate
            {
                get
                {
                    return this.baudrate;
                }
                set
                {
                    this.baudrate = value;
                }
            }

            [ChassisMessageData(4)]
            public byte[] DeviceTimeoutIn1Ms
            {
                get
                {
                    return this.deviceTimeoutIn1ms;
                }
                set
                {
                    this.deviceTimeoutIn1ms = value;
                }
            }
        }

        [ChassisMessageResponse(FunctionCode.OpenConsole)]
        internal class SerialConsolePortOpenResponse : ChassisResponse
        {
            // Nothing to receive
        }

        [ChassisMessageRequest(FunctionCode.CloseConsole)]
        internal class SerialConsolePortCloseRequest : ChassisRequest
        {
            // Nothing to send
        }

        [ChassisMessageResponse(FunctionCode.CloseConsole)]
        internal class SerialConsolePortCloseResponse : ChassisResponse
        {
            // Nothing to receive
        }

        [ChassisMessageRequest(FunctionCode.SendConsole)]
        internal class SerialConsolePortSendRequest : ChassisRequest
        {
            private byte[] serialSendData;

            public SerialConsolePortSendRequest(byte[] data)
            {
                this.SerialSendData = data;
            }

            [ChassisMessageData(0)]
            public byte[] SerialSendData
            {
                get
                {
                    return this.serialSendData;
                }

                set
                {
                    this.serialSendData = value;
                }
            }
        }

        [ChassisMessageResponse(FunctionCode.SendConsole)]
        internal class SerialConsolePortSendResponse : ChassisResponse
        {
            // Nothing to receive
        }

        [ChassisMessageRequest(FunctionCode.ReceiveConsole)]
        internal class SerialConsolePortReceiveRequest : ChassisRequest
        {
            // Nothing to send
        }

        [ChassisMessageResponse(FunctionCode.ReceiveConsole)]
        internal class SerialConsolePortReceiveResponse : ChassisResponse
        {
            private byte[] receiveData;

            [ChassisMessageData(0)]
            public byte[] ReceiveData
            {
                get { return this.receiveData; }
                set { this.receiveData = value; }
            }
        }

        #endregion

    }
}
