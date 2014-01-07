/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*   							                        *
*   							                        *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{

    /// <summary>
    /// Represents the IPMI 'Get FRU Inventory Info' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.WriteFruData)]
    internal class WriteFruDataRequest : IpmiRequest
    {
        private byte devId = 0x00;

        private const byte readFruData = 0x11;

        private byte offSetLS;

        private byte offSetMS;

        private byte[] payload;
        
        internal WriteFruDataRequest(byte offSetLS, byte offSetMS, byte[] payload)
        {
            this.offSetLS = offSetLS;
            this.offSetMS = offSetMS;
            this.payload = payload;
        
        }

        /// <summary>
        /// Gets and sets Device Id.
        /// </summary>       
        [IpmiMessageData(0)]
        public byte DeviceId
        {
            get { return this.devId; }
            set { this.devId = value; }
        }

        /// <summary>
        /// Offset to read, LS byte.
        /// </summary>       
        [IpmiMessageData(1)]
        public byte OffSetLS
        {
            get { return this.offSetLS; }
            set { this.offSetLS = value; }
        }

        /// <summary>
        /// Offset to read, MS byte.
        /// </summary>       
        [IpmiMessageData(2)]
        public byte OffSetMS
        {
            get { return this.offSetMS; }
            set { this.offSetMS = value; }
        }


        /// <summary>
        /// Count to read, 1 based
        /// </summary>       
        [IpmiMessageData(3)]
        public byte[] Payload
        {
            get { return this.payload; }
            set { this.payload = value; }
        }
    }
}
