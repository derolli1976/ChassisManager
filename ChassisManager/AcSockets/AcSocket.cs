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
using System.Threading;

namespace Microsoft.GFS.WCS.ChassisManager
{
    /// <summary>
    /// Class for AC Socket (get state, turn on/off commands are supported)
    /// </summary>
    public class AcSocket : ChassisSendReceive
    {
        protected byte _deviceId;

        private Object lockObject = new Object();
        
        /// <summary>
        /// Constructor for Status LED
        /// </summary>
        /// <param name="deviceId"></param>
        public AcSocket(byte deviceId)
        {
            this._deviceId = deviceId;
        }

        /// <summary>
        /// Public function that calls internal turnOnLED function
        /// </summary>
        public byte turnOnAcSocket()
        {
            return turnOnAcSocket(this._deviceId);
        }
        /// <summary>
        /// Turns the Status LED on
        /// </summary>
        private byte turnOnAcSocket(byte deviceId)
        {
            TurnOnAcSocketResponse response;
            lock (lockObject)
            {
                response = (TurnOnAcSocketResponse)this.SendReceive(DeviceType.PowerSwitch,
                    deviceId, new TurnOnAcSocketRequest(),
                    typeof(TurnOnAcSocketResponse), (byte)PriorityLevel.User);
            }
            if (response.CompletionCode != 0)
            {
                return response.CompletionCode;
            }
            else
            {
                return 0;
            }
        }

        

        /// <summary>
        /// Public function calls internal turn off LED
        /// </summary>
        public byte turnOffAcSocket()
        {
            return turnOffAcSocket(this._deviceId);
        }
        /// <summary>
        /// Turns off LED
        /// </summary>
        private byte turnOffAcSocket(byte deviceId)
        {
            TurnOffAcSocketResponse response; 
            
            lock (lockObject)
            {
                response = (TurnOffAcSocketResponse) this.SendReceive(DeviceType.PowerSwitch, 
                    deviceId, new TurnOffAcSocketRequest(),
                    typeof(TurnOffAcSocketResponse), (byte)PriorityLevel.User);

            // This is a temporary fix to handle hardware issue - VR slow draining after socket off immediately followed by socketOn
                Thread.Sleep((int) ConfigLoaded.WaitTimeAfterACSocketPowerOffInMsecs);
            }

            if (response.CompletionCode != 0)
            {
                return response.CompletionCode;
            }
            else
            {
                return 0;
            }
        }

        public Contracts.PowerState getAcSocketStatus()
        {
            return getAcSocketStatus(this._deviceId);
        }

        private Contracts.PowerState getAcSocketStatus(byte deviceId)
        {
            GetAcSocketStatusResponse response = (GetAcSocketStatusResponse)this.SendReceive(DeviceType.PowerSwitch,
                deviceId, new GetAcSocketStatusRequest(),
                typeof(GetAcSocketStatusResponse), (byte)PriorityLevel.User);

            if (response.CompletionCode != 0)
            {
                Tracer.WriteInfo("getACSocketStatus - Returning error code: " + response.CompletionCode);
                return Contracts.PowerState.NA;
            }
            else
            {
                Tracer.WriteInfo("getACSocketStatus - Returning status: " + response.status);
                if (response.status == (byte)Contracts.PowerState.ON)
                    return Contracts.PowerState.ON;
                else if (response.status == (byte)Contracts.PowerState.OFF)
                    return Contracts.PowerState.OFF;
                else
                    return Contracts.PowerState.NA;
            }
        }

        /// <summary>
        /// Turn on ACSocket - only Command needed
        /// </summary>
        [ChassisMessageRequest(FunctionCode.TurnOnPowerSwitch)]
        internal class TurnOnAcSocketRequest : ChassisRequest
        {
        }

        /// <summary>
        /// Reset request
        /// </summary>
        [ChassisMessageRequest(FunctionCode.TurnOffPowerSwitch)]
        internal class TurnOffAcSocketRequest : ChassisRequest
        {
        }

        /// <summary>
        /// Empty response for turn on AC Socket
        /// </summary>
        [ChassisMessageResponse(FunctionCode.TurnOnPowerSwitch)]
        internal class TurnOnAcSocketResponse : ChassisResponse
        {
        }
        /// <summary>
        /// Empty response for turn off AC Socket
        /// </summary>
        [ChassisMessageResponse(FunctionCode.TurnOffPowerSwitch)]
        internal class TurnOffAcSocketResponse : ChassisResponse
        {
        }

        /// <summary>
        /// Get AC Switch status request
        /// </summary>
        [ChassisMessageRequest(FunctionCode.GetPowerSwitchStatus)]
        internal class GetAcSocketStatusRequest : ChassisRequest
        {
        }

        [ChassisMessageResponse(FunctionCode.GetPowerSwitchStatus)]
        internal class GetAcSocketStatusResponse : ChassisResponse
        {
            // fan rpm
            private byte _status;

            /// <summary>
            /// Switch Status
            /// </summary>
            [ChassisMessageData(0)]
            public byte status
            {
                get { return this._status; }
                set { this._status = value; }
            }
            
        }
    }
}

