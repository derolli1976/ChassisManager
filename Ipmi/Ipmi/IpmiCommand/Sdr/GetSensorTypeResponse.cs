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
