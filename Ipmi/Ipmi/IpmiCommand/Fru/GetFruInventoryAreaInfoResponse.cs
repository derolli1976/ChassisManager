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
    /// Represents the IPMI 'Get FRU Inventory Info' application request message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Storage, IpmiCommand.GetFruInventoryAreaInfo)]
    internal class GetFruInventoryAreaInfoResponse : IpmiResponse
    {

        private byte offSetMS;

        private byte offSetLS;

        private byte accessType;

        /// <summary>
        /// Gets offset to read.
        /// </summary>       
        [IpmiMessageData(0)]
        public byte OffSetLS
        {
            get { return this.offSetLS; }
            set { this.offSetLS= value; }
        }

        /// <summary>
        /// Gets offset to read.
        /// </summary>       
        [IpmiMessageData(1)]
        public byte OffSetMS
        {
            get { return this.offSetMS; }
            set { this.offSetMS = value; }
        }

        /// <summary>
        /// Gets access type.
        /// </summary>
        /// <values>
        /// 0b = device is accessed by bytes
        /// 1b = device is accessed by word
        /// </values>
        [IpmiMessageData(3)]
        public byte AccessType
        {
            get { return this.accessType; }
            set { this.accessType = value; }
        }

    }
}
