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
    /// Represents the IPMI 'Set User Access ' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.SetUserAccess, 4)]
    class SetUserAccessRequest : IpmiRequest
    {

        /// <summary>
        /// Max number of sessions allowed.
        /// </summary>    
        byte sessionLimit;

        /// <summary>
        /// User Id.
        /// </summary> 
        byte userId;


        /// <summary>
        /// Priviledge level for a User
        /// </summary> 
        byte userLimits;

        /// <summary>
        /// Request byte 1 for SetUserAccessRequest
        /// </summary> 
        byte requestByte1;


        public SetUserAccessRequest(byte userId, byte userLimit, byte requestbyte1, byte sessionLimit)
        {           

            this.userId = userId;
            this.userLimits = userLimit;            
            this.sessionLimit = sessionLimit;
            this.requestByte1 = requestbyte1;                        

        }

        

        /// <summary>
        /// Gets request byte 1 for SetUserAccessRequest.
        /// </summary>
        [IpmiMessageData(0)]
        public byte RequestByte1
        {
            get { return this.requestByte1; }
        }


        /// <summary>
        /// Gets the User Id.
        /// </summary>
        [IpmiMessageData(1)]
        public byte UserId
        {
            get { return this.userId; }
        }


        /// <summary>
        /// Gets the user limit\level.
        /// </summary>
        [IpmiMessageData(2)]
        public byte UserLimits
        {
            get { return this.userLimits; }
        }


        /// <summary>
        /// Sets the Session Limit.
        /// </summary>
        [IpmiMessageData(3)]
        public byte SessionLimit
        {
            get { return this.sessionLimit; }
        }
    }
}
