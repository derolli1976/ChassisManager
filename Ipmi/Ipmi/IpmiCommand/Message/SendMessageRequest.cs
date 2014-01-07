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
    /// Represents the IPMI 'Send Message' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.SendMessage)]
    internal class SendMessageRequest : IpmiRequest
    {

        /// <summary>
        /// Channel to send the message.
        /// </summary>
        private readonly byte channel;

        /// <summary>
        /// Message payload to send over the channel.
        /// </summary>
        private byte[] messageData;

        /// <summary>
        /// Initializes a new instance of the SendMessageRequest class.
        /// </summary>
        /// <param name="channel">Channel to direct the message.</param>
        /// <param name="messageData">Message Data. Format dependent on target channel type.</param>
        internal SendMessageRequest(byte channel, byte[] messageData)
        {
            this.channel = (byte)(channel & 0x0f);
            this.messageData = messageData;
        }

        /// <summary>
        /// Initializes a new instance of the SendMessageRequest class.
        /// </summary>
        /// <param name="channel">Channel to direct the message.</param>
        /// <param name="tracking">BMC should Track Request.</param>
        /// <param name="messageData">Message Data. Format dependent on target channel type.</param>
        internal SendMessageRequest(byte channel, bool tracking, byte[] messageData)
        {
            channel = (byte)(channel & 0x0f);

            if (tracking)
                channel = (byte)(channel | 0x40);

            this.channel = channel;
            this.messageData = messageData;
        }

        /// <summary>
        /// Initializes a new instance of the SendMessageRequest class.
        /// </summary>
        /// <param name="channel">Channel to direct the message.</param>
        /// <param name="tracking">BMC should Track Request.</param>
        /// <param name="encryption">Send message with encryption.</param>
        /// <param name="authentication">Send message with authentication.</param>
        /// <param name="messageData">Message Data. Format dependent on target channel type.</param>
        internal SendMessageRequest(byte channel, bool tracking, bool encryption, bool authentication, byte[] messageData)
        {
            channel = (byte)(channel & 0x0f);

            if(tracking)
                channel = (byte)(channel | 0x40);

            if (encryption)
                channel = (byte)(channel | 0x20);

            if(authentication)
                channel = (byte)(channel | 0x10);

            this.channel = channel;
            this.messageData = messageData;
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
        /// Message Data. Format dependent on target channel type.
        /// </summary>
        [IpmiMessageData(1)]
        public byte[] MessageData
        {
            get { return this.messageData; }
        }

    }
}
