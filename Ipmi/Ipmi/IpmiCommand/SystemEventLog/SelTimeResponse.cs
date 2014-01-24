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
    using System;
    /// <summary>
    /// Represents the IPMI 'Get SEL Time' response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Storage, IpmiCommand.GetSelTime)]
    internal class SelTimeResponse : IpmiResponse
    {
        private byte[] offset;

        /// <summary>
        /// Sel Time
        /// </summary>       
        [IpmiMessageData(0)]
        public byte[] SecondsOffset
        {
            get { return this.offset; }
            set { this.offset = value; }
        }

        /// <summary>
        /// Returns fromatted DateTime of System Event Log
        /// </summary>
        internal DateTime Time
        {
            get { return IpmiSharedFunc.SecondsOffSet(BitConverter.ToInt32(offset, 0)); }
        }

    }
}
