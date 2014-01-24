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

namespace Microsoft.GFS.WCS.ChassisManager
{
    /// <summary>
    /// Class for LED (turn on/off, status commands are supported)
    /// </summary>
    public class StatusLed : ChassisSendReceive
    {

        // default device Id for this command.
        private readonly byte deviceId = 0x01;

        /// <summary>
        /// Turns the Chassis Status LED on
        /// </summary>
        internal byte TurnLedOn()
        {
            LedOnResponse response = (LedOnResponse) this.SendReceive(DeviceType.RearAttentionLed, deviceId, new TurnOnLed(),
                typeof(LedOnResponse), (byte)PriorityLevel.User);

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
        /// Turns off LED
        /// </summary>
        internal byte TurnLedOff()
        {
            LedOffResponse response = (LedOffResponse) this.SendReceive(DeviceType.RearAttentionLed, deviceId, new TurnOffLed(),
                typeof(LedOffResponse), (byte)PriorityLevel.User);
            
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
        /// get LED status
        /// </summary>
        internal Contracts.LedStatusResponse GetLedStatus()
        {
            Contracts.LedStatusResponse response = new Contracts.LedStatusResponse();
            response.completionCode = Contracts.CompletionCode.Failure;
            response.ledState = Contracts.LedState.NA;
            
            LedStatusResponse ledStatus = (LedStatusResponse)this.SendReceive(DeviceType.RearAttentionLed, deviceId, new LedStatusRequest(),
                typeof(LedStatusResponse), (byte)PriorityLevel.User);

            if (ledStatus.CompletionCode != 0)
            {
                return response;
            }
            else
            {
                if (ledStatus.LedStatus == 0)
                {
                    response.ledState = Contracts.LedState.OFF;
                    response.completionCode = Contracts.CompletionCode.Success;
                }
                else if (ledStatus.LedStatus == 1)
                {
                    response.ledState = Contracts.LedState.ON;
                    response.completionCode = Contracts.CompletionCode.Success;
                }
                else
                {
                    response.ledState = Contracts.LedState.NA;
                    response.completionCode = Contracts.CompletionCode.Failure;
                }
                return response;
            }
        }

        #region Led Request Structures

        [ChassisMessageRequest(FunctionCode.GetLedStatus)]
        internal class LedStatusRequest : ChassisRequest
        {
        }

        /// <summary>
        /// Turn on LED - only Command needed
        /// </summary>
        [ChassisMessageRequest(FunctionCode.TurnOnLed)]
        internal class TurnOnLed : ChassisRequest
        {
        }

        /// <summary>
        /// Reset request
        /// </summary>
        [ChassisMessageRequest(FunctionCode.TurnOffLed)]
        internal class TurnOffLed : ChassisRequest
        {
        }

        #endregion

        #region Led Response Structures

        [ChassisMessageResponse(FunctionCode.GetLedStatus)]
        internal class LedStatusResponse : ChassisResponse
        {
            private byte ledStatus;

            [ChassisMessageData(0)]
            public byte LedStatus
            {
                get { return this.ledStatus; }
                set { this.ledStatus = value; }
            }
        }

        /// <summary>
        /// Empty response for LED
        /// </summary>
        [ChassisMessageResponse(FunctionCode.TurnOnLed)]
        internal class LedOnResponse : ChassisResponse
        {
        }

        [ChassisMessageResponse(FunctionCode.TurnOffLed)]
        internal class LedOffResponse : ChassisResponse
        {
        }

        #endregion

    }
}
