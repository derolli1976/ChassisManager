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
    [IpmiMessageRequest(IpmiFunctions.Oem, IpmiCommand.GetDiskInfo)]
    internal class GetDiskInfoRequest : IpmiRequest
    {

        /// <summary>
        /// JBOD Expander Channel.  Default = 0x00
        /// </summary>
        private readonly byte _channel = 0x00;

        /// <summary>
        /// JBOD Disk Number.  Default = 0x00,
        /// which indicates individual disks are 
        /// not supported, JBOD information is
        /// returned instead.
        /// </summary>
        private readonly byte _disk = 0x00;

        /// <summary>
        /// Get Disk Info Request
        /// </summary>
        internal GetDiskInfoRequest()
        { }

        /// <summary>
        /// Initialize Get Disk Info Request
        /// </summary>
        /// <param name="channel">JBOD Channel Number</param>
        /// <param name="disk">Disk Number</param>
        internal GetDiskInfoRequest(byte channel, byte disk)
        {
            this._channel = channel;
            this._disk = disk;
        }

        /// <summary>
        /// Channel Byte
        /// </summary>       
        [IpmiMessageData(0)]
        public byte Channel
        {
            get { return this._channel; }

        }

        /// <summary>
        /// Disk Byte
        /// </summary>       
        [IpmiMessageData(1)]
        public byte Disk
        {
            get { return this._disk; }

        }

    }
}
