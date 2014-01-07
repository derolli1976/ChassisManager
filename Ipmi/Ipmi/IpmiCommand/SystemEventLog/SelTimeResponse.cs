/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*                                       	            *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    using System;
    /// <summary>
    /// Represents the IPMI 'Get SEL Time' response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Storage, IpmiCommand.GetSelTime)]
    internal class SelTimeResponse : IpmiResponse
    {
        private byte[] offset;

        /// <summary>
        /// Sel Time
        /// </summary>       
        [IpmiMessageData(0)]
        public byte[] SecondsOffset
        {
            get { return this.offset; }
            set { this.offset = value; }
        }

        /// <summary>
        /// Returns fromatted DateTime of System Event Log
        /// </summary>
        internal DateTime Time
        {
            get { return IpmiSharedFunc.SecondsOffSet(BitConverter.ToInt32(offset, 0)); }
        }

    }
}
