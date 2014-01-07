/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Author:  chong@Microsoft.com                        *
*   							                        *
*   							                        *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{

    /// <summary>
    /// Represents the IPMI 'BMC Debug' OEM request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.OemGroup, IpmiCommand.BmcDebug)]
    internal class BmcDebugRequest : IpmiRequest
    {

        /// <summary>
        /// Type of debug message to output
        /// </summary>
        private readonly byte _process;

        /// <summary>
        /// Enable/Disable
        /// </summary>
        private readonly byte _enable;

        /// <summary>
        /// Get Processor Info Request
        /// </summary>
        internal BmcDebugRequest(BmcDebugProcess process, bool enable)
        { 
            this._process = (byte)process;

            if (enable)
                this._enable = 0x01;
        }


        /// <summary>
        /// Process Type
        /// </summary>       
        [IpmiMessageData(0)]
        public byte Process
        {
            get { return this._process; }
        }

        /// <summary>
        /// Enable/Disable
        /// </summary>       
        [IpmiMessageData(1)]
        public byte Enable
        {
            get { return this._enable; }
        }
    }
}
