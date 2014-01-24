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
