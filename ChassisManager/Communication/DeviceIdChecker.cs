/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Author:  woongkib@Microsoft.com                     *
*                                       	            *
********************************************************/

using System;
using System.Threading;
using System.Collections;
using System.IO.Ports;
using System.Diagnostics;
using System.Collections.Generic;

namespace Microsoft.GFS.WCS.ChassisManager
{
    internal static class DeviceIdChecker
    {
        /// <summary>
        /// Validate the device ID. Device ID should start from 1 for all devices
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="logicalDeviceId"></param>
        /// <returns></returns>
        static internal bool IsValidLogicalDeviceId(byte deviceType, byte logicalDeviceId)
        {
            bool bIsValid = false;
            switch (deviceType)
            {
                case (byte)DeviceType.Fan:
                    bIsValid = (logicalDeviceId > 0 && logicalDeviceId <= ConfigLoaded.NumFans);
                    break;
                case (byte)DeviceType.Psu:
                    bIsValid = (logicalDeviceId > 0 && logicalDeviceId <= ConfigLoaded.NumPsus);
                    break;
                case (byte)DeviceType.Power:
                case (byte)DeviceType.BladeConsole:
                    // Fall throughs. Blade power switch, blade console, and servers 
                    // have the same ID range
                case (byte)DeviceType.Server:
                    bIsValid = (logicalDeviceId > 0 && logicalDeviceId <= ConfigLoaded.Population);
                    break;
                case (byte)DeviceType.PowerSwitch:
                    bIsValid = (logicalDeviceId > 0 && logicalDeviceId <= ConfigLoaded.NumPowerSwitches);
                    break;
                case (byte)DeviceType.SerialPortConsole:
                    // TODO: the number of the serial port devices should be specified 
                    // in the configuration file
                    bIsValid = (logicalDeviceId > 0 && logicalDeviceId <= ConfigLoaded.MaxSerialConsolePorts);
                    break;
                case (byte)DeviceType.WatchDogTimer:
                case (byte)DeviceType.FanCage:
                case (byte)DeviceType.StatusLed:
                case (byte)DeviceType.RearAttentionLed:
                case (byte)DeviceType.ChassisFruEeprom:
                    // The devices above do not have a device ID
                    bIsValid = true;
                    break;
                default:
                    Tracer.WriteError("Invalid logical device ID (type: {0}, id: {1}) in SendReceive", deviceType, logicalDeviceId);
                    break;
            }
            return bIsValid;
        }
    }
}