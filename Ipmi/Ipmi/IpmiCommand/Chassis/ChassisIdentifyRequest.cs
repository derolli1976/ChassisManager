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
    /// Represents the IPMI 'Chassis Identify' chassis request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Chassis, IpmiCommand.ChassisIdentify, 2)]
    internal class ChassisIdentifyRequest : IpmiRequest
    {
        /// <summary>
        /// Identify interval in seconds.
        /// </summary>
        private readonly byte interval;

        /// <summary>
        /// Initializes a new instance of the ChassisIdentifyRequest class.
        /// </summary>
        /// <param name="interval">Identify interval in seconds.</param>
        internal ChassisIdentifyRequest(byte interval)
        {
            this.interval = interval;
        }

        /// <summary>
        /// Gets the interval in seconds.
        /// </summary>
        [IpmiMessageData(0)]
        public byte Interval
        {
            get { return this.interval; }
        }

        /// <summary>
        /// Gets the interval in seconds.
        /// </summary>
        [IpmiMessageData(1)]
        public byte ForceOn
        {
            get { return (this.interval == 0xFF) ? (byte)0x01 : (byte)0x00; }
        }
    }
}
