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
