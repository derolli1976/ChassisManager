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
