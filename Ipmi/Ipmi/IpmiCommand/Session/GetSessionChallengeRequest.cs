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
    /// Represents the IPMI 'Get Session Challenge' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetSessionChallenge, 17)]
    internal class GetSessionChallengeRequest : IpmiRequest
    {
        /// <summary>
        /// Challenge authentication type (MD5 by default).
        /// </summary>
        private readonly byte challengeAuthenticationType = 0x02;

        /// <summary>
        /// User name.
        /// </summary>
        private readonly byte[] UserId;

        /// <summary>
        /// Initializes a new instance of the GetSessionChallengeRequest class.
        /// </summary>
        /// <param name="authenticationType">Authentication type for challenge.</param>
        /// <param name="userid">Userid or null.</param>
        internal GetSessionChallengeRequest(AuthenticationType authenticationType, string userId)
        {
            this.challengeAuthenticationType = (byte)authenticationType;

            this.UserId = new byte[16];

            if (!string.IsNullOrEmpty(userId))
            {
                for (int i = 0; i < userId.Length; i++)
                {
                    this.UserId[i] = (byte)userId[i];
                }
            }
        }

        /// <summary>
        /// Gets the Challenge authentication type (always MD5).
        /// </summary>
        /// <value>Challenge authentication type (always MD5).</value>
        [IpmiMessageData(0)]
        public byte ChallengeAuthenticationType
        {
            get { return this.challengeAuthenticationType; }
        }

        /// <summary>
        /// Gets the user name.
        /// </summary>
        /// <value>16 byte array representing the user name or all 0's for null.</value>
        [IpmiMessageData(1)]
        public byte[] UserName
        {
            get { return this.UserId; }
        }
    }
}
