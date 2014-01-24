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
    /// Represents the IPMI 'Get User Access' application response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Application, IpmiCommand.GetUserAccess)]
    internal class GetUserAccessResponse : IpmiResponse
    {
        ///<summary>
        /// Maximum number of User Ids.
        /// </summary>
        private byte maxUsers;

        ///<summary>
        /// Number of enabled User Ids.
        /// </summary>
        private byte enabledUsers;

        ///<summary>
        /// Privilege level of a given User Id.
        /// </summary>
        private byte accessLevel;

        ///<summary>
        /// Number of fixed names
        /// </summary>
        private byte fixedNames;

        ///<summary>
        /// Link Authorization
        /// </summary>
        private byte linkAuth;

        ///<summary>
        /// IPMI Messaging
        /// </summary>
        private byte ipmiMessaging;

        /// <summary>
        /// Gets and sets the Max Users.
        /// </summary>
        [IpmiMessageData(0)]
        public byte MaxUsers
        {
            get { return this.maxUsers; }
            set { this.maxUsers = value; }
        }

        /// <summary>
        /// Gets count of enabled users.
        /// </summary>
        /// <value>Session Handle.</value>
        [IpmiMessageData(1)]
        public byte EnabledUsers
        {
            get { return this.enabledUsers; }
            set { this.enabledUsers = (byte)(value & 0x1F); }
        }

        /// <summary>
        /// Gets Count of User Ids with fixed names.
        /// </summary>
        [IpmiMessageData(2)]
        public byte FixedNames
        {
            get { return this.fixedNames; }
            set { this.fixedNames = (byte)(value & 0x1F); }
        }

        /// <summary>
        /// Link authentication.
        /// </summary>  
        [IpmiMessageData(3)]
        public byte LinkAuth
        {
            get { return this.linkAuth; }
            set { this.linkAuth = (byte)((value & 0x20) >> 5); }

        }

        /// <summary>
        /// Ipmi Messaging.
        /// </summary>      
        [IpmiMessageData(3)]
        public byte IpmiMessaging
        {
            get { return this.ipmiMessaging; }
            set { this.ipmiMessaging = (byte)((value & 0x10)  >> 4); }

        }

        /// <summary>
        /// Access Level.
        /// </summary>
        /// <returns>
        /// 0h = reserved       
        /// 1h = CALLBACK level
        /// 2h = USER level
        /// 3h = OPERATOR level
        /// 4h = ADMINISTRATOR level
        /// 5h = OEM Proprietary level
        /// </returns>
        [IpmiMessageData(3)]
        public byte AccessLevel
        {
            get { return this.accessLevel; }
            set { this.accessLevel = (byte)(value & 0x0F); }

        }
    }
}
