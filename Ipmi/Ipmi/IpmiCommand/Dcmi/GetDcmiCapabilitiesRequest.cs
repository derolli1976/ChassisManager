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
    /// Represents the DCMI 'Get DCMI Capabilities' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Dcgrp, IpmiCommand.DcmiCapability)]
    internal class GetDcmiCapabilitiesRequest : IpmiRequest
    {
        /// <summary>
        /// Group Extension byte.  Always 0xDC
        /// </summary> 
        private byte groupextension = 0xDC;

        /// <summary>
        /// Parameter Selector byte.
        /// </summary> 
        private byte parameterselector;

        /// <summary>
        /// Initializes a new instance of the GetDcmiCapabilitiesRequest class.
        /// </summary>
        internal GetDcmiCapabilitiesRequest(byte Selector)
        {
            this.parameterselector = Selector;
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
        /// Sets reservation MS byte
        /// </summary>       
        [IpmiMessageData(1)]
        public byte Selector
        {
            get { return this.parameterselector; }

        }
    }
}
