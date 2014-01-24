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
    /// Represents the IPMI 'Get Device Id' application response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Application, IpmiCommand.GetDeviceId)]
    internal class GetDeviceIdResponse : IpmiResponse
    {

        /// <summary>
        /// device Id
        /// </summary>
        private byte deviceId;

        /// <summary>
        /// device revision number
        /// </summary>
        private byte deviceRevision;

        /// <summary>
        /// major firmware number
        /// </summary>
        private byte majorFirmware;

        /// <summary>
        /// minor firmware number
        /// </summary>
        private byte minorFirmware;

        /// <summary>
        /// Ipmi Version
        /// </summary>
        private byte ipmiVersion;

        /// <summary>
        /// additional device support
        /// </summary>
        private byte deviceSupport;

        /// <summary>
        /// Manufacture Id
        /// </summary>
        private byte[] manufactureId;

        /// <summary>
        /// Product Id
        /// </summary>
        private byte[] productId;
       
        /// <summary>
        /// BMC Device Id.
        /// </summary>       
        [IpmiMessageData(0)]
        public byte DeviceId
        {
            get { return this.deviceId; }
            set { this.deviceId = value; }  
        }

        /// <summary>
        /// Hardware revision number.
        /// </summary>       
        [IpmiMessageData(1)]
        public byte DeviceRevision
        {
            get { return this.deviceRevision; }
            set { this.deviceRevision = value; }
        }

        /// <summary>
        /// Major Revision. 7-bit.
        /// [7]   Device available: 0=normal operation, 1= device firmware update
        /// [6:0] Major Firmware Revision, binary encoded.
        /// </summary>       
        [IpmiMessageData(2)]
        public byte MajorFirmware
        {
            get { return this.majorFirmware; }
            set { this.majorFirmware = value; }
        }

        /// <summary>
        /// Minor Firmware Revision. BCD encoded.
        /// </summary>       
        [IpmiMessageData(3)]
        public byte MinorFirmware
        {
            get { return this.minorFirmware; }
            set { this.minorFirmware = value; }
        }

        /// <summary>
        /// Ipmi Version Device Support.
        /// </summary>       
        [IpmiMessageData(4)]
        public byte IpmiVersion
        {
            get { return this.ipmiVersion; }
            set { this.ipmiVersion = value; }
        }

        /// <summary>
        /// Additional Device Support.
        /// </summary>       
        [IpmiMessageData(5)]
        public byte DeviceSupport
        {
            get { return this.deviceSupport; }
            set { this.deviceSupport = value; }
        }

        /// <summary>
        /// IANA ManufactureId:
        /// 0000h = unspecified. FFFFh = reserved. 
        /// </summary>       
        [IpmiMessageData(6,3)]
        public byte[] ManufactureId
        {
            get { return this.manufactureId; }
            set { this.manufactureId = value; }
        }

        /// <summary>
        /// Additional Product Id:
        /// 0000h = unspecified. FFFFh = reserved. 
        /// </summary>       
        [IpmiMessageData(9, 2)]
        public byte[] ProductId
        {
            get { return this.productId; }
            set { this.productId = value; }
        }


    }
}
