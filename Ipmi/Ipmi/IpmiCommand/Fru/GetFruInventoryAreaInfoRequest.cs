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
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.GetFruInventoryAreaInfo, 1)]
    internal class GetFruInventoryAreaInfoRequest : IpmiRequest
    {
        private byte deviceId;

        
        /// <summary>
        /// Initializes a new instance of the GetFruInventoryAreaInfoRequest class.
        /// </summary>
        /// <param name="deviceId">byte value for device Id</param>
        internal GetFruInventoryAreaInfoRequest(byte deviceId)
        {
            this.deviceId = deviceId;
        }

        /// <summary>
        /// Gets FRU Inventory Area Info.
        /// </summary>       
        [IpmiMessageData(0)]
        public byte DeviceId
        {
            get { return this.deviceId; }
        
        }
    }
}
