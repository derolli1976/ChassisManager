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
    /// Represents the IPMI 'Get Nic Info Request' OEM request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.OemGroup, IpmiCommand.GetNicInfo)]
    internal class GetNicInfoRequest : IpmiRequest
    {

        /// <summary>
        /// Nic zero based device index.  Default = 0x00
        /// </summary>
        private readonly byte _nic;

        /// <summary>
        /// Get Nic Info Request.  Index 0 based.
        /// </summary>
        internal GetNicInfoRequest(byte nic)
        { this._nic = nic; }


        /// <summary>
        /// Nic Number (zero based)
        /// </summary>       
        [IpmiMessageData(0)]
        public byte NIC
        {
            get { return this._nic; }

        }
    }
}
