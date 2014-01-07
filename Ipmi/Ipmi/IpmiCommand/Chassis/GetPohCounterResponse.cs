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
    /// Represents the IPMI 'Get Chassis Status' response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Chassis, IpmiCommand.GetPOHCounter)]
    internal class GetPohCounterResponse : IpmiResponse
    {
        /// <summary>
        /// Minutes per count.
        /// </summary>
        private byte minutesCount;

        /// <summary>
        /// Counter Reading.
        /// </summary>
        private byte[] counter;

        /// <summary>
        /// Minutes per count.
        /// </summary>
        /// <value>Minutes Per Count.</value>
        [IpmiMessageData(0)]
        public byte MinutesCount
        {
            get { return this.minutesCount; }
            set { this.minutesCount = value; }
        }

        /// <summary>
        /// Counter Reading.
        /// </summary>
        /// <value>Counter Reading.</value>
        [IpmiMessageData(1)]
        public byte[] Counter
        {
            get { return this.counter; }
            set { this.counter = value; }
        }
    }
}
