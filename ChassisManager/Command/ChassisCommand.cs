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
    /// <summary>
    /// Enumerates the Psu Models we support
    /// </summary>
    internal enum PsuModel
    {
        Delta,
        Emerson,
        Default,
    }

    /// <summary>
    /// State management - Blade State enum
    /// </summary>
    public enum BladeState : byte
    {
        HardPowerOff = 0x0,
        HardPowerOn = 0x1,
        Initialization = 0x2,
        Probation = 0x3,
        Healthy = 0x4,
        Fail = 0x5,
    }

    /// <summary>
    /// Type of Blade
    /// </summary>
    public enum BladeTypeName: byte
    {
        Unknown = 0x00,
        Server = 0x04,
        Jbod = 0x05,
        IEB = 0x06
    }

    /// <summary>
    /// Serial port baud rate
    /// </summary>
    public enum BaudRate : int
    {
        Rate_75 = 1,
        Rate_110 = 2,
        Rate_300 = 3,
        Rate_1200 = 4,
        Rate_2400 = 5,
        Rate_4800 = 6,
        Rate_9600 = 7,
        Rate_19200 = 8,
        Rate_38400 = 9,
        Rate_57600 = 10,
        Rate_115200 = 11
    }

    /// <summary>
    /// LedStatus 
    /// </summary>
    public enum LedStatus : byte
    {
        On = 1,
        Off = 0,
        NA = 2
    }

    /// <summary>
    /// Disk Status for disks
    /// </summary>
    public enum DiskStatus : byte
    {
        Presence = 0x00,
        Fault = 0x01,
        PredictiveFailure = 0x02,
        HotSpare = 0x03,
        ConsistencyParityCheckInProgress = 0x04,
        InCriticalArray = 0x05,
        InFailedArray = 0x06,
        RebuildRemapInProgress = 0x07,
        RebuildRemapAborted = 0x08
    }

    /// <summary>
    /// Enumerate all priority levels.
    /// </summary>
    public enum PriorityLevel : byte
    {
        System = 0x0,
        User = 0x1,
    };

    /// <summary>
    /// Chassis Command Function Codes
    /// </summary>
    public enum FunctionCode : byte
    {
        SledInfo	        = 0x01,
        SledTypeId          = 0x02,
        SledGuid            = 0x03,
        SledStatus          = 0x04,
        SledFanRequirement  = 0x05,
        SledIdentify	    = 0x0A,
        SledPowerControl	= 0x0B,
        SledSystemLog	    = 0x14,
        SledClearLog	    = 0x15,
        SledFruAreaInfo     = 0x16,
        SledReadFruData     = 0x17,
        SledWriteFruData     = 0x18,
        SledSensorReading   = 0x1E,
        SledSerialActivate  = 0x26,
        SledSerialDeactivate = 0x27,
        SledSerialPayload   = 0x28,
        SetFanSpeed = 0x0,
        GetFanSpeed = 0x1,
        PsuOperations = 0x0,
        TurnOffServer = 0x0,
        TurnOnServer = 0x1,
        GetServerPowerStatus = 0x2,
        TurnOffLed = 0x0,
        TurnOnLed = 0x1,
        GetLedStatus = 0x2,
        TurnOffPowerSwitch = 0x0,
        TurnOnPowerSwitch = 0x1,
        GetPowerSwitchStatus = 0x2,
        DisableWatchDogTimer = 0x0,
        EnableWatchDogTimer = 0x1,
        ResetWatchDogTimer = 0x2,
        ReadFanCageIntrude = 0x0,
        ReadEeprom = 0x0,
        WriteEeprom = 0x1,
        Invalid = 0xFF,
        OpenConsole = 0x0,
        CloseConsole = 0x1,
        SendConsole = 0x2,
        ReceiveConsole = 0x3,
    };

    /// <summary>
    /// Chassis Device Type
    /// </summary>
    public enum DeviceType : byte
    {
        Fan = 0x1,
        Psu = 0x2,
        Power = 0x3,
        Server = 0x4,
        WatchDogTimer = 0x5,
        StatusLed = 0x6,
        RearAttentionLed = 0x7,
        PowerSwitch = 0x8,
        FanCage = 0x9,
        ChassisFruEeprom = 0xA,
        BladeConsole = 0xB,
        SerialPortConsole = 0xC,
    }

    /// <summary>
    /// Enumerates all the completioncode
    /// </summary>
    public enum CompletionCode : byte
    {
        // Common error codes
        Success = 0x0,
        UnspecifiedError = 0xFF,

        // CM Client Codes
        InvalidDevice = 0xFE,
        // Unknown Blade Type
        UnknownBladeType = 0xFD,

        // Error codes for CM API
        NodeBusy = 0xA0,
        InvalidCommand = 0xA1,
        InvalidCommandForLun = 0xA2,
        Timeout = 0xA3,
        OutOfSpace = 0xA4,
        CanceledOrInvalidReservationId = 0xA5,
        RequestDataTruncated = 0xA6,
        InvalidRequestDataLength = 0xA7,
        RequestDataFieldLengthExceeded = 0xA8,
        ParameterOutOfRange = 0xA9,
        CannotReturnRequestedDataBytes = 0xAA,
        RequestedDataNotPresent = 0xAB,
        InvalidDataFieldInRequest = 0xAC,
        ResponseNotProvided = 0xAE,
        ResponseDataInvalid = 0xAD,
        CannotExecuteDuplicatedRequest = 0xAF,
        ResponseNotProvidedSdrReposInUpdate = 0xB0,
        ResponseNotProvidedDeviceInUpdate = 0xB1,
        ResponseNotProvidedBmcInInit = 0xB2,
        DestinationUnavailable = 0xB3,
        CmdFailedInsufficientPrivLevel = 0xB4,
        CmdFailedNotSupportedInPresentState = 0xB5,
        CmdFailedIllegalParameter = 0xB6,
        ServiceTerminating = 0xB7,
        CommDevFailedToInit = 0xB8,
        SerialPortOtherErrors = 0xB9,
        I2cErrors = 0xBA,
        FailToOpenSerialPort = 0xBB,
        FailToCloseSerialPort = 0xBC,
        CannotExecuteRequestInSafeMode = 0xBD,
        IpmiTimeOutHandShake = 0xBE,
        CmdNotSupportAtPresentTime = 0xBF,

        // Error codes for IPMI
        IpmiNodeBusy = 0xC0,
        IpmiInvalidCommand = 0xC1,
        IpmiInvalidCommandForLun = 0xC2,
        IpmiTimeout = 0xC3,
        IpmiOutOfSpace = 0xC4,
        IpmiCanceledOrInvalidReservationId = 0xC5,
        IpmiRequestDataTruncated = 0xC6,
        IpmiInvalidRequestDataLength = 0xC7,
        IpmiRequestDataFieldLengthExceeded = 0xC8,
        IpmiParameterOutOfRange = 0xC9,
        IpmiCannotReturnRequestedDataBytes = 0xCA,
        IpmiRequestedDataNotPresent = 0xCB,
        IpmiInvalidDataFieldInRequest = 0xCC,
        IpmiResponseNotProvided = 0xCE,
        IpmiCannotExecuteDuplicatedRequest = 0xCF,
        IpmiResponseNotProvidedSdrReposInUpdate = 0xD0,
        IpmiResponseNotProvidedDeviceInUpdate = 0xD1,
        IpmiResponseNotProvidedBmcInInit = 0xD2,
        IpmiDestinationUnavailable = 0xD3,
        IpmiCmdFailedInsufficientPrivLevel = 0xD4,
        IpmiCmdFailedNotSupportedInPresentState = 0xD5,
        IpmiCmdFailedIllegalParameter = 0xD6,
        IpmiFruVersionNotSupported = 0xD7,

        // Chassis Manager Top Level Error Codes
        InvalidBladeId = 0xE1,
    }

    
    /// <summary>
    /// A helper class that checks the completion code
    /// </summary>
    public static class CompletionCodeChecker
    {
        public static bool Failed(CompletionCode completionCode)
        {
            return (completionCode != CompletionCode.Success);
        }

        public static bool Succeeded(CompletionCode completionCode)
        {
            return (completionCode == CompletionCode.Success);
        }
    }
}
