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
    /// Set Fan Speed Request
    /// </summary>
    [ChassisMessageRequest(FunctionCode.SetFanSpeed)]
    internal class FanSetRpmRequest : ChassisRequest 
    {
       
        /// <summary>
        /// Set the Fan Speed
        /// </summary>
        public FanSetRpmRequest(byte pwm)
        {
            this.Pwm = pwm;
        }

        /// <summary>
        /// Sets RPM for the Fan
        /// </summary>
        [ChassisMessageData(0)]
        public byte Pwm
        {
            get; set;            
        }
    }

    [ChassisMessageResponse(FunctionCode.SetFanSpeed)]
    internal class FanSetResponse : ChassisResponse
    {
    }
}
