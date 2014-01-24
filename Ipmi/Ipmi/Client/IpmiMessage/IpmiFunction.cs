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
    using System;

    /// <summary>
    /// Defines IPMI network function codes.
    /// </summary>
    [FlagsAttribute]
    internal enum IpmiFunctions
    {
        /// <summary>
        /// Chassis.
        /// </summary>
        Chassis = 0,

        /// <summary>
        /// Bridge.
        /// </summary>
        Bridge = 2,

        /// <summary>
        /// Sensor.
        /// </summary>
        Sensor = 4,

        /// <summary>
        /// Application.
        /// </summary>
        Application = 6,

        /// <summary>
        /// Firmware.
        /// </summary>
        Firmware = 8,

        /// <summary>
        /// Storage.
        /// </summary>
        Storage = 10,

        /// <summary>
        /// Transport.
        /// </summary>
        Transport = 12,

        /// <summary>
        /// DCMI.
        /// </summary>
        Dcgrp = 44,

        /// <summary>
        /// OEM
        /// </summary>
        Oem = 46,

        /// <summary>
        /// Oem/Group Vendor Specific
        /// </summary>
        OemGroup = 48,

        /// <summary>
        /// Oem Custom Group
        /// </summary>
        OemCustomGroup = 50,

        /// <summary>
        /// RMCP+ Session Setup.
        /// </summary>
        SessionSetup = 4096,
    }

    /// <summary>
    /// Defines IPMI message format based on transport type.
    /// </summary>
    internal enum IpmiTransport
    { 
        /// <summary>
        /// IPMI over Serial
        /// </summary>
        Serial = 0x00,

        /// <summary>
        /// IPMI over LAN
        /// </summary>
        Lan = 0x01,

        /// <summary>
        /// IPMI over KCS
        /// </summary>
        Wmi = 0x02,
    }
}
