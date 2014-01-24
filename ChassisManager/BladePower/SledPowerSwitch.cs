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
    /// <summary>
    /// Response class for Blade Power Statue (Blade_EN)
    /// </summary>
    public class BladePowerStatePacket
    {
        public CompletionCode CompletionCode;
        public byte BladePowerState;
    }

    /// <summary>
    /// Blade Power Switch (Blade_EN) class
    /// </summary>
    public class BladePowerSwitch : ChassisSendReceive
    {
        private DeviceType deviceType;
        private byte deviceId;
        private byte powerState;

        public BladePowerSwitch(byte deviceId)
        {
            this.deviceId = deviceId;
            deviceType = DeviceType.Power;
        }

        internal DeviceType DeviceType
        {
            get
            {
                return deviceType;
            }
        }

        internal byte DeviceId
        {
            get
            {
                return deviceId;
            }
        }

        internal byte PowerState
        {
            get
            {
                return powerState;
            }
            set
            {
                powerState = value;
            }
        }

        /// <summary>
        /// Returns the state of the blade power switch (blade enable)
        /// </summary>
        /// <returns></returns>
        internal BladePowerStatePacket GetBladePowerState()
        {
            return GetBladePowerState(this.DeviceId);
        }

        internal BladePowerStatePacket GetBladePowerState(byte deviceId)
        {
            // Initialize return packet 
            BladePowerStatePacket returnPacket = new BladePowerStatePacket();
            returnPacket.CompletionCode = CompletionCode.UnspecifiedError;
            returnPacket.BladePowerState = 2;

            try
            {
                BladePowerStateResponse stateResponse = (BladePowerStateResponse)this.SendReceive(this.DeviceType, deviceId, new BladePowerStateRequest(), typeof(BladePowerStateResponse));
                if (stateResponse.CompletionCode != 0)
                {
                    returnPacket.CompletionCode = (CompletionCode)stateResponse.CompletionCode;
                }
                else
                {
                    returnPacket.CompletionCode = CompletionCode.Success;
                    this.PowerState = stateResponse.BladePowerState;
                    returnPacket.BladePowerState = this.PowerState;
                }
            }
            catch (System.Exception ex)
            {
                returnPacket.CompletionCode = CompletionCode.UnspecifiedError;
                returnPacket.BladePowerState = 2;
                Tracer.WriteError(ex);
            }
            return returnPacket;
        }

        internal BladePowerStatePacket SetBladePowerState(byte state)
        {
            return SetBladePowerState(this.DeviceId, state);
        }

        internal BladePowerStatePacket SetBladePowerState(byte deviceId, byte state)
        {
            // Initialize return packet 
            BladePowerStatePacket returnPacket = new BladePowerStatePacket();
            returnPacket.CompletionCode = CompletionCode.UnspecifiedError;
            returnPacket.BladePowerState = 2;

            BladePowerStateResponse stateResponse=new BladePowerStateResponse();
            Tracer.WriteInfo("SetSledPowerState Switch id: " + deviceId);

            if (state == (byte)Contracts.PowerState.ON)
            {
                try
                {
                    stateResponse = (BladePowerStateResponse)this.SendReceive(this.DeviceType, deviceId, new BladePowerStateOnRequest(state), typeof(BladePowerStateResponse));
                    if (stateResponse.CompletionCode != 0)
                    {
                        returnPacket.CompletionCode = (CompletionCode)stateResponse.CompletionCode;
                    }
                    else
                    {
                        returnPacket.CompletionCode = CompletionCode.Success;
                        returnPacket.BladePowerState = 1;
                    }
                }
                catch (System.Exception ex)
                {
                    returnPacket.CompletionCode = CompletionCode.UnspecifiedError;
                    returnPacket.BladePowerState = 2;
                    Tracer.WriteError(ex);
                }
            }
            else 
            {
                try
                {
                    stateResponse = (BladePowerStateResponse)this.SendReceive(this.DeviceType, deviceId, new BladePowerStateOffRequest(state), typeof(BladePowerStateResponse));
                    if (stateResponse.CompletionCode != 0)
                    {
                        returnPacket.CompletionCode = (CompletionCode)stateResponse.CompletionCode;
                    }
                    else
                    {
                        returnPacket.CompletionCode = CompletionCode.Success;
                        returnPacket.BladePowerState = 0;
                    }
                }
                catch (System.Exception ex)
                {
                    returnPacket.CompletionCode = CompletionCode.UnspecifiedError;
                    returnPacket.BladePowerState = 2;
                    Tracer.WriteError(ex);
                }
            }

            return returnPacket;
        }
    }

    #region Blade Power Request Structures

    /// <summary>
    /// Gets the state of the blade power switch (Blade_EN)
    /// </summary>
    [ChassisMessageRequest(FunctionCode.GetServerPowerStatus)]
    internal class BladePowerStateRequest : ChassisRequest
    {
    }

    /// <summary>
    /// Turns the Blade Power Switch On (Blade_EN = On)
    /// </summary>
    [ChassisMessageRequest(FunctionCode.TurnOnServer)]
    internal class BladePowerStateOnRequest : ChassisRequest
    {
        public BladePowerStateOnRequest(byte state)
        {
            this.PowerState = state;
        }

        [ChassisMessageData(0)]
        public byte PowerState
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Turns the Blade Power Switch Off (Blade_EN = Off)
    /// </summary>
    [ChassisMessageRequest(FunctionCode.TurnOffServer)]
    internal class BladePowerStateOffRequest : ChassisRequest
    {
        public BladePowerStateOffRequest(byte state)
        {
            this.PowerState = state;
        }

        [ChassisMessageData(0)]
        public byte PowerState
        {
            get;
            set;
        }
    }

    #endregion

    #region Blade Power Response Structures

    [ChassisMessageResponse(FunctionCode.GetServerPowerStatus)]
    internal class BladePowerStateResponse : ChassisResponse
    {
        private byte bladePowerState;

        [ChassisMessageData(0)]
        public byte BladePowerState
        {
            get { return this.bladePowerState; }
            set { this.bladePowerState = value; }
        }
    }

    #endregion

}
