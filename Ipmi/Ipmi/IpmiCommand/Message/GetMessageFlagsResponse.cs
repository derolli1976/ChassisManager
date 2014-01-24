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
    /// Represents the IPMI 'Get Message Flags' application response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetMessageFlags)]
    internal class GetMessageFlagsResponse : IpmiResponse
    {

        /// <summary>
        /// Message Flags
        /// </summary>
        private byte flags;

        /// <summary>
        /// Channel Number
        /// </summary>
        [IpmiMessageData(0)]
        public byte Flags
        {
            get { return this.flags; }
            set { this.flags = value;}
        }

        /// <summary>
        /// Receive Message Available
        /// </summary>
        public byte MessageAvailable
        {
            get { return (byte)(this.flags & 0x01); }
        }

        /// <summary>
        /// Receive Buffer full
        /// </summary>
        public byte BufferFull
        {
            get { return (byte)((this.flags & 0x02) >> 1); }
        }

        /// <summary>
        /// Watch Dog pre-timeout interrupt
        /// </summary>
        public byte WatchDogTimeout
        {
            get { return (byte)((this.flags & 0x08) >> 3); }
        }

        /// <summary>
        /// OEM 1 Data Available
        /// </summary>
        public byte OEM1
        {
            get { return (byte)((this.flags & 0x20) >> 5); }
        }

        /// <summary>
        /// OEM 2 Data Available
        /// </summary>
        public byte OEM2
        {
            get { return (byte)((this.flags & 0x40) >> 6); }
        }

        /// <summary>
        /// OEM 3 Data Available
        /// </summary>
        public byte OEM3
        {
            get { return (byte)((this.flags & 0x80) >> 7); }
        }
    }
}
