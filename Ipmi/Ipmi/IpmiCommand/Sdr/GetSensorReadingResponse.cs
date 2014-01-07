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
    /// Represents the IPMI 'Get Sensor Reading' application response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Sensor, IpmiCommand.SensorReading)]
    internal  class SensorReadingResponse : IpmiResponse
    {
        /// <summary>
        /// Sensor Reading.
        /// </summary>
        private byte sensorReading;
        
        /// <summary>
        /// Sensor Status.
        /// </summary>
        private byte sensorStatus;

        /// <summary>
        /// Threshold / Discrete Offset.
        /// </summary>
        private byte stateOffset;

        /// <summary>
        /// Optional Discrete Offset.
        /// </summary>
        private byte optionalOffset;

        /// <summary>
        /// Gets and sets the Sensor Reading.
        /// </summary>
        /// <value>Sensor Reading.</value>
        [IpmiMessageData(0)]
        public byte SensorReading
        {
            get { return this.sensorReading; }
            set { this.sensorReading = value; }
        }

        /// <summary>
        /// Gets and sets the Sensor Status.
        /// </summary>
        /// <value>Sensor Status.</value>
        [IpmiMessageData(1)]
        public byte SensorStatus
        {
            get { return this.sensorStatus; }
            set { this.sensorStatus = value; }
        }

        /// <summary>
        /// Gets and sets the State OffSet.
        /// (Only applies to Threshold/Discrete)
        /// </summary>
        /// <value>State Offset.</value>
        [IpmiMessageData(2)]
        public byte StateOffset
        {
            get { return this.stateOffset; }
            set { this.stateOffset = value; }
        }

        /// <summary>
        /// Gets and sets the Optional OffSet.
        /// </summary>
        /// <value>Optional Offset.</value>
        [IpmiMessageData(3)]
        public byte OptionalOffset
        {
            get { return this.optionalOffset; }
            set { this.optionalOffset = value; }
        }
    }
}
