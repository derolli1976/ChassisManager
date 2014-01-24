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
    /// Represents the IPMI 'BMC Debug' OEM request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.OemGroup, IpmiCommand.BmcDebug)]
    internal class BmcDebugRequest : IpmiRequest
    {

        /// <summary>
        /// Type of debug message to output
        /// </summary>
        private readonly byte _process;

        /// <summary>
        /// Enable/Disable
        /// </summary>
        private readonly byte _enable;

        /// <summary>
        /// Get Processor Info Request
        /// </summary>
        internal BmcDebugRequest(BmcDebugProcess process, bool enable)
        { 
            this._process = (byte)process;

            if (enable)
                this._enable = 0x01;
        }


        /// <summary>
        /// Process Type
        /// </summary>       
        [IpmiMessageData(0)]
        public byte Process
        {
            get { return this._process; }
        }

        /// <summary>
        /// Enable/Disable
        /// </summary>       
        [IpmiMessageData(1)]
        public byte Enable
        {
            get { return this._enable; }
        }
    }
}
