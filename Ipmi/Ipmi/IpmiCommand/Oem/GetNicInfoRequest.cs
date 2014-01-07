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
    /// Represents the IPMI 'Get Nic Info Request' OEM request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.OemGroup, IpmiCommand.GetNicInfo)]
    internal class GetNicInfoRequest : IpmiRequest
    {

        /// <summary>
        /// Nic zero based device index.  Default = 0x00
        /// </summary>
        private readonly byte _nic;

        /// <summary>
        /// Get Nic Info Request.  Index 0 based.
        /// </summary>
        internal GetNicInfoRequest(byte nic)
        { this._nic = nic; }


        /// <summary>
        /// Nic Number (zero based)
        /// </summary>       
        [IpmiMessageData(0)]
        public byte NIC
        {
            get { return this._nic; }

        }
    }
}
