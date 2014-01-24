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
    /// Represents the IPMI 'Get Sensor Reading Factors' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Sensor, IpmiCommand.SensorFactors, 2)]
    internal class ReadingFactorsRequest : IpmiRequest
    {
        /// <summary>
        /// Sensor Number.
        /// </summary> 
        private byte sensornumber;

        /// <summary>
        /// Reading byte.
        /// </summary> 
        private byte readingbyte;

        /// <summary>
        /// Initializes a new instance of the ReadingFactorsRequest class.
        /// </summary>
        internal ReadingFactorsRequest(byte SensorNumber, byte ReadingByte)
        {
            this.sensornumber = SensorNumber;
            this.readingbyte = ReadingByte;
        }

        /// <summary>
        /// Sensor Number byte
        /// </summary>       
        [IpmiMessageData(0)]
        public byte SensorNumber
        {
            get { return this.sensornumber; }

        }
        /// <summary>
        /// Sensor Reading byte
        /// </summary>     
        [IpmiMessageData(1)]
        public byte Readingbyte
        {
            get { return this.readingbyte; }

        }
    }
}
