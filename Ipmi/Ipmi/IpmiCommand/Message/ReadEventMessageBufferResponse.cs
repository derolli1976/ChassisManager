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
    /// Represents the IPMI 'Read Event Message Buffer' application response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.ReadEventMessageBuffer)]
    internal class ReadEventMessageBufferResponse : IpmiResponse
    {

        /// <summary>
        /// Response Read Event Message Buffer payload.
        /// </summary>
        private byte[] messageData;

        /// <summary>
        /// Response Read Event Message Buffer payload.
        /// </summary>
        [IpmiMessageData(0)]
        public byte[] MessageData
        {
            get { return this.messageData; }
            set { this.messageData = value; }
        }
    }
}
