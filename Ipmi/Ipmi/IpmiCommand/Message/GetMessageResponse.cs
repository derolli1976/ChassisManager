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
    /// Represents the IPMI 'Get Message' application response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetMessage)]
    internal class GetMessageResponse : IpmiResponse
    {

        /// <summary>
        /// Channel Number.
        /// </summary>
        private byte channel;

        /// <summary>
        /// Privilege Level.
        /// </summary>
        private byte privilegeLevel;

        /// <summary>
        /// Response message payload.
        /// </summary>
        private byte[] messageData;

        /// <summary>
        /// Channel Number
        /// </summary>
        [IpmiMessageData(0)]
        public byte Channel
        {
            get { return this.channel; }
            set { this.channel = (byte)(value & 0x0f);
                  this.privilegeLevel = (byte)((value >> 4) & 0x0f);
                }
        }

        /// <summary>
        /// Response message payload.
        /// </summary>
        [IpmiMessageData(1)]
        public byte[] MessageData
        {
            get { return this.messageData; }
            set { this.messageData = value; }
        }

        /// <summary>
        /// Privilege Level
        /// </summary>
        public byte PrivilegeLevel
        {
            get { return this.privilegeLevel; }
        }
    }
}
