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
    using System.Collections;

    /// <summary>
    /// Represents the IPMI 'Get Memory Info Command with non-zero index' response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.OemGroup, IpmiCommand.GetMemoryInfo)]
    class GetMemoryInfoResponse : IpmiResponse
    {
        /// <summary>
        /// DIMM Type
        /// </summary>
        private byte _type;

        /// <summary>
        /// DIMM Speed
        /// </summary>
        private ushort _dimmSpeed;

        /// <summary>
        /// DIMM Size
        /// </summary>
        private ushort _dimmSize;

        /// <summary>
        /// DIMM Status
        /// </summary>
        private byte _dimmStatus;

        /// <summary>
        /// Set Memory Type
        /// </summary>       
        [IpmiMessageData(0)]
        public byte ModuleType
        {
            get { return this._type; }
            set { this._type = value; }
        }

        /// <summary>
        /// Memory Speed
        /// </summary>       
        [IpmiMessageData(1)]
        public ushort MemorySpeed
        {
            get { return this._dimmSpeed; }
            set { this._dimmSpeed = value; }
        }

        /// <summary>
        /// Memory Size
        /// </summary>       
        [IpmiMessageData(3)]
        public ushort MemorySize
        {
            get { return this._dimmSize; }
            set { this._dimmSize = value; }
        }

        /// <summary>
        /// Memory Size
        /// </summary>       
        [IpmiMessageData(5)]
        public byte Status
        {
            get { return this._dimmStatus; }
            set { this._dimmStatus = value; }
        }

        /// <summary>
        /// Memory Type
        /// </summary>
        public byte MemoryType
        {
            // type = bite[5:0] of Type Byte
            get { return (byte)(this._type & 0x3F); }
        }

        /// <summary>
        /// Actual DIMM Running Speed
        ///     0 = Not 1333Mhz
        ///     1 = 1333Mhz
        /// </summary>
        public byte RunningSpeed
        {
            // Bit[6]: 0 = Not 1333Mhz 1 = 1333Mhz
            get { return (byte)((this._type & 0x40) >> 6); }
        }

        /// <summary>
        /// Is Low Voltage DIMM
        ///     0 = Normal Voltage (1.5V)
        ///     1 = Low Voltage (1.3V)
        /// </summary>
        public byte Voltage
        {
            // Bit[7]: 0 = Normal Voltage (1.5V) 1 = Low Voltage (1.3V)
            get { return (byte)((this._type & 0x80) >> 7); }
        }

    }

    /// <summary>
    /// Represents the IPMI 'Get Memory Info Command with zero index' response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.OemGroup, IpmiCommand.GetMemoryInfo)]
    class GetMemoryIndexResponse : IpmiResponse
    {
        /// <summary>
        /// DIMM Slot Count
        /// </summary>
        private byte _slot;

        /// <summary>
        /// DIMM Presence info in bit map for DIMM1 to DIMM8
        /// </summary>
        private byte _mapOneToEight;

        /// <summary>
        /// DIMM Presence info in bit map for DIMM9 to DIMM16
        /// </summary>
        private byte _mapNineToSixteen;

        /// <summary>
        /// DIMM Slot Count
        /// </summary>       
        [IpmiMessageData(0)]
        public byte SlotCount
        {
            get { return this._slot; }
            set { this._slot = value; }
        }

        /// <summary>
        /// Memory Speed
        /// </summary>       
        [IpmiMessageData(1)]
        public byte MemoryMapOne
        {
            get { return this._mapOneToEight; }
            set { this._mapOneToEight = value; }
        }

        /// <summary>
        /// DIMM Presence info in bit map for DIMM9 to DIMM16
        /// </summary>       
        [IpmiMessageData(2)]
        public byte MemoryMapTwo
        {
            get { return this._mapNineToSixteen; }
            set { this._mapNineToSixteen = value; }
        }

        /// <summary>
        /// DIMM Presence Bit Array
        /// </summary>
        public BitArray Presence
        {
            // Combine Map 1-8 & Map 9-16 for DIMM presence map
            get { return new BitArray(new byte[2] { _mapOneToEight, _mapNineToSixteen }); }
        }
    }

}
