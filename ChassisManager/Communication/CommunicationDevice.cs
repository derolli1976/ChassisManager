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
using System.Collections.Generic;
using System.IO.Ports;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.GFS.WCS.ChassisManager
{
    /// <summary>
    /// Contains information on the request received from high-level SW components
    /// Contains an event variable to synchronize the user-level thread and 
    /// the device-level thread
    /// </summary>
    class WorkItem : IDisposable
    {
        // Information on the request
        private byte[] request;
        private byte[] response;
        internal byte deviceType { get; set; }
        internal byte deviceId { get; set; }

        // sessionId associated with the request
        internal ushort sessionId { get; set; }

        // Event variable to synchronize the user-level thread and 
        // the device-level thread
        private AutoResetEvent autoEvent;

        internal WorkItem(byte devType, byte devId, byte[] srcRequest, ushort sId)
        {
            deviceType = devType;
            deviceId = devId;

            if (srcRequest != null)
            {
                request = new byte[srcRequest.Length];
                Buffer.BlockCopy(srcRequest, 0, request, 0, srcRequest.Length);
            }

            response = null;
            sessionId = sId;
            // Create an event variable
            autoEvent = new AutoResetEvent(false);
        }

        ~WorkItem()
        {
            Dispose(false);
        }

        /// <summary>
        /// User-level thread must call this method to wait until signaled
        /// by the device-level thread
        /// </summary>
        internal void Wait()
        {
            autoEvent.WaitOne();
        }

        /// <summary>
        /// Device-level thread must call this method to signal
        /// the waiting user-level thread
        /// </summary>
        internal void Signal()
        {
            autoEvent.Set();
        }

        /// <summary>
        /// Allocate memory for the response buffer and copy the contents
        /// from the source
        /// </summary>
        /// <param name="srcResponse"></param>
        internal void SetResponse(byte[] srcResponse)
        {
            if (srcResponse != null)
            {
                response = new byte[srcResponse.Length];
                Buffer.BlockCopy(srcResponse, 0, response, 0, srcResponse.Length);
            }
        }

        /// <summary>
        /// Allocate the memory for the destination buffer and copy the response
        /// to the destination buffer
        /// </summary>
        /// <param name="dstResponse"></param>
        internal void GetResponse(out byte[] dstResponse)
        {
            if (response != null)
            {
                dstResponse = new byte[response.Length];
                Buffer.BlockCopy(response, 0, dstResponse, 0, response.Length);
            }
            else
            {
                ResponsePacketUtil.GenerateResponsePacket(CompletionCode.ResponseNotProvided, out dstResponse);
            }
        }

        internal byte[] GetRequest()
        {
            return request;
        }

        /// <summary>
        /// Static code analyzer suggests to implment this class
        /// as an inherited class of IDisposable and to implement Dispose method 
        /// because of the use of AutoResetEvent.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (autoEvent != null)
                {
                    autoEvent.Dispose();
                    autoEvent = null;
                }
            }
        }
    }

    /// <summary>
    /// Abstracts physical hardware devices from higher-level software components
    /// All the transactions should be performed only by calling the SendReceive method
    /// </summary>
    static internal class CommunicationDevice
    {
        private enum LogicalPortId
        {
            SerialPortOtherDevices = 0,
            SerialPortServers = 1,
            SerialPortConsole1 = 2,
            SerialPortConsole2 = 3,
            SerialPortConsole3 = 4,
            SerialPortConsole4 = 5,
            InvalidLogicalPortId = 0xFF,
        }

        /// <summary>
        /// logicalTimeStamp: monotonically increases whenever SendReceive method is invoked
        /// </summary>
        static long logicalTimeStamp;

        /// <summary>
        /// Each physical communication port in the system is managed 
        /// by an instance of PortManager class
        /// logical port id is to access an instance of this class
        /// </summary>
        static PortManager[] portManagers;

        static internal int numPorts = 6;
        static internal int numPriorityLevels = Enum.GetValues(typeof(PriorityLevel)).Length;
        static internal int maxNumServersPerChassis = 48;

        /// <summary>
        /// This flag is set if CM is terminating
        /// </summary>
        volatile static private bool isTerminating = false;

        /// <summary>
        /// This flag is set if CM is running in the safe mode
        /// </summary>
        volatile static private bool isSafeModeEnabled = false;

        /// <summary>
        /// Physical server layout table
        /// </summary>
        static private byte[] physicalServerIdTable = {
                                             1, 2, 3, 4, 
                                             5, 6, 7, 8, 
                                             9, 10, 11, 12,
                                             13, 14, 15, 16, 
                                             17, 18, 19, 20, 
                                             21, 22, 23, 24,
                                             25, 26, 27, 28, 
                                             29, 30, 31, 32, 
                                             33, 34, 35, 36,
                                             37, 38, 39, 40, 
                                             41, 42, 43, 44, 
                                             45, 46, 47, 48
                                         };

        /// <summary>
        /// Logical server layout table
        /// </summary>
        static private byte[] logicalServerIdTable = {
                                            1, 25, 13, 37,
                                            2, 26, 14, 38,
                                            3, 27, 15, 39,
                                            4, 28, 16, 40,
                                            5, 29, 17, 41,
                                            6, 30, 18, 42,
                                            7, 31, 19, 43,
                                            8, 32, 20, 44,
                                            9, 33, 21, 45,
                                            10, 34, 22, 46,
                                            11, 35, 23, 47,
                                            12, 36, 24, 48
                                        };

        /// <summary>
        /// This method should be called prior to using this class
        /// </summary>
        static internal CompletionCode Init()
        {
            CompletionCode completionCode = CompletionCode.CommDevFailedToInit;

            // Clear this flag in case that CM retries to initialize
            isTerminating = false;

            // Safe mode is disabled by default when initialized
            isSafeModeEnabled = false;

            // Enforce that the update to the flag variable is visible to other threads orderly
            Thread.MemoryBarrier();

            portManagers = new PortManager[numPorts];

            for (int i = 0; i < numPorts; i++)
            {
                portManagers[i] = new SerialPortManager(i, numPriorityLevels);
                completionCode = portManagers[i].Init();
                if (CompletionCodeChecker.Failed(completionCode) == true)
                {
                    Tracer.WriteError("Failed to initialize portManager: {0} Logical Id: {1}", i,
                        PortManager.GetPhysicalPortNameFromLogicalId(i));
                    Release();
                    return completionCode;
                }
            }

            return completionCode;
        }

        /// <summary>
        /// Release all the resource.
        /// This method must be called to gracefully terminate CM
        /// </summary>
        static internal void Release()
        {
            // Set the flag to true to ensure no more incoming requests
            // can be accepted
            isTerminating = true;

            // Enforce that the update to the flag variable is visible to other threads orderly
            Thread.MemoryBarrier();

            for (int i = 0; i < numPorts; i++)
            {
                if (portManagers[i] != null)
                {
                    portManagers[i].Release();
                }
            }
        }

        static private ushort IncrementTimeStampAndGetSessionId()
        {
            long ts = Interlocked.Increment(ref logicalTimeStamp);
            return (ushort)(ts & 0xFFFF);
        }

        /// <summary>
        /// Every component in the higher level of the stack should call this method
        /// to send/receive data to/from actual hardware devices
        /// </summary>
        /// <param name="priorityLevel"></param>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request">
        /// Input
        /// [0]: function code
        /// [1:2]: byte count (N)
        /// [3:N+2]: payload
        /// </param>
        /// <param name="response">
        /// Output
        /// [0]: completion code
        /// [1:2]: byte count (N)
        /// [3:N+2]: payload
        /// Note: response can be null
        /// </param>
        static internal void SendReceive(PriorityLevel priorityLevel, byte deviceType, byte deviceId, byte[] request, out byte[] response)
        {
            Tracer.WriteInfo("CommunicationDevice.SendReceive({0})", deviceType);
            ushort sessionId = IncrementTimeStampAndGetSessionId();
            byte physicalId;

            // If CM is terminating, do not accept any more new requests
            if (isTerminating == true)
            {
                ResponsePacketUtil.GenerateResponsePacket(CompletionCode.ServiceTerminating, out response);
                return;
            }

            if (IsValidRequest(deviceType, deviceId, ref request) == false)
            {
                ResponsePacketUtil.GenerateResponsePacket(CompletionCode.InvalidCommand, out response);
                return;
            }

            physicalId = GetPhysicalIdFromLogicalId(deviceType, deviceId);
            Tracer.WriteInfo("CommunicationDevice.SendReceive PhysicalID ({0})", physicalId);

            using (WorkItem workItem = new WorkItem(deviceType, physicalId, request, sessionId))
            {
                byte logicalPortId = (byte)LogicalPortId.InvalidLogicalPortId;
                byte functionCode = RequestPacketUtil.GetFunctionCode(ref request);

                switch (deviceType)
                {
                    case (byte)DeviceType.Server:
                        // Fall through
                    case (byte)DeviceType.BladeConsole:
                        logicalPortId = (byte)LogicalPortId.SerialPortServers;
                        break;
                    case (byte)DeviceType.SerialPortConsole:
                        // TODO: Extend the code for more serial consoles
                        if (deviceId == 1)
                        {
                            logicalPortId = (byte)LogicalPortId.SerialPortConsole1;
                        }
                        else if (deviceId == 2)
                        {
                            logicalPortId = (byte)LogicalPortId.SerialPortConsole2;
                        }
                        else if (deviceId == 3)
                        {
                            logicalPortId = (byte)LogicalPortId.SerialPortConsole3;
                        }
                        else if (deviceId == 4)
                        {
                            logicalPortId = (byte)LogicalPortId.SerialPortConsole4;
                        }
                        break;
                    default:
                        logicalPortId = (byte)LogicalPortId.SerialPortOtherDevices;
                        break;
                }

                if (logicalPortId == (byte)LogicalPortId.InvalidLogicalPortId)
                {
                    ResponsePacketUtil.GenerateResponsePacket(CompletionCode.InvalidCommand, out response);
                    return;
                }

                // If currently in safe mode, reject all the requests routed to COM4 except for BladeConsole commands
                if ((isSafeModeEnabled == true) &&
                    (logicalPortId == (byte)LogicalPortId.SerialPortServers) &&
                    (deviceType != (byte)DeviceType.BladeConsole))
                {
                    ResponsePacketUtil.GenerateResponsePacket(CompletionCode.CannotExecuteRequestInSafeMode, out response);
                    return;
                }
                
                if (portManagers[logicalPortId].SendReceive(priorityLevel, workItem) == true)
                {
                    // Successfully added the request in the work queue
                    Tracer.WriteInfo("[logicalPortId: {0}, priorityLevel: {1}] SendReceive succeeded and wait", logicalPortId, priorityLevel);

                    // Sleep until signaled by the device worker thread
                    // Wait time: wait time in the queue + worker thread processing time
                    workItem.Wait();

                    // Copy the response to the output buffer
                    workItem.GetResponse(out response);
                }
                else
                {
                    // Could not add the reuqest in the work queue
                    Tracer.WriteWarning("[logicalPortId: {0}, priorityLevel: {1}] SendReceive failed", logicalPortId, priorityLevel);
                    ResponsePacketUtil.GenerateResponsePacket(CompletionCode.OutOfSpace, out response);
                }
            }
        }

        /// <summary>
        /// Check if the request is valid with respect to device type, id, and request length
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        static private bool IsValidRequest(byte deviceType, byte deviceId, ref byte[] request)
        {
            if (DeviceTypeChecker.IsValidDeviceType(deviceType) == false)
            {
                Tracer.WriteError("Invalid device type: {0}", deviceType);
                return false;
            }
            if (DeviceIdChecker.IsValidLogicalDeviceId(deviceType, deviceId) == false)
            {
                Tracer.WriteError("Invalid device ID: {0}", deviceId);
                return false;
            }
            if (request == null)
            {
                Tracer.WriteError("Null request packet");
                return false;
            }

            // For server commands, simply pass through.
            // Thus, no need to check the request packet length as long as it is not null
            if (deviceType != (byte)DeviceType.Server)
            {
                if (RequestPacketUtil.IsValidRequestLength(ref request) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get a logical ID from a physical ID
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="physicalDeviceId"></param>
        /// <returns></returns>
        static internal byte GetLogicalIdFromPhysicalId(byte deviceType, byte physicalDeviceId)
        {
            byte logicalDeviceId = physicalDeviceId;
            // For non-server device types, physical and logical IDs are same
            if (DeviceTypeChecker.IsServerOrPowerDeviceType(deviceType) == true)
            {
                int index = Array.IndexOf(physicalServerIdTable, physicalDeviceId);
                logicalDeviceId = logicalServerIdTable[index];
            }
            return logicalDeviceId;
        }

        /// <summary>
        /// Get a physical ID from a logical ID
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="physicalDeviceId"></param>
        /// <returns></returns>
        static internal byte GetPhysicalIdFromLogicalId(byte deviceType, byte logicalDeviceId)
        {
            byte physicalDeviceId = logicalDeviceId;
            // For non-server device types, physical and logical IDs are same
            if (DeviceTypeChecker.IsServerOrPowerDeviceType(deviceType) == true)
            {
                int index = Array.IndexOf(logicalServerIdTable, logicalDeviceId);
                physicalDeviceId = physicalServerIdTable[index];
            }
            return physicalDeviceId;
        }

        /// <summary>
        /// Check if in safe mode
        /// </summary>
        static internal bool IsSafeMode()
        {
            int logicalPortIdForServers = (byte)LogicalPortId.SerialPortServers;
            if ((portManagers == null) ||
                (portManagers[logicalPortIdForServers] == null))
            {
                Tracer.WriteError("[CommunicationDevice] Failed to enable safe mode");
                return false;
            }
            bool status = portManagers[logicalPortIdForServers].IsSafeMode();
            Tracer.WriteInfo("[CommunicationDevice] Safe mode status {0}", status);
            return status;

        }

        /// <summary>
        /// Disable safe mode
        /// </summary>
        static internal bool DisableSafeMode()
        {
            int logicalPortIdForServers = (byte)LogicalPortId.SerialPortServers;
            if ((portManagers == null) ||
                (portManagers[logicalPortIdForServers] == null))
            {
                Tracer.WriteError("[CommunicationDevice] Failed to disable safe mode");
                return false;
            }
            portManagers[logicalPortIdForServers].DisableSafeMode();
            isSafeModeEnabled = false;
            Tracer.WriteInfo("[CommunicationDevice] Safe mode has been disabled");
            return true;
        }

        /// <summary>
        /// Enable safe mode
        /// </summary>
        static internal bool EnableSafeMode()
        {
            int logicalPortIdForServers = (byte)LogicalPortId.SerialPortServers;
            if ((portManagers == null) ||
                (portManagers[logicalPortIdForServers] == null))
            {
                Tracer.WriteError("[CommunicationDevice] Failed to enable safe mode");
                return false;
            }
            portManagers[logicalPortIdForServers].EnableSafeMode();
            isSafeModeEnabled = true;
            Tracer.WriteInfo("[CommunicationDevice] Safe mode has been enabled");
            return true;
        }
    }
}
