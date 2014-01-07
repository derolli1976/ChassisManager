/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   							                        *
*   							                        *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{

    /// <summary>
    /// Represents the IPMI 'Get System Boot Options' request message. See
    /// IPMI, 28.13 .
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Chassis,
        IpmiCommand.GetSystemBootOptions)]
    internal class GetSystemBootOptionsRequest : IpmiRequest
    {
        private byte parameterSelector;
        private byte setSelector;
        private byte blockSelector = 0x00; // by standard, currently always 0

        internal GetSystemBootOptionsRequest(byte parameterSelector,
            byte setSelector)
        {
            // TODO bit 7 of parameterSelector is reserved, should we check
            // that?
            this.parameterSelector = parameterSelector;
            this.setSelector = setSelector;
        }

        [IpmiMessageData(0)]
        public byte ParameterSelector
        {
            get { return this.parameterSelector; }
        }

        [IpmiMessageData(1)]
        public byte SetSelector
        {
            get { return this.setSelector; }
        }

        [IpmiMessageData(2)]
        public byte BlockSelector
        {
            get { return this.blockSelector; }
        }
    }
}
