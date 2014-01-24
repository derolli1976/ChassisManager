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
    /// Represents the IPMI 'Activate Session' application response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.ActivateSession)]
    internal class ActivateSessionResponse : IpmiResponse
    {
        /// <summary>
        /// Session authentication type.
        /// </summary>
        private AuthenticationType sessionAuthenticationType;

        /// <summary>
        /// Session Id.
        /// </summary>
        private uint sessionId;

        /// <summary>
        /// Initial inbound sequence number (can't be 0).
        /// </summary>
        private uint initialInboundSequenceNumber;

        /// <summary>
        /// Maximum privilege level for this session.
        /// </summary>
        private PrivilegeLevel maximumPrivilegeLevel;

        /// <summary>
        /// Gets and sets the Session authentication type.
        /// </summary>
        /// <value>Session authentication type.</value>
        [IpmiMessageData(0)]
        public byte SessionAuthenticationType
        {
            get { return (byte)this.sessionAuthenticationType; }
            set { this.sessionAuthenticationType = (AuthenticationType)value; }
        }

        /// <summary>
        /// Gets and sets the Session Id.
        /// </summary>
        /// <value>Session Id.</value>
        [IpmiMessageData(1)]
        public uint SessionId
        {
            get { return this.sessionId; }
            set { this.sessionId = value; }
        }

        /// <summary>
        /// Gets and sets the Initial inbound sequence number.
        /// </summary>
        /// <value>Initial inbound sequence number.</value>
        [IpmiMessageData(5)]
        public uint InitialInboundSequenceNumber
        {
            get { return this.initialInboundSequenceNumber; }
            set { this.initialInboundSequenceNumber = value; }
        }

        /// <summary>
        /// Gets and sets the Maximum privilege level for this session.
        /// </summary>
        /// <value>Maximum privilege level for this session.</value>
        [IpmiMessageData(9)]
        public byte MaximumPrivilegeLevel
        {
            get { return (byte)this.maximumPrivilegeLevel; }
            set { this.maximumPrivilegeLevel = (PrivilegeLevel)value; }
        }
    }
}
