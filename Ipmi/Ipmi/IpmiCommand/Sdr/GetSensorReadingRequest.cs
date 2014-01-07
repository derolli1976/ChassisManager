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
    /// Represents the IPMI 'Get Sensor Reading' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Sensor, IpmiCommand.SensorReading, 1)]
    internal class SensorReadingRequest : IpmiRequest
    {
        /// <summary>
        /// Sensor Number.
        /// </summary> 
        private byte sensornumber;

        /// <summary>
        /// Initializes a new instance of the SensorReadingRequest class.
        /// </summary>
        internal SensorReadingRequest(byte SensorNumber)
        {
            this.sensornumber = SensorNumber;
        }

        /// <summary>
        /// Sensor Number byte
        /// </summary>       
        [IpmiMessageData(0)]
        public byte SensorNumber
        {
            get { return this.sensornumber; }

        }
    }
}
