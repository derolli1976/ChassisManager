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
    /// Represents the IPMI 'Get Sensor Type' application response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Sensor, IpmiCommand.SensorType)]
    internal  class SensorTypeResponse : IpmiResponse
    {
        /// <summary>
        /// Sensor Reading.
        /// </summary>
        private byte sensorType;
        
        /// <summary>
        /// Sensor Event Reading Code.
        /// </summary>
        private byte typeCode;

        /// <summary>
        /// Gets and sets the Sensor Type.
        /// </summary>
        /// <value>Sensor Type.</value>
        [IpmiMessageData(0)]
        public byte SensorType
        {
            get { return this.sensorType; }
            set { this.sensorType = value; }
        }

        /// <summary>
        /// Gets and sets the Sensor Type Code.
        /// </summary>
        /// <value>Sensor Type Code.</value>
        [IpmiMessageData(1)]
        public byte EventTypeCode
        {
            get { return (byte)(this.typeCode & 0x7F); }
            set { this.typeCode = value; }
        }
    }
}
