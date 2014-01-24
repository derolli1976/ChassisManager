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
        /// Ipmi Commands
        /// </summary>
        internal enum IpmiCommand
        {

            #region Application

            // IPM Device ‘Global’ Commands
            GetDeviceId = 0x01,
            ColdReset = 0x02,
            WarmReset = 0x03,
            SetAcpiPowerState = 0x06,
            GetAcpiPowerState = 0x07,

            // BMC Watchdog Timer Commands
            ResetWatchdogTimer = 0x22,
            SetWatchdogTimer = 0x24,
            GetWatchdogTimer = 0x25,

            // IPMI Messaging Support Commands 
            ClearMessageFlags = 0x30,
            GetMessageFlags = 0x31,
            EnableMessageChannelReceive = 0x32,
            GetMessage = 0x33,
            SendMessage = 0x34,
            ReadEventMessageBuffer = 0x35,
            GetSystemGuid = 0x37,
            GetChannelAuthenticationCapabilities = 0x38,
            GetChannelCipherSuites = 0x54,
            GetSessionChallenge = 0x39,
            ActivateSession = 0x3A,
            SetSessionPrivilegeLevel = 0x3B,
            CloseSession = 0x3C,
            GetSessionInfo = 0x3D,
            GetChannelInfo = 0x42,
            SetChannelAccess = 0x40,
            GetChannelAccess = 0x41,
            MasterReadWrite = 0x52,

            // RMCP+ Support and Payload Commands
            ActivatePayload = 0x48,
            DeactivatePayload = 0x49,
            GetPayloadActivationStatus = 0x4A,
            GetPayloadInstanceInfo = 0x4B,
            SetUserPayloadAccess = 0x4C,
            GetUserPayloadAccess = 0x4D,
            GetChannelPayloadSpport = 0x4E,
            GetChannelPayloadVersion = 0x4F,
            GetChannelOemPayloadInfo = 0x50,

            // Ipmi Base System Commands
            SetSystemInfoParameters = 0x58,
            GetSystemInfoParameters = 0x59,

            // Account management 
            SetUserAccess = 0x43,
            GetUserAccess = 0x44,
            SetUserName = 0x45,
            GetUserName = 0x46,
            SetUserPassword = 0x47,

            // Chassis Commands
            GetChassisCapabilities = 0x00,
            GetChassisStatus = 0x01,
            ChassisControl = 0x02,
            ChassisReset = 0x03,
            ChassisIdentify = 0x04,
            SetPowerRestore = 0x06,
            SetSystemBootOptions = 0x08,
            GetSystemBootOptions = 0x09,
            GetPOHCounter = 0x0F,
            SetPowerCycleInterval = 0x0B,

            #endregion

            #region Storage

            // Sensor Device Commands 
            GetSdrRepositoryInfo = 0x20,
            GetSdrRepositoryAllocationInfo = 0x21,
            ReserveSdrRepository = 0x22,
            GetSdr = 0x23,
            GetSdrRepositoryTime = 0x28,
            SensorReading = 0x2D,
            SensorType = 0x2F,
            SensorFactors = 0x23,

            // FRU Inventory Device Commands 
            GetFruInventoryAreaInfo = 0x10,
            ReadFruData = 0x11,
            WriteFruData = 0x12,

            // SEL Device Commands 
            GetSelInfo = 0x40,
            GetSelAllocInto = 0x41,
            SelReserve = 0x42,
            GetSelEntry = 0x43,
            SelClear = 0x47,
            GetSelTime = 0x48,
            SetSelTime = 0x49,

            #endregion

            #region Transport

            // Transport (NetFn) Commands.
            SetLanConfigurationParameters = 0x01,
            GetLanConfigurationParameters = 0x02,
            SetSerialModemConfiguration = 0x10,
            GetSerialModemConfiguration = 0x11,
            SetSerialModelMux = 0x12,
            GetTapResponseCode = 0x13,
            SetPppUdpProxyTransmitData = 0x14,
            GetPppUdpProxyTransmitData = 0x15,
            SendPppUdpProxyPacket = 0x16,
            GetPppUdpProxyReceiveData = 0x17,
            SerialModemConnectionActive = 0x18,
            Callback = 0x19,
            SetUserCallbackOptions = 0x1A,
            GetUserCallbackOptions = 0x1B,
            SetSerialRoutingMux = 0x1C,
            SolActivating = 0x20,
            SetSolConfigurationParameters = 0x21,
            GetSolConfigurationParameters = 0x22,

            // RMCP+ Session Setup Payload Types.
            OpenSessionRequest = 0x10,
            OpenSessionResponse = 0x11,
            RAKP1 = 0x12,
            RAKP2 = 0x13,
            RAKP3 = 0x14,
            RAKP4 = 0x15,

            #endregion

            #region DCMI

            // DCMI Specific Commands
            DcmiCapability = 0x01,
            DcmiPowerReading = 0x02,
            DcmiGetLimit = 0x03,
            DcmiSetLimit = 0x04,
            DcmiActivateLimit = 0x05,
            DcmiGetAssetTag = 0x06,

            #endregion

            #region Oem / Jbod

            GetDiskStatus = 0xC4,

            GetDiskInfo = 0xC5,

            #endregion

            #region Oem / Group

            GetProcessorInfo = 0x1B,
            GetMemoryInfo = 0x1D,
            GetPCIeInfo = 0x44,
            GetNicInfo = 0x19,
            BmcDebug = 0xF9,

            #endregion

        }

        /// <summary>
        /// Represents the IPMI versions that affects the wire RMCP protocol.
        /// </summary>
        public enum IpmiVersion
        {
            /// <summary>
            /// Invalid IPMI version.
            /// </summary>
            Invalid = 0,

            /// <summary>
            /// IPMI V1.5 (use RMCP).
            /// </summary>
            V15 = 1,

            /// <summary>
            /// IPMI V2.0 (use RMCP+).
            /// </summary>
            V20 = 2,
        }

        /// <summary>
        /// Ipmi session privilege leves
        /// </summary>
        public enum PrivilegeLevel
        {
            None = 0,

            Callback = 1,

            User = 2,

            Operator = 3,

            Administrator = 4,

            Oem = 5,
        }

        public enum IpmiPowerState
        {
            Invalid,

            On,

            Off,

            Reset,

            Cycle,

            SoftOff
        }

        /// <summary>
        /// Chassis Status Power Fault
        /// </summary>
        public enum PowerEvent
        {
            ACfailed = 0,
            PowerOverload = 1,
            PowerInterlockActive = 2,
            PowerFault = 3,
            IpmiSetState = 4,
            Unknown,
        }

        /// <summary>
        /// JBOD Disk Status
        /// </summary>
        public enum DiskStatus
        {
            Normal = 0, 
            Failed = 1,
            Error = 2,
            Unknown
        }

        /// <summary>
        /// Chassis LED Status
        /// </summary>
        public enum IdentityState
        {

            On,

            Off,

            TemporaryOn,

            Unknown,
        }

        /// <summary>
        /// Controls the connection state
        /// of this client.
        /// </summary>
        public enum IpmiClientState
        {
            /// <summary>
            /// State is invalid or unknown
            /// </summary>
            Invalid,

            /// <summary>
            /// Session was lost or unknown
            /// </summary>
            Disconnected,

            /// <summary>
            /// Client is connecting
            /// </summary>
            Connecting,

            /// <summary>
            /// Client is connected
            /// </summary>
            Connected,

            /// <summary>
            /// Session is Opening
            /// </summary>
            OpenSession,

            /// <summary>
            /// Authentication Challanged issued
            /// </summary>
            AuthenticatingChallenge,

            /// <summary>
            /// Session Challanged issued
            /// </summary>
            SessionChallenge,

            /// <summary>
            /// Session is Activating
            /// </summary>
            ActivateSession,

            /// <summary>
            /// Session is Authenticated.
            /// </summary>
            Authenticated,
        }

        /// <summary>
        /// Chassis Power Policy: The power restore policy determines 
        /// how the system or chassis behaves when AC power returns 
        /// after an AC power loss
        /// </summary>
        public enum PowerRestoreOption
        {
            /// <summary>
            /// Chassis always stays powered off after AC/mains is applied, 
            /// power pushbutton or command required to power on system 
            /// </summary>
            StayOff          = 0,

            /// <summary>
            /// After AC/mains is applied or returns, power is restored to the 
            /// state that was in effect when AC/mains was removed or lost
            /// </summary>
            PreviousState    = 1,

            /// <summary>
            /// Chassis always powers up after AC/mains is applied or 
            /// returns 
            /// </summary>
            AlwaysPowerUp    = 2,

            /// <summary>
            /// No change (just get present policy support) 
            /// </summary>
            GetCurrentPolicy = 3,

            /// <summary>
            /// Unknown Power Restore Type
            /// </summary>
            Unknown = 0xFF
        }

        /// <summary>
        /// User Account actions.  Supports user 
        /// configuration ipmi commands
        /// </summary>
        public enum IpmiAccountManagment
        {
            DisableUser = 0,

            EnableUser = 1,

            SetPassword = 16,

            TestPassword = 17,

            Invalid = 34,
        }

        /// <summary>
        /// Entity Id's are used for identifying the physical entity 
        /// that a sensor or device is associated with
        /// </summary>
        public enum IpmiEntity : byte
        {
            Unspecified = 0,
            Other = 1,
            Unknown = 2,
            Processor = 3,
            Disk = 4,
            Peripheral = 5,
            MgmtModule = 6,
            SystemBoard = 7,
            MemoryModule = 8,
            ProcessorModule = 9,
            PowerSupply = 10,
            AddInCard = 11,
            FrontPanel = 12,
            BackPanel = 13,
            PowerSystem = 14,
            DriveBackplane = 15,
            IntExp = 16,
            OtherSystemBoard = 17,
            ProcessorBoard = 18,
            PowerUnit = 19,
            PowerModule = 20,
            PowerManagement = 21,
            ChassisPanel = 22,
            SystemChassis = 23,
            SubChassis = 24,
            ChassisBoard = 25,
            DiskBay = 26,
            PeripheralBay = 27,
            DeviceBay = 28,
            FanDevice = 29,
            CoolingUnit = 30,
            CableConnect = 31,
            MemoryDevice = 32,
            MgmtSoftware = 33,
            SystemFirmware = 34,
            OS = 35,
            SystemBus = 36,
            EntityGroup = 37,
            RemoteDevice = 38,
            ExternalEnvironment = 39,
            Battery = 40,
            ProcessorBlade = 41,
            ConSwitch = 42,
            ProcessorMemModule = 43,
            IOModule = 44,
            ProcessorIOModule = 45,
            MgmtCntrlFirmware = 46,
            IpmiChannel = 47,
            PCIeBus = 48,
            ExpressBus = 49,
            SCSIBus = 50,
            SataSAS = 51,
            ProcessorFSB = 52,
            Clock = 53,
            ReservedSystemFirmware = 54,
            AirInlet = 55,
            Reserved = 56,
            Baseboard = 66,
            ChassisSpecific = 144,
            BoardSpecific = 176,
            Oem = 208	
        }

        /// <summary>
        /// Ipmi entity type signals whether entity is
        /// a physical component or logical group of
        /// physical components
        /// </summary>
        public enum IpmiEntityType
        {
            Physical = 0,
            Group = 1,
            Unknown = 2
        }

        /// <summary>
        /// Used by the Set Serial/Modem Mux command to switch
        /// BMC Control.
        /// </summary>
        public enum MuxSwtich
        {
            GetMuxSetting = 0,
            SwitchSystem = 1,
            SwitchBmc = 2,
            ForceSystem = 3,
            ForceBmc = 4,
            BlockRequeststoSystem = 5,
            AllowRequeststoSystem = 6,
            BlockRequeststoBmc = 7,
            AllowRequeststoBmc = 8
        }

        /// <summary>
        /// ipmi payload types
        /// </summary>
        internal enum IpmiPayloadType
        {
            Ipmi = 0,
            SOL = 1
        }

        #region Sensor Data Record Enums

            /// <summary>
            /// Sensor Data Record (Sdr) Record types
            /// </summary>
            public enum SdrRecordType
            {
                Unspecified = 0,
                Full = 1,
                Compact = 2,
                EventOnly = 3,
                Association = 8,
                DeviceRelative = 9,
                GenericDevice = 16,
                FRUDevice = 17,
                ManagementDevice = 18,
                ManagementController = 19,
                BmcMessageChannel = 20,
                Oem = 12
            }

            /// <summary>
            /// Sensor Data Record Units Type Codes
            /// </summary>
            public enum SensorUnitTypeCode : byte
            {
                Unspecified = 0x00,
                DegreesC = 0x01,
                DegreesF = 0x02,
                DegreesK = 0x03,
                Volts = 0x04,
                Amps = 0x05,
                Watts = 0x06,
                Joules = 0x07,
                Coulombs = 0x08,
                VA = 0x09,
                Nits = 0x0A,
                Lumen = 0x0B,
                Lux = 0x0C,
                Candela = 0x0D,
                kPa = 0x0E,
                PSI = 0x0F,
                Newton = 0x10,
                CFM = 0x11,
                RPM = 0x12,
                Hz = 0x13,
                Microsecond = 0x14,
                Millisecond = 0x15,
                Second = 0x16,
                Minute = 0x17,
                Hour = 0x18,
                Day = 0x19,
                Week = 0x1A,
                Mil = 0x1B,
                Inches = 0x1C,
                Feet = 0x1D,
                CuIn = 0x1E,
                CuFeet = 0x1F,
                MM = 0x20,
                CM = 0x21,
                M = 0x22,
                CuCM = 0x23,
                CuM = 0x24,
                Liters = 0x25,
                FluidOunce = 0x26,
                Radians = 0x27,
                Steradians = 0x28,
                Revolutions = 0x29,
                Cycles = 0x2A,
                Gravities = 0x2B,
                Ounce = 0x2C,
                Pound = 0x2D,
                FtLb = 0x2E,
                OzIn = 0x2F,
                Gauss = 0x30,
                Gilberts = 0x31,
                Henry = 0x32,
                Millihenry = 0x33,
                Farad = 0x34,
                Microfarad = 0x35,
                Ohms = 0x36,
                Siemens = 0x37,
                Mole = 0x38,
                Becquerel = 0x39,
                PPM = 0x3A,
                Reserved = 0x3B,
                Decibels = 0x3C,
                DbA = 0x3D,
                DbC = 0x3E,
                Gray = 0x3F,
                Sievert = 0x40,
                ColorTemp = 0x41,
                Bit = 0x42,
                Kilobit = 0x43,
                Megabit = 0x44,
                Gigabit = 0x45,
                Byte = 0x46,
                Kilobyte = 0x47,
                Megabyte = 0x48,
                Igabyte = 0x49,
                Word = 0x4A,
                DWord = 0x4B,
                QWord = 0x4C,
                Line = 0x4D,
                Hit = 0x4E,
                Miss = 0x4F,
                Retry = 0x50,
                Reset = 0x51,
                Overrun = 0x52,
                Underrun = 0x53,
                Collision = 0x54,
                Packets = 0x55,
                Messages = 0x56,
                Characters = 0x57,
                Error = 0x58,
                CorrectErr = 0x59,
                UncorrectErr = 0x5A,
                FatalErr = 0x5B,
                Grams = 0x5C
            }

            /// <summary>
            /// Sdr state (enabled/disabled)
            /// </summary>
            public enum SdrState : byte
            {
                Disabled = 0x00,
                Enabled = 0x01,
                Unavailable = 0x03
            }

            /// <summary>
            /// Sensor Data Record unit rates
            /// </summary>
            public enum SdrUnitRate : byte
            {
                none = 0,
                µS = 1,
                ms = 2,
                s = 3,
                minute = 4,
                hour = 5,
                day = 6,
                reserved =7
            }

            /// <summary>
            /// Sdr linearization formulas
            /// </summary>
            public enum Linearization : byte
            {
                Linear = 0x00,
                Ln = 0x01,
                Log10 = 0x02,
                Log2 = 0x03,
                E = 0x04,
                Exp10 = 0x05,
                Exp2 = 0x06,
                OneX = 0x07,
                Sqr = 0x08,
                Cube = 0x09,
                Sqrt = 0x0A,
                OverCube = 0x0B,
                Nonlinear = 0x70,
            }

        #endregion

        #region OEM Commands

            /// <summary>
            /// Processor Types
            /// </summary>
            public enum ProcessorType : byte
            {
                None = 0x00,
                IntelCeleron = 0x01,
                IntelPentiumIII = 0x02,
                IntelPentium4 = 0x03,
                IntelXeon = 0x04,
                IntelPrestonia = 0x05,
                IntelNocona = 0x06,
                AmdOpteron = 0x07,
	            IntelDempsey = 0x08,
	            IntelClovertown = 0x09,
	            IntelTigerton = 0x0A,
                IntelDunnington = 0x0B,
                IntelHapertown = 0x0C,
                IntelWoldDaleDp = 0x0D,
                IntelNehalemEP = 0x0E,
                IntelLynnfield = 0x0F,	 
                AmdLibson = 0x10,
                AmdPhenomII = 0x11,
                AmdAthlonII = 0x12,
                AmdOperation = 0x13,
                AmdSuzuka = 0x14,
                IntelCorei3 = 0x15,
                IntelSandyBridge = 0x16,
                IntelIvyBridge = 0x17,	 
                NoCpuPresent = 0x18,
                Unknown = 0xFF
            }

            /// <summary>
            /// Processor State
            /// </summary>
            public enum ProcessorState : byte
            { 
                Present = 0x01,
                NotPresent = 0xFF,
                Unknown = 0x00,
            }

            /// <summary>
            /// PCIe State
            /// </summary>
            public enum PCIeState : byte
            {
                Present = 0x01,
                NotPresent = 0x02,
                Unknown = 0x00,
            }

            /// <summary>
            /// Memory Types
            /// </summary>
            public enum MemoryType : byte
            {
                SDRAM = 0x00,
                DDR1 = 0x01,
                Rambus = 0x02,
                DDR2 = 0x03,
                FBDIMM = 0x04,
                DDR3 = 0x05,
                NODIMM = 0xFF,
                Unknown = 0xFE
            }

            /// <summary>
            /// Memory Types
            /// </summary>
            public enum MemoryStatus : byte
            {
                Reserved = 0x00,
                Unknown = 0x01,
                Ok = 0x02,
                NotPresent = 0x03,
                SingleBitError = 0x05,
                MultiBitError = 0x07
            }

            /// <summary>
            /// Memory Voltage
            /// </summary>
            public enum MemoryVoltage : byte
            {
                /// <summary>
                /// 0 = Normal Voltage (1.5V)
                /// </summary>
                V15 = 0x00,

                /// <summary>
                /// 1 = Low Voltage (1.3V)
                /// </summary>
                V13 = 0x01,

                /// <summary>
                /// Unknown
                /// </summary>
                Unknown = 0xFF
            }

            /// <summary>
            /// PCIe Slot Types
            /// </summary>
            public enum PCIeSlot : byte
            {
                /// <summary>
                /// No PCIe Card
                /// </summary>
                None = 0x00,

                /// <summary>
                /// PCIe x16 Type Slot
                /// </summary>
                PCIex16 = 0x01,

                /// <summary>
                /// 10G Mess
                /// </summary>
                Mezz10G = 0x02,

                /// <summary>
                /// SAS Mezz
                /// </summary>
                MezzSAS = 0x03
            }

            /// <summary>
            /// BMC Policy Trigger Types
            /// </summary>
            public enum BmcPolicyTrigType : byte
            {
                NoTrigger = 0x00,
                Pwm = 0x01,
                InletTemp = 0x02,
                Invalid = 0xFF,
            }

            /// <summary>
            /// BMC Policy Trigger Command
            /// </summary>
            public enum BmcPolicyTrigCmd : byte
            {
                NoCommand = 0x00,
                SendBridgeMsg = 0x01,
            }

            /// <summary>
            /// BMC Policy Status
            /// </summary>
            public enum BmcPolicyStatus : byte
            {
                Inactive = 0x00,
                Active = 0x01,
                Remove = 0x02,
                Invalid = 0xFF,
            }

            /// <summary>
            /// BMC Policy Persistence
            /// </summary>
            public enum BmcPolicyPersistence : byte
            {
                Volatile = 0x00,
                Persistent = 0x01,
            }


            /// <summary>
            /// BMC Debug Process Types
            /// </summary>
            public enum BmcDebugProcess : byte
            {
                /// <summary>
                /// All KCS and Serial command trace debug messages
                /// </summary>
                KcsAndSerial = 0xFD,

                /// <summary>
                /// Fan Control Function trace debug messages
                /// </summary>
                FanControl = 0xFC,
            }

            /// <summary>
            ///  Energy Storage State
            /// </summary>
            public enum EnergyStorageState : byte
            {
                /// <summary>
                /// Energy Storage State: Unkonwn
                /// </summary>
                Unknown = 0x00,

                /// <summary>
                /// Energy Storage State: Charging
                /// </summary>
                Charging = 0x01,

                /// <summary>
                /// Energy Storage State: Discharging
                /// </summary>
                Discharging = 0x02,

                /// <summary>
                /// Energy Storage State: Floating
                /// </summary>
                Floating = 0x03, 
            }

            /// <summary>
            /// BMC action on SMB_Alert GPI Assertion
            /// </summary>
            public enum BmcSmbAlertAction : byte
            { 
                /// <summary>
                /// Bmc does nothing upon SMB_Alert GPI assertion
                /// </summary>
                NoAction = 0x00,

                /// <summary>
                /// Bmc asserts Fast Prochot GPO on SMB_Alert GPI 
                /// assertion, then applies a Default Power Cap,
                /// then waits and deasserts the Fast ProcHot. 01h
                /// </summary>
                ProcHotAndDpc = 0x04,

                /// <summary>
                /// Bmc applies the Default Power Cap without any
                /// Fast ProcHot on SMB_Alert GPI assertion. 02h
                /// </summary>
                DpcOnly = 0x08
            }

            /// <summary>
            /// NVDIMM Trigger Actions
            /// </summary>
            public enum NvDimmTrigger : byte
            { 
                /// <summary>
                /// Do nothing upon NVDIMM Trigger. HW switch on 
                /// the mainboard will be disabled.  Signal loss
                /// from the HSC comparator will not result in 
                /// ADR trigger on the PCH
                /// </summary>
                Disabled = 0x00,

                /// <summary>
                /// Automatically assert PCH GPI37 upon signal loss.
                /// HW switch on the mainboard will be enabled.  
                /// Signal loss from the HSC comparator will result 
                /// in ADR trigger.
                /// </summary>
                PchAdrGpi = 0x01,

                /// <summary>
                /// Bmc will trigger SMI routine in the BIOS.  The
                /// command must be executed manually with this bit
                /// set for the manual SMI.
                /// </summary>
                PchSmiGpi = 0x02,

                /// <summary>
                /// Unknown Trigger.  Response Type Only.
                /// </summary>
                Unknown = 0xFF
            }

        #endregion

        #region System Event Log Enums

        /// <summary>
        /// System Event Log event message types
        /// </summary>
        public enum EventLogMsgType : byte
        {
            /// <summary>
            /// None, default message
            /// </summary>
            None = 0x00,

            /// <summary>
            /// Threshold event type
            /// </summary>
            Threshold = 0x01,

            /// <summary>
            /// Discrete event type
            /// </summary>
            Discrete = 0x02,

            /// <summary>
            /// Sensor-specific event type
            /// </summary>
            SensorSpecific = 0x03,

            /// <summary>
            /// OEM event type
            /// </summary>
            Oem = 0x04,

            /// <summary>
            /// OEM Timestamped event type
            /// </summary>
            OemTimestamped = 0x05,

            /// <summary>
            /// OEM NonTimeStamped event type
            /// </summary>
            OemNonTimeStamped = 0x06,

            /// <summary>
            /// Unspecified event
            /// </summary>
            Unspecified = 0x07
        }

        /// <summary>
        /// Signals message reading offset
        /// </summary>
        public enum SelEventType
        {
            /// <summary>
            /// Unspecified event
            /// </summary>
            Unspecified = 0,

            /// <summary>
            /// Sensor Specific event. Extended
            /// data in event data record.
            /// </summary>
            Extension = 1,

            /// <summary>
            /// digital discrete event.  additional
            /// data in the event data record
            /// </summary>
            Digital = 2,

            /// <summary>
            /// Threshold trigger in the event data
            /// record
            /// </summary>
            Trigger = 3,

            /// <summary>
            /// OEM event.  Limited data available for
            /// event.
            /// </summary>
            OEMCode = 4
        }

        /// <summary>
        /// System Event Log event format
        /// </summary>
        public enum EventMessageFormat : byte
        {
            None = 0x00,
            SystemEvent = 0x02,
            OemTimeStamped = 0xC0,
            OemNonTimeStamped = 0xE0
        }

        /// <summary>
        /// System Event Log Message format
        /// </summary>
        public enum MsgVersion : byte
        {   
            Unknown = 0x00,
            OEM = 0x01,
            IpmiV1 = 0x03,
            IpmiV2 = 0x04
        }

        /// <summary>
        /// Sensor types
        /// </summary>
        public enum SensorType: byte
        {
            Reserved = 0x00,
            Temperature = 0x01,
            Voltage = 0x02,
            Current = 0x03,
            Fan = 0x04,
            ChassisIntrusion = 0x05,
            SecurityViolation = 0x06,
            Processor = 0x07,
            PowerSupply = 0x08,
            PowerUnit = 0x09,
            CoolingDevice = 0x0A,
            FruSensor = 0x0B,
            Memory = 0x0C,
            DriveSlot = 0x0D,
            MemoryResize = 0x0E,
            Firmware = 0x0F,
            EventLogging = 0x10,
            Watchdog1 = 0x11,
            SystemEvent = 0x12,
            CriticalInterrupt = 0x13,
            Button = 0x14,
            Board = 0x15,
            Microcontroller = 0x16,
            AddInCard = 0x17,
            Chassis = 0x18,
            Chipset = 0x19,
            OtherFru = 0x1A,
            CableConnect = 0x1B,
            Terminator = 0x1C,
            BootInitiated = 0x1D,
            BootError = 0x1E,
            OSBoot = 0x1F,
            CriticalStop = 0x20,
            SlotConnector = 0x21,
            AcpiPowerState = 0x22,
            Watchdog2 = 0x23,
            PlatformAlert = 0x24,
            EntityPresence = 0x25,
            MonitorAsic = 0x26,
            LAN = 0x27,
            MgmtHealth = 0x28,
            Battery = 0x29,
            SessionAudit = 0x2A,
            VersionChange = 0x2B,
            FruState = 0x2C,
            Unknown = 0xFF
        }

        /// <summary>
        /// System Event Log event direction
        /// </summary>
        public enum EventDir : byte
        {
            Assertion = 0x00,
            Desertion = 0x01,
            Unknown,
        }

    #endregion

}
