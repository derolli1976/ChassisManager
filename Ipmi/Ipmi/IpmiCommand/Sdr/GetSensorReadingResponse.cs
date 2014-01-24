// Copyright © Microsoft Open Technologies, Inc.
// All Rights Reserved
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0 

// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR
// CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 
// See the Apache 2 License for the specific language governing permissions and limitations under the License. 

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
