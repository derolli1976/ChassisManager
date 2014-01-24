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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    /// <summary>
    /// Represents the IPMI 'Get Channel Info' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetChannelInfo, 2)]
    internal class GetChannelInfoResponse : IpmiResponse
    {

        /// <summary>
        /// Channel number.
        /// </summary>
        private byte channelNumber;

        /// <summary>
        /// Channel Medium.
        /// </summary>
        private byte channelMedium;

        /// <summary>
        /// Channel Protocol
        /// </summary>
        private byte channelProtocol;

        /// <summary>
        /// Session Support
        /// </summary>
        private byte sessionSupport;

        /// <summary>
        /// Gets and sets the Actual Channel number.
        /// </summary>
        /// <value>Channel number.</value>
        [IpmiMessageData(0)]
        public byte ChannelNumber
        {
            get { return this.channelNumber; }
            set { this.channelNumber = (byte)(value & 0x0F); }
        }

        /// <summary>
        /// Channel Medium.
        /// </summary>
        [IpmiMessageData(1)]
        public byte ChannelMedium
        {
            get { return this.channelMedium; }
            set { this.channelMedium = value; }
        }

        /// <summary>
        /// Channel Protocol
        /// </summary>
        [IpmiMessageData(2)]
        public byte ChannelProtocol
        {
            get { return this.channelProtocol; }
            set { this.channelProtocol = value; }
        }

        /// <summary>
        /// Channel Protocol
        /// </summary>
        [IpmiMessageData(3)]
        public byte SessionSupport
        {
            get { return this.sessionSupport; }
            set { this.sessionSupport = value; }
        }

        /// <summary>
        /// Channel Session Support
        ///     00b = channel is session-less
        ///     01b = channel is single-session
        ///     10b = channel is multi-session
        ///     11b = channel is session-based
        /// </summary>
        internal byte ChannelSessionSupport
        {
            get { return (byte)(this.sessionSupport & 0xC0); }
        }

        /// <summary>
        /// Number of sessions
        /// </summary>
        internal byte NumberOfSessions
        {
            get { return (byte)(this.sessionSupport & 0x3F); }
        }
    }
}
