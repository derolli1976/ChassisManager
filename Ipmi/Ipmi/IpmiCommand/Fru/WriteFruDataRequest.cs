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
    /// Represents the IPMI 'Get FRU Inventory Info' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.WriteFruData)]
    internal class WriteFruDataRequest : IpmiRequest
    {
        private byte devId = 0x00;

        private const byte readFruData = 0x11;

        private byte offSetLS;

        private byte offSetMS;

        private byte[] payload;
        
        internal WriteFruDataRequest(byte offSetLS, byte offSetMS, byte[] payload)
        {
            this.offSetLS = offSetLS;
            this.offSetMS = offSetMS;
            this.payload = payload;
        
        }

        /// <summary>
        /// Gets and sets Device Id.
        /// </summary>       
        [IpmiMessageData(0)]
        public byte DeviceId
        {
            get { return this.devId; }
            set { this.devId = value; }
        }

        /// <summary>
        /// Offset to read, LS byte.
        /// </summary>       
        [IpmiMessageData(1)]
        public byte OffSetLS
        {
            get { return this.offSetLS; }
            set { this.offSetLS = value; }
        }

        /// <summary>
        /// Offset to read, MS byte.
        /// </summary>       
        [IpmiMessageData(2)]
        public byte OffSetMS
        {
            get { return this.offSetMS; }
            set { this.offSetMS = value; }
        }


        /// <summary>
        /// Count to read, 1 based
        /// </summary>       
        [IpmiMessageData(3)]
        public byte[] Payload
        {
            get { return this.payload; }
            set { this.payload = value; }
        }
    }
}
