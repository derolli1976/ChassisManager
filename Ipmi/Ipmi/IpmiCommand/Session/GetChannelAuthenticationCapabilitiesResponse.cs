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
    /// Represents the IPMI 'Get Channel Authentication Capabilities' application response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetChannelAuthenticationCapabilities)]
    internal class GetChannelAuthenticationCapabilitiesResponse : IpmiResponse
    {
        /// <summary>
        /// Channel number.
        /// </summary>
        private byte channelNumber;

        /// <summary>
        /// Authentication Type Support (first byte).
        /// </summary>
        private byte authenticationTypeSupport1;

        /// <summary>
        /// Authentication Type Support (second byte).
        /// </summary>
        private byte authenticationTypeSupport2;

        /// <summary>
        /// Authentication Type Support (third byte).
        /// </summary>
        private byte extendedCapabilities;

        /// <summary>
        /// OEM Id.
        /// </summary>
        private byte[] oemId;

        /// <summary>
        /// OEM Data.
        /// </summary>
        private byte oemData;

        /// <summary>
        /// Gets and sets the Channel number.
        /// </summary>
        /// <value>Channel number.</value>
        [IpmiMessageData(0)]
        public byte ChannelNumber
        {
            get { return this.channelNumber; }
            set { this.channelNumber = value; }
        }

        /// <summary>
        /// Gets and sets the Authentication Type Support (first byte).
        /// </summary>
        /// <value>Authentication Type Support (first byte).</value>
        [IpmiMessageData(1)]
        public byte AuthenticationTypeSupport1
        {
            get { return this.authenticationTypeSupport1; }
            set { this.authenticationTypeSupport1 = value; }
        }

        /// <summary>
        /// Gets and sets the Authentication Type Support (second byte).
        /// </summary>
        /// <value>Authentication Type Support (second byte).</value>
        [IpmiMessageData(2)]
        public byte AuthenticationTypeSupport2
        {
            get { return this.authenticationTypeSupport2; }
            set { this.authenticationTypeSupport2 = value; }
        }

        /// <summary>
        /// Gets and sets the Authentication Type Support (third byte).
        /// </summary>
        /// <value>Authentication Type Support (third byte).</value>
        [IpmiMessageData(3)]
        public byte ExtendedCapabilities
        {
            get { return this.extendedCapabilities; }
            set { this.extendedCapabilities = value; }
        }

        /// <summary>
        /// Gets and sets the OEM Id.
        /// </summary>
        /// <value>OEM Id and Data.</value>
        [IpmiMessageData(4,3)]
        public byte[] OemId
        {
            get { return this.oemId; }
            set { this.oemId = value; }
        }

        /// <summary>
        /// OEM auxiliary data
        /// </summary>
        /// <value>OEM auxiliary data</value>
        [IpmiMessageData(7)]
        public byte OemData
        {
            get { return this.oemData; }
            set { this.oemData = value; }
        }
    }
}
