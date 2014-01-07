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
    /// Represents the IPMI 'Close Session' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.CloseSession, 4)]
    internal class CloseSessionRequest : IpmiRequest
    {
        /// <summary>
        /// Session ID to close.
        /// </summary>
        private readonly uint sessionId;

        /// <summary>
        /// Initializes a new instance of the CloseSessionRequest class.
        /// </summary>
        /// <param name="sessionId">Session ID to close.</param>
        internal CloseSessionRequest(uint sessionId)
        {
            this.sessionId = sessionId;
        }

        /// <summary>
        /// Gets the Session ID to close.
        /// </summary>
        /// <value>Session ID to close.</value>
        [IpmiMessageData(0)]
        public uint SessionId
        {
            get { return this.sessionId; }
        }
    }
}
