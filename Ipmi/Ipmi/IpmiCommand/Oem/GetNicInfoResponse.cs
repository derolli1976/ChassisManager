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
    /// Represents the IPMI 'Get Nic Info Command' request message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.OemGroup, IpmiCommand.GetNicInfo)]
    class GetNicInfoResponse : IpmiResponse
    {
        /// <summary>
        /// MAC address
        /// </summary>
        private byte[] _hwAddress;

        /// <summary>
        /// MAC Address
        /// </summary>       
        [IpmiMessageData(0)]
        public byte[] HardwareAddress
        {
            get { return this._hwAddress; }
            set { this._hwAddress = value; }
        }
    }
}
