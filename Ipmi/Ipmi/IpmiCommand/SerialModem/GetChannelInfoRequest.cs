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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    /// <summary>
    /// Represents the IPMI 'Get Channel Info' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetChannelInfo, 1)]
    internal class GetChannelInfoRequest : IpmiRequest
    {

        /// <summary>
        /// Channel Number  
        /// 0x0E = Channel the request is being sent over.
        /// </summary>
        private readonly byte channel;

        /// <summary>
        /// Initializes a new instance of the GetChannelInfoRequest class.
        /// </summary>
        internal GetChannelInfoRequest(byte channel)
        {
            this.channel = channel;
        }

        /// <summary>
        /// Initializes a new instance of the GetChannelInfoRequest class.
        /// Based on the Channel the request is being sent over.
        /// </summary>
        internal GetChannelInfoRequest()
        {
            this.channel = 0x0E;
        }

        /// <summary>
        /// Channel Number  
        /// 0x0E = Channel the request is being sent over.
        /// </summary>
        [IpmiMessageData(0)]
        public byte Channel
        {
            get { return this.channel; }
        }



    }
}
