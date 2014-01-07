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
    /// Represents the IPMI 'Get FRU Inventory Info' application request message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Storage, IpmiCommand.GetFruInventoryAreaInfo)]
    internal class GetFruInventoryAreaInfoResponse : IpmiResponse
    {

        private byte offSetMS;

        private byte offSetLS;

        private byte accessType;

        /// <summary>
        /// Gets offset to read.
        /// </summary>       
        [IpmiMessageData(0)]
        public byte OffSetLS
        {
            get { return this.offSetLS; }
            set { this.offSetLS= value; }
        }

        /// <summary>
        /// Gets offset to read.
        /// </summary>       
        [IpmiMessageData(1)]
        public byte OffSetMS
        {
            get { return this.offSetMS; }
            set { this.offSetMS = value; }
        }

        /// <summary>
        /// Gets access type.
        /// </summary>
        /// <values>
        /// 0b = device is accessed by bytes
        /// 1b = device is accessed by word
        /// </values>
        [IpmiMessageData(3)]
        public byte AccessType
        {
            get { return this.accessType; }
            set { this.accessType = value; }
        }

    }
}
