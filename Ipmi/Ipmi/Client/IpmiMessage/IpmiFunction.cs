/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*                                       	            *
********************************************************/

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
