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

namespace Microsoft.GFS.WCS.ChassisManager
{
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using Microsoft.GFS.WCS.ChassisManager.Ipmi;

    public static class WcsBladeFacade
    {

        // variable access read/write locker.
        private static object _locker = new object();

        /// <summary>
        /// Number of blades populated in the chassis
        /// </summary>
        private static int _population;

        // number of blade clients initialized
        private static int _bladesInitialized = 0;

        // Array of possible valid blade addresses.  Population is a count into the valid addresses
        private static readonly byte[] _validAddr = new byte[48]{ 0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,0x0B,
                                                                  0x0C,0x0D,0x0E,0x0F,0x10,0x11,0x12,0x13,0x14,0x15,0x16,
                                                                  0x17,0x18,0x19,0x1A,0x1B,0x1C,0x1D,0x1E,0x1F,0x20,0x21,
                                                                  0x22,0x23,0x24,0x25,0x26,0x27,0x28,0x29,0x2A,0x2B,0x2C,
                                                                  0x2D,0x2E,0x2F,0x30};
        /// <summary>
        /// Dictionary containing Initialized blade client classes.  Dictionary Key = DeviceId.
        /// </summary>
        internal static ConcurrentDictionary<byte, WcsBladeClient> clients = new ConcurrentDictionary<byte, WcsBladeClient>();

        /// <summary>
        /// Number of blades initialized
        /// </summary>
        public static int Initialized
        {
            get
            {
                lock (_locker)
                {
                    return _bladesInitialized;
                }
            }
        }

        /// <summary>
        /// Class Initialize, should be 
        /// </summary>
        public static void Initialize(int population = 0)
        {
            if (population == 0)
                _population = ConfigLoaded.Population;
            else
                _population = population;

            InitializeClients();
        }

        /// <summary>
        /// Initialize all blade Clients
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static void InitializeClients()
        {
            for (int i = 0; i < _population; i++)
            {
                // create an instance of the blade Client
                WcsBladeClient blade = new WcsBladeClient(_validAddr[i]);
                blade.IpmiUserId = ConfigLoaded.BmcUserName;
                blade.IpmiPassword = ConfigLoaded.BmcUserKey;

                // add blade client to dictionary for retreival.  Keeps object in scope, so dispose does not need to be called.
                clients.TryAdd(_validAddr[i], blade);
            }

            // get the number of blades initialized
            int initialized = GetInitialized();

            lock (_locker)
            {
                // set the intialized blade count.
                _bladesInitialized = initialized;
            }
        }

        /// <summary>
        /// Closes Blade IPMI sessions on blades of type 0x04
        /// </summary>
        internal static void Release()
        {
            foreach (KeyValuePair<byte, WcsBladeClient> blade in clients)
            {
                // attempt to close the blade where initialized or not
                if (blade.Value.BladeClassification == BladeType.Server)
                    Close(blade.Key);
            }
        }

        /// <summary>
        /// Gets the number of blades that have been initialized before.
        /// </summary>
        private static int GetInitialized()
        {
            return clients.Count;
        }

        /// <summary>
        /// Command to Initialize a target client
        /// </summary>
        /// <param name="deviceId"></param>
        internal static bool InitializeClient(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].Initialize();
            }

            bool valid = false;

            for (int i = 0; i < _validAddr.Length; i++)
            {
                if(_validAddr[i] == deviceId)
                    valid = true;
            }   

            if(valid)
            {
                // create an instance of the blade Client
                WcsBladeClient blade = new WcsBladeClient(deviceId);
                blade.IpmiUserId = ConfigLoaded.BmcUserName;
                blade.IpmiPassword = ConfigLoaded.BmcUserKey;

                // add blade client to dictionary for retreival.  Keeps object in scope, so dispose does not need to be called.
                if (!clients.ContainsKey(deviceId))
                    clients.TryAdd(deviceId, blade);

                return blade.Initialize();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Command to Logon to a target client
        /// </summary>
        internal static void LogOn(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                clients[deviceId].LogOn();
            }
        }

        /// <summary>
        /// Ipmi consecutive communication error counter.
        /// valid respone:   0 - 2,147,483,647 (integer)
        /// invalid respone: -1
        /// </summary>
        internal static int GetCommErrorCount(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                try 
	            {	        
		            return Convert.ToInt32(clients[deviceId].CommError);
	            }
	            catch (Exception ex)
	            {
                    Tracer.WriteError(ex);
	            }
            }

            // error occored
            return -1;            
        }
     
        /// <summary>
        /// Get the Power Status of a given device
        /// </summary>
        /// <param name="deviceId">Identify of the target device</param>
        /// <param name="priority">WCS CM Priority level</param>
        /// <returns>SystemPowerState Class</returns>
        public static SystemStatus GetPowerStatus(byte deviceId, PriorityLevel priority = PriorityLevel.User)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetChassisState();
            }
            else
            {
                return new SystemStatus((byte)CompletionCode.InvalidDevice);
            }
        }

        public static SensorReading GetSensorReading(byte deviceId, byte sensorNumber, PriorityLevel priority = PriorityLevel.User)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetSensorReading(sensorNumber, priority);
            }
            else
            {
                return new SensorReading((byte)CompletionCode.InvalidDevice);
            }
        }

        public static string GetSensorDescription(byte deviceId, byte sensorNumber)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetSensorDescription(sensorNumber);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Graceful close of the Ipmi Session
        /// </summary>
        private static void Close(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                clients[deviceId].Close();
            }
        }

        #region Ipmi Commands

        /// <summary>
        /// Get Blade Info for Given Device Id
        /// </summary>
        public static BladeStatusInfo GetBladeInfo(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetBladeInfo();
            }
            else
            {
                return new BladeStatusInfo((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Get Fru Device info for Given Device Id
        /// </summary>
        public static FruDevice GetFruDeviceInfo(byte deviceId, int componentId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetFruDeviceInfo(componentId);
            }
            else
            {
                return new FruDevice((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Get Fru Device info
        /// </summary>
        public static FruDevice GetFruDeviceInfo(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetFruDeviceInfo(0);
            }
            else
            {
                return new FruDevice((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Queries BMC for the currently set boot device.
        /// </summary>
        /// <returns>Flags indicating the boot device.</returns>
        public static NextBoot GetNextBoot(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetNextBoot();
            }
            else
            {
                return new NextBoot((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// The helper for several boot type setting methods, as they
        /// essentially send the same sequence of messages.
        /// </summary>
        /// <param name="bootType">The desired boot type.</param>
        public static NextBoot SetNextBoot(byte deviceId, BootType bootType, bool uefi, bool persistent, byte bootInstance = 0x00)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].SetNextBoot(bootType, uefi, persistent, bootInstance);
            }
            else
            {
                return new NextBoot((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Write Fru Data Command.  Note:
        ///     The command writes the specified byte or word to the FRU Inventory Info area. This is a ‘low level’ direct 
        ///     interface to a non-volatile storage area. The interface does not interpret or check any semantics or 
        ///     formatting for the data being written.  The offset used in this command is a ‘logical’ offset that may or may not 
        ///     correspond to the physical address. For example, FRU information could be kept in FLASH at physical address 1234h, 
        ///     however offset 0000h would still be used with this command to access the start of the FRU information.
        ///     
        ///     IPMI FRU device data (devices that are formatted per [FRU]) as well as processor and DIMM FRU data always starts 
        ///     from offset 0000h unless otherwise noted.
        /// </summary>
        public static WriteFruDevice WriteFruDevice(byte deviceId, int fruId, ushort offset, byte[] payload)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].WriteFruDevice(fruId, offset, payload);
            }
            else
            {
                return new WriteFruDevice((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Write Fru Data to Baseboard containing BMC FRU.
        /// </summary>
        public static WriteFruDevice WriteFruDevice(byte deviceId, ushort address, byte[] payload)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].WriteFruDevice(0, address, payload);
            }
            else
            {
                return new WriteFruDevice((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Physically identify the computer by using a light or sound.
        /// </summary>
        /// <param name="interval">Identify interval in seconds or 255 for indefinite.</param>
        public static bool Identify(byte deviceId, byte interval)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].Identify(interval);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Set the Power Cycle interval.
        /// </summary>
        /// <param name="interval">Identify interval in seconds or 255 for indefinite.</param>
        public static bool SetPowerCycleInterval(byte deviceId, byte interval)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].SetPowerOnTime(interval);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Set the computer power state.
        /// </summary>
        /// <param name="powerState">Power state to set.</param>
        public static byte SetPowerState(byte deviceId, IpmiPowerState powerState)
        {
           
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].SetPowerState(powerState);
            }
            else
            {
                return (byte)CompletionCode.InvalidDevice;
            }
        }

        /// <summary>
        /// Gets BMC firmware revision.  Returns HEX string.
        /// </summary>
        /// <returns>firmware revision</returns>
        public static BmcFirmware GetFirmware(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetFirmware();
            }
            else
            {
                return new BmcFirmware((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Get the Power-On-Hours (POH) of the host computer.
        /// </summary>
        /// <returns>System Power On Hours.</returns>
        /// <remarks> Specification Note: Power-on hours shall accumulate whenever the system is in 
        /// the operational (S0) state. An implementation may elect to increment power-on hours in the S1 
        /// and S2 states as well.
        /// </remarks>
        public static PowerOnHours PowerOnHours(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].PowerOnHours();
            }
            else
            {
                return new PowerOnHours((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Queries BMC for the GUID of the system.
        /// </summary>
        /// <returns>GUID reported by Baseboard Management Controller.</returns>
        public static DeviceGuid GetSystemGuid(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetSystemGuid();
            }
            else
            {
                return new DeviceGuid((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Reset SEL Log
        /// </summary>
        public static bool ClearSel(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].ClearSel();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Recursively retrieves System Event Log entries.
        /// </summary>
        public static SystemEventLog GetSel(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetSel();
            }
            else
            {
                return new SystemEventLog((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        ///  Get System Event Log Information. Returns SEL Info.
        /// </summary>
        public static SystemEventLogInfo GetSelInfo(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetSelInfo();
            }
            else
            {
                return new SystemEventLogInfo((byte)CompletionCode.InvalidDevice);
            }

        }

        /// <summary>
        /// Sensor Data Record
        /// </summary>
        public static ConcurrentDictionary<byte, SensorMetadataBase> Sdr(byte deviceId)
        {
            return clients[deviceId].SensorDataRecords;
        }

        /// <summary>
        /// Get Device Id Command
        /// </summary>
        public static BmcDeviceId GetDeviceId(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetDeviceId();
            }
            else
            {
                return new BmcDeviceId((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Get/Set Power Policy
        /// </summary>
        public static PowerRestorePolicy SetPowerRestorePolicy(byte deviceId, PowerRestoreOption policyOption)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].SetPowerRestorePolicy(policyOption);
            }
            else
            {
                return new PowerRestorePolicy((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Switches Serial Port Access from BMC to System for Console Redirection
        /// </summary>
        public static SerialMuxSwitch SetSerialMuxSwitch(byte deviceId, MuxSwtich mux)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].SetSerialMuxSwitch(mux);
            }
            else
            {
                return new SerialMuxSwitch((byte)CompletionCode.InvalidDevice);
            }        
        }

        /// <summary>
        /// Switches Serial port sharing from System to Bmc
        /// </summary>
        public static SerialMuxSwitch ResetSerialMux(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].ResetSerialMux();
            }
            else
            {
                return new SerialMuxSwitch((byte)CompletionCode.IpmiResponseNotProvided);
            }        
        }

        /// <summary>
        /// Get the current advanced state of the host computer.
        /// </summary>
        /// <returns>ImpiPowerState enumeration.</returns>
        /// <devdoc>
        /// Originally used the 'Get ACPI Power State' message to retrieve the power state but not supported
        /// by the Arima's Scorpio IPMI card with firmware 1.10.00610100.  The 'Get Chassis Status' message
        /// returns the correct information for all IPMI cards tested.
        /// </devdoc>
        public static SystemStatus GetChassisState(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetChassisState();
            }
            else
            {
                return new SystemStatus((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Get JBOD Disk Status
        /// </summary>
        public static DiskStatusInfo GetDiskStatus(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetDiskStatus();
            }
            else
            {
                return new DiskStatusInfo((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Get JBOD Disk Information
        /// </summary>
        public static DiskInformation GetDiskInfo(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetDiskInfo();
            }
            else
            {
                return new DiskInformation((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Get JBOD Disk Information
        /// </summary>
        public static DiskInformation GetDiskInfo(byte deviceId, byte channel, byte disk)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetDiskInfo(channel, disk);
            }
            else
            {
                return new DiskInformation((byte)CompletionCode.InvalidDevice);
            }
        }


        /// <summary>
        /// Get Processor Information
        /// </summary>
        public static ProcessorInfo GetProcessorInfo(byte deviceId, byte proc)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetProcessorInfo(proc);
            }
            else
            {
                return new ProcessorInfo((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Get Memory Information
        /// </summary>
        public static MemoryInfo GetMemoryInfo(byte deviceId, byte dimm)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetMemoryInfo(dimm);
            }
            else
            {
                return new MemoryInfo((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Get PCIe Info
        /// </summary>
        public static PCIeInfo GetPCIeInfo(byte deviceId, byte PCIe)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetPCIeInfo(PCIe);
            }
            else
            {
                return new PCIeInfo((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Get Nic Info
        /// </summary>
        public static NicInfo GetNicInfo(byte deviceId, byte NicNo)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetNicInfo(NicNo);
            }
            else
            {
                return new NicInfo((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Get Hardware Info
        /// </summary>
        public static HardwareStatus GetHardwareInfo(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetHardwareInfo();
            }
            else
            {
                return new UnknownBlade((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Get Hardware Info
        /// </summary>
        public static HardwareStatus GetHardwareInfo(byte deviceId, bool proc, bool mem, 
            bool disk, bool me, bool temp, bool power, bool fru, bool pcie, bool misc)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetHardwareInfo(proc, mem, disk, me, 
                    temp, power, fru, pcie, misc);
            }
            else
            {
                return new UnknownBlade((byte)CompletionCode.InvalidDevice);
            }
        }

        #endregion

        #region Bridge Commands

        /// <summary>
        /// Send sync Bridge Command
        /// </summary>
        /// <param name="channel">Channel to send command (Intel ME = 6)</param>
        /// <param name="slaveId">Channel Slave Id</param>
        /// <param name="messageData">Message payload</param>
        public static BridgeMessage SendMessage(byte deviceId, byte channel, byte slaveId, byte[] requestMessage)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].SendMessage(channel, slaveId, requestMessage);
            }
            else
            {
                return new BridgeMessage((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Get Message Flags
        /// </summary>
        public static MessageFlags GetMessageFlags(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetMessageFlags();
            }
            else
            {
                return new MessageFlags((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Read Event Message Buffer
        /// </summary>
        public static BridgeMessage ReadEventMessageBuffer(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].ReadEventMessageBuffer();
            }
            else
            {
                return new BridgeMessage((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Get Message Response
        /// </summary>
        public static BridgeMessage GetMessage(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetMessage();
            }
            else
            {
                return new BridgeMessage((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Get the channel state for bridging commands
        /// </summary>
        /// <param name="channel">Channel number to check</param>
        /// <param name="enabled">Channel Disabled = 0x00, Channel Enabled = 0x001</param>
        public static BridgeChannelReceive BridgeChannelEnabled(byte deviceId, byte channel)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].BridgeChannelEnabled(channel);
            }
            else
            {
                return new BridgeChannelReceive((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// Enable or Disable the Ipmi Bridge Channel
        /// </summary>
        /// <param name="channel">Channel number to enable</param>
        /// <param name="enabled">Enabled = true, Disabled = false</param>
        public static BridgeChannelReceive EnableDisableBridgeChannel(byte deviceId, byte channel, bool enabled)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].EnableDisableBridgeChannel(channel, enabled);
            }
            else
            {
                return new BridgeChannelReceive((byte)CompletionCode.InvalidDevice);
            }
        }

        #endregion

        #region Dcmi Commands

        /// <summary>
        /// DCMI Get Power Limit Command
        /// </summary>
        public static PowerLimit GetPowerLimit(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetPowerLimit();
            }
            else
            {
                return new PowerLimit((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// DCMI Set Power Limit Command
        /// </summary>
        public static ActivePowerLimit SetPowerLimit(byte deviceId, short watts, int correctionTime, byte action, short samplingPeriod)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].SetPowerLimit(watts, correctionTime, action, samplingPeriod);
            }
            else
            {
                return new ActivePowerLimit((byte)CompletionCode.InvalidDevice);
            }
        }

        /// <summary>
        /// DCMI Get Power Reading Command
        /// </summary>
        public static List<PowerReading> GetPowerReading(byte deviceId)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].GetPowerReading();
            }
            else
            {
                return new List<PowerReading>(1) { new PowerReading((byte)CompletionCode.InvalidDevice) };                
            }
        }

        /// <summary>
        /// Activate/Deactivate DCMI power limit
        /// </summary>
        /// <param name="enable">Activate/Deactivate</param>
        public static bool ActivatePowerLimit(byte deviceId, bool enable)
        {
            if (clients.ContainsKey(deviceId))
            {
                return clients[deviceId].ActivatePowerLimit(enable);
            }
            else
            {
                return false;
            }
        }

        #endregion

    }
}
