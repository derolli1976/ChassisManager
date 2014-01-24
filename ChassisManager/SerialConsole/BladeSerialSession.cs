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
    internal class BladeSerialSession : ChassisSendReceive
    {
        /// <summary>
        ///  Blade id
        /// </summary>
        internal byte bladeId;

        /// <summary>
        ///  Define Device Type
        /// </summary>
        internal static DeviceType bladeSerialDeviceType;

        internal BladeSerialSession(byte id)
        {
            this.bladeId = id;
            bladeSerialDeviceType = DeviceType.BladeConsole;
        }

        internal byte BladeId
        {
            get
            {
                return this.bladeId;
            }
            set
            {
                if (value > 0 && value < ConfigLoaded.Population)
                    this.bladeId = value;
            }
        }

        internal DeviceType BladeSerialDeviceType
        {
            get
            {
                return bladeSerialDeviceType;
            }
        }

        internal SerialStatusPacket sendSerialData(byte[] data)
        {
            return sendSerialData(this.BladeId, data);
        }

        internal SerialStatusPacket sendSerialData(byte id, byte[] data)
        {
            // Initialize return packet 
            SerialStatusPacket returnPacket = new SerialStatusPacket();
            returnPacket.completionCode = CompletionCode.UnspecifiedError;

            try
            {
                // Call device layer below
                BladeSerialSendResponse serialResponse =
                    (BladeSerialSendResponse)this.SendReceive(this.BladeSerialDeviceType, this.BladeId, new BladeSerialSendRequest(data), typeof(BladeSerialSendResponse));

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

        internal SerialDataPacket receiveSerialData()
        {
            return receiveSerialData(this.BladeId);
        }

        internal SerialDataPacket receiveSerialData(byte id)
        {
            // Initialize return packet 
            SerialDataPacket returnPacket = new SerialDataPacket();
            returnPacket.completionCode = CompletionCode.UnspecifiedError;
            returnPacket.data = null;

            try
            {
                // Call device layer below
                BladeSerialReceiveResponse serialResponse =
                    (BladeSerialReceiveResponse)this.SendReceive(this.BladeSerialDeviceType, this.BladeId, new BladeSerialReceiveRequest(), typeof(BladeSerialReceiveResponse));

                // check for completion code 
                if (serialResponse.CompletionCode != 0)
                {
                    returnPacket.completionCode = (CompletionCode)serialResponse.CompletionCode;
                }
                else
                {
                    returnPacket.completionCode = CompletionCode.Success;
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
    }

    #region Command Classes

    [ChassisMessageRequest(FunctionCode.SendConsole)]
    internal class BladeSerialSendRequest : ChassisRequest
    {
        private byte[] serialSendData;

        public BladeSerialSendRequest(byte[] data)
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
    internal class BladeSerialSendResponse : ChassisResponse
    {
        // Nothing to receive
    }

    [ChassisMessageRequest(FunctionCode.ReceiveConsole)]
    internal class BladeSerialReceiveRequest : ChassisRequest
    {
        // Nothing to send
    }

    [ChassisMessageResponse(FunctionCode.ReceiveConsole)]
    internal class BladeSerialReceiveResponse : ChassisResponse
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
