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

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    /// <summary>
    /// Represents the DCMI 'Activate/Deactivate Power Limit' response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Dcgrp, IpmiCommand.DcmiActivateLimit)]
    internal class DcmiActivatePowerLimitResponse : IpmiResponse
    {
        /// <summary>
        /// Group Extension (0xDC).
        /// </summary>
        private byte groupExtension;

        /// <summary>
        /// Group Extension
        /// </summary>
        /// <value>0xDC</value>
        [IpmiMessageData(0)]
        public byte GroupExtension
        {
            get { return this.groupExtension; }
            set { this.groupExtension = value; }
        }
    }
}
