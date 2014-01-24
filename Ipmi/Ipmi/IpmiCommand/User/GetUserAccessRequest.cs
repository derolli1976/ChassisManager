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
    /// Represents the IPMI 'Get User Access ' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetUserAccess)]
    internal class GetUserAccessRequest : IpmiRequest
    {
        /// <summary>
        // channel number
        /// <summary>
        private byte channelNumber;

        /// <summary>
        /// User Id
        /// <summary>
        private byte userId;
       
        /// <summary>
        /// Initializes a new instance of the GetUserAccessRequest class.
        /// </summary>
        public GetUserAccessRequest(byte channelNum, byte userId)
        {
            this.channelNumber = channelNum;
            this.userId = userId;
        }

        /// <summary>
        /// Gets the Channel number.
        /// </summary>
        /// <value>Channel number.</value>
        [IpmiMessageData(0)]
        public byte ChannelNumber
        {
            get { return this.channelNumber; }
        }

        /// <summary>
        /// Sets the reserved value.
        /// </summary>
        /// <value>reserved</value>
        [IpmiMessageData(1)]
        public byte UserId
        {
            get { return this.userId; }
        }


        
    }
}
