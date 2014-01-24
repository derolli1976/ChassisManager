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
using System.Reflection;
using System.Threading;

namespace Microsoft.GFS.WCS.ChassisManager
{

    /// <summary>
    /// PsuBase is the base class for derived Psu device classes
    /// </summary>
    internal class PsuBase : ChassisSendReceive
    {   
        /// <summary>
        ///  Device Type
        /// </summary>
        private static DeviceType psuDeviceType;

        /// <summary>
        /// Device Id for the Psu 
        /// </summary>
        private byte psuId;

        internal PsuBase(byte deviceId)
        {
            this.psuId = deviceId;
            psuDeviceType = DeviceType.Psu;
        }

        internal DeviceType PsuDeviceType
        {
            get { return psuDeviceType; }
        }

        internal byte PsuId
        {
            get { return psuId; }
        }


        #region Virtual Methods

        /// <summary>
        /// Gets the Psu Model Number
        /// </summary>
        /// <returns></returns>
        internal virtual PsuModelNumberPacket GetPsuModel()
        {
            return GetPsuModel(this.PsuId);
        }

        /// <summary>
        /// Gets the Psu Serial Number
        /// </summary>
        /// <returns></returns>
        internal virtual PsuSerialNumberPacket GetPsuSerialNumber()
        {
            return GetPsuSerialNumber(this.PsuId);
        }

        /// <summary>
        /// Gets the Psu Status
        /// </summary>
        /// <returns></returns>
        internal virtual PsuStatusPacket GetPsuStatus()
        {
            return GetPsuStatus(this.PsuId);
        }

        /// <summary>
        /// Gets the Psu Power usage.
        /// </summary>
        /// <returns></returns>
        internal virtual PsuPowerPacket GetPsuPower()
        {
            return GetPsuPower(this.PsuId);
        }

        /// <summary>
        /// Clears Psu Error Status.
        /// </summary>
        /// <returns></returns>
        internal virtual CompletionCode SetPsuClearFaults()
        {
            return SetPsuClearFaults(this.PsuId);
        }

        /// <summary>
        /// Set PSU On/OFF
        /// </summary>
        /// <param name="off">true = OFF, false = ON</param>
        /// <returns>Completion code success/failure</returns>
        internal virtual CompletionCode SetPsuOnOff(bool off)
        {
            return CompletionCode.CmdFailedNotSupportedInPresentState;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Attempts to retrieve the Psu Model Number.  This method
        /// calls down to the Chassis Manager with SendReceive
        /// </summary>
        private PsuModelNumberPacket GetPsuModel(byte psuId)
        {
            // Initialize return packet 
            PsuModelNumberPacket returnPacket = new PsuModelNumberPacket();
            returnPacket.CompletionCode = CompletionCode.UnspecifiedError;
            returnPacket.ModelNumber = "";

            try
            {
                PsuModelResponse myResponse = new PsuModelResponse();
                myResponse = (PsuModelResponse)this.SendReceive(this.PsuDeviceType, this.PsuId, new PsuRequest((byte)PmBusCommand.MFR_MODEL,(byte)PmBusResponseLength.MFR_MODEL), typeof(PsuModelResponse));
                
                // check for completion code 
                if (myResponse.CompletionCode != 0)
                {
                    returnPacket.CompletionCode = (CompletionCode)myResponse.CompletionCode;
                }
                else
                {
                    returnPacket.CompletionCode = CompletionCode.Success;
                    if (myResponse.PsuModelNumber != null)
                    {
                        byte[] inModelNumber = myResponse.PsuModelNumber;
                        byte[] outModelNumber = null;
                        PmBus.PsuModelNumberParser(ref inModelNumber, out outModelNumber);
                        returnPacket.ModelNumber = System.BitConverter.ToString(outModelNumber, 0);
                    }
                    else
                    {
                        returnPacket.ModelNumber = "";
                    }
                }
            }
            catch (System.Exception ex)
            {
                returnPacket.CompletionCode = CompletionCode.UnspecifiedError;
                returnPacket.ModelNumber = "";
                Tracer.WriteError(ex);
            }

            return returnPacket;
        }

        /// <summary>
        /// Attempts to retrieve the Psu Serial Number. This method
        /// calls down to the Chassis Manager with SendReceive
        /// </summary>
        private PsuSerialNumberPacket GetPsuSerialNumber(byte psuId)
        {
            PsuSerialNumberPacket returnPacket = new PsuSerialNumberPacket();
            returnPacket.CompletionCode = CompletionCode.UnspecifiedError;
            returnPacket.SerialNumber = "";

            try
            {
                PsuSerialResponse myResponse = new PsuSerialResponse();
                myResponse = (PsuSerialResponse)this.SendReceive(this.PsuDeviceType, this.PsuId, new PsuRequest((byte)PmBusCommand.MFR_SERIAL,(byte)PmBusResponseLength.MFR_SERIAL), typeof(PsuSerialResponse));

                // check for completion code 
                if (myResponse.CompletionCode != 0)
                {
                    returnPacket.CompletionCode = (CompletionCode)myResponse.CompletionCode;
                }
                else
                {
                    returnPacket.CompletionCode = CompletionCode.Success;
                    if (myResponse.PsuSerialNumber != null)
                    {
                        byte[] inSerialNumber = myResponse.PsuSerialNumber;
                        byte[] outSerialNumber = null;
                        PmBus.PsuSerialNumberParser(ref inSerialNumber, out outSerialNumber);
                        returnPacket.SerialNumber = System.BitConverter.ToString(outSerialNumber, 0);
                    }
                    else
                    {
                        returnPacket.SerialNumber = "";
                    }
                }
            }
            catch (System.Exception ex)
            {
                returnPacket.CompletionCode = CompletionCode.UnspecifiedError;
                returnPacket.SerialNumber = "";
                Tracer.WriteError(ex);
            }

            return returnPacket;
        }

        /// <summary>
        /// Attempts to retrieve the Psu Power. This method
        /// calls down to the Chassis Manager with SendReceive
        /// </summary>
        private PsuPowerPacket GetPsuPower(byte psuId)
        {
            // Initialize return packet 
            PsuPowerPacket returnPacket = new PsuPowerPacket();
            returnPacket.CompletionCode = CompletionCode.UnspecifiedError;
            returnPacket.PsuPower = 0;

            byte[] powerValue = new byte[100];
            try
            {
                PsuPowerResponse myResponse = new PsuPowerResponse();
                myResponse = (PsuPowerResponse)this.SendReceive(this.PsuDeviceType, this.PsuId, new PsuRequest((byte)PmBusCommand.READ_POUT,(byte)PmBusResponseLength.READ_POUT), typeof(PsuPowerResponse));

                if (myResponse.CompletionCode != 0)
                {
                    returnPacket.CompletionCode = (CompletionCode)myResponse.CompletionCode;
                }
                else
                {
                    returnPacket.CompletionCode = CompletionCode.Success;
                    powerValue = myResponse.PsuPower;
                    byte[] convertedPowerValue = null;
                    PmBus.ReadPowerMilliWattConverter(ref powerValue, out convertedPowerValue);
                    powerValue = convertedPowerValue;
                    returnPacket.PsuPower = System.BitConverter.ToInt32(powerValue, 0) / 1000;
                }
            }
            catch (System.Exception ex)
            {
                returnPacket.CompletionCode = CompletionCode.UnspecifiedError;
                returnPacket.PsuPower = 0;
                Tracer.WriteError(ex);
            }
            return returnPacket;
        }

        /// <summary>
        /// Attempts to retrieve the Psu Status. This method
        /// calls down to the Chassis Manager with SendReceive
        /// </summary>
        private PsuStatusPacket GetPsuStatus(byte psuId)
        {
            // Initialize return packet 
            PsuStatusPacket returnPacket = new PsuStatusPacket();
            returnPacket.CompletionCode = CompletionCode.UnspecifiedError;
            returnPacket.PsuStatus = 0;

            try
            {
                PsuStatusResponse myResponse = new PsuStatusResponse();
                myResponse = (PsuStatusResponse)this.SendReceive(this.PsuDeviceType, this.PsuId, new PsuRequest((byte)PmBusCommand.STATUS_WORD,(byte)PmBusResponseLength.STATUS_WORD), typeof(PsuStatusResponse));

                if (myResponse.CompletionCode != 0)
                {
                    returnPacket.CompletionCode = (CompletionCode)myResponse.CompletionCode;
                    Tracer.WriteWarning("GetPsuStatus Failure: status({0})", returnPacket.PsuStatus);
                }
                else
                {
                    returnPacket.CompletionCode = CompletionCode.Success;
                    byte varStatus;
                    byte[] psuStatus = myResponse.PsuStatus;
                    
                    // If there are any other faults, print the full status word
                    if (!PmBus.ExtractPowerGoodFromPsuStatus(psuStatus, out varStatus))
                    {
                        Tracer.WriteWarning("Psu({0}) PowerGood Negated ({1} {2}) in/out curr/volt faults (See StatusWord in PmBusII Manual)", this.PsuId, System.Convert.ToString(psuStatus[0], 2).PadLeft(8, '0'), System.Convert.ToString(psuStatus[1], 2).PadLeft(8, '0'));
                    }

                    returnPacket.PsuStatus = varStatus;
                }
            }
            catch (System.Exception ex)
            {
                returnPacket.CompletionCode = CompletionCode.UnspecifiedError;
                returnPacket.PsuStatus = 0;
                Tracer.WriteError("GetPsuStatus Exception: " + ex);
            }
            return returnPacket;
        }

        /// <summary>
        /// Attempts to clear the Psu error status. This method
        /// calls down to the Chassis Manager with SendReceive
        /// </summary>
        private CompletionCode SetPsuClearFaults(byte psuId)
        {
            CompletionCode returnPacket = new CompletionCode();
            returnPacket = CompletionCode.UnspecifiedError;

            try
            {
                PsuClearfaultsResponse myResponse = new PsuClearfaultsResponse();
                myResponse = (PsuClearfaultsResponse)this.SendReceive(this.PsuDeviceType, this.PsuId, new PsuRequest((byte)PmBusCommand.CLEAR_FAULTS,(byte)PmBusResponseLength.CLEAR_FAULTS), typeof(PsuClearfaultsResponse));

                // check for completion code 
                if (myResponse.CompletionCode != 0)
                {
                    returnPacket = (CompletionCode)myResponse.CompletionCode;
                }
                else
                {
                    returnPacket = CompletionCode.Success;
                }
            }
            catch (System.Exception ex)
            {
                returnPacket = CompletionCode.UnspecifiedError;
                Tracer.WriteError(ex);
            }

            return returnPacket;
        }


        #endregion

    }

    #region Psu Response Structures

    public class PsuModelNumberPacket
    {
        public CompletionCode CompletionCode;
        public string ModelNumber;
    }

    public class PsuSerialNumberPacket
    {
        public CompletionCode CompletionCode;
        public string SerialNumber;
    }

    public class PsuStatusPacket
    {
        public CompletionCode CompletionCode;
        public byte PsuStatus;
    }

    public class PsuPowerPacket
    {
        public CompletionCode CompletionCode;
        public double PsuPower;
    }

    #endregion

    #region Psu Request Structures

    /// <summary>
    /// Represents the Psu single master request message
    /// The first byte of the payload indicates the PSU pmbus command op code
    /// The second byte of the payload indicates the expected number of bytes to be read back from the PSU
    /// </summary>
    [ChassisMessageRequest(FunctionCode.PsuOperations)]
    internal class PsuRequest : ChassisRequest
    {
        public PsuRequest(byte command, byte length)
        {
            this.PsuCommand = command;
            this.ExpectedPsuResponseLength = length;
        }

        /// <summary>
        /// Psu command byte to be send on the wire
        /// </summary>
        [ChassisMessageData(0)]
        public byte PsuCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Expected length of the response message
        /// </summary>
        [ChassisMessageData(1)]
        public byte ExpectedPsuResponseLength
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents the Psu single master request message
    /// The first byte of the payload indicates the PSU pmbus command op code
    /// The second byte of the payload indicates the expected number of bytes to be read back from the PSU
    /// </summary>
    [ChassisMessageRequest(FunctionCode.PsuOperations)]
    internal class PsuPayloadRequest : ChassisRequest
    {
        public PsuPayloadRequest(byte command, byte payload, byte length)
        {
            this.PsuCommand = command;
            this.Payload = payload;
            this.ExpectedPsuResponseLength = length;
        }

        /// <summary>
        /// Psu command byte to be send on the wire
        /// </summary>
        [ChassisMessageData(0)]
        public byte PsuCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Psu command payload
        /// </summary>
        [ChassisMessageData(1)]
        public byte Payload
        {
            get;
            set;
        }

        /// <summary>
        /// Expected length of the response message
        /// </summary>
        [ChassisMessageData(2)]
        public byte ExpectedPsuResponseLength
        {
            get;
            set;
        }
    }

    #endregion

    #region Psu Response Classes

    /* Since Response packet structures will be interpreted 
       differently depending on requested PSU functionality, 
       create separate response classes */

    /// <summary>
    /// Represents the Psu 'Get Status' response message.
    /// </summary>
    [ChassisMessageResponse((byte)FunctionCode.PsuOperations)]
    internal class PsuStatusResponse : ChassisResponse
    {
        private byte[] psuStatus;

        [ChassisMessageData(0, (int)PmBusResponseLength.STATUS_WORD)] // We are only interested in the high byte
        public byte[] PsuStatus
        {
            get { return this.psuStatus; }
            set { this.psuStatus = value; }
        }
    }

    /// <summary>
    /// Represents the Psu 'Get Model' response message.
    /// </summary>
    [ChassisMessageResponse((byte)FunctionCode.PsuOperations)]
    internal class PsuModelResponse : ChassisResponse
    {
        private byte[] psuModelNumber;

        [ChassisMessageData(0, (int)PmBusResponseLength.MFR_MODEL)]
        public byte[] PsuModelNumber
        {
            get { return this.psuModelNumber; }
            set { this.psuModelNumber = value; }
        }
    }

    /// <summary>
    /// Represents the Psu 'Get Serial' response message.
    /// </summary>
    [ChassisMessageResponse((byte)FunctionCode.PsuOperations)]
    internal class PsuSerialResponse : ChassisResponse
    {
        private byte[] psuSerialNumber;

        [ChassisMessageData(0, (int)PmBusResponseLength.MFR_SERIAL)]
        public byte[] PsuSerialNumber
        {
            get { return this.psuSerialNumber; }
            set { this.psuSerialNumber = value; }
        }
    }

    /// <summary>
    /// Represents the Psu 'Get Power' response message.
    /// </summary>
    [ChassisMessageResponse((byte)FunctionCode.PsuOperations)]
    internal class PsuPowerResponse : ChassisResponse
    {
        private byte[] psuPower;

        [ChassisMessageData(0, (int)PmBusResponseLength.READ_POUT)]
        public byte[] PsuPower
        {
            get { return this.psuPower; }
            set { this.psuPower = value; }

        }

    }

    /// <summary>
    /// Represents the Psu 'Clear Faults' response message.
    /// </summary>
    [ChassisMessageResponse((byte)FunctionCode.PsuOperations)]
    internal class PsuClearfaultsResponse : ChassisResponse
    {
    }

    /// <summary>
    /// Represents the Psu ON/OFF response message.
    /// </summary>
    [ChassisMessageResponse((byte)FunctionCode.PsuOperations)]
    internal class PsuOnOffResponse : ChassisResponse
    {
        /// <summary>
        /// Psu State:
        ///    80 = On
        ///     0 = Off
        /// </summary>
        private byte psuState;

        /// <summary>
        /// Psu State:
        ///     1 = On
        ///     0 = Off
        /// </summary>
        [ChassisMessageData(0)]
        public byte PsuState
        {
            get { return this.psuState; }
            set { this.psuState = value; }
        }
    }

    #endregion

}
