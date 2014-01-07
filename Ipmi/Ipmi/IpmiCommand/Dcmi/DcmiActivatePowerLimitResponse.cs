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
    /// Represents the DCMI 'Activate/Deactivate Power Limit' response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Dcgrp, IpmiCommand.DcmiActivateLimit)]
    internal class DcmiActivatePowerLimitResponse : IpmiResponse
    {
        /// <summary>
        /// Group Extension (0xDC).
        /// </summary>
        private byte groupExtension;

        /// <summary>
        /// Group Extension
        /// </summary>
        /// <value>0xDC</value>
        [IpmiMessageData(0)]
        public byte GroupExtension
        {
            get { return this.groupExtension; }
            set { this.groupExtension = value; }
        }
    }
}
