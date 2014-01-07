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
    /// Represents the IPMI 'Get Session Info' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetSessionInfo)]
    internal class GetSessionInfoRequest : IpmiRequest
    {
        /// <summary>
        /// Session index to retrieve information on.
        /// </summary>
        private readonly byte sessionIndex;

        /// <summary>
        /// Session Handle.
        /// </summary>
        private readonly uint sessionHandle;

        /// <summary>
        /// Initializes a new instance of the GetSessionInfoRequest class.
        /// </summary>
        /// <param name="sessionIndex">Session Index or 0 for current session.</param>
        internal GetSessionInfoRequest(byte sessionIndex)
        {
            this.sessionIndex = sessionIndex;
        }

        /// <summary>
        /// Initializes a new instance of the GetSessionInfoRequest class.
        /// </summary>
        /// <param name="sessionIndex">Session Index or 0 for current session.</param>
        /// <param name="sessionHandle">Session Handle or 0 for current session</param>
        internal GetSessionInfoRequest(byte sessionIndex, uint sessionHandle)
        {
            this.sessionIndex = sessionIndex;
            this.sessionHandle = sessionHandle;
        }

        /// <summary>
        /// Gets the Session Index.
        /// </summary>
        /// <value>Session Index or 0 for current session.</value>
        [IpmiMessageData(0)]
        public byte SessionIndex
        {
            get { return this.sessionIndex; }
        }

        /// <summary>
        /// Gets the Session Handle.
        /// </summary>
        /// <value>Session Handle.</value>
        [IpmiMessageData(1)]
        public uint SessionHandle
        {
            get { return this.sessionHandle; }
        }
    }
}
