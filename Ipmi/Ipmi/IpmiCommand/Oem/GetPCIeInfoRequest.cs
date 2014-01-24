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
    /// Represents the IPMI 'Get PCIe Info Request' OEM request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.OemGroup, IpmiCommand.GetPCIeInfo)]
    internal class GetPCIeInfoRequest : IpmiRequest
    {

        /// <summary>
        /// PCIe device index.  Default = 0x01
        /// </summary>
        private readonly byte _pcie;

        /// <summary>
        /// Get PCIe Info Request.  Index 1 based.
        /// </summary>
        internal GetPCIeInfoRequest(byte index)
        { this._pcie = index; }

        /// <summary>
        /// Get PCIe Info Request.  Index 1 based.
        /// </summary>
        internal GetPCIeInfoRequest(PCIeSlot slot)
        { this._pcie = (byte)(slot); }


        /// <summary>
        /// PCIe Number
        /// </summary>       
        [IpmiMessageData(0)]
        public byte PCIe
        {
            get { return this._pcie; }

        }
    }
}
