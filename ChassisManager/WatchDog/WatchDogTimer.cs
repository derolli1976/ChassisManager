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
    /// Class for Watch Dog Timer (only enable and reset commands are supported)
    /// </summary>
    public class WatchDogTimer : ChassisSendReceive
    {
        protected byte _deviceId;

        /// <summary>
        /// Constructor for WatchDogTimer
        /// </summary>
        /// <param name="deviceId"></param>
        public WatchDogTimer(byte deviceId)
        {
            this._deviceId = deviceId;
        }

        /// <summary>
        /// Public function that calls internal watchdog enable function
        /// </summary>
        public void EnableWatchDogTimer()
        {
            EnableWatchDogTimer(this._deviceId);
        }
        /// <summary>
        /// Enables the WatchDog Timer during initialization
        /// </summary>
        private void EnableWatchDogTimer(byte deviceId)
        {
            this.SendReceive(DeviceType.WatchDogTimer, deviceId, new WatchDogEnable(),
                typeof(WatchDogEnableResponse), (byte) PriorityLevel.System);
        }

        /// <summary>
        /// Public function calls internal reset watchdog timer
        /// </summary>
        public void ResetWatchDogTimer()
        {
            ResetWatchDogTimer(this._deviceId);
        }
        /// <summary>
        /// Resets the WatchDog timer when called
        /// </summary>
        private void ResetWatchDogTimer(byte deviceId)
        {
            this.SendReceive(DeviceType.WatchDogTimer, deviceId, new WatchDogReset(),
                typeof(WatchDogResetResponse), (byte) PriorityLevel.System);
        }

        /// <summary>
        /// Enable chassis request - only Command needed
        /// </summary>
        [ChassisMessageRequest(FunctionCode.EnableWatchDogTimer)]
        internal class WatchDogEnable : ChassisRequest
        {
        }

        /// <summary>
        /// Reset request
        /// </summary>
        [ChassisMessageRequest(FunctionCode.ResetWatchDogTimer)]
        internal class WatchDogReset : ChassisRequest
        {
        }

        /// <summary>
        /// Empty response for watchdog timer enable
        /// </summary>
        [ChassisMessageResponse(FunctionCode.EnableWatchDogTimer)]
        internal class WatchDogEnableResponse : ChassisResponse
        {
        }

        /// <summary>
        /// Empty response for watchdog timer reset
        /// </summary>
        [ChassisMessageResponse(FunctionCode.ResetWatchDogTimer)]
        internal class WatchDogResetResponse : ChassisResponse
        {
        }
    }
}
