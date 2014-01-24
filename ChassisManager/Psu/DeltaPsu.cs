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
    internal class DeltaPsu : PsuBase
    {
        /// <summary>
        /// Lock used to affinitize on/off requests, and prevent repeated on/off
        /// </summary>
        private object locker = new object();

        /// <summary>
        /// Time in seconds where additional power off requests are not permitted.
        /// </summary>
        private int backoff = 30;

        /// <summary>
        /// Time since the last time the Psu was powered off
        /// </summary>
        private DateTime lastPowerOff;

        /// <summary>
        /// Function to determine if a the PSU can be turned off.  The purpose of
        /// this function is to prevent multiple reboots of a PSU in quick succession
        /// </summary>
        private bool PowerOffPermitted()
        {
            bool permitted = false;

            lock (locker)
            {
                if (DateTime.Now > lastPowerOff.AddSeconds(backoff))
                {
                    lastPowerOff = DateTime.Now;
                    permitted = true;
                }
                else
                {
                    permitted = false;
                }

                return permitted;
            }

        }

        /// <summary>
        /// Initializes instance of the class.
        /// </summary>
        /// <param name="deviceId"></param>
        internal DeltaPsu(byte deviceId)
            : base(deviceId)
        {
        }

        internal override CompletionCode SetPsuOnOff(bool off)
        {
            if (off)
            {
                if (PowerOffPermitted())
                    return this.SetPsuOnOff(PmBusCommandPayload.POWER_OFF);
                else
                    return CompletionCode.CmdFailedNotSupportedInPresentState;
            }
            else
            {
                return this.SetPsuOnOff(PmBusCommandPayload.POWER_ON);
            }
        }


        /// <summary>
        /// Set PSU On/OFF
        /// </summary>
        /// <param name="psuId">Psu Id</param>
        /// <param name="cmd">command ON or OFF</param>
        /// <returns>Completion code success/failure</returns>
        private CompletionCode SetPsuOnOff(PmBusCommandPayload payload)
        {
            CompletionCode returnPacket = new CompletionCode();
            returnPacket = CompletionCode.UnspecifiedError;

            try
            {
                PsuOnOffResponse response = new PsuOnOffResponse();

                response = (PsuOnOffResponse)this.SendReceive(this.PsuDeviceType, this.PsuId,
                new PsuPayloadRequest((byte)PmBusCommand.SET_POWER, (byte)payload, (byte)PmBusResponseLength.SET_POWER), typeof(PsuOnOffResponse));

                // check for completion code 
                if (response.CompletionCode != 0)
                {
                    returnPacket = (CompletionCode)response.CompletionCode;
                }
                else
                {
                    returnPacket = CompletionCode.Success;
                }
            }
            catch (System.Exception ex)
            {
                Tracer.WriteError("SetPsuOnOff failed with the exception: " + ex);
            }

            return returnPacket;
        }

    }
}
