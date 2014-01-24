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
    /// Represents the IPMI 'Get Disk Status Command for WCS JBOD' OEM request message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Oem, IpmiCommand.GetDiskStatus)]
    class GetDiskStatusResponse : IpmiResponse
    {
        private byte channel;

        private byte diskcount;

        private byte[] statusData;

        /// <summary>
        /// Disk Controller Channel
        /// </summary>       
        [IpmiMessageData(0)]
        public byte Channel
        {
            get { return this.channel; }
            set { this.channel = value; }
        }

        /// <summary>
        /// Disk Count on Controller
        /// </summary>       
        [IpmiMessageData(1)]
        public byte DiskCount
        {
            get { return this.diskcount; }
            set { this.diskcount = value; }
        }

        /// <summary>
        /// Disk Status Data
        /// Each byte = [7-6]:  Disk Status (0 = Normal, 1 = Failed, 2 = Error)
        ///             [5-0]:  Disk #: Number/Location Id
        /// </summary>       
        [IpmiMessageData(2)]
        public byte[] StatusData
        {
            get { return this.statusData; }
            set { this.statusData = value; }
        }
    }
}
