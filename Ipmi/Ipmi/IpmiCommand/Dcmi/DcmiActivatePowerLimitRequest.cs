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
    /// Represents the DCMI 'Activate/Deactivate Power Limit' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Dcgrp, IpmiCommand.DcmiActivateLimit)]
    internal class DcmiActivatePowerLimitRequest : IpmiRequest
    {
        /// <summary>
        /// Group Extension byte.  Always 0xDC
        /// </summary> 
        private byte groupExtension = 0xDC;

        /// <summary>
        /// Power Limit Activation (0x00 = Deactivate 0x01 = Activate) 
        /// </summary> 
        private byte powerLimitActivation;

        /// <summary>
        /// 2 bytes reserved for future use
        /// </summary>
        private byte[] reservation = { 0x00, 0x00 };

        /// <summary>
        /// Initializes a new instance of the DcmiActivatePowerLimitRequest class.
        /// </summary>
        internal DcmiActivatePowerLimitRequest(bool enable)
        {
            if (enable)
            {
                // Activate
                this.powerLimitActivation = 0x01;
            }
            else
            {
                // Deactivate
                this.powerLimitActivation = 0x00;
            }
        }

        /// <summary>
        /// Group Extension byte
        /// </summary>       
        [IpmiMessageData(0)]
        public byte GroupExtension
        {
            get { return this.groupExtension; }

        }

        /// <summary>
        /// Reserved
        /// </summary>       
        [IpmiMessageData(1)]
        public byte PowerLimitActivation
        {
            get { return this.powerLimitActivation; }

        }

        /// <summary>
        /// Exception Action 
        /// </summary>       
        [IpmiMessageData(2,2)]
        public byte[] Reservation
        {
            get { return this.reservation; }
        }

    }
}
