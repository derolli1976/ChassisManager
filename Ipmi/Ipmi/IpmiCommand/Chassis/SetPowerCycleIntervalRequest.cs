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
