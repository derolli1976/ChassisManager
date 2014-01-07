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
    /// Represents the IPMI 'Get Channel Authentication Capabilities' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetChannelAuthenticationCapabilities, 2)]
    internal class GetChannelAuthenticationCapabilitiesRequest : IpmiRequest
    {
        /// <summary>
        /// Bit [7] = true for IPMI v2 RMCP+ extended data
        /// Channel number (0x0E == current channel this request was issued on).
        /// </summary>
        private readonly byte channelNumber = 0x0E;

        /// <summary>
        /// Requested maximum privilege level.
        /// </summary>
        private readonly byte requestedPrivilegeLevel;

        /// <summary>
        /// Initializes a new instance of the GetChannelAuthenticationCapabilitiesRequest class.
        /// </summary>
        /// <param name="maximumPrivilegeLevel">Requested maximum privilege level.</param>
        internal GetChannelAuthenticationCapabilitiesRequest(PrivilegeLevel privilegeLevel)
        {
            this.requestedPrivilegeLevel = (byte)privilegeLevel;
        }

        /// <summary>
        /// Initializes a new instance of the GetChannelAuthenticationCapabilitiesRequest class.
        /// </summary>
        /// <param name="maximumPrivilegeLevel">Requested maximum privilege level.</param>
        internal GetChannelAuthenticationCapabilitiesRequest(byte channelNumber, PrivilegeLevel privilegeLevel)
        {
            // Channel number (0x0E == current channel this request was issued on)
            this.channelNumber = channelNumber;
            this.requestedPrivilegeLevel = (byte)privilegeLevel;
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
        /// Gets the Requested maximum privilege level.
        /// </summary>
        /// <value>Requested maximum privilege level.</value>
        [IpmiMessageData(1)]
        public byte RequestedPrivilegeLevel
        {
            get { return this.requestedPrivilegeLevel; }
        }
    }
}
