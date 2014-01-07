/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*   							                        *
********************************************************/

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
