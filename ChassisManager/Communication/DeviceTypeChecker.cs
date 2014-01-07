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
    internal static class DeviceTypeChecker
    {
        /// <summary>
        /// Check if the device type is valid
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        static internal bool IsValidDeviceType(byte deviceType)
        {
            if (Enum.IsDefined(typeof(DeviceType), deviceType) == false)
            {
                Tracer.WriteError("Invalid Device Type ({0}) in SendReceive", deviceType);
                return false;
            }
            else
            {
                return true;
            }
        }

        static internal bool IsServerOrPowerDeviceType(byte deviceType)
        {
            if ((deviceType == (byte)DeviceType.Power) ||
                (deviceType == (byte)DeviceType.Server))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static internal bool IsConsoleDeviceType(byte deviceType)
        {
            if ((deviceType == (byte)DeviceType.BladeConsole) ||
                (deviceType == (byte)DeviceType.SerialPortConsole))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}