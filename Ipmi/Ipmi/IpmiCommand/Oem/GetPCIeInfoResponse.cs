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
    /// Represents the IPMI 'Get PCIe Info Command' request message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.OemGroup, IpmiCommand.GetPCIeInfo)]
    class GetPCIeInfoResponse : IpmiResponse
    {
        /// <summary>
        /// Vendor Id
        /// </summary>
        private ushort _vendorId;

        /// <summary>
        /// Device Id
        /// </summary>
        private ushort _deviceId;

        /// <summary>
        /// System Id
        /// </summary>
        private ushort _systemId;

        /// <summary>
        /// Sub System Id
        /// </summary>
        private ushort _subSystemId;

        /// <summary>
        /// Vendor Id
        /// </summary>       
        [IpmiMessageData(0)]
        public ushort VendorId
        {
            get { return this._vendorId; }
            set { this._vendorId = value; }
        }

        /// <summary>
        /// Device Id
        /// </summary>       
        [IpmiMessageData(2)]
        public ushort DeviceId
        {
            get { return this._deviceId; }
            set { this._deviceId = value; }
        }

        /// <summary>
        /// Device Id
        /// </summary>       
        [IpmiMessageData(4)]
        public ushort SystemId
        {
            get { return this._systemId; }
            set { this._systemId = value; }
        }

        /// <summary>
        /// Memory Size
        /// </summary>       
        [IpmiMessageData(6)]
        public ushort SubSystemId
        {
            get { return this._subSystemId; }
            set { this._subSystemId = value; }
        }

    }
}
