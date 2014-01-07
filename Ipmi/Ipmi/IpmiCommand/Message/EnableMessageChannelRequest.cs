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
    /// Represents the IPMI 'Enable Message Channel Receive Message' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.EnableMessageChannelReceive)]
    internal class EnableMessageChannelReceiveRequest : IpmiRequest
    {

        /// <summary>
        /// Channel to send the message.
        /// </summary>
        private readonly byte channel;

        /// <summary>
        /// Channel Enable/Disable State.
        /// </summary>
        private byte channelState;

        /// <summary>
        /// Initializes a new instance of the EnableMessageChannelReceiveRequest class.
        /// </summary>
        /// <param name="channel">Channel to enable/disable.</param>
        /// <param name="enableMessageReceive">Channel Enable/Disable State.</param>
        internal EnableMessageChannelReceiveRequest(byte channel, bool enableMessageReceive)
        {
            this.channel = (byte)(channel & 0x0f);

            if(enableMessageReceive)
                this.channelState = 0x01; // 01b = enable channel
            else
                this.channelState = 0x00; // 00b = disable channel
        }

        /// <summary>
        /// Initializes a new instance of the EnableMessageChannelReceiveRequest class.
        /// </summary>
        /// <param name="channel">Channel to enable/disable.</param>
        internal EnableMessageChannelReceiveRequest(byte channel)
        {
            this.channel = (byte)(channel & 0x0f);

            // 10b = get channel enable/disable state
            this.channelState = 0x02;
        }

        /// <summary>
        /// Channel to send the request message.
        /// </summary>
        [IpmiMessageData(0)]
        public byte Channel
        {
            get { return this.channel; }
        }

        /// <summary>
        /// Channel State
        /// </summary>
        [IpmiMessageData(1)]
        public byte ChannelState
        {
            get { return this.channelState; }
        }

    }
}
