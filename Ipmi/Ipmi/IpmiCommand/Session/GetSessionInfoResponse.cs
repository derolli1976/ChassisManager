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
    /// Represents the IPMI 'Get Session Info' application response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Application, IpmiCommand.GetSessionInfo)]
    internal class GetSessionInfoResponse : IpmiResponse
    {
        /// <summary>
        /// Session Handle.
        /// </summary>
        private byte sessionHandle;

        /// <summary>
        /// Gets and sets the Session Handle.
        /// </summary>
        /// <value>Session Handle.</value>
        [IpmiMessageData(0)]
        public byte SessionHandle
        {
            get { return this.sessionHandle; }
            set { this.sessionHandle = value; }
        }
    }
}
