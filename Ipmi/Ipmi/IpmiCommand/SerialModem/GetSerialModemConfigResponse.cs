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
    /// Represents the IPMI 'Set Serial Modem Configuration' application response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Transport, IpmiCommand.GetSerialModemConfiguration)]
    internal class GetSerialModemConfigResponse : IpmiResponse
    {
        /// <summary>
        /// Paramater Version
        /// </summary>
        private byte version;

        /// <summary>
        /// Payload
        /// </summary>
        private byte[] payload;

        /// <summary>
        /// Paramater Version
        /// </summary>
        [IpmiMessageData(0)]
        public byte Version
        {
            get { return this.version; }
            set { this.version = value; }
        }

        /// <summary>
        /// Paramater Payload.
        /// </summary>
        [IpmiMessageData(1)]
        public byte[] Payload
        {
            get { return this.payload; }
            set { this.payload = value; }
        }
    }
}
