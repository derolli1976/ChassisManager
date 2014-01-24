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
    /// Represents the IPMI 'Get Sensor Reading Factors' response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Sensor, IpmiCommand.SensorFactors, 8)]
    internal class ReadingFactorsResponse : IpmiResponse
    {
        /// <summary>
        /// Next known Reading byte.
        /// </summary> 
        private byte nextReading;

        /// <summary>
        /// Reading Factors.
        /// </summary> 
        private byte[] factors;

        /// <summary>
        /// Next known Reading byte
        /// </summary>       
        [IpmiMessageData(0)]
        public byte NextReading
        {
            get { return this.nextReading; }
            set { this.nextReading = value; }
        }

        /// <summary> 
        /// bytes [1-7] Sensor Reading factores
        /// </summary>     
        [IpmiMessageData(1)]
        public byte[] Factors
        {
            get { return this.factors; }
            set { this.factors = value; }
        }
    }
}
