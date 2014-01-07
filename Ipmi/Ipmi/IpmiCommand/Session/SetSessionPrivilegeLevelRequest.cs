/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*                                       	            *
********************************************************/

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