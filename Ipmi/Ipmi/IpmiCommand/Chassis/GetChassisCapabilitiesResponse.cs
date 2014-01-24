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
    /// Represents the IPMI 'Get Chassis Capabilities' chassis response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Chassis, IpmiCommand.GetChassisCapabilities)]
    internal class GetChassisCapabilitiesResponse : IpmiResponse
    {
        /// <summary>
        /// Capabilities Flags.
        /// </summary>
        private byte capabilities;

        /// <summary>
        /// Chassis FRU device address.
        /// </summary>
        private byte fruDeviceAddress;

        /// <summary>
        /// Chassis SDR device address.
        /// </summary>
        private byte sdrDeviceAddress;

        /// <summary>
        /// Chassis SEL device address.
        /// </summary>
        private byte selDeviceAddress;

        /// <summary>
        /// Chassis System Management device address.
        /// </summary>
        private byte systemManagementDeviceAddress;

        /// <summary>
        /// Chassis Bridge device address (optional).
        /// </summary>
        private byte bridgeDeviceAddress = 0x20;

        /// <summary>
        /// Gets and sets the Capabilities Flags.
        /// </summary>
        /// <value>Capabilities Flags.</value>
        [IpmiMessageData(0)]
        public byte Capabilities
        {
            get { return this.capabilities; }
            set { this.capabilities = value; }
        }

        /// <summary>
        /// Gets and sets the Chassis FRU device address.
        /// </summary>
        /// <value>Chassis FRU device address.</value>
        [IpmiMessageData(1)]
        public byte FruDeviceAddress
        {
            get { return this.fruDeviceAddress; }
            set { this.fruDeviceAddress = value; }
        }

        /// <summary>
        /// Gets and sets the Chassis SDR device address.
        /// </summary>
        /// <value>Chassis SDR device address.</value>
        [IpmiMessageData(2)]
        public byte SdrDeviceAddress
        {
            get { return sdrDeviceAddress; }
            set { this.sdrDeviceAddress = value; }
        }

        /// <summary>
        /// Gets and sets the Capabilities Flags.
        /// </summary>
        /// <value>Capabilities Flags.</value>
        [IpmiMessageData(3)]
        public byte SelDeviceAddress
        {
            get { return this.selDeviceAddress; }
            set { this.selDeviceAddress = value; }
        }

        /// <summary>
        /// Gets and sets the Chassis System Management device address.
        /// </summary>
        /// <value>Chassis System Management device address.</value>
        [IpmiMessageData(4)]
        public byte SystemManagementDeviceAddress
        {
            get { return this.systemManagementDeviceAddress; }
            set { this.systemManagementDeviceAddress = value; }
        }

        /// <summary>
        /// Gets and sets the Chassis Bridge device address (optional).
        /// </summary>
        /// <value>Chassis Bridge device address (optional).  Defaults to 0x20.</value>
        [IpmiMessageData(5)]
        public byte BridgeDeviceAddress
        {
            get { return this.bridgeDeviceAddress; }
            set { this.bridgeDeviceAddress = value; }
        }

        /// <summary>
        /// Indicates support for power interlock.
        /// </summary>
        /// <value>True if supported; else false.</value>
        internal bool SupportsPowerInterlock
        {
            get { return (this.Capabilities & 0x08) == 0x08; }
        }

        /// <summary>
        /// Indicates support for diagnostic interrupt (FP NMI).
        /// </summary>
        /// <value>True if supported; else false.</value>
        internal bool SupportsDiagnosticInterrupt
        {
            get { return (this.Capabilities & 0x04) == 0x04; }
        }

        /// <summary>
        /// Indicates support for front panel lockout.
        /// </summary>
        /// <value>True if supported; else false.</value>
        internal bool SupportsFrontPanelLockout
        {
            get { return (this.Capabilities & 0x02) == 0x02; }
        }

        /// <summary>
        /// Indicates support for a physical intrusion sensor.
        /// </summary>
        /// <value>True if supported; else false.</value>
        internal bool SupportsIntrusionSensor
        {
            get { return (this.Capabilities & 0x01) == 0x01; }
        }
    }
}
