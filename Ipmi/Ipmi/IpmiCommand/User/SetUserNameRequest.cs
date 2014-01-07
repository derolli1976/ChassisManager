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
    /// Represents the IPMI 'Set User Name' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.SetUserName, 17)]
    internal class SetUserNameRequest : IpmiRequest
    {
        byte userId;

        byte[] userName;


        internal SetUserNameRequest(byte userId, byte[] userName)
        {
            this.userId = userId;
            this.userName = userName;
        }

         /// <summary>
        /// Set the password operation.
        /// </summary>       
        [IpmiMessageData(0)]
        public byte UserId
        {
            get { return this.userId; }
        }


        /// <summary>
        /// Set the UserName.
        /// </summary>       
        [IpmiMessageData(1,16)]
        public byte[] UserName
        {
            get { return this.userName; }
        }
    }
    
    
}
