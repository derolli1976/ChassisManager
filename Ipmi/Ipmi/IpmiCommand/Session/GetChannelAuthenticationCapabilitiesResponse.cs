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
