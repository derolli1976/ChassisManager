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
    /// Represents the IPMI 'Get Processor Info Command' OEM request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.OemGroup, IpmiCommand.GetProcessorInfo)]
    internal class GetProcessorInfoRequest : IpmiRequest
    {

        /// <summary>
        /// Processor device index.  Default = 0x00
        /// Get Processor Information changed to zero based 
        /// indexing in firmware V3.05
        /// </summary>
        private readonly byte _processor;

        /// <summary>
        /// Get Processor Info Request
        /// </summary>
        internal GetProcessorInfoRequest(byte processor)
        { this._processor = processor; }


        /// <summary>
        /// Processor Number
        /// </summary>       
        [IpmiMessageData(0)]
        public byte Processor
        {
            get { return this._processor; }

        }
    }
}
