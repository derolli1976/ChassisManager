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
    /// Represents the Fan 'Get Fan Speed' request message.
    /// </summary>
    [ChassisMessageRequest(FunctionCode.GetFanSpeed)]
    internal class FanSpeedRequest : ChassisRequest
    {
    }

    /// <summary>
    /// Represents the Fan 'Get Fan Speed' application response message.
    /// </summary>
    [ChassisMessageResponse(FunctionCode.GetFanSpeed)]
    internal class FanSpeedResponse : ChassisResponse
    {
        // fan rpm
        private ushort rpm;

        /// <summary>
        /// Fan RPM
        /// </summary>
        [ChassisMessageData(0)]
        public ushort Rpm
        {
            get { return this.rpm; }
            set { this.rpm = value; }
        }
    }

}
