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
    /// Represents the IPMI 'Get Channel Authentication Capabilities' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetChannelCipherSuites, 3)]
    internal class GetChannelCipherSuitesRequest : IpmiRequest
    {
        /// <summary>
        /// Channel number (0x0E == current channel this request was issued on).
        /// </summary>
        private readonly byte channelNumber = 0x0E;

        /// <summary>
        /// Payload Type
        /// (0x00) IPMI
        /// (0x01) SOL
        /// </summary>
        private byte payloadType;

        /// <summary>
        /// List index (0x00 - 0x3F)
        /// 0x00 = first set of 16 
        /// </summary>
        private readonly byte cipherIndex = 0x00;

        /// <summary>
        /// Initializes a new instance of the GetChannelCipherSuitesRequest class.
        /// </summary>
        /// <param name="IpmiPayloadType">Ipmi Payload Type.</param>
        internal GetChannelCipherSuitesRequest(IpmiPayloadType payload)
        {
            this.payloadType = (byte)payload;
        }

        /// <summary>
        /// Gets the Channel number.
        /// </summary>
        /// <value>Channel number.</value>
        [IpmiMessageData(0)]
        public byte ChannelNumber
        {
            get { return this.channelNumber; }
        }

        /// <summary>
        /// Gets the Ipmi pay load type.
        /// </summary>
        /// <value>IpmiPayloadType</value>
        [IpmiMessageData(1)]
        public byte PayloadType
        {
            get { return this.payloadType; }
        }

        /// <summary>
        /// Cipher suite index (0x00 = first 16)
        /// </summary>
        /// <value>Cipher suite index</value>
        [IpmiMessageData(2)]
        public byte CipherIndex
        {
            get { return this.cipherIndex; }
        }
    }
}

