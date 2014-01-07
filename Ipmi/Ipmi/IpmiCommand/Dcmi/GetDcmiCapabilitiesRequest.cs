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
    /// Represents the DCMI 'Get DCMI Capabilities' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Dcgrp, IpmiCommand.DcmiCapability)]
    internal class GetDcmiCapabilitiesRequest : IpmiRequest
    {
        /// <summary>
        /// Group Extension byte.  Always 0xDC
        /// </summary> 
        private byte groupextension = 0xDC;

        /// <summary>
        /// Parameter Selector byte.
        /// </summary> 
        private byte parameterselector;

        /// <summary>
        /// Initializes a new instance of the GetDcmiCapabilitiesRequest class.
        /// </summary>
        internal GetDcmiCapabilitiesRequest(byte Selector)
        {
            this.parameterselector = Selector;
        }

        /// <summary>
        /// Group Extension byte
        /// </summary>       
        [IpmiMessageData(0)]
        public byte GroupExtension
        {
            get { return this.groupextension; }

        }

        /// <summary>
        /// Sets reservation MS byte
        /// </summary>       
        [IpmiMessageData(1)]
        public byte Selector
        {
            get { return this.parameterselector; }

        }
    }
}
