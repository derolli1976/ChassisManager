/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*                                       	            *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    /// <summary>
    /// Represents the IPMI 'Clear SEL' response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Storage, IpmiCommand.SelClear)]
    internal class SelLogClearResponse : IpmiResponse
    {
        private byte erasureProgress;

        /// <summary>
        /// Gets erasureProgress
        /// </summary>       
        [IpmiMessageData(0)]
        internal byte ErasureProgress
        {
            get { return this.erasureProgress; }
            set { this.erasureProgress = value; }
        }


    }
}
