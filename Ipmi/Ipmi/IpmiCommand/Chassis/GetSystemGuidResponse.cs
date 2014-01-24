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
    /// Represents the IPMI 'Get Guid' application response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Application, IpmiCommand.GetSystemGuid)]
    internal class GetSystemGuidResponse : IpmiResponse
    {
        /// <summary>
        /// Device Guid.
        /// </summary>
        private byte[] guid = new byte[16];

        /// <summary>
        /// Gets Device Guid.
        /// </summary>
        [IpmiMessageData(0)]
        public byte[] Guid
        {
            get { return this.guid; }
            set {
                if (value != null)
                {
                    if (value.Length == 16)
                    {
                        Buffer.BlockCopy(value, 0, this.guid, 0, 16);
                    }
                }
            }
        }
    }
}
