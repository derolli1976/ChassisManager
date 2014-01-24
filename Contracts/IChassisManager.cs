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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Collections;

namespace Microsoft.GFS.WCS.Contracts
{
    // Define a service contract.
    [ServiceContract]
    public interface IChassisManager
    {
        // Create the method declaration for the contract.
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "GetChassisInfo?bladeinfo={bladeInfo}&psuInfo={psuInfo}&chassisControllerInfo={chassisControllerInfo}",
        BodyStyle = WebMessageBodyStyle.Bare)]
        ChassisInfoResponse GetChassisInfo(bool bladeInfo, bool psuInfo, bool chassisControllerInfo);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeInfoResponse GetBladeInfo(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        GetAllBladesInfoResponse GetAllBladesInfo();

        [OperationContract]
        [WebGet]
        ChassisResponse SetChassisAttentionLEDOn();

        [OperationContract]
        [WebGet]
        ChassisResponse SetChassisAttentionLEDOff();

        [OperationContract]
        [WebGet]
        LedStatusResponse GetChassisAttentionLEDStatus();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeResponse SetBladeAttentionLEDOn(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AllBladesResponse SetAllBladesAttentionLEDOn();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeResponse SetBladeAttentionLEDOff(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AllBladesResponse SetAllBladesAttentionLEDOff();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeResponse SetBladeDefaultPowerStateOn(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AllBladesResponse SetAllBladesDefaultPowerStateOn();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeResponse SetBladeDefaultPowerStateOff(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AllBladesResponse SetAllBladesDefaultPowerStateOff();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeStateResponse GetBladeDefaultPowerState(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        GetAllBladesStateResponse GetAllBladesDefaultPowerState();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeResponse SetPowerOn(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AllBladesResponse SetAllPowerOn();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeResponse SetPowerOff(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AllBladesResponse SetAllPowerOff();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeResponse SetBladeOn(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AllBladesResponse SetAllBladesOn();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeResponse SetBladeOff(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AllBladesResponse SetAllBladesOff();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeResponse SetBladeActivePowerCycle(int bladeId, uint offTime);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AllBladesResponse SetAllBladesActivePowerCycle(uint offTime);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        PowerStateResponse GetPowerState(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        GetAllPowerStateResponse GetAllPowerState();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeStateResponse GetBladeState(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        GetAllBladesStateResponse GetAllBladesState();

        [OperationContract]
        [WebInvoke(Method = "GET")]
        ChassisResponse SetACSocketPowerStateOn(uint portNo);

        [OperationContract]
        [WebInvoke(Method = "GET")]
        ChassisResponse SetACSocketPowerStateOff(uint portNo);

        [OperationContract]
        [WebInvoke(Method = "GET")]
        ACSocketStateResponse GetACSocketPowerState(uint portNo);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        StartSerialResponse StartBladeSerialSession(int bladeId, int sessionTimeoutInSecs);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ChassisResponse StopBladeSerialSession(int bladeId, string sessionToken, bool forceKill=false);
        
        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ChassisResponse SendBladeSerialData(int bladeId, string sessionToken, byte[] data);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        SerialDataResponse ReceiveBladeSerialData(int bladeId, string sessionToken);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        StartSerialResponse StartSerialPortConsole(int portId, int sessionTimeoutInSecs, int deviceTimeoutInMsecs);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ChassisResponse StopSerialPortConsole(int portId, string sessionToken, bool forceKill);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ChassisResponse SendSerialPortData(int portId, string sessionToken, byte[] data);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        SerialDataResponse ReceiveSerialPortData(int portId, string sessionToken);

        [OperationContract]
        [WebGet]
        ChassisLogResponse ReadChassisLogWithTimestamp(DateTime startTimestamp, DateTime endTimestamp);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare)]
        ChassisLogResponse ReadChassisLog();

        [OperationContract]
        [WebGet(UriTemplate = "/ClearChassisLog")]
        ChassisResponse ClearChassisLog();

        [OperationContract]
        [WebGet]
        ChassisLogResponse ReadBladeLogWithTimestamp(int bladeId, DateTime startTimestamp, DateTime endTimestamp);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ChassisLogResponse ReadBladeLog(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeResponse ClearBladeLog(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladePowerReadingResponse GetBladePowerReading(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        GetAllBladesPowerReadingResponse GetAllBladesPowerReading();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladePowerLimitResponse GetBladePowerLimit(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        GetAllBladesPowerLimitResponse GetAllBladesPowerLimit();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeResponse SetBladePowerLimit(int bladeId, double powerLimitInWatts);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AllBladesResponse SetAllBladesPowerLimit(double powerLimitInWatts);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeResponse SetBladePowerLimitOn(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AllBladesResponse SetAllBladesPowerLimitOn();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeResponse SetBladePowerLimitOff(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AllBladesResponse SetAllBladesPowerLimitOff();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ChassisNetworkPropertiesResponse GetChassisNetworkProperties();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ChassisResponse AddChassisControllerUser(string userName, string passwordString, WCSSecurityRole role);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ChassisResponse RemoveChassisControllerUser(string userName);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ChassisResponse ChangeChassisControllerUserRole(string userName, WCSSecurityRole role);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ChassisResponse ChangeChassisControllerUserPassword(string userName, string newPassword);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ChassisHealthResponse GetChassisHealth(bool bladeHealth, bool psuHealth, bool fanHealth);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BladeHealthResponse GetBladeHealth(int bladeId, bool cpuInfo, bool memInfo, bool diskInfo, bool pcieInfo, bool sensorInfo, bool temp, bool fruInfo);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BootResponse GetNextBoot(int bladeId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BootResponse SetNextBoot(int bladeId, BladeBootType bootType, bool uefi, bool persistent, int bootInstance);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare)]
        ServiceVersionResponse GetServiceVersion();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare)]
        MaxPwmResponse GetMaxPwmRequirement();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare)]
        ChassisResponse ResetPsu(int psuId);
    }

    /// <summary>
    /// Boot type for blades. 
    /// The boot should follow soon (within one minute) after the boot type is set.
    /// </summary>
    public enum BladeBootType : int
    {
        Unknown = 0,
        NoOverride = 1,
        ForcePxe = 2,
        ForceDefaultHdd = 3,
        ForceIntoBiosSetup = 4,
        ForceFloppyOrRemovable = 5
    }


    /// <summary>
    /// Enumerates all the completioncode
    /// </summary>
    public enum CompletionCode : byte
    {
        // Common error codes
        Success = 0x0,
        Failure = 0xFF,
        Timeout = 0xA3,
        Unknown = 0xA4,
        ParameterOutOfRange = 0xA5,
        SerialSessionActive = 0xA6,
        UserAccountExists = 0xA7,
        UserPasswordDoesNotMeetRequirement = 0xA8,
        CommandNotValidForBlade = 0xA9,
        UserNotFound = 0xB0,
        DevicePoweredOff = 0xB1,
        NoActiveSerialSession = 0xB2,
        CommandNotValidAtThisTime = 0xBF,
        FanlessChassis = 0xB3
    }

    public enum PowerState : byte
    {
        // power state codes
        ON = 0x1,
        OFF = 0x0,
        NA = 0x3,
    }

    public enum WCSSecurityRole : int
    {
        // WCS Roles
        WcsCmAdmin = 2,
        WcsCmOperator = 1,
        WcsCmUser = 0
    }

    public enum LedState : byte
    {
        // power state codes
        ON = 0x1,
        OFF = 0x0,
        NA = 0x3,
    }

    [DataContract]
    public class SerialDataResponse : ChassisResponse
    {
        [DataMember]
        public byte[] data = new byte[]{};
    }

    /// <summary>
    /// Structure for GetLedStatus
    /// </summary>	
    [DataContract]
    public class LedStatusResponse : ChassisResponse
    {
        [DataMember]
        public LedState ledState = LedState.NA;
    }

    [DataContract]
    public class ChassisNetworkPropertiesResponse : ChassisResponse
    {
        [DataMember]
        public List<ChassisNetworkProperty> chassisNetworkPropertyCollection = new List<ChassisNetworkProperty>();
    }

    [DataContract]
    public class ChassisNetworkProperty : ChassisResponse
    {
        [DataMember(Order=0)]
        public string macAddress = String.Empty;
        [DataMember(Order=1)]
        public string ipAddress = String.Empty;
        [DataMember(Order=2)]
        public string subnetMask = String.Empty;
        [DataMember(Order=3)]
        public string gatewayAddress = String.Empty;
        [DataMember(Order=4)]
        public string dnsAddress = String.Empty;
        [DataMember(Order=5)]
        public string dhcpServer = String.Empty;
        [DataMember(Order=6)]
        public string dnsDomain = String.Empty;
        [DataMember(Order=7)]
        public string dnsHostName = String.Empty;      
        [DataMember(Order=8)]
        public bool dhcpEnabled;
    }

    /// <summary>
    /// </summary>	
    [DataContract]
    public class PowerStateResponse : BladeResponse
    {
        [DataMember]
        public PowerState powerState = PowerState.NA;
    }

    /// <summary>
    /// </summary>	
    [DataContract]
    public class GetAllPowerStateResponse : ChassisResponse
    {
        [DataMember]
        public List<PowerStateResponse> powerStateResponseCollection = new List<PowerStateResponse>();
    }

    /// <summary>
    /// </summary>	
    [DataContract]
    public class BladeStateResponse : BladeResponse
    {
        [DataMember]
        public PowerState bladeState = PowerState.NA;
    }

    /// <summary>
    /// </summary>	
    [DataContract]
    public class GetAllBladesStateResponse : ChassisResponse
    {
        [DataMember]
        public List<BladeStateResponse> bladeStateResponseCollection=new List<BladeStateResponse>();
    }

    /// <summary>
    /// Structure for GetBladePowerReading
    /// </summary>	
    [DataContract]
    public class BladePowerReadingResponse : BladeResponse
    {
        [DataMember]
        public double powerReading;
    }

    /// <summary>
    /// Structure for GetBladePowerLimit
    /// </summary>	
    [DataContract]
    public class BladePowerLimitResponse : BladeResponse
    {
        [DataMember]
        public double powerLimit;
    }

    /// <summary>
    /// </summary>	
    [DataContract]
    public class GetAllBladesPowerReadingResponse : ChassisResponse
    {
        [DataMember]
        public List<BladePowerReadingResponse> bladePowerReadingCollection = new List<BladePowerReadingResponse>();
    }

    /// <summary>
    /// </summary>	
    [DataContract]
    public class GetAllBladesPowerLimitResponse : ChassisResponse
    {
        [DataMember]
        public List<BladePowerLimitResponse> bladePowerLimitCollection = new List<BladePowerLimitResponse>();
    }

    /// <summary>
    /// Structure for getpowerintstate
    /// </summary>	
    [DataContract]
    public class ACSocketStateResponse : ChassisResponse
    {
        [DataMember(Order=0)]
        public uint portNo;

        [DataMember(Order=1)]
        public PowerState powerState = PowerState.NA;

    }

    /// <summary>
    /// clrBladelog, clrnclog, ncidon, ncidoff, setBladeponstate, powerinton, powerintoff
    /// </summary>
    [DataContract]
    public class ChassisResponse
    {
        [DataMember(Order=0)]
        public CompletionCode completionCode = CompletionCode.Unknown;

        [DataMember(Order=1)]
        public int apiVersion = 1;

        [DataMember(Order=2)]
        public string statusDescription = String.Empty;

    }

    /// <summary>
    /// Structure for Bladeidon, Bladeidoff, poweron, poweroff, powercycle
    /// </summary>
    [DataContract]
    public class BladeResponse : ChassisResponse
    {
        [DataMember]
        public int bladeNumber;
    }

    /// <summary>
    /// </summary>
    [DataContract]
    public class AllBladesResponse : ChassisResponse
    {
        [DataMember]
        public List<BladeResponse> bladeResponseCollection = new List<BladeResponse>();
    }

    public class GetAllBladesInfoResponse : ChassisResponse
    {
        [DataMember]
        public List<BladeInfoResponse> bladeInfoResponseCollection = new List<BladeInfoResponse>();
    }

    /// <summary>
    /// Response to BladeInfo
    /// </summary>
    [DataContract]
    public class BladeInfo :BladeResponse
    {
        /// <summary>
        /// blade Guid
        /// </summary>
        [DataMember(Order=0)]
        public Guid bladeGuid = new Guid();

        /// <summary>
        /// blade NAme
        /// </summary>
        [DataMember(Order=1)]
        public string bladeName = String.Empty;

        /// <summary>
        /// blade Power State (On/Off)
        /// </summary>
        [DataMember(Order=2)]
        public PowerState powerState = PowerState.NA;

        /// <summary>
        /// blade MAC Addresses
        /// </summary>
        [DataMember(Order=3)]
        public List<NicInfo> bladeMacAddress = new List<NicInfo>();
    }

    /// <summary>
    /// This class defines the response for PSU information.
    /// </summary>
    [DataContract]
    public class PsuInfo : ChassisResponse
    {
        /// <summary>
        /// PSU ID
        /// </summary>
        [DataMember(Order=0)]
        public uint id;

        /// <summary>
        /// PSU Serial Number
        /// </summary>
        [DataMember(Order=1)]
        public string serialNumber = String.Empty;

        /// <summary>
        /// PSU State
        /// </summary>
        [DataMember(Order=3)]
        public PowerState state = PowerState.NA;

        /// <summary>
        /// PSU Power Reading 
        /// </summary>
        [DataMember(Order=4)]
        public uint powerOut;
    }

    /// <summary>
    /// This class defines the response for chassis controller. 
    /// </summary>
    [DataContract]
    public class ChassisControllerInfo : ChassisResponse
    {
        /// <summary>
        /// Chassis Serial Number
        /// </summary>
        [DataMember(Order=0)]
        public string serialNumber = String.Empty;

        /// <summary>
        /// Chassis Asset Tag
        /// </summary>
        [DataMember(Order=1)]
        public string assetTag = String.Empty;    
        
        /// <summary>
        /// Chassis Firmware Version
        /// </summary>
        [DataMember(Order=2)]
        public string firmwareVersion = String.Empty;

        /// <summary>
        /// Chassis Hardware Version
        /// </summary>
        [DataMember(Order=3)]
        public string hardwareVersion = String.Empty;      

        /// <summary>
        /// Time for which the Chassis manager is active.
        /// </summary>
        [DataMember(Order=4)]
        public string systemUptime = String.Empty;

        /// <summary>
        /// Details about CM network interfaces
        /// </summary>
        [DataMember(Order=5)]
        public ChassisNetworkPropertiesResponse networkProperties = new ChassisNetworkPropertiesResponse();
    }

    /// <summary>
    /// This class defines response to get info command,
    /// includes information on blade, PSU, and chassis controller
    /// </summary>
    [DataContract]
    public class ChassisInfoResponse : ChassisResponse
    {
        /// <summary>
        /// Chassis controller
        /// </summary>
        [DataMember(Order=0)]
        public ChassisControllerInfo chassisController = new ChassisControllerInfo();

        /// <summary>
        /// PSU object collection
        /// </summary>
        [DataMember(Order=1)]
        public List<PsuInfo> psuCollections = new List<PsuInfo>();

        /// <summary>
        /// Blade object collection
        /// </summary>
        [DataMember(Order=2)]
        public List<BladeInfo> bladeCollections = new List<BladeInfo>();        

    }

    /// <summary>
    /// This class defines response to getBladeinfo command,
    /// collection of blade
    /// </summary>
    [DataContract]
    public class BladeInfoResponse : BladeResponse
    {
        /// <summary>
        /// Blade Type
        /// </summary>
        [DataMember(Order=0)]
        public String bladeType = String.Empty;

        /// <summary>
        /// Blade Baseboard Serial Number
        /// </summary>
        [DataMember(Order=1)]
        public string serialNumber = String.Empty;

        /// <summary>
        /// Blade Asset Tag
        /// </summary>
        [DataMember(Order=2)]
        public string assetTag = String.Empty;

        /// <summary>
        /// Blade Firmware Version
        /// </summary>
        [DataMember(Order=3)]
        public string firmwareVersion = String.Empty;

        /// <summary>
        /// Blade Hardware Version
        /// </summary>
        [DataMember(Order=4)]
        public string hardwareVersion = String.Empty;
   
        /// <summary>
        /// Blade MAC Address
        /// </summary>
        [DataMember(Order=5)]
        public List<NicInfo> macAddress = new List<NicInfo>();
    }

    /// <summary>
    /// This defines the response to readBladelog
    /// </summary>
    [DataContract]
    public class LogResponse : ChassisResponse
    {
        /// <summary>
        /// Event Time collection
        /// </summary>
        [DataMember(Order=0)]
        public DateTime[] eventTime = new DateTime[]{};

        /// <summary>
        /// Even Description collection
        /// </summary>
        [DataMember(Order=1)]
        public string[] eventDescription = new string[]{};
    }

    /// <summary>
    /// This defines the response to readChassislog
    /// </summary>
    [DataContract]
    public class ChassisLogResponse : ChassisResponse
    {
        /// <summary>
        /// Event Time collection
        /// </summary>
        [DataMember]
        public List<LogEntry> logEntries = new List<LogEntry>();
    }

    /// <summary>
    /// This class defines log data format
    /// </summary>
    [DataContract]
    public class LogEntry
    {
        [DataMember(Order=0)]
        public DateTime eventTime = new DateTime();

        [DataMember(Order=1)]
        public string eventDescription = string.Empty;
    }

    /// <summary>
    /// This class defines the packet structure of startsersession
    /// </summary>
    [DataContract]
    public class StartSerialResponse : ChassisResponse
    {
        [DataMember]
        public string serialSessionToken = String.Empty;
    }

    /// <summary>
    /// Class structure to capture Health information for entire chassis
    /// </summary>
    [DataContract]
    public class ChassisHealthResponse : ChassisResponse
    {
        [DataMember(Order=0)]
        public List<BladeShellResponse> bladeShellCollection = new List<BladeShellResponse>();

        [DataMember(Order=1)]
        public List<FanInfo> fanInfoCollection = new List<FanInfo>();

        [DataMember(Order=2)]
        public List<PsuInfo> psuInfoCollection= new List<PsuInfo>();

    }

    /// <summary>
    /// Contains Blade shell information including Blade Id, Blade Type, and Blade Health Status
    /// </summary>
    [DataContract]
    public class BladeShellResponse : BladeResponse
    {
        [DataMember(Order=0)]
        public String bladeType = String.Empty;

        [DataMember(Order=1)]
        public String bladeState = String.Empty;
    }

    [DataContract]
    public class FanInfo : ChassisResponse
    {
        [DataMember(Order=0)]
        public int fanId;

        [DataMember(Order=1)]
        public bool isFanHealthy;

        [DataMember(Order=2)]
        public int fanSpeed;

    }

    /// <summary>
    /// Class structure to capture Health information for each blade
    /// </summary>
    [DataContract]
    public class BladeHealthResponse : BladeResponse
    {
        /// <summary>
        /// Blade Baseboard Serial Number
        /// </summary>
        [DataMember(Order=0)]
        public string serialNumber = String.Empty;

        /// <summary>
        /// Blade Asset Tag
        /// </summary>
        [DataMember(Order=1)]
        public string assetTag = String.Empty;

        /// <summary>
        /// Blade Hardware Version
        /// </summary>
        [DataMember(Order = 2)]
        public string hardwareVersion = String.Empty;

        /// <summary>
        /// Product Type
        /// </summary>
        [DataMember(Order=3)]
        public string productType = String.Empty;   

        [DataMember(Order=4)]
        public BladeShellResponse bladeShell = new BladeShellResponse();

        [DataMember(Order=5)]
        public List<ProcessorInfo> processorInfo = new List<ProcessorInfo>();

        [DataMember(Order=6)]
        public List<MemoryInfo> memoryInfo = new List<MemoryInfo>();

        [DataMember(Order=7)]
        public List<PCIeInfo> pcieInfo = new List<PCIeInfo>();

        [DataMember(Order=8)]
        public List<DiskInfo> bladeDisk = new List<DiskInfo>();

        [DataMember(Order=9)]
        public List<SensorInfo> sensors = new List<SensorInfo>();

        [DataMember(Order=10)]
        public JbodInfo jbodInfo = new JbodInfo();

        [DataMember(Order=11)]
        public JbodDiskStatus jbodDiskInfo = new JbodDiskStatus();
    
    }

    /// <summary>
    /// Processor information for blades
    /// </summary>
    [DataContract]
    public class ProcessorInfo : ChassisResponse
    {
        public ProcessorInfo()
        { }

        public ProcessorInfo(CompletionCode completionCode)
        { this.completionCode = completionCode; }

        public ProcessorInfo(CompletionCode completionCode, byte procId, string type, string state, ushort frequency)
            : this(completionCode)
        {
            this.procId = procId;
            this.procType = type;
            this.state = state;
            this.frequency = frequency;
        }

        // Processor Type
        [DataMember(Order=0)]
        public byte procId;

        // Processor Type
        [DataMember(Order=1)]
        public string procType = String.Empty;

        // Processor state
        [DataMember(Order=2)]
        public string state = String.Empty;

        // processor frequency
        [DataMember(Order=3)]
        public ushort frequency;
    }

    /// <summary>
    /// Memory information for blades
    /// </summary>
    [DataContract]
    public class MemoryInfo : ChassisResponse
    {
        public MemoryInfo()
        {
        }
        public MemoryInfo(CompletionCode completionCode)
        { this.completionCode = completionCode; }

        public MemoryInfo(CompletionCode completionCode, byte dimm, string type, ushort speed,
            ushort capacity, string memVoltage, string status)
            : this(completionCode)
        {
            this.dimm = dimm;

            this.dimmType = type;

            // Memory Speed
            this.speed = speed;

            // Dimm Size
            this.size = capacity;

            this.memVoltage = memVoltage;

            this.status = status;
        }

        // Memory Type
        [DataMember(Order=0)]
        public byte dimm;

        // Memory Type
        [DataMember(Order=1)]
        public string dimmType = String.Empty;

        // Status
        [DataMember(Order=2)]
        public string status = String.Empty;

        // Memory Speed
        [DataMember(Order=3)]
        public ushort speed;

        // Dimm Size
        [DataMember(Order=4)]
        public ushort size;

        // Memory Voltage
        [DataMember(Order=5)]
        public string memVoltage = String.Empty;        
    }

    /// <summary>
    /// PCIe Info for Blades
    /// </summary>
    [DataContract]
    public class PCIeInfo : ChassisResponse
    {
        public PCIeInfo() { }
        public PCIeInfo(CompletionCode completionCode)
        { this.completionCode = completionCode; }

        public PCIeInfo(CompletionCode completionCode, byte number, string state, ushort vendorId, ushort deviceId,
            ushort systemId, ushort subSystemId)
            : this(completionCode)
        {
            this.status = state;
            this.pcieNumber = number;
            this.vendorId = vendorId;
            this.deviceId = deviceId;
            this.systemId = systemId;
            this.subSystemId = subSystemId;
        }

        [DataMember(Order=0)]
        public byte pcieNumber;

        [DataMember(Order=1)]
        public ushort vendorId;

        [DataMember(Order=2)]
        public ushort deviceId;

        [DataMember(Order=3)]
        public ushort systemId;

        [DataMember(Order=4)]
        public ushort subSystemId;

        [DataMember(Order=5)]
        public string status = string.Empty;
    }

    /// <summary>
    /// Entire JBOD information
    /// </summary>
    [DataContract]
    public class JbodDiskStatus : ChassisResponse
    {
        public JbodDiskStatus() { }
        public JbodDiskStatus(CompletionCode completionCode)
        { this.completionCode = completionCode; }

        public JbodDiskStatus(CompletionCode completionCode, byte channel, byte diskCount)
            : this(completionCode)
        {
            this.channel = channel;
            this.diskCount = diskCount;
        }

        [DataMember(Order=0)]
        public byte channel;

        [DataMember(Order=1)]
        public byte diskCount;

        [DataMember(Order=2)]
        public List<DiskInfo> diskInfo = new List<DiskInfo>();
    }

    /// <summary>
    /// Properties in the Get JBOD Disk Status command response
    /// </summary>
    public class JbodInfo : ChassisResponse
    {
        /// <summary>
        /// Properties in the Get Disk information command response
        /// </summary>
        public JbodInfo(CompletionCode completionCode)
        {
            this.completionCode = completionCode;
        }

        /// <summary>
        /// Properties in the Get Disk information command response
        /// </summary>
        public JbodInfo(CompletionCode completionCode, string unit, string reading)
            : this(completionCode)
        {
            this.unit = unit;
            this.reading = reading;
        }

        /// <summary>
        /// Properties in the Get Disk information command response
        /// </summary>
        public JbodInfo()
        {
        }

        /// <summary>
        /// JBOD Disk Channel
        /// </summary>
        [DataMember(Order=0)]
        public string unit = String.Empty;

        /// <summary>
        /// JBOD Disk Count
        /// </summary>
        [DataMember(Order=1)]
        public string reading = String.Empty;

    }

    /// <summary>
    /// Hardare Sensor information class
    /// </summary>
    [DataContract]
    public class SensorInfo : ChassisResponse
    {
        public SensorInfo() { }
        public SensorInfo(CompletionCode completionCode)
        { this.completionCode = completionCode; }

        public SensorInfo(CompletionCode completionCode, byte sensor, string type, string entity, string entityInstance,
            string reading, string status, string description)
            : this(completionCode)
        {

            this.sensorNumber = sensor;
            this.reading = reading == null ? string.Empty : reading;
            this.entity = entity == null ? string.Empty : entity;
            this.sensorType = type == null ? string.Empty : type;
            this.entityInstance = entityInstance == null ? string.Empty : entityInstance;
            this.status = status == null ? string.Empty : status;
            this.description = description == null ? string.Empty : description;
        }

        public SensorInfo(CompletionCode completionCode, byte sensor, string type, string reading,
            string status, string description)
            : this(completionCode)
        {
            this.sensorNumber = sensor;
            this.reading = reading == null ? string.Empty : reading;
            this.entity = entity == null ? string.Empty : entity;
            this.sensorType = type == null ? string.Empty : type;
            this.entityInstance = entityInstance == null ? string.Empty : entityInstance;
            this.status = status == null ? string.Empty : status;
            this.description = description == null ? string.Empty : description;
        }

        [DataMember(Order=0)]
        public byte sensorNumber;

        [DataMember(Order=1)]
        public string sensorType=String.Empty;

        [DataMember(Order=2)]
        public string status = String.Empty;

        [DataMember(Order=3)]
        public string entity = String.Empty;

        [DataMember(Order=4)]
        public string entityInstance = String.Empty;

        [DataMember(Order=5)]
        public string reading = String.Empty;

        [DataMember(Order=6)]
        public string description = String.Empty;
    }

    /// <summary>
    /// Disk information for JBOD class
    /// </summary>
    [DataContract]
    public class DiskInfo : ChassisResponse
    {
        public DiskInfo() { }
        public DiskInfo(CompletionCode completionCode)
        { this.completionCode = completionCode; }

        public DiskInfo(CompletionCode completionCode, byte disk, string status)
            : this(completionCode)
        {
            this.diskId = disk;
            this.diskStatus = status;
        }

        [DataMember(Order=0)]
        public byte diskId;

        [DataMember(Order=1)]
        public string diskStatus = String.Empty;
    }

    /// <summary>
    /// Nic Information
    /// </summary>
    [DataContract]
    public class NicInfo : ChassisResponse
    {
        [DataMember(Order=0)]
        public int deviceId;

        [DataMember(Order=1)]
        public string macAddress = String.Empty;
    }

    [DataContract]
    public class BootResponse : BladeResponse
    {
        [DataMember]
        public BladeBootType nextBoot = BladeBootType.Unknown;
    }

    /// <summary>
    /// Service version information
    /// </summary>
    [DataContract]
    public class ServiceVersionResponse : ChassisResponse
    {
        [DataMember]
        public string serviceVersion;
    }

    /// <summary>
    /// Returns Max Blade PWM Requirement
    /// </summary>
    [DataContract]
    public class MaxPwmResponse : ChassisResponse
    {
        [DataMember]
        public byte maxPwmRequirement;
    }
}
