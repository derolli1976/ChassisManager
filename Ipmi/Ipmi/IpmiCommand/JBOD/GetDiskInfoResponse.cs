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
    /// Represents the IPMI 'Get Disk Info Command for WCS JBOD' OEM request message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Oem, IpmiCommand.GetDiskInfo)]
    class GetDiskInfoResponse : IpmiResponse
    {
        /// <summary>
        /// Ipmi Unit of Measurement
        /// </summary>
        private byte _unit;

        /// <summary>
        /// Multiplier byte
        /// </summary>
        private byte _multiplier;

        /// <summary>
        /// Reading Byte Array
        /// </summary>
        private byte[] _reading;

        /// <summary>
        /// Reading Unit
        /// </summary>       
        [IpmiMessageData(0)]
        public byte Unit
        {
            get { return this._unit; }
            set { this._unit = value; }
        }

        /// <summary>
        /// Disk Reading Multiplier:
        /// [7] 1b = negative multiplier 
        ///     0b = positive multiplier 
        ///[6-0] Reading MS byte multiplier 
        /// </summary>       
        [IpmiMessageData(1)]
        public byte Multiplier
        {
            get { return this._multiplier; }
            set { this._multiplier = value; }
        }

        /// <summary>
        /// Disk/JBOD Reading:
        ///     UInt16 numeric value.
        /// </summary>       
        [IpmiMessageData(2)]
        public byte[] Reading
        {
            get { return this._reading; }
            set { this._reading = value; }
        }
    }
}
