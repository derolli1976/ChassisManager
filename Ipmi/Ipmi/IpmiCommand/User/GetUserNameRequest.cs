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
    /// Represents the IPMI 'Get UserName' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetUserName, 1)]
    internal class GetUserNameRequest : IpmiRequest
    {

        // Id of the user account
        private byte UserId;

        
        /// <summary>
        /// Initializes a new instance of the SetSessionPrivilegeLevelRequest class.
        /// </summary>
        public GetUserNameRequest(byte userId)
        {
            this.UserId = userId;
        }

        /// <summary>
        /// Gets the user name for a given id.
        /// </summary>
        /// <value>User name</value>
        [IpmiMessageData(0)]
        public byte GetUserId
        {
            get { return this.UserId; }
        }
    }
}
