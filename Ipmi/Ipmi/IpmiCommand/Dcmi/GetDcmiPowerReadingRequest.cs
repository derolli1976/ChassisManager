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
    /// Represents the DCMI 'Get Power Reading' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Dcgrp, IpmiCommand.DcmiPowerReading)]
    internal class GetDcmiPowerReadingRequest : IpmiRequest
    {
        /// <summary>
        /// Group Extension byte.  Always 0xDC
        /// </summary> 
        private byte groupextension = 0xDC;

        /// <summary>
        /// Mode byte.
        /// </summary> 
        private byte readingMode;

        /// <summary>
        /// Rolling Average byte.
        /// </summary> 
        private byte rollingAverage;

        /// <summary>
        /// byte 3 is currently reserved.
        /// </summary> 
        private byte reserved = 0x00;

        /// <summary>
        /// Initializes a new instance of the GetDcmiPowerReadingRequest class.
        /// </summary>
        internal GetDcmiPowerReadingRequest(byte readingMode, byte rollingAverage)
        {
            this.rollingAverage = rollingAverage;
            this.readingMode = readingMode;
        }

        /// <summary>
        /// Group Extension byte
        /// </summary>       
        [IpmiMessageData(0)]
        public byte GroupExtension
        {
            get { return this.groupextension; }

        }

        /// <summary>
        /// Mode byte
        /// </summary>       
        [IpmiMessageData(1)]
        public byte ReadingMode
        {
            get { return this.readingMode; }

        }

        /// <summary>
        /// Sets Rolling Average
        /// </summary>       
        [IpmiMessageData(2)]
        public byte RollingAverage
        {
            get { return this.rollingAverage; }

        }

        /// <summary>
        /// Reserved
        /// </summary>       
        [IpmiMessageData(3)]
        public byte Reserved
        {
            get { return this.reserved; }

        }
    }
}
