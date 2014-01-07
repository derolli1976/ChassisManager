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
    /// Represents the IPMI 'Set Power Cycle Interval' chassis request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Chassis, IpmiCommand.SetPowerCycleInterval, 1)]
    internal class SetPowerCycleIntervalRequest : IpmiRequest
    {
        /// <summary>
        /// Power Cycle Interval
        /// </summary>
        private byte interval = 0;

        /// <summary>
        /// Initializes a new instance of the SetPowerCycleIntervalRequest class.
        /// </summary>
        /// <param name="operation">Operation to perform.</param>
        internal SetPowerCycleIntervalRequest(byte interval)
        {
            this.interval = interval;
        }

        /// <summary>
        /// Power Cycle Interval.
        /// </summary>
        [IpmiMessageData(0)]
        public byte Interval
        {
            get { return this.interval; }
        }
    }
}