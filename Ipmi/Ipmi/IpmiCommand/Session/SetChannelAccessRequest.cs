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
    using System.Collections;

    /// <summary>
    /// Represents the IPMI 'Set Channel Access Command' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.SetChannelAccess)]
    internal class SetChannelAccessRequest : IpmiRequest
    {
        /// <summary>
        /// Session index to retrieve information on.
        /// </summary>
        private byte channel;

        /// <summary>
        /// Change Access byte.  
        /// </summary>
        private byte access;

        /// <summary>
        /// Privilege byte.  
        /// </summary>
        private byte privilege;


        /// <summary>
        /// Initializes a new instance of the SetChannelAccessRequest class.
        /// </summary>
        /// <param name="channel">Target Channel Number.</param>
        internal SetChannelAccessRequest(byte channel)
        {
            this.channel = (byte)(channel & 0x0F);
        }

        /// <summary>
        /// Initializes a new instance of the SetChannelAccessRequest class.
        /// </summary>
        /// <param name="channel">Target Channel Number.</param>
        internal SetChannelAccessRequest(byte channel, bool enablePef, bool userAuth, bool accessMode)
        {
            this.channel = (byte)(channel & 0x0F);

            byte[] accessByte = new byte[1];

            BitArray accessBits = new BitArray(accessByte);

            accessBits[0] = true;           // [2:0] -  Access Mode for IPMI messaging
            accessBits[1] = accessMode;     // [2:0] -  Access Mode for IPMI messaging
            accessBits[2] = false;          // [2:0] -  Access Mode for IPMI messaging
            accessBits[3] = userAuth;       // User Level AuthenticationEnable/Disable
            accessBits[4] = true;           //1b = disable Per-message Authentication
            accessBits[5] = enablePef;      // Enable PEF Alerting
            accessBits[6] = true;          // [7:6] 10b = set volatile (active) setting of Channel Access according to bits
            accessBits[7] = false;           // [7:6] 10b = set volatile (active) setting of Channel Access according to bits

            accessBits.CopyTo(accessByte, 0);

            // set access byte
            access = accessByte[0];

            byte[] privilegeByte = new byte[1];

            BitArray privilegeBits = new BitArray(privilegeByte);

            privilegeBits[0] = false;       // [3:0] -  Channel Privilege Level Limit
            privilegeBits[1] = false;       // [3:0] -  Channel Privilege Level Limit
            privilegeBits[2] = true;        // [3:0] -  Channel Privilege Level Limit
            privilegeBits[3] = false;       // [3:0] -  Channel Privilege Level Limit
            privilegeBits[4] = false;       // [5:4] -  Reserved
            privilegeBits[5] = enablePef;   // [5:4] -  Reserved
            privilegeBits[6] = true;        // [7:6] set volatile setting of Privilege Level Limit according to bits [3:0] 
            privilegeBits[7] = false;       // [7:6] set volatile setting of Privilege Level Limit according to bits [3:0] 

            privilegeBits.CopyTo(privilegeByte, 0);

            // set privilege byte
            privilege = privilegeByte[0];

        }

        /// <summary>
        /// Target Channel Number.
        /// </summary>
        /// <value>Target Channel Number..</value>
        [IpmiMessageData(0)]
        public byte Channel
        {
            get { return this.channel; }
        }

        /// <summary>
        /// Channel Access Mode
        /// </summary>
        /// <value>Channel Access Mode</value>
        [IpmiMessageData(1)]
        public byte Access
        {
            get { return this.access; }
        }

        /// <summary>
        /// Privilege.
        /// </summary>
        /// <value>Authentication Privilege Level.</value>
        [IpmiMessageData(2)]
        public byte Privilege
        {
            get { return this.privilege; }
        }
    }
}
