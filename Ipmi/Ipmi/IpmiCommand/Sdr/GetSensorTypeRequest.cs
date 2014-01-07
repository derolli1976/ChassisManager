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
    /// Represents the IPMI 'Get Sensor Type' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Sensor, IpmiCommand.SensorType, 1)]
    internal class SensorTypeRequest : IpmiRequest
    {
        /// <summary>
        /// Sensor Number.
        /// </summary> 
        private byte sensornumber;

        /// <summary>
        /// Initializes a new instance of the SensorTypeRequest class.
        /// </summary>
        internal SensorTypeRequest(byte SensorNumber)
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
