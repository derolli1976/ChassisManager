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
    /// Represents the IPMI 'Get PCIe Info Request' OEM request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.OemGroup, IpmiCommand.GetPCIeInfo)]
    internal class GetPCIeInfoRequest : IpmiRequest
    {

        /// <summary>
        /// PCIe device index.  Default = 0x01
        /// </summary>
        private readonly byte _pcie;

        /// <summary>
        /// Get PCIe Info Request.  Index 1 based.
        /// </summary>
        internal GetPCIeInfoRequest(byte index)
        { this._pcie = index; }

        /// <summary>
        /// Get PCIe Info Request.  Index 1 based.
        /// </summary>
        internal GetPCIeInfoRequest(PCIeSlot slot)
        { this._pcie = (byte)(slot); }


        /// <summary>
        /// PCIe Number
        /// </summary>       
        [IpmiMessageData(0)]
        public byte PCIe
        {
            get { return this._pcie; }

        }
    }
}
