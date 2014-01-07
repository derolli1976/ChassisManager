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
    /// Represents the IPMI 'Get Session Challenge' application response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetSessionChallenge)]
    internal class GetSessionChallengeResponse : IpmiResponse
    {
        /// <summary>
        /// Temporary Session Id.
        /// </summary>
        private uint temporarySessionId;

        /// <summary>
        /// Challenge String Data.
        /// </summary>
        private byte[] challengeStringData;

        /// <summary>
        /// Gets the Temporary Session Id.
        /// </summary>
        /// <value>Temporary Session Id.</value>
        [IpmiMessageData(0)]
        public uint TemporarySessionId
        {
            get { return this.temporarySessionId; }
            set { this.temporarySessionId = value; }
        }

        /// <summary>
        /// Gets and sets the Challenge String Data.
        /// </summary>
        /// <value>16 byte array representing the Challenge String Data.</value>
        [IpmiMessageData(4, 16)]
        public byte[] ChallengeStringData
        {
            get { return this.challengeStringData; }
            set { this.challengeStringData = value; }
        }
    }
}
