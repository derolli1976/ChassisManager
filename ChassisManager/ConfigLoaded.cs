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

    using Config = System.Configuration.ConfigurationManager;
    using Microsoft.GFS.WCS.ChassisManager.Ipmi;
    using System.Collections.Generic;
    using System.Collections;
    using System.Linq;
    using System.Xml;
    using System.IO;
    using System;

    static class ConfigLoaded
    {
        /// <summary>
        /// Chassis Manager blade population count.  Set & retrieved
        /// from the App.Config.
        /// </summary>
        internal static readonly int Population;

        /// <summary>
        /// Chassis Manager number of serial console ports (like TOR switches).  Set & retrieved
        /// from the App.Config.
        /// </summary>
        internal static readonly int MaxSerialConsolePorts;

        /// <summary>
        /// Inactive Serial Port (like TOR switches) Session id - used by SerialPortConsoleMetadata class.  Set & retrieved
        /// from the App.Config.
        /// </summary>
        internal static readonly int InactiveSerialPortId;

        /// <summary>
        /// Serial Port Read/Write Timeout in ms.  Set & retrieved from the App.Config.
        /// </summary>
        internal static readonly int SerialTimeout;

        /// <summary>
        /// Gpio Master Chip consecutive error limit before the chip is reset.
        /// </summary>
        internal static readonly int GpioErrorLimit;

        /// <summary>
        /// Inactive Serial Port (like TOR switches) Session token - used by SerialPortConsoleMetadata class.  Set & retrieved
        /// from the App.Config.
        /// </summary>
        internal static readonly string InactiveSerialPortSessionToken;

        /// <summary>
        /// Secret Serial Port (like TOR switches) Session token - used by SerialPortConsoleMetadata class.  Set & retrieved
        /// from the App.Config.
        /// </summary>
        internal static readonly string SecretSerialPortSessionToken;

        /// <summary>
        /// Timeout for Serial Console Port - used by SerialPortConsoleMetadata class.  
        /// from the App.Config.
        /// </summary>
        internal static readonly int SerialPortConsoleClientSessionInactivityTimeoutInSecs;
        
        /// <summary>
        /// Timeout for Serial Console Port Device Communication send/receive calls - used by SerialPortConsoleMetadata class.  
        /// from the App.Config.
        /// </summary>
        internal static readonly int SerialPortConsoleDeviceCommunicationTimeoutInMsecs;

        /// <summary>
        /// Inactive Blade Session id for Blade serial session - used by BladeSerialSessionMetadata class.  
        /// from the App.Config.
        /// </summary>
        internal static readonly int InactiveBladePortId;

        /// <summary>
        /// Inactive Session token for Blade serial session - used by BladeSerialSessionMetadata class.  
        /// Set & retrieved from the App.Config.
        /// </summary>
        internal static readonly string InactiveBladeSerialSessionToken;

        /// <summary>
        /// Secret Blade Session id for Blade serial session - used by BladeSerialSessionMetadata class.  
        /// from the App.Config.
        /// </summary>
        internal static readonly int SecretBladePortId;

        /// <summary>
        /// Secret Session token for Blade serial session - used by BladeSerialSessionMetadata class.  
        /// Set & retrieved from the App.Config.
        /// </summary>
        internal static readonly string SecretBladeSerialSessionToken;

        /// <summary>
        /// Timeout for Blade serial session - used by BladeSerialSessionMetadata class.  
        /// from the App.Config.
        /// </summary>
        /// 

        /// <summary>
        /// Timeout for Blade serial session - used by BladeSerialSessionMetadata class.  
        /// from the App.Config.
        /// </summary>
        private static int bladeSerialTimeout;


        /// <summary>
        /// volitale serial blade serial session timeout, this value is set by success in the
        /// startbladeSerialSession and reset to zero by success in the stopbladeserialsession.
        /// </summary>
        private static int volatileBladeSerialTimeout;

        /// <summary>
        /// Timeout for Blade serial session - used by BladeSerialSessionMetadata class.  
        /// from the App.Config.
        /// </summary>
        internal static int TimeoutBladeSerialSessionInSecs
        {
            get
            {
                if (volatileBladeSerialTimeout == 0)
                    return bladeSerialTimeout;
                else
                    return volatileBladeSerialTimeout;
            }

            set
            {
                volatileBladeSerialTimeout = value;
            }
        }

        /// <summary>
        /// BMC login user name.  Set & retrieved from 
        /// the App.Config.
        /// </summary>
        internal static string BmcUserName = string.Empty;

        /// <summary>
        /// BMC user key.  Set & retrieved from 
        /// the App.Config.
        /// </summary>
        internal static string BmcUserKey = string.Empty;

        /// <summary>
        /// BMC session inactivity time-out.  This setting increase the BMC session to time-out. 
        /// The value is calculated by increments of 30 seconds.  The default value is 6, which 
        /// corresponds to 3 minutes.
        /// </summary>
        internal static int BmcSessionTime = 6;

        /// <summary>
        /// Number of fans in chassis.  Set & retrieved
        /// from the App.Config.
        /// </summary>
        internal static readonly int NumFans;

        /// <summary>
        /// Number of PSUs in chassis.  Set & retrieved
        /// from the App.Config.
        /// </summary>
        internal static readonly int NumPsus;

        /// <summary>
        /// Number of Nics in Blades.  Set & retrieved
        /// from the App.Config.
        /// </summary>
        internal static readonly int NumNicsPerBlade;

        /// <summary>
        /// Number of power switches in chassis.  Set & retrieved
        /// from the App.Config.
        /// </summary>
        internal static readonly int NumPowerSwitches;

        /// <summary>
        /// Wait time after AC socket power off in milliseconds. Set & retrieved
        /// from the App.Config.
        /// </summary>
        internal static readonly int WaitTimeAfterACSocketPowerOffInMsecs;

        /// <summary>
        /// Wait time after Blade hard power off in milliseconds. Set & retrieved
        /// from the App.Config.
        /// </summary>
        internal static readonly int WaitTimeAfterBladeHardPowerOffInMsecs;

        /// <summary>
        /// Trace log file path
        /// </summary>
        internal static readonly string TraceLogFilePath;

        /// <summary>
        /// Trace log file size
        /// </summary>
        internal static readonly int TraceLogFileSize;

        /// <summary>
        /// User log file path
        /// </summary>
        internal static readonly string UserLogFilePath;

        /// <summary>
        /// User log file size
        /// </summary>
        internal static readonly int UserLogFileSize;

        /// <summary>
        /// Time Period for Monitoring Get thread
        /// </summary>
        internal static readonly int GetTimePeriod;

        /// <summary>
        /// Time Period for Set thread (set fan speed, reset WatchDogTimer)
        /// </summary>
        internal static readonly int SetTimePeriod;

        /// <summary>
        /// Maximum value for PWM setting (This should be equal to 100)
        /// </summary>
        internal static readonly int MaxPWM;

        /// <summary>
        /// Minimum value for PWM setting
        /// </summary>
        internal static readonly int MinPWM;

        /// <summary>
        /// Step value for PWM - ramp down policy
        /// </summary>
        internal static readonly int StepPWM;

        /// <summary>
        /// InputSensor number - varies based on HW implementation
        /// </summary>
        internal static readonly int InputSensor;

        /// <summary>
        /// LowThreshold for the sensor 
        /// </summary>
        internal static readonly int SensorLowThreshold;

        /// <summary>
        /// High Threshold for the sensor
        /// </summary>
        internal static readonly int SensorHighThreshold;

        /// <summary>
        /// AltitudeCorrectionFactor - based on HW Spec for CM
        /// </summary>
        internal static readonly float AltitudeCorrectionFactor;

        /// <summary>
        /// Altitude - varies based on location - integer (feet above sea level)
        /// </summary>
        internal static readonly int Altitude;

        /// <summary>
        /// Maximum number of retries allowed
        /// </summary>
        internal static readonly int MaxRetries;

        /// <summary>
        /// LED high value
        /// </summary>
        internal static readonly int LEDHigh;

        /// <summary>
        /// LED Low value
        /// </summary>
        internal static readonly int LEDLow;

        /// <summary>
        /// Min balde power limit value
        /// </summary>
        internal static readonly int MinPowerLimit;

        /// <summary>
        /// Max blade power limit value
        /// </summary
        internal static readonly int MaxPowerLimit;

        /// <summary>
        /// Maximum Failure Count before trying reinitialization
        /// </summary>
        internal static readonly int MaxFailCount;

        /// <summary>
        /// Service timeout in minutes
        /// </summary>
        internal static readonly double ServiceTimeoutInMinutes;

        /// <summary>
        /// CM Fru Variables (names are self-explanatory)
        /// </summary>
        internal static readonly int ChassisStartingOffset;
        internal static readonly int ChassisFruLength;
        internal static readonly int InternalUseSize;
        internal static readonly int ChassisInfoSize;
        internal static readonly int BoardInfoSize;
        internal static readonly int ProductInfoSize;

        /// <summary>
        /// Disk Sensor Ids for On Blade Disks
        /// </summary>
        internal static readonly int BladeDisk0;
        internal static readonly int BladeDisk1;
        internal static readonly int BladeDisk2;
        internal static readonly int BladeDisk3;

        /// <summary>
        /// Sensor ids for Health Diagnostics
        /// </summary>
        internal static int CPU0ProcHotSensor;
        internal static int CPU1ProcHotSensor;
        internal static int PCIeBusSensor;
        internal static int SPSFWSensor;

        /// <summary>
        /// Max work queue length of the port manager
        /// </summary>
        internal static readonly int MaxPortManagerWorkQueueLength;
 
        internal static int CmServicePortNumber;
        internal static string SslCertificateName;
        internal static bool EnableSslEncryption;
        internal static bool KillSerialConsoleSession;
        internal static bool EnableFan;

        // List of event log strings. Loaded from EventDataStrings.xml.  This xml file 
        // is similar to a traditional resource file
        internal static List<EventLogData> EventStrings;

        // Event log formatting seperator used to break-up strings. Loaded in EventDataStrings.xml
        internal static readonly string EventLogStrSeparator;

        // Event log formatting string Error Code message. Loaded in EventDataStrings.xml
        internal static readonly string EventLogStrError;

        // Event log formatting space. Loaded in EventDataStrings.xml
        internal static readonly string EventLogStrSpacer;

        // Event log string for SensorType. Loaded in EventDataStrings.xml
        internal static readonly string EventLogStrSensor;

        // Unknown string loaded from EventDataStrings.xml
        internal static readonly string Unknown;

        /// <summary>
        /// Class Constructor.
        /// </summary>
        static ConfigLoaded()
        {

            // check app.config for Population, if population is not found
            // in the app.config default the value to 24.
            int.TryParse(Config.AppSettings["Population"], out Population);
            Population = Population == 0 ? 24 : Population;

            // check app.config for MaxSerialConsolePorts, if MaxSerialConsolePorts is not found
            // in the app.config default the value to 1.
            int.TryParse(Config.AppSettings["MaxSerialConsolePorts"], out MaxSerialConsolePorts);
            MaxSerialConsolePorts = MaxSerialConsolePorts == 0 ? 1 : MaxSerialConsolePorts;

            int.TryParse(Config.AppSettings["InactiveSerialPortId"], out InactiveSerialPortId);
            InactiveSerialPortId = InactiveSerialPortId == 0 ? -1 : InactiveSerialPortId;

            InactiveSerialPortSessionToken = Config.AppSettings["InactiveSerialPortSessionToken"].ToString();
            InactiveSerialPortSessionToken = InactiveSerialPortSessionToken == string.Empty ? "-1" : InactiveSerialPortSessionToken;

            SecretSerialPortSessionToken = Config.AppSettings["SecretSerialPortSessionToken"].ToString();
            SecretSerialPortSessionToken = SecretSerialPortSessionToken == string.Empty ? "-99" : SecretSerialPortSessionToken;

            int.TryParse(Config.AppSettings["SerialPortConsoleClientSessionInactivityTimeoutInSecs"], out SerialPortConsoleClientSessionInactivityTimeoutInSecs);
            SerialPortConsoleClientSessionInactivityTimeoutInSecs = SerialPortConsoleClientSessionInactivityTimeoutInSecs == 0 ? 120 : SerialPortConsoleClientSessionInactivityTimeoutInSecs;

            int.TryParse(Config.AppSettings["SerialPortConsoleDeviceCommunicationTimeoutInMsecs"], out SerialPortConsoleDeviceCommunicationTimeoutInMsecs);
            SerialPortConsoleDeviceCommunicationTimeoutInMsecs = SerialPortConsoleDeviceCommunicationTimeoutInMsecs == 0 ? 100 : SerialPortConsoleDeviceCommunicationTimeoutInMsecs;

            int.TryParse(Config.AppSettings["InactiveBladePortId"], out InactiveBladePortId);
            InactiveBladePortId = InactiveBladePortId == 0 ? -1 : InactiveBladePortId;

            InactiveBladeSerialSessionToken = Config.AppSettings["InactiveBladeSerialSessionToken"].ToString();
            InactiveBladeSerialSessionToken = InactiveBladeSerialSessionToken == string.Empty ? "-1" : InactiveBladeSerialSessionToken;

            int.TryParse(Config.AppSettings["SecretBladePortId"], out SecretBladePortId);
            SecretBladePortId = SecretBladePortId == 0 ? -99 : SecretBladePortId;

            SecretBladeSerialSessionToken = Config.AppSettings["SecretBladeSerialSessionToken"].ToString();
            SecretBladeSerialSessionToken = SecretBladeSerialSessionToken == string.Empty ? "-99" : SecretBladeSerialSessionToken;

            int.TryParse(Config.AppSettings["bladeSerialTimeout"], out bladeSerialTimeout);
            bladeSerialTimeout = bladeSerialTimeout == 0 ? 2 : bladeSerialTimeout;

            int.TryParse(Config.AppSettings["SerialTimeout"], out SerialTimeout);
            SerialTimeout = SerialTimeout == 0 ? 100 : SerialTimeout;

            int.TryParse(Config.AppSettings["GpioErrorLimit"], out GpioErrorLimit);
            GpioErrorLimit = GpioErrorLimit == 0 ? 3 : GpioErrorLimit;

            // check app.config for BmcSessionTime.  if not found, the default value
            // is 6.
            int.TryParse(Config.AppSettings["BmcSessionTime"], out BmcSessionTime);
            BmcSessionTime = BmcSessionTime == 0 ? 6 : BmcSessionTime;

            // check app.config for BmcUserName, if BmcUserName is not found
            // in the app.config default the value to root.
            BmcUserName = Config.AppSettings["BmcUserName"].ToString();
            BmcUserName = BmcUserName == string.Empty ? "root" : BmcUserName;

            // check app.config for BmcUserKey, if BmcUserName is not found
            // in the app.config default the value to root.
            BmcUserKey = Config.AppSettings["BmcUserKey"].ToString();
            BmcUserKey = BmcUserKey == string.Empty ? "root" : BmcUserKey;

            // check app.config for NumFans, if NumFans is not found
            // in the app.config default the value to 6.
            int.TryParse(Config.AppSettings["NumFans"], out NumFans);
            NumFans = NumFans == 0 ? 6 : NumFans;

            // check app.config for NumPsus, if NumPsus is not found
            // in the app.config default the value to 6.
            int.TryParse(Config.AppSettings["NumPsus"], out NumPsus);
            NumPsus = NumPsus == 0 ? 6 : NumPsus;

            // check app.config for NumNicsPerBlade, if not found
            // in the app.config default the value to 2.
            int.TryParse(Config.AppSettings["NumNicsPerBlade"], out NumNicsPerBlade);
            NumNicsPerBlade = NumNicsPerBlade == 0 ? 2 : NumNicsPerBlade;

            // check app.config for NumPowerSwitches, if NumPowerSwitches is not found
            // in the app.config default the value to 2.
            int.TryParse(Config.AppSettings["NumPowerSwitches"], out NumPowerSwitches);
            NumPowerSwitches = NumPowerSwitches == 0 ? 2 : NumPowerSwitches;

            // check app.config for WaitTimeAfterACSocketPowerOffInMsecs, if not found
            // in the app.config default the value to 1000 ms.
            int.TryParse(Config.AppSettings["WaitTimeAfterACSocketPowerOffInMsecs"], out WaitTimeAfterACSocketPowerOffInMsecs);
            WaitTimeAfterACSocketPowerOffInMsecs = WaitTimeAfterACSocketPowerOffInMsecs == 0 ? 1000 : WaitTimeAfterACSocketPowerOffInMsecs;

            // check app.config for WaitTimeAfterBladeHardPowerOffInMsecs, if not found
            // in the app.config default the value to 100 ms.
            int.TryParse(Config.AppSettings["WaitTimeAfterBladeHardPowerOffInMsecs"], out WaitTimeAfterBladeHardPowerOffInMsecs);
            WaitTimeAfterBladeHardPowerOffInMsecs = WaitTimeAfterBladeHardPowerOffInMsecs == 0 ? 100 : WaitTimeAfterBladeHardPowerOffInMsecs;

            // check app.config for ChassisManagerTraceFilePath, if not found
            // in the app.config default the value to C:\ChassisManagerTrace.txt.
            TraceLogFilePath = Config.AppSettings["TraceLogFilePath"].ToString();
            TraceLogFilePath = TraceLogFilePath == string.Empty ? @"C:\ChassisManagerTrace.txt" : TraceLogFilePath;

            // check app.config for ChassisManagerTraceFileSize, if not found
            // in the app.config default the value to 100 KB.
            int.TryParse(Config.AppSettings["TraceLogFileSize"], out TraceLogFileSize);
            TraceLogFileSize = TraceLogFileSize == 0 ? 100 : TraceLogFileSize;

            // check app.config for ChassisManagerTraceFilePath, if not found
            // in the app.config default the value to C:\ChassisManagerTrace.txt.
            UserLogFilePath = Config.AppSettings["UserLogFilePath"].ToString();
            UserLogFilePath = UserLogFilePath == string.Empty ? @"C:\ChassisManagerUserLog.txt" : UserLogFilePath;

            // check app.config for ChassisManagerUserFileSize, if not found
            // in the app.config default the value to 100 KB.
            int.TryParse(Config.AppSettings["UserLogFileSize"], out UserLogFileSize);
            UserLogFileSize = UserLogFileSize == 0 ? 100 : UserLogFileSize;

            // check app.config for GetTimePeriod, if it is not found
            // in the app.config default the value to 30000.
            int.TryParse(Config.AppSettings["GetTimePeriod"], out GetTimePeriod);
            GetTimePeriod = GetTimePeriod == 0 ? 30000 : GetTimePeriod;

            // check app.config for SetTimePeriod, if it is not found
            // in the app.config default the value to 30000.
            int.TryParse(Config.AppSettings["SetTimePeriod"], out SetTimePeriod);
            SetTimePeriod = SetTimePeriod == 0 ? 30000 : SetTimePeriod;

            // check app.config for MaxPWM, if it is not found
            // in the app.config default the value to 100.
            int.TryParse(Config.AppSettings["MaxPWM"], out MaxPWM);
            MaxPWM = MaxPWM == 0 ? 100 : MaxPWM;

            // check app.config for MinPWM, if it is not found
            // in the app.config default the value to 20.
            int.TryParse(Config.AppSettings["MinPWM"], out MinPWM);
            MinPWM = MinPWM == 0 ? 20 : MinPWM;

            // check app.config for StepPWM, if it is not found
            // in the app.config default the value to 10.
            int.TryParse(Config.AppSettings["StepPWM"], out StepPWM);
            StepPWM = StepPWM == 0 ? 10 : StepPWM;

            // check app.config for InputSensor, if it is not found
            // in the app.config default the value to 1.
            int.TryParse(Config.AppSettings["InputSensor"], out InputSensor);
            InputSensor = InputSensor == 0 ? 1 : InputSensor;

            // check app.config for SensorLowThreshold, if it is not found
            // in the app.config default the value to 0.
            int.TryParse(Config.AppSettings["SensorLowThreshold"], out SensorLowThreshold);
            SensorLowThreshold = SensorLowThreshold == 0 ? 0 : SensorLowThreshold;

            // check app.config for SensorHighThreshold, if it is not found
            // in the app.config default the value to 100.
            int.TryParse(Config.AppSettings["SensorHighThreshold"], out SensorHighThreshold);
            SensorHighThreshold = SensorHighThreshold == 0 ? 100 : SensorHighThreshold;

            // check app.config for AltitudeCorrectionFactor, if it is not found
            // in the app.config default the value to 0.032 (3.2%).
            float.TryParse(Config.AppSettings["AltitudeCorrectionFactor"], out AltitudeCorrectionFactor);
            AltitudeCorrectionFactor = AltitudeCorrectionFactor == 0 ? (float)0.032 : AltitudeCorrectionFactor;

            // check app.config for Altitude, if it is not found
            // in the app.config default the value to 0 (0 feet above sea level).
            int.TryParse(Config.AppSettings["Altitude"], out Altitude);
            Altitude = Altitude == 0 ? 0 : Altitude;

            // check app.config for MaxRetries, if it is not found
            // in the app.config default the value to 3.
            int.TryParse(Config.AppSettings["MaxRetries"], out MaxRetries);
            MaxRetries = MaxRetries == 0 ? 3 : MaxRetries;

            // check app.config for LEDHigh, if it is not found
            // in the app.config default the value to 255.
            int.TryParse(Config.AppSettings["LEDHigh"], out LEDHigh);
            LEDHigh = LEDHigh == 0 ? 255 : LEDHigh;

            // check app.config for LEDLow, if it is not found
            // in the app.config default the value to 0.
            int.TryParse(Config.AppSettings["LEDLow"], out LEDLow);
            LEDLow = LEDLow == 0 ? 0 : LEDLow;

            // check app.config for MinPowerLimit, if it is not found
            // in the app.config default the value to 120W.
            int.TryParse(Config.AppSettings["MinPowerLimit"], out MinPowerLimit);
            MinPowerLimit = MinPowerLimit == 0 ? 120 : MinPowerLimit;

            // check app.config for MaxPowerLimit, if it is not found
            // in the app.config default the value to 1000W.
            int.TryParse(Config.AppSettings["MaxPowerLimit"], out MaxPowerLimit);
            MaxPowerLimit = MaxPowerLimit == 0 ? 1000 : MaxPowerLimit;

            // check app.config for MaxFailCount, if it is not found
            // in the app.config default the value to 0.
            int.TryParse(Config.AppSettings["MaxFailCount"], out MaxFailCount);
            MaxFailCount = MaxFailCount == 0 ? 0 : MaxFailCount;

            // check app.config for CM Fru variables, if it is not found
            // in the app.config default the value FRU values found in Spec V1.0.
            int.TryParse(Config.AppSettings["CMStartingOffset"], out ChassisStartingOffset);
            ChassisStartingOffset = ChassisStartingOffset == 0 ? 0 : ChassisStartingOffset;

            int.TryParse(Config.AppSettings["CMFruLength"], out ChassisFruLength);
            ChassisFruLength = ChassisFruLength == 0 ? 256 : ChassisFruLength;

            int.TryParse(Config.AppSettings["InternalUseSize"], out InternalUseSize);
            InternalUseSize = InternalUseSize == 0 ? 72 : InternalUseSize;

            int.TryParse(Config.AppSettings["ChassisInfoSize"], out ChassisInfoSize);
            ChassisInfoSize = ChassisInfoSize == 0 ? 32 : ChassisInfoSize;

            int.TryParse(Config.AppSettings["BoardInfoSize"], out BoardInfoSize);
            BoardInfoSize = BoardInfoSize == 0 ? 64 : BoardInfoSize;

            int.TryParse(Config.AppSettings["ProductInfoSize"], out ProductInfoSize);
            ProductInfoSize = ProductInfoSize == 0 ? 80 : ProductInfoSize;

            // check app.config for Hard Disk Drive Sensor ids
            int.TryParse(Config.AppSettings["BladeDisk0"], out BladeDisk0);
            BladeDisk0 = BladeDisk0 == 0 ? 243 : BladeDisk0;

            int.TryParse(Config.AppSettings["BladeDisk1"], out BladeDisk1);
            BladeDisk1 = BladeDisk1 == 0 ? 244 : BladeDisk1;

            int.TryParse(Config.AppSettings["BladeDisk2"], out BladeDisk2);
            BladeDisk2 = BladeDisk2 == 0 ? 245 : BladeDisk2;

            int.TryParse(Config.AppSettings["BladeDisk3"], out BladeDisk3);
            BladeDisk3 = BladeDisk3 == 0 ? 246 : BladeDisk3;
            
            // check app.config for Health Diagnostics Sensor ids
            int.TryParse(Config.AppSettings["CPU0ProcHotSensor"], out CPU0ProcHotSensor);
            CPU0ProcHotSensor = CPU0ProcHotSensor == 0 ? 187 : CPU0ProcHotSensor;

            int.TryParse(Config.AppSettings["CPU1ProcHotSensor"], out CPU1ProcHotSensor);
            CPU1ProcHotSensor = CPU1ProcHotSensor == 0 ? 188 : CPU1ProcHotSensor;

            int.TryParse(Config.AppSettings["PCIeBusSensor"], out PCIeBusSensor);
            PCIeBusSensor = PCIeBusSensor == 0 ? 161 : PCIeBusSensor;

            int.TryParse(Config.AppSettings["SPSFWSensor"], out SPSFWSensor);
            SPSFWSensor = SPSFWSensor == 0 ? 23 : SPSFWSensor;

            // check app.config for MaxPortManagerWorkQueueLength, if it is not found
            // in the app.config default the value to 10.
            int.TryParse(Config.AppSettings["MaxPortManagerWorkQueueLength"], out MaxPortManagerWorkQueueLength);
            MaxPortManagerWorkQueueLength = MaxPortManagerWorkQueueLength == 0 ? 20 : MaxPortManagerWorkQueueLength;

            // check app.config for ServiceTimeoutInMinutes, if it is not found
            // in the app.config default the value to 2 minutes.
            double.TryParse(Config.AppSettings["ServiceTimeoutInMinutes"], out ServiceTimeoutInMinutes);
            ServiceTimeoutInMinutes = ServiceTimeoutInMinutes == 0 ? 2 : ServiceTimeoutInMinutes;

            // check app.config for CMServicePortNumber, if it is not found
            // in the app.config default the value to 8000.
            int.TryParse(Config.AppSettings["CmServicePortNumber"], out CmServicePortNumber);
            CmServicePortNumber = CmServicePortNumber < 1 ? 8000 : CmServicePortNumber;

            // check app.config for SslCertificateName, if not found
            // in the app.config default the value to "CMServiceServer".
            SslCertificateName = Config.AppSettings["SslCertificateName"].ToString();
            SslCertificateName = SslCertificateName == string.Empty ? @"CMServiceServer" : SslCertificateName;

            // check app.config for EnableSslEncryption, if not found
            // in the app.config default the value to true/enable.
            int tempSslEncrypt = 1;
            int.TryParse(Config.AppSettings["EnableSslEncryption"], out tempSslEncrypt);
            EnableSslEncryption = tempSslEncrypt == 0 ? false : true;

            // Check App.config for KillSerialConsoleSession, if not found, default to true/enable.
            int tempKillSerialConsoleSession = 1;
            int.TryParse(Config.AppSettings["KillSerialConsoleSession"], out tempKillSerialConsoleSession);
            KillSerialConsoleSession = tempKillSerialConsoleSession == 0 ? false : true;
            
            // Check App.config for EnableFan, if not found, default to true/enable.
            int tempEnableFan = 1;
            int.TryParse(Config.AppSettings["EnableFan"], out tempEnableFan);
            EnableFan = tempEnableFan == 0 ? false : true;
            
            // check app.config for EventLogXml, if not found
            // in the app.config default the value to EventDataStrings.xml
            string evtLogFile = string.Empty;
            evtLogFile = Config.AppSettings["EventLogXml"].ToString();
            evtLogFile = evtLogFile == string.Empty ? @"EventDataStrings.xml" : evtLogFile;

            // format event log strings dictionary
            Dictionary<string, string> formatStrings = new Dictionary<string, string>();

            try
            {
                FileStream fs = new FileStream(evtLogFile, FileMode.Open, FileAccess.Read);

                XmlDocument XmlEventLogDoc = new XmlDocument();
                
                // load xml document
                XmlEventLogDoc.Load(fs);

                // convert xml document into class objects
                EventStrings = XmlToObject("EventLogTypeCode", XmlEventLogDoc);

                // populate format event log strings dictionary
                XmlFormatStrings("EventLogStrings", formatStrings, XmlEventLogDoc);

            }
            catch (System.Exception ex)
            {
                Tracer.WriteWarning(string.Format("ERROR: Could not load Event Log Strings from {0}", evtLogFile));
                Tracer.WriteError(ex);

                // set event strings to default empty list.
                EventStrings = new List<EventLogData>();
            }

            if (formatStrings.ContainsKey("ErrorCode"))
            {
                EventLogStrError = formatStrings["ErrorCode"].ToString();
            }
            else
            {
                EventLogStrError = string.Empty;
            }

            if (formatStrings.ContainsKey("Separator"))
            {
                EventLogStrSeparator = formatStrings["Separator"].ToString();
            }
            else
            {
                EventLogStrSeparator = string.Empty;
            }

            if (formatStrings.ContainsKey("Space"))
            {
                EventLogStrSpacer = formatStrings["Space"].ToString();
            }
            else
            {
                EventLogStrSpacer = string.Empty;
            }

            if (formatStrings.ContainsKey("SensorType"))
            {
                EventLogStrSensor = formatStrings["SensorType"].ToString();
            }
            else
            {
                EventLogStrSensor = string.Empty;
            }

            if (formatStrings.ContainsKey("Unknown"))
            {
                Unknown = formatStrings["Unknown"].ToString();
            }
            else
            {
                Unknown = string.Empty;
            }    

        }

        #region EventLog

        private static Dictionary<byte, byte> MemoryMap = new Dictionary<byte,byte>(12){
            {0x01, 0xA1},
            {0x02, 0xA2},
            {0x03, 0xB1},
            {0x04, 0xB2},
            {0x05, 0xC1},
            {0x06, 0xC2},
            {0x07, 0xD1},
            {0x08, 0xD2},
            {0x09, 0xE1},
            {0x0A, 0xE2},
            {0x0B, 0xF1},
            {0x0C, 0xF2}
        };

        private static List<EventLogData> XmlToObject(string xmlTag, XmlDocument xmlEventLogDoc)
        {
            List<EventLogData> response = new List<EventLogData>();

            try
            {
                XmlNodeList rootNodeList = xmlEventLogDoc.GetElementsByTagName(xmlTag);

                if (rootNodeList.Count > 0)
                {
                    // root level node: EventLogTypeCode
                    foreach (XmlNode root in rootNodeList)
                    {
                        // GenericEvent/SensorSpecificEvent
                        foreach (XmlNode node in root.ChildNodes)
                        {
                            EventLogMsgType clasification = GetEventLogClass(node.Name.ToString());

                            XmlNodeList firstTierNodes = node.ChildNodes;

                            // enumerate first level child nodes
                            foreach (XmlNode firstNode in firstTierNodes)
                            {
                                int number = Convert.ToInt32(firstNode.Attributes["Number"].Value.ToString());
                                string description = firstNode.Attributes["ReadingClass"].Value.ToString();

                                XmlNodeList secondTierNodes = firstNode.ChildNodes;

                                // enumerate second level xml nodes
                                foreach (XmlNode secondNode in secondTierNodes)
                                {
                                    int offset = Convert.ToInt32(secondNode.Attributes["Number"].Value.ToString());
                                    string message = secondNode.Attributes["Description"].Value.ToString();

                                    EventLogData respObj = new EventLogData(number, offset, clasification, message, description);

                                    XmlNodeList thirdTierNodes = secondNode.ChildNodes;

                                    if (thirdTierNodes.Count > 0)
                                    {
                                        // enumerate third level xml nodes
                                        foreach (XmlNode extension in thirdTierNodes)
                                        {
                                            int id = Convert.ToInt32(extension.Attributes["Number"].Value.ToString());
                                            string detail = extension.Attributes["Description"].Value.ToString();

                                            respObj.AddExtension(id, detail);
                                        } // enumerate third level xml nodes
                                    }

                                    response.Add(respObj);
                                } // enumerate second level xml nodes
                            } // enumerate first level child nodes
                        } // GenericEvent/SensorSpecificEvent
                    }
                }
                else
                {
                    Tracer.WriteWarning("ERROR: Could not load Event Log Strings, could not find xml root node in file");
                }
            }
            catch (Exception ex)
            {
                Tracer.WriteError(string.Format("ERROR: Could not load Event Log Strings. Error: {0}", ex.ToString()));
            }

            return response;
        }

        private static void XmlFormatStrings(string xmlTag, Dictionary<string, string> formatStrings, XmlDocument xmlEventLogDoc)
        {
            try
            {
                XmlNodeList rootNodeList = xmlEventLogDoc.GetElementsByTagName(xmlTag);

                // root level node: EventLogStrings
                foreach (XmlNode root in rootNodeList)
                {
                    // EventString
                    foreach (XmlNode node in root.ChildNodes)
                    {
                        string key = node.Attributes["Name"].Value.ToString();
                        string val = node.Attributes["String"].Value.ToString();
                        formatStrings.Add(key, val);
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.WriteError(string.Format("ERROR: Could not load Event Log Format Strings. Error: {0}", ex.ToString()));
            }

        }

        private static EventLogMsgType GetEventLogClass(string name)
        {
            if (name.ToLowerInvariant() == "genericevent")
                return EventLogMsgType.Discrete;
            else if (name.ToLowerInvariant() == "sensorspecificevent")
                return EventLogMsgType.SensorSpecific;
            else if (name.ToLowerInvariant() == "oemevent")
                return EventLogMsgType.Oem;
            else if (name.ToLowerInvariant() == "oemtimestampedevent")
                return EventLogMsgType.OemTimestamped;
            else if (name.ToLowerInvariant() == "oemnontimestampedevent")
                return EventLogMsgType.OemNonTimeStamped;
            else
                return EventLogMsgType.Unspecified;
        }

        internal static EventLogData GetEventLogData(Ipmi.EventLogMsgType eventType, int number, int offset)
        {
            EventLogData logDataQuery =
            (from eventLog in EventStrings
             where eventLog.Number == number
             && eventLog.OffSet == offset
             && eventLog.MessageClass == eventType
             select eventLog).FirstOrDefault();

            if (logDataQuery != null)
            {
                // Create new instance of EventLogData to avoid overwriting EventMessage and Description 
                EventLogData tempData = new EventLogData(logDataQuery.Number, logDataQuery.OffSet, logDataQuery.MessageClass,
                    string.Copy(logDataQuery.EventMessage), string.Copy(logDataQuery.Description));
                return tempData;
            }
            else
            {
                return new EventLogData();
            }
        }

        /// <summary>
        /// Returns the actual DIMM Number of WCS blades
        /// </summary>
        /// <param name="dimm">Dimm Index</param>
        /// <returns>WCS DIMM Name</returns>
        internal static byte GetDimmNumber(byte dimm)
        {
            if (MemoryMap.ContainsKey(dimm))
                return MemoryMap[dimm];
            else
                return 0xFF;
        }

        #endregion

    }
}
