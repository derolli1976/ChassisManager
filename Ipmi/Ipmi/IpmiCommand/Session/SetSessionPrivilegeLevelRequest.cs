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
    /// Represents the IPMI 'Set Session Privilege Level' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.SetSessionPrivilegeLevel, 1)]
    internal class SetSessionPrivilegeLevelRequest : IpmiRequest
    {
        /// <summary>
        /// Requested Privilege Level.
        /// </summary>
        private readonly PrivilegeLevel requestedPrivilegeLevel;

        /// <summary>
        /// Initializes a new instance of the SetSessionPrivilegeLevelRequest class.
        /// </summary>
        internal SetSessionPrivilegeLevelRequest(PrivilegeLevel requestedPrivilegeLevel)
        {
            this.requestedPrivilegeLevel = requestedPrivilegeLevel;
        }

        /// <summary>
        /// Gets the Requested Privilege Level.
        /// </summary>
        /// <value>Requested Privilege Level.</value>
        [IpmiMessageData(0)]
        public byte RequestedPrivilegeLevel
        {
            get { return (byte)this.requestedPrivilegeLevel; }
        }
    }
}
