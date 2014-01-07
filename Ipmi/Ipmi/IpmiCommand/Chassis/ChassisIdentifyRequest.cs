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
    /// Represents the IPMI 'Chassis Identify' chassis request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Chassis, IpmiCommand.ChassisIdentify, 2)]
    internal class ChassisIdentifyRequest : IpmiRequest
    {
        /// <summary>
        /// Identify interval in seconds.
        /// </summary>
        private readonly byte interval;

        /// <summary>
        /// Initializes a new instance of the ChassisIdentifyRequest class.
        /// </summary>
        /// <param name="interval">Identify interval in seconds.</param>
        internal ChassisIdentifyRequest(byte interval)
        {
            this.interval = interval;
        }

        /// <summary>
        /// Gets the interval in seconds.
        /// </summary>
        [IpmiMessageData(0)]
        public byte Interval
        {
            get { return this.interval; }
        }

        /// <summary>
        /// Gets the interval in seconds.
        /// </summary>
        [IpmiMessageData(1)]
        public byte ForceOn
        {
            get { return (this.interval == 0xFF) ? (byte)0x01 : (byte)0x00; }
        }
    }
}