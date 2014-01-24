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
