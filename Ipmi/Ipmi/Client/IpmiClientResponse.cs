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
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public abstract class ResponseBase
    {
        // ipmi completion code
        private byte _completionCode;

        /// <summary>
        /// Completion Code
        /// </summary>
        public byte CompletionCode
        {
            get { return this._completionCode; }
            internal set { this._completionCode = value; }
        }

        internal abstract void SetParamaters(byte[] param);
    
    }

    /// <summary>
    /// Numeric Sensor Reading Class
    /// </summary>
    public class SensorReading : ResponseBase
    {
        /// <summary>
        /// raw reading byte
        /// </summary>
        private byte _rawreading = 0;

        /// <summary>
        /// sensor thresholds
        /// </summary>
        private bool _hasThresholds = false;

        /// <summary>
        /// converted reading
        /// </summary>
        private double _converted = 0;

        /// <summary>
        /// Sensor Number
        /// </summary>
        private byte _sensorNumber;

        /// <summary>
        /// Sensor type
        /// </summary>
        private byte _sensorType;

        /// <summary>
        /// sensor state byte.
        /// </summary>
        private byte _eventState;

        /// <summary>
        /// sensor event state desription.
        /// </summary>
        private string _eventStateDesc;

        /// <summary>
        /// sensor event state extension.
        /// </summary>
        private byte _eventStateExtension;

        /// <summary>
        /// upper non-recoverable threshold value
        /// </summary>
        private double _thresholdUpperNonRecoverable;

        /// <summary>
        /// upper critical threshold value
        /// </summary>
        private double _thresholdUppercritical;

        /// <summary>
        /// upper non-critical threshold value
        /// </summary>
        private double _thresholdUpperNoncritical;

        /// <summary>
        /// lower non-recoverable threshold value
        /// </summary>
        private double _thresholdLowerNonRecoverable;

        /// <summary>
        /// lower critical threshold value
        /// </summary>
        private double _thresholdLowercritical;

        /// <summary>
        /// lower non-critical threshold value
        /// </summary>
        private double _thresholdLowerNoncritical;

        /// <summary>
        /// Sensor Description
        /// </summary>
        private string _description = string.Empty;

        /// <summary>
        /// Initialize class
        /// </summary>
        public SensorReading(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            SensorReadingResponse response =
                (SensorReadingResponse)IpmiSharedFunc.ConvertResponse(data, typeof(SensorReadingResponse));

            this.SetParamaters(response.SensorReading, response.SensorStatus, response.StateOffset, response.OptionalOffset);
        
        }


        internal void SetParamaters(byte reading, byte status, byte state, byte optionalState)
        {

            if (base.CompletionCode == 0x00)
            {

                // set the raw sensor reading
                this.SetReading(reading);

                byte[] statusByteArray = new byte[1];
                statusByteArray[0] = status;

                BitArray sensorStatusBitArray = new BitArray(statusByteArray);
                bool eventMsgEnabled = sensorStatusBitArray[7];
                bool sensorScanEnabled = sensorStatusBitArray[6];
                bool readingUnavailable = sensorStatusBitArray[5];
                this.EventState = state;
                this.EventStateExtension = optionalState;
            }
        
        
        }

        /// <summary>
        /// Convert Sensor Reading into Converted Reading
        /// </summary>
        public void ConvertReading(SensorMetadataBase sdr)
        {
            if (sdr != null)
            {
                if (sdr.GetType() == typeof(FullSensorRecord))
                {
                    FullSensorRecord record = (FullSensorRecord)sdr;

                    if (record.IsNumeric)
                    {
                        this._converted = record.ConvertReading(_rawreading);

                        if (record.ThresholdReadable > 0)
                            _hasThresholds = true;

                        SetThresholds(record);
                    }
                }
            }
        }

        /// <summary>
        /// Convert Sensor Reading into Converted using supplied factors
        /// </summary>
        public void ConvertReading(byte[] factors, FullSensorRecord sdr)
        {
            if(sdr != null)
            {
                if (sdr.IsNumeric)
                {
                    this._converted = sdr.ConvertReading(this._rawreading, factors);

                    if (sdr.ThresholdReadable > 0)
                        _hasThresholds = true;

                    SetThresholds(sdr);
                }
            }
        }

        /// <summary>
        /// Set Upper Thresholds
        /// </summary>
        private void SetThresholds(FullSensorRecord sdr)
        {     
            // Upper thresholds
            this._thresholdUpperNonRecoverable = sdr.ThresholdUpperNonRecoverable;
            this._thresholdUppercritical = sdr.ThresholdUpperCritical;
            this._thresholdUpperNoncritical = sdr.ThresholdUpperNonCritical;
            // Lower thresholds
            this._thresholdLowerNonRecoverable = sdr.ThresholdLowerNonRecoverable;
            this._thresholdLowercritical = sdr.ThresholdLowerCritical;
            this._thresholdLowerNoncritical = sdr.ThresholdLowerNonCritical;
        }

        /// <summary>
        /// Set the Raw Reading byte
        /// </summary>
        public void SetReading(byte rawReading)
        {
            this._rawreading = rawReading;
        }

        /// <summary>
        /// Set the Event State
        /// </summary>
        public void SetEventState(byte eventState)
        {
            this._eventState = eventState;
        }

        /// <summary>
        /// Sensor Reading
        /// </summary>
        public double Reading
        {
            get { return this._converted; }
        }

        /// <summary>
        /// Raw Analog Byte
        /// </summary>
        public byte RawReading
        {
            get { return this._rawreading; }
            internal set { this._rawreading = value; }
        }

        /// <summary>
        /// Sensor State Description
        /// </summary>
        public string EventDescription
        {
            get { return this._eventStateDesc; }
            internal set { this._eventStateDesc = value; }
        }

        /// <summary>
        /// Sensor State
        /// </summary>
        public byte EventState
        {
            get { return this._eventState; }
            internal set { this._eventState = value; }
        }

        /// <summary>
        /// Sensor State Option Extension
        /// </summary>
        public byte EventStateExtension
        {
            get { return this._eventStateExtension; }
            internal set { this._eventStateExtension = value; }
        }

        /// <summary>
        /// upper non-recoverable threshold value
        /// </summary>
        public double ThresholdUpperNonRecoverable
        {
            get { return this._thresholdUpperNonRecoverable; }
        }

        /// <summary>
        /// upper critical threshold value
        /// </summary>
        public double ThresholdUpperCritical
        {
            get { return this._thresholdUppercritical; }
        }

        /// <summary>
        /// upper non-critical threshold value
        /// </summary>
        public double ThresholdUpperNoncritical
        {
            get { return this._thresholdUpperNoncritical; }
        }

        /// <summary>
        /// lower non-recoverable threshold value
        /// </summary>
        public double ThresholdLowerNonRecoverable
        {
            get { return this._thresholdLowerNonRecoverable; }
        }

        /// <summary>
        /// lower critical threshold value
        /// </summary>
        public double ThresholdLowerCritical
        {
            get { return this._thresholdLowercritical; }
        }

        /// <summary>
        /// lower non-critical threshold value
        /// </summary>
        public double ThresholdLowerNoncritical
        {
            get { return this._thresholdLowerNoncritical; }
        }

        /// <summary>
        /// Indicates sensor has thresholds values
        /// </summary>
        public bool HasThreasholds
        {
            get { return this._hasThresholds; }
            internal set { this._hasThresholds = value; }
        }

        /// <summary>
        /// Sensor Number
        /// </summary>
        public byte SensorNumber
        {
            get { return this._sensorNumber; }
            internal set { this._sensorNumber = value; }
        }

        /// <summary>
        /// Sensor Type
        /// </summary>
        public byte SensorType
        {
            get { return this._sensorType; }
            internal set {this._sensorType = value;}
        }

        /// <summary>
        /// Sensor Description
        /// </summary>
        public string Description
        {
            get { return this._description; }
            internal set {this._description = value;}
        }
    }

    /// <summary>
    /// Sensor Type Code Class
    /// </summary>
    public class SensorTypeCode : ResponseBase
    {
        /// <summary>
        /// Sensor type
        /// </summary>
        private byte _sensorType;

        /// <summary>
        /// sensor event type Code.
        /// </summary>
        private byte _typeCode;

        /// <summary>
        /// Initialize class
        /// </summary>
        public SensorTypeCode(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            SensorTypeResponse response =
                (SensorTypeResponse)IpmiSharedFunc.ConvertResponse(data, typeof(SensorTypeResponse));

            this.SetParamaters(response.SensorType, response.EventTypeCode);

        }


        internal void SetParamaters(byte sensorType, byte eventTypeCode)
        {

            if (base.CompletionCode == 0x00)
            {
                this._sensorType = sensorType;
                this._typeCode = eventTypeCode;
            }


        }

        /// <summary>
        /// Sensor Event Reading Type Code
        /// </summary>
        public byte EventTypeCode
        {
            get { return this._typeCode; }
            internal set { this._typeCode = value; }
        }

        /// <summary>
        /// Sensor Type
        /// </summary>
        public byte SensorType
        {
            get { return this._sensorType; }
            internal set { this._sensorType = value; }
        }
    }

    /// <summary>
    /// Response to User Name Command
    /// </summary>
    public class UserName : ResponseBase
    {
        private string _userName;

        public UserName(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            // Trim padded '\0' from User Name
            this.TextName = System.Text.ASCIIEncoding.ASCII.GetString(data).TrimEnd('\0');        
        }

        /// <summary>
        /// User Name
        /// </summary>
        public string TextName
        {
            get { return this._userName; }
            set { this._userName = value; }
        }

    }

    /// <summary>
    /// Response to Device Guid Command
    /// </summary>
    public class DeviceGuid : ResponseBase
    {
        private Guid _guid;

        public DeviceGuid(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] param)
        {
            if (base.CompletionCode == 0x00)
            {
                this.Guid = new Guid(param);
            }
        
        }

        /// <summary>
        /// Device Guid
        /// </summary>
        public Guid Guid
        {
            get { return this._guid; }
            private set { this._guid = value; }
        }

    }

    /// <summary>
    /// Response to BMC Firmware Command
    /// </summary>
    public class BmcFirmware : ResponseBase
    {
        private string _firmware;

        public BmcFirmware(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            if (base.CompletionCode == 0x00)
            {
                // major firmware version [6:0] + . +  minor firmware version
                this.SetParamaters(data[0], data[1]);
            }
        }

        internal void SetParamaters(byte major, byte minor)
        {
            if (base.CompletionCode == 0x00)
            {
                // major firmware version [6:0] + . +  minor firmware version
                this.Firmware = (major & 0x7F).ToString("X2", CultureInfo.InvariantCulture) +
                    Convert.ToChar(0x2E) + (minor).ToString("X2", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// BMC Firmware
        /// </summary>
        public string Firmware
        {
            get { return this._firmware; }
            set { this._firmware = value; }
        }
    }

    /// <summary>
    /// Get Device Id command response
    /// </summary>
    public class BmcDeviceId : ResponseBase
    {
        private string _firmware;

        private int _manufactureId;

        private short _productId;

        public BmcDeviceId(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            GetDeviceIdResponse response =
                (GetDeviceIdResponse)IpmiSharedFunc.ConvertResponse(data, typeof(GetDeviceIdResponse));

            this.SetParamaters(response.MajorFirmware, response.MinorFirmware, 
                response.ManufactureId, response.ProductId);
        }

        internal void SetParamaters(byte major, byte minor, byte[] manufacture, byte[] productId)
        {
                // major firmware version [6:0] + . +  minor firmware version
                this.Firmware = (major & 0x7F).ToString("X2", CultureInfo.InvariantCulture) +
                    Convert.ToChar(0x2E) + (minor).ToString("X2", CultureInfo.InvariantCulture);

                // convert 3 byte oem id into integer using bitwise operation
                this.ManufactureId = ((manufacture[0] << 0) + (manufacture[1] << 8) + (manufacture[2] << 16));

                this.ProductId = BitConverter.ToInt16(productId, 0);       
        }

        /// <summary>
        /// BMC Firmware
        /// </summary>
        public string Firmware
        {
            get { return this._firmware; }
            private set { this._firmware = value; }
        }

        /// <summary>
        /// BMC ManufactureId
        /// </summary>
        public int ManufactureId
        {
            get { return this._manufactureId; }
            private set { this._manufactureId = value; }
        }

        /// <summary>
        /// BMC Product Id
        /// </summary>
        public short ProductId
        {
            get { return this._productId; }
            private set { this._productId = value; }
        }
    }

    /// <summary>
    /// Response to System Power State
    /// </summary>
    public class SystemPowerState : ResponseBase
    {
        internal IpmiPowerState state;

        internal PowerRestoreOption powerOnPolicy;

        public SystemPowerState(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] param)
        { }

        /// <summary>
        /// System Power State
        /// </summary>
        public IpmiPowerState PowerState
        {
            get { return state; }
        }

        public PowerRestoreOption PowerOnPolicy
        {
            get { return this.powerOnPolicy; }
        }
    }

    /// <summary>
    /// Response to Get Chassis Status command
    /// </summary>
    public class SystemStatus : ResponseBase
    {
        // current power state
        private IpmiPowerState powerstate;

        // AC power restore policy
        private PowerRestoreOption powerOnPolicy;

        // previous power event cause
        private PowerEvent lastPowerEvent;

        // identity led supported (default = false)
        private bool identitySupported = false;
        
        // identity led state
        private IdentityState identityState;

        public SystemStatus(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            GetChassisStatusResponse response =
                (GetChassisStatusResponse)IpmiSharedFunc.ConvertResponse(data, typeof(GetChassisStatusResponse));

            if(base.CompletionCode == 0x00)
                this.SetParamaters(response.CurrentPowerState, response.LastPowerEvent, response.MiscellaneousChassisState);
        }

        internal void SetParamaters(byte currentPowerState, byte lastEvent, byte miscState)
        {
            if (base.CompletionCode == 0)
            {

                #region Power State

                // [0] Power is on, 1 = On, 0 = Off
                byte state = Convert.ToByte((currentPowerState & 0x01));

                // Translate the current power state into an enumeration.
                switch (state)
                {
                    case 0x00:
                        this.powerstate = IpmiPowerState.Off;
                        break;
                    case 0x01:
                        this.powerstate = IpmiPowerState.On;
                        break;
                    default:
                        this.powerstate = IpmiPowerState.Invalid;
                        break;
                }

                #endregion

                #region Power Policy

                state = Convert.ToByte((currentPowerState & 0x60) >> 5);

                // Translate the state into Power on Policy.
                switch (state)
                {
                    case 0x00:
                        this.powerOnPolicy = PowerRestoreOption.StayOff;
                        break;
                    case 0x01:
                        this.powerOnPolicy = PowerRestoreOption.PreviousState;
                        break;
                    case 0x02:
                        this.powerOnPolicy = PowerRestoreOption.AlwaysPowerUp;
                        break;
                    default:
                        this.powerOnPolicy = PowerRestoreOption.Unknown;
                        break;
                }

                #endregion

                #region Power Fault

                // [7:5] -  reserved
                // [4]   -   1b = last ‘Power is on’ state was entered via IPMI command 
                // [3]   -   1b = last power downcaused by power fault
                // [2]   -   1b = last power down caused by a power interlockbeing activated 
                // [1]   -   1b = last power down caused by a Power overload
                // [0]   -   1b = AC failed
                state = Convert.ToByte((lastEvent & 0x1F));

                switch (state)
                {
                    case 0x00:
                        this.lastPowerEvent = PowerEvent.ACfailed;
                        break;
                    case 0x01:
                        this.lastPowerEvent = PowerEvent.PowerOverload;
                        break;
                    case 0x02:
                        this.lastPowerEvent = PowerEvent.PowerInterlockActive;
                        break;
                    case 0x03:
                        this.lastPowerEvent = PowerEvent.PowerFault;
                        break;
                    case 0x04:
                        this.lastPowerEvent = PowerEvent.IpmiSetState;
                        break;
                    default:
                        this.lastPowerEvent = PowerEvent.Unknown;
                        break;
                }

                #endregion

                #region Identity LED

                // [7:4] -  reserved
                // [6] -    1b = Chassis Identify command and state info supported (Optional)
                //          0b = Chassis Identify command support unspecified via this 
                //          command.
                byte identitySupport = Convert.ToByte((miscState & 0x40) >> 6);

                if (identitySupport == 0x01)
                    this.identitySupported = true;

                // [5:4] -  Chassis Identify State.  Mandatory when bit [6] = 1b, reserved (return 
                // as 00b) otherwise.Returns the present chassis identify state. Refer to 
                // the Chassis Identify command for more info.
                // 00b = chassis identify state = Off
                // 01b = chassis identify state = Temporary (timed) On
                // 10b = chassis identify state = Indefinite On
                // 11b = reserved

                byte Identity = Convert.ToByte((miscState & 0x30) >> 4);

                switch (Identity)
                {
                    case 0x00:
                        this.identityState = IdentityState.Off;
                        break;
                    case 0x01:
                        this.identityState = IdentityState.TemporaryOn;
                        break;
                    case 0x02:
                        this.identityState = IdentityState.On;
                        break;
                    default:
                        this.identityState = IdentityState.Unknown;
                        break;
                }

                #endregion

            }
            else
            {
                this.powerstate = IpmiPowerState.Invalid;
            }          
        }

        /// <summary>
        /// System Power State
        /// </summary>
        public IpmiPowerState PowerState
        {
            get { return this.powerstate; }
            internal set { this.powerstate = value; }
        }

        /// <summary>
        /// AC Power Restore Policy
        /// </summary>
        public PowerRestoreOption PowerOnPolicy
        {
            get { return this.powerOnPolicy; }
        }

        /// <summary>
        /// Previous Power Down
        /// </summary>
        public PowerEvent LastPowerEvent
        {
            get { return this.lastPowerEvent; }
        }

        /// <summary>
        /// Chassis Identity LED State Supported
        /// </summary>
        public bool IdentitySupported
        {
            get { return this.identitySupported; }
        }

        /// <summary>
        /// Chassis Identity LED State
        /// </summary>
        public IdentityState IdentityState
        {
            get { return this.identityState; }
        }
    }

    /// <summary>
    /// Resposne to Power On Hours Command
    /// </summary>
    public class PowerOnHours : ResponseBase
    {
        private int _hours;

        public PowerOnHours(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            if (base.CompletionCode == 0)
            {
                this.Hours = BitConverter.ToInt32(data, 0);
            }
        }

        /// <summary>
        /// Power On Hours
        /// </summary>
        public int Hours
        {
            get { return this._hours; }
            private set { this._hours = value; }
        }

    }

    /// <summary>
    /// Response to User Privilege Command
    /// </summary>
    public class UserPrivilege : ResponseBase
    {
        private PrivilegeLevel _privilege;

        public UserPrivilege(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            Privilege = (PrivilegeLevel)Convert.ToInt32(data[0]);
        }

        /// <summary>
        /// Privilege Level
        /// </summary>
        public PrivilegeLevel Privilege
        {
            get { return this._privilege; }
            private set { this._privilege = value; }
        }

    }

    /// <summary>
    /// Properties in the Get Power Limit DCMI command response
    /// </summary>
    public class PowerLimit : ResponseBase
    {
        private bool _activeLimit;
        private short _limitValue;
        private short _samplingPeriod;
        private short _correctionAction;
        private short _rawCorrectionTime;
        private TimeSpan _correctionTime;

        public PowerLimit(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        /// <summary>
        /// Convert Ipmi Response data into the Response object
        /// </summary>
        internal override void SetParamaters(byte[] data)
        {
            GetDcmiPowerLimitResponse response = 
                (GetDcmiPowerLimitResponse)IpmiSharedFunc.ConvertResponse(data, typeof(GetDcmiPowerLimitResponse));

            this.SetParamaters(response.PowerLimit, response.SamplingPeriod, response.ExceptionActions, response.CorrectionTime);
        }

        /// <summary>
        /// Set the Class Paramaters
        /// </summary>
        internal void SetParamaters(byte[] powerlimit, byte[] samplingPeriod, byte exceptionActions, byte[] correctionTime)
        {
            if (this.CompletionCode == 0)
            {
                // power limit in watts
                this._limitValue = BitConverter.ToInt16(powerlimit, 0);

                // sampling period in seconds
                this._samplingPeriod = BitConverter.ToInt16(samplingPeriod, 0);

                // exception action (actions, taken if the Power limit exceeded and cannot be 
                // controlled within the correction time limit)
                this._correctionAction = Convert.ToInt16(exceptionActions);

                // time span of correction time.  value given in ms, convert to days, hours, minutes, seconds, milliseconds
                this._correctionTime = new TimeSpan(0, 0, 0, 0, BitConverter.ToInt16(correctionTime, 0));

                this._rawCorrectionTime = BitConverter.ToInt16(correctionTime, 0);

                // no exception indicates there is a limit
                // 0x80 indicates there is no limit applied
                this._activeLimit = true;
            }
            else
            {
                // ipmi response exception 80 = no power limit set.
                if (this.CompletionCode == 0x80)
                {
                    // set the default value if no limit is set
                    short zero = 0;

                    // 0x80 indicates there is no limit applied
                    this._activeLimit = false;

                    // power limit in watts
                    this._limitValue = zero;

                    // sampling period in seconds
                    this._samplingPeriod = zero;

                    // exception action (actions, taken if the Power limit exceeded and cannot be 
                    // controlled within the correction time limit)
                    this._correctionAction = zero;

                    // time span of correction time.  value given in ms, convert to days, hours, minutes, seconds, milliseconds
                    this._correctionTime = new TimeSpan(zero);

                    // swtich completion code to zero. we do not want
                    // to report No Active Limit Set as failure.
                    base.CompletionCode = 0;
                }
            }       
        }

        /// <summary>
        /// Indicates whether a system power limit is active
        /// </summary>
        public bool ActiveLimit
        {
            get { return this._activeLimit; }
        }

        /// <summary>
        /// Provides the power limit reading in watts
        /// </summary>
        public short LimitValue
        {
            get { return this._limitValue; }
        }

        /// <summary>
        /// The system statistics sampling period
        /// </summary>
        public short SamplingPeriod
        {
            get { return this._samplingPeriod; }
        }

        /// <summary>
        /// The time allowed for the system to enfoce a power limit
        /// before corrective action is taken
        /// </summary>
        public TimeSpan CorrectionTime
        {
            get { return this._correctionTime; }
        }

        /// <summary>
        /// The time allowed for the system to enfoce a power limit
        /// before corrective action is taken
        /// </summary>
        public short RawCorrectionTime
        {
            get { return this._rawCorrectionTime; }
        }

        /// <summary>
        /// Action taken should the system fail to enfoce a power limit
        /// within the correction time:
        /// 0    = No Action
        /// 1    = Shutdown system
        /// 2-10 = OEM defined actions
        /// </summary>
        public short CorrectionAction
        {
            get { return this._correctionAction; }
        }
    }

    /// <summary>
    /// Properties in the Activate Power Limit DCMI command.
    /// </summary>
    public class ActivePowerLimit : ResponseBase
    {
        private bool limitSet;

        /// <summary>
        /// Properties in the Activate Power Limit DCMI command.
        /// </summary>
        public ActivePowerLimit(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            if (base.CompletionCode == 0)
                this.limitSet = true;
            else
                this.limitSet = false;

        }

        /// <summary>
        /// Indicates the power limit has been succesfully set
        /// </summary>
        public bool LimitSet
        {
            get { return this.limitSet; }
        }
    }

    /// <summary>
    /// Properties in the Get Power Reading DCMI command response
    /// </summary>
    public class PowerReading : ResponseBase
    {
        private bool powerSupport;
        private short present;
        private short maximum;
        private short minimum;
        private short average;
        private short timeUnit;
        private int timeNumber;
        private uint statistics;

        /// <summary>
        /// Properties in the Get Power Reading DCMI command response
        /// </summary>
        public PowerReading(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            if (base.CompletionCode == 0)
            {
                // system does support power readings
                this.PowerSupport = true;

                // Current power reading (DCMI spec)
                this.present = BitConverter.ToInt16(data, 0);
                // Minimum power reading (DCMI spec)
                this.minimum = BitConverter.ToInt16(data, 2);
                // Maximum power reading (DCMI spec)
                this.maximum = BitConverter.ToInt16(data, 4);
                // Average power reading (DCMI spec)
                this.average = BitConverter.ToInt16(data, 6);

                // No rolling averages are supplied with standard power statistics 
                // instead the statistics reporting time period is converted to 
                // minutes and used in place of the rolling average.
                this.timeNumber = unchecked(((BitConverter.ToInt32(data, 8)) / 1000) / 60);

                // set time sample to minutes
                this.timeUnit = 1;

                this.statistics = BitConverter.ToUInt32(data, 8);
            }
        }

        /// <summary>
        /// Indicates whether power readings are supported
        /// on the platform
        /// </summary>
        public bool PowerSupport
        {
            get { return this.powerSupport; }
            internal set { this.powerSupport = value; }
        }

        /// <summary>
        /// Present system level power reading in watts
        /// </summary>
        public short Present
        {
            get { return this.present; }
        }

        /// <summary>
        /// Maximum system level power reading in watts
        /// over the given sampling period
        /// </summary>
        public short Maximum
        {
            get { return this.maximum; }
        }

        /// <summary>
        /// Minimum system level power reading in watts
        /// over the given sampling period
        /// </summary>
        public short Minimum
        {
            get { return this.minimum; }
        }

        /// <summary>
        /// Average system level power reading in watts
        /// over the given sampling period
        /// </summary>
        public short Average
        {
            get { return this.average; }
        }

        /// <summary>
        /// Sampling time unit values:
        /// 0 = Seconds
        /// 1 = Minutes
        /// 2 = Hours
        /// 4 = Days
        /// </summary>
        public short TimeUnit
        {
            get { return this.timeUnit; }
            internal set { this.timeUnit = value; }
        }

        /// <summary>
        /// Samping time number is the total count of time units
        /// in the samping period
        /// </summary>
        public int TimeNumber
        {
            get { return this.timeNumber; }
            internal set { this.timeNumber = value; }
        }

        /// <summary>
        /// Raw Reading Statistics
        /// </summary>
        public uint Statistics
        {
            get { return this.statistics; }
        }

    }

    /// <summary>
    /// Resposne to Get Next Boot Command
    /// </summary>
    public class NextBoot : ResponseBase
    {
        private BootType _bootDevice = BootType.Unknown;

        public NextBoot(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            if (base.CompletionCode == 0)
            {
                byte flags = (byte)(((byte)(data[1] & 0x3C)) >> 2);

                switch (flags)
                {
                    case 0:
                        _bootDevice = BootType.NoOverride;
                        break;
                    case 1:
                        _bootDevice = BootType.ForcePxe;
                        break;
                    case 2:
                        _bootDevice = BootType.ForceDefaultHdd;
                        break;
                    case 3:
                        _bootDevice = BootType.ForceDefaultHddSafeMode;
                        break;
                    case 4:
                        _bootDevice = BootType.ForceDefaultDiagPartition;
                        break;
                    case 5:
                        _bootDevice = BootType.ForceDefaultDvd;
                        break;
                    case 6:
                        _bootDevice = BootType.ForceIntoBiosSetup;
                        break;
                    case 15:
                        _bootDevice = BootType.ForceFloppyOrRemovable;
                        break;
                    default:
                        _bootDevice = BootType.Unknown;
                        break;
                }
            }        
        
        }

        /// <summary>
        /// Boot Device
        /// </summary>
        public BootType BootDevice
        {
            get { return this._bootDevice; }
            internal set { this._bootDevice = value; }
        }

    }

    /// <summary>
    /// Response to Write Fru Data Command.
    /// </summary>
    public class WriteFruDevice : ResponseBase
    {
        private byte bytesWritten;

        /// <summary>
        /// Write Fru Data Command Response
        /// </summary>
        public WriteFruDevice(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            if (base.CompletionCode == 0)
                bytesWritten = data[0];
        }

        /// <summary>
        /// Indicates the number of bytes written
        /// </summary>
        public byte BytesWritten
        {
            get { return this.bytesWritten; }
        }

    }

    /// <summary>
    /// Properties in the Get/Set Serial Mux command response
    /// </summary>
    public class SerialMuxSwitch : ResponseBase
    {
        /// <summary>
        /// [7] -  	0b = requests to switch mux to system are allowed 
        ///         1b = requests to switch mux to system are blocked 
        /// </summary>
        private bool _muxSwitchAllowed = false;

        /// <summary>
        /// [6] -  	0b = requests to switch mux to BMC are allowed 
        ///         1b = requests to switch mux to BMC are blocked 
        /// </summary>
        private bool _requestToBmcAllowed = false;

        /// <summary>
        /// [3] -  	0b = no alert presently in progress 
        ///         1b = alert in progress on channel 
        /// </summary>
        private bool _alertInProgress = false;

        /// <summary>
        /// [2] -  	0b = no IPMI or OEM messaging presently active on channel 
        ///         1b = IPMI or OEM messaging session active on channel 
        /// </summary>
        private bool _messagingActive = false;

        /// <summary>
        /// [1] -  	0b = request was rejected 
        ///         1b = request was accepted (see note, below) or switch was forced 
        /// </summary>
        private bool _requestAccepted = false;

        /// <summary>
        /// [0] -  	0b = mux is set to system (system can transmit and receive) 
        ///         1b = mux is set to BMC  (BMC can transmit. System can neither 
        ///         transmit nor receive) 
        /// </summary>
        private bool _muxSetToSystem = false;

        /// <summary>
        /// Properties in the Get/Set Serial Mux command response
        /// </summary>
        public SerialMuxSwitch(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        { 
            SetSerialMuxResponse response =
            (SetSerialMuxResponse)IpmiSharedFunc.ConvertResponse(data, typeof(SetSerialMuxResponse));

            if (response.CompletionCode == 0)
            {
                response.GetMux();

                this.SetParamaters(
                        response.AlertInProgress,
                        response.MessagingActive,
                        response.MuxSetToSystem,
                        response.MuxSwitchAllowed,
                        response.RequestAccepted,
                        response.RequestToBmcAllowed
                        );
            }
        }

        internal void SetParamaters(bool alertInProgress, bool messagingActive, bool muxSetToSystem,
            bool muxSwitchAllowed, bool requestAccepted, bool requestToBmcAllowed)
        {
            this._alertInProgress = alertInProgress;
            this._messagingActive = messagingActive;
            this._muxSetToSystem = muxSetToSystem;
            this._muxSwitchAllowed = muxSwitchAllowed;
            this._requestAccepted = requestAccepted;
            this._requestToBmcAllowed = requestToBmcAllowed;
        }

        /// <summary>
        /// false = requests to switch mux to system are allowed 
        /// true = requests to switch mux to system are blocked 
        /// </summary>
        public bool MuxSwitchAllowed
        {
            get { return this._muxSwitchAllowed; }
        }

        /// <summary>
        /// false = requests to switch mux to BMC are allowed 
        /// true =  requests to switch mux to BMC are blocked 
        /// </summary>
        public bool RequestToBmcAllowed
        {
            get { return this._requestToBmcAllowed; }
        }

        /// <summary>
        /// false = no alert presently in progress 
        /// true =  alert in progress on channel 
        /// </summary>
        public bool AlertInProgress
        {
            get { return this._alertInProgress; }
        }

        /// <summary>
        /// false = no IPMI or OEM messaging presently active on channel 
        /// true =  IPMI or OEM messaging session active on channel 
        /// </summary>
        public bool MessagingActive
        {
            get { return this._messagingActive; }
        }

        /// <summary>
        /// false = request was rejected 
        /// true =  request was accepted (see note, below) or switch was forced 
        /// </summary>
        public bool RequestAccepted
        {
            get { return this._requestAccepted; }
        }

        /// <summary>
        /// false = mux is set to system (system can transmit and receive) 
        /// true =  mux is set to BMC  (BMC can transmit. System can neither 
        ///         transmit nor receive) 
        /// </summary>
        public bool MuxSetToSystem
        {
            get { return this._muxSetToSystem; }
        }
    }

    /// <summary>
    /// Properties in the Get JBOD Disk Status command response
    /// </summary>
    public class DiskStatusInfo : ResponseBase
    {
        // indicates JBOD disk channel
        private byte _channel;

        // indicates JBOD disk count
        private byte _diskcount;

        // indicates disks Id and disk status
        private Dictionary<byte, DiskStatus> disks = new Dictionary<byte, DiskStatus>();

        /// <summary>
        /// Properties in the Get Disk Status command response
        /// </summary>
        public DiskStatusInfo(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            GetDiskStatusResponse response =
            (GetDiskStatusResponse)IpmiSharedFunc.ConvertResponse(data, 
            typeof(GetDiskStatusResponse));

            if (response.CompletionCode == 0)
            {
                this.SetParamaters(response.Channel, response.DiskCount,
                    response.StatusData);
            }
        }

        internal void SetParamaters(byte channel, byte diskcount, byte[] diskInfo)
        {
            if (base.CompletionCode == 0)
            {
                this._channel = channel;
                this._diskcount = diskcount;

                foreach (byte disk in diskInfo)
                {
                    // Get the status byte
                    int status = (int)((disk & 0xC0) >> 6);

                    // initialize the disk status to unknown
                    DiskStatus diskStatus = DiskStatus.Unknown;

                    // change the disk status if it is in the enum.
                    if (Enum.IsDefined(typeof(DiskStatus), status))
                    {
                        diskStatus = (DiskStatus)status;
                    }

                    // add the disk status to the response list
                    this.disks.Add((byte)(disk & 0x3F), diskStatus);
                }
            }
        
        }

        /// <summary>
        /// JBOD Disk Channel
        /// </summary>
        public byte Channel
        {
            get { return this._channel; }
        }

        /// <summary>
        /// JBOD Disk Count
        /// </summary>
        public byte DiskCount
        {
            get { return this._diskcount; }
        }

        /// <summary>
        /// Indicates Disk Status
        /// </summary>
        public Dictionary<byte, DiskStatus> DiskState
        {
            get { return this.disks; }
        }
    }

    /// <summary>
    /// Properties in the Get JBOD Disk Status command response
    /// </summary>
    public class DiskInformation : ResponseBase
    {
        // indicates JBOD unit of measurement
        private SensorUnitTypeCode _unit = SensorUnitTypeCode.Unspecified;

        // indicates JBOD disk count
        private string _reading = string.Empty;

        /// <summary>
        /// Properties in the Get Disk information command response
        /// </summary>
        public DiskInformation(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            GetDiskInfoResponse response =
            (GetDiskInfoResponse)IpmiSharedFunc.ConvertResponse(data, 
            typeof(GetDiskInfoResponse));

            if (response.CompletionCode == 0)
            {
                this.SetParamaters(response.Unit, response.Multiplier, response.Reading);
            }

        }

        internal void SetParamaters(byte unit, byte multiplier, byte[] diskReading)
        {
            if (base.CompletionCode == 0)
            {
                // set the ipmi sensor unit type.
                if (Enum.IsDefined(typeof(SensorUnitTypeCode), unit))
                {
                    this._unit = (SensorUnitTypeCode)unit;
                }
                else
                {
                    this._unit = SensorUnitTypeCode.Unspecified;
                }

                bool negative = false;

                // check for negative reading
                if ((multiplier & 0x80) > 0)
                {
                    negative = true;
                }

                byte multiply = (byte)(multiplier & 0x7F);

                // zero multiplier is
                // not valid.
                if (multiply == 0)
                    multiply = 1;

                int reading = ((int)diskReading[1] * multiply);

                // invert the value to return minus.
                if (negative)
                {
                    reading = (reading * -1);
                }

                // return the converted reading reading, and the LS
                // reading value.
                this._reading = (reading + "." + diskReading[0]);
            }        
        }

        /// <summary>
        /// JBOD Disk Channel
        /// </summary>
        public SensorUnitTypeCode Unit
        {
            get { return this._unit; }
        }

        /// <summary>
        /// JBOD Disk Count
        /// </summary>
        public string Reading
        {
            get { return this._reading; }
        }

    }

    /// <summary>
    /// Processor Info command response
    /// </summary>
    public class ProcessorInfo : ResponseBase
    {
        // Processor Type
        private ProcessorType _type;

        // Processor state
        private ProcessorState _state;

        // processor frequency
        private ushort _frequency;

        /// <summary>
        /// Properties in the Get Processor information command response
        /// </summary>
        public ProcessorInfo(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            GetProcessorInfoResponse response =
            (GetProcessorInfoResponse)IpmiSharedFunc.ConvertResponse(data,
            typeof(GetProcessorInfoResponse));

            this.SetParamaters(response.Frequency, response.ProcessorType, 
                response.ProcessorState);
        
        }

        internal void SetParamaters(ushort frequency, byte processorType, byte processorState)
        {
            if (base.CompletionCode == 0)
            {
                // set the response frequency
                this.Frequency = frequency;

                // set the processor type.
                if (Enum.IsDefined(typeof(ProcessorType), processorType))
                {
                    this.ProcessorType = (ProcessorType)processorType;
                }
                else
                {
                    this.ProcessorType = ProcessorType.Unknown;
                }

                // set the processor state.
                if (Enum.IsDefined(typeof(ProcessorState), processorState))
                {
                    this.ProcessorState = (ProcessorState)processorState;
                }
                else
                {
                    this.ProcessorState = ProcessorState.Unknown;
                }

            }
        }

        /// <summary>
        /// Processor Type
        /// </summary>
        public ProcessorType ProcessorType
        {
            get { return this._type; }
            private set { this._type = value; }
        }

        /// <summary>
        /// Processor State
        /// </summary>
        public ProcessorState ProcessorState
        {
            get { return this._state; }
            private set { this._state = value; }
        }

        /// <summary>
        /// Processor Frequency
        /// </summary>
        public ushort Frequency
        {
            get { return this._frequency; }
            private set { this._frequency = value; }
        }
    }

    /// <summary>
    /// Memory Info command response
    /// </summary>
    public class MemoryInfo : ResponseBase
    {
        // Dimm Type
        private MemoryType _type;

        // Dimm Speed
        private ushort _speed;

        // Dimm Size
        private ushort _size;

        // Actual Memory Speed
        private bool _actualSpeed;

        /// <summary>
        /// Memory Voltage
        /// </summary>
        private MemoryVoltage _memVoltage;

        /// <summary>
        /// Memory Status
        /// </summary>
        private MemoryStatus _status;

        /// <summary>
        /// Properties in the Get Memory information command response
        /// </summary>
        public MemoryInfo(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            GetMemoryInfoResponse response =
        (GetMemoryInfoResponse)IpmiSharedFunc.ConvertResponse(data,
        typeof(GetMemoryInfoResponse));

            this.SetParamaters(response.MemorySpeed, response.MemorySize, response.RunningSpeed,
                response.MemoryType, response.Voltage, response.Status);
        
        
        }

        internal void SetParamaters(ushort memorySpeed, ushort memorySize, byte runningSpeed, byte memType, byte voltage, byte status)
        {
            if (base.CompletionCode == 0)
            {
                // set memory Speed
                this.Speed = memorySpeed;

                // set memory size
                this.MemorySize = memorySize;

                // Dimm Running at Actual Speed
                this.RunningActualSpeed = Convert.ToBoolean(runningSpeed);

                // Memory Type
                if (Enum.IsDefined(typeof(MemoryType), memType))
                {
                    this.MemoryType = (MemoryType)memType;
                }
                else
                {
                    this.MemoryType = MemoryType.Unknown;
                }

                // set the memory voltage
                if (Enum.IsDefined(typeof(MemoryVoltage), voltage))
                {
                    this.Voltage = (MemoryVoltage)voltage;
                }
                else
                {
                    this.Voltage = MemoryVoltage.Unknown;
                }

                // set the memory status
                if (Enum.IsDefined(typeof(MemoryStatus), status))
                {
                    this.Status = (MemoryStatus)status;
                }
                else
                {
                    this.Status = MemoryStatus.Unknown;
                }
            }
        
        }

        /// <summary>
        /// Memory Type
        /// </summary>
        public MemoryType MemoryType
        {
            get { return this._type; }
            private set { this._type = value; }
        }

        /// <summary>
        /// Dimm Speed
        /// </summary>
        public ushort Speed
        {
            get { return this._speed; }
            private set { this._speed = value; }
        }

        /// <summary>
        /// Dimm Size
        /// </summary>
        public ushort MemorySize
        {
            get { return this._size; }
            private set { this._size = value; }
        }

        /// <summary>
        /// Dimm Running Actual Speed
        /// </summary>
        public bool RunningActualSpeed
        {
            get { return this._actualSpeed; }
            private set { this._actualSpeed = value; }
        }

        /// <summary>
        /// Dimm Voltage
        /// </summary>
        public MemoryVoltage Voltage
        {
            get { return this._memVoltage; }
            private set { this._memVoltage = value; }
        }

        /// <summary>
        /// Memory Status
        /// </summary>
        public MemoryStatus Status
        {
            get { return this._status; }
            private set { this._status = value; }
        }
    }

    /// <summary>
    /// Memory Index command response
    /// </summary>
    public class MemoryIndex : ResponseBase
    {

        // Dimm Speed
        private int _slotCount;

        // Dimm Presense Map
        private Dictionary<int, bool> _map = new Dictionary<int, bool>();

        /// <summary>
        /// Properties in the Get Memory information command response
        /// </summary>
        public MemoryIndex(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            GetMemoryIndexResponse response =
        (GetMemoryIndexResponse)IpmiSharedFunc.ConvertResponse(data,
        typeof(GetMemoryIndexResponse));

            this.SetParamaters(response.SlotCount, response.Presence);
        }

        internal void SetParamaters(byte count, BitArray presence)
        {
            if (base.CompletionCode == 0)
            {
                // DIMM slot count
                this._slotCount = (int)count;

                for (int i = 0; i < presence.Count; i++)
                {
                    // add 1 to avoid zero on physical DIMM count.
                    _map.Add((i+1), presence[i]);
                }
            }
        }

        /// <summary>
        /// Slot Count
        /// </summary>
        public int SlotCount
        {
            get { return this._slotCount; }
            private set { this._slotCount = value; }
        }

        /// <summary>
        /// Dimm Presense Map
        /// </summary>
        public Dictionary<int, bool> PresenceMap
        {
            get { return this._map; }
            private set { this._map = value; }
        }
    }

    /// <summary>
    /// PCIe Info command response
    /// </summary>
    public class PCIeInfo : ResponseBase
    {
        // PCIe Index
        private byte _slotIndex;

        // PCIe Vendor Id
        private ushort _vendorId;

        // PCIe Device Id
        private ushort _deviceId;

        // PCIe System Id
        private ushort _systemId;

        // PCIe SubSystemId
        private ushort _subSystemId;

        // PCIe State
        private PCIeState _state = PCIeState.Unknown;

        /// <summary>
        /// Properties in the Get PCIe information command response
        /// </summary>
        public PCIeInfo(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        { 
            GetPCIeInfoResponse response =
            (GetPCIeInfoResponse)IpmiSharedFunc.ConvertResponse(data,
            typeof(GetPCIeInfoResponse));

            if (response.CompletionCode == 0x00)
            {
                if (response.VendorId == 65535 && response.SystemId == 65535)
                {
                    this.SetParamaters(PCIeState.NotPresent, 0, 0,
                        0, 0);
                }
                else
                {
                    this.SetParamaters(PCIeState.Present, response.VendorId, response.DeviceId,
                    response.SystemId, response.SubSystemId);
                }
            }
        }

        internal void SetParamaters(PCIeState state, ushort vendorId, ushort deviceId, ushort systemId, ushort subsystem)
        {
            // Slot State
            this._state = state;
            // Vendor Id
            this.VendorId = vendorId;
            // Device Id
            this.DeviceId = deviceId;
            // System Id
            this.SystemId = systemId;
            // Sub System Id
            this.SubsystemId = subsystem;

        }

        /// <summary>
        /// PCIe Index
        /// </summary>
        public byte SlotIndex
        {
            get { return this._slotIndex; }
            internal set { this._slotIndex = value; }
        }

        /// <summary>
        /// PCIe Card State
        /// </summary>
        public PCIeState CardState
        {
            get { return this._state; }
            private set { this._state = value; }
        }


        /// <summary>
        /// PCIe VendorId
        /// </summary>
        public ushort VendorId
        {
            get { return this._vendorId; }
            private set { this._vendorId = value; }
        }

        /// <summary>
        /// PCIe DeviceId
        /// </summary>
        public ushort DeviceId
        {
            get { return this._deviceId; }
            private set { this._deviceId = value; }
        }

        /// <summary>
        /// PCIe SystemId
        /// </summary>
        public ushort SystemId
        {
            get { return this._systemId; }
            private set { this._systemId = value; }
        }

        /// <summary>
        /// PCIe SubSystemId
        /// </summary>
        public ushort SubsystemId
        {
            get { return this._subSystemId; }
            private set { this._subSystemId = value; }
        }
    }

    /// <summary>
    /// NIC Info command response
    /// </summary>
    public class NicInfo : ResponseBase
    {
        // Nic Index
        private int _index;

        // MAC Address
        private string _mac = string.Empty;

        /// <summary>
        /// Properties in the Get Nic information command response
        /// </summary>
        public NicInfo(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            // if success attempt to parse the mac address
            if (base.CompletionCode == 0)
            {
                // parse the MAC address
                for (int i = 0; i < data.Length; i++)
                {
                    // convert bytes to their Hex value
                    this.MacAddress += data[i].ToString("X2", CultureInfo.InvariantCulture);

                    if (i != data.Length - 1)
                        this.MacAddress += ":";
                }
            }
        }

        /// <summary>
        /// Nic Number
        /// </summary>
        public int DeviceId
        {
            get { return this._index; }
            internal set { this._index = value; }
        }

        /// <summary>
        /// MAC Address
        /// </summary>
        public string MacAddress
        {
            get { return this._mac; }
            private set { this._mac = value; }
        }
    }

    /// <summary>
    /// Properties in the Set Power Restore Policy command response
    /// </summary>
    public class PowerRestorePolicy : ResponseBase
    {
        internal List<PowerRestoreOption> _restorePolicy =  new  List<PowerRestoreOption>();

        /// <summary>
        /// Properties in the Set Power Restore Policy command response
        /// </summary>
        public PowerRestorePolicy(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            if (base.CompletionCode == 0x00)
            {
                if ((data[0] & 0x01) == 0x01)
                    this._restorePolicy.Add(PowerRestoreOption.StayOff);

                if ((data[0] & 0x02) == 0x02)
                    this._restorePolicy.Add(PowerRestoreOption.PreviousState);

                if ((data[0] & 0x04) == 0x04)
                    this._restorePolicy.Add(PowerRestoreOption.AlwaysPowerUp);
            }
        }

        /// <summary>
        /// Power Restore Policy
        /// </summary>
        public List<PowerRestoreOption> SupportedOptions
        {
            get { return this._restorePolicy; }
        }

    }

    /// <summary>
    /// Response to Get Channel Authentication Capabilities Command
    /// </summary>
    public class ChannelAuthenticationCapabilities : ResponseBase
    {
        /// <summary>
        /// Current Channel Number
        /// </summary>
        private byte _channelNumber;

        /// <summary>
        /// 1b = IPMI v2.0+ extended capabilities available. See Extended 
        /// Capabilities field, below. 
        /// 0b = IPMI v1.5 support only.
        /// </summary>
        private IpmiVersion _authentication;

        /// <summary>
        /// [5] -  OEM proprietary (per OEM identified by the IANA OEM ID in 
        ///        the RMCP Ping Response) 
        /// [4] -  straight password / key 
        /// [3] -  reserved 
        /// [2] -  MD5 
        /// [1] -  MD2 
        /// [0] -  none  
        /// </summary>
        private List<AuthenticationType> _authTypes = new List<AuthenticationType>();

        /// <summary>
        ///  false = KgAllZero
        ///  true  = KgNoneZero
        /// </summary>
        private bool _kGStatus;

        /// <summary>
        /// Per Message Authentication
        /// </summary>
        private bool _messageAuthentication;

        /// <summary>
        /// User Level Authentication
        /// </summary>
        private bool _userLevelAuthentication;

        /// <summary>
        /// Non Null User Id Enabled.
        /// </summary>
        private bool _nonNullUserId;

        /// <summary>
        /// Nul User Id
        /// </summary>
        private bool _nullUserId;

        /// <summary>
        /// Anonymous Login
        /// </summary>
        private bool _anonymousLogon;

        /// <summary>
        /// OEM Id
        /// </summary>
        private int _oemId;

        /// <summary>
        /// Auxiliary Data
        /// </summary>
        private byte _auxiliaryData;

        /// <summary>
        /// Ipmi Channel Support
        /// </summary>
        private List<IpmiVersion> _ChannelSupport = new List<IpmiVersion>(2);

        /// <summary>
        /// Set Ipmi Authentication Type
        /// </summary>
        private void SetAuthType(IpmiVersion version)
        {
            this._authentication = version;
        }

        /// <summary>
        /// Add Ipmi Authentication Type
        /// </summary>
        private void AddAuthType(AuthenticationType authType)
        {
            this._authTypes.Add(authType);
        }

        /// <summary>
        /// Set Non Null & Null User Loging Flags
        /// </summary>
        private void SetUserId(byte non_null_user, byte null_UserId, byte anonymous_Login)
        {
            this._nonNullUserId = Convert.ToBoolean(non_null_user);
            this._nullUserId = Convert.ToBoolean(null_UserId);
            this._anonymousLogon = Convert.ToBoolean(anonymous_Login);
        }

        /// <summary>
        /// Add Ipmi Channel Protocol Suppor
        /// </summary>
        private void AddChannelSupport(IpmiVersion support)
        {
            this._ChannelSupport.Add(support);
        }

        /// <summary>
        /// User Level Authentication
        /// </summary>
        private void SetMessageAuth(bool userLevelAuthentication, 
            bool perMessageAuthenticatoin, bool kgStatus)
        {
            this._kGStatus = kgStatus;
            this._messageAuthentication = perMessageAuthenticatoin;
            this._userLevelAuthentication = userLevelAuthentication;
        }

        /// <summary>
        /// Set Oem Payload data
        /// </summary>
        private void SetOemData(int oemId, byte auxiliary)
        {
            this._oemId = oemId;
            this._auxiliaryData = auxiliary;
        }

        public ChannelAuthenticationCapabilities(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            GetChannelAuthenticationCapabilitiesResponse response =
            (GetChannelAuthenticationCapabilitiesResponse)IpmiSharedFunc.ConvertResponse(data, 
                typeof(GetChannelAuthenticationCapabilitiesResponse));

            this.SetParamaters(response.ChannelNumber, response.AuthenticationTypeSupport1,
                    response.AuthenticationTypeSupport2, response.ExtendedCapabilities,
                    response.OemId, response.OemData);
        }

        internal void SetParamaters(byte channel, byte authSupOne, byte authSupTwo, 
            byte extCapabilities, byte[] oemId, byte oemData)
        {
            // set the current channel number
            this._channelNumber = channel;

            if (base.CompletionCode == 0)
            {
                // Get authentication support message field 1 and split
                byte[] authSupport1 = IpmiSharedFunc.ByteSplit(authSupOne, new int[3] { 7, 6, 0 });

                // [0] bmc supports v1.5 only
                // [1] bmc v2.0 capabilities available
                if (authSupport1[0] == 0x00)
                {
                    this.SetAuthType(IpmiVersion.V15);
                }
                else
                {
                    this.SetAuthType(IpmiVersion.V20);
                }

                // Convert bmc auth types byte to an array for BitArray breakdown.
                byte[] bmcAuthTypes = new byte[1];
                bmcAuthTypes[0] = authSupport1[2];
                // bmc supported authentication types
                BitArray authenticatonTypes = new BitArray(bmcAuthTypes);
                // only validate client supported authentication types
                // propose an auth type in order of client perference: (MD5, straight, None).
                if (authenticatonTypes[2])      // MD5
                    this.AddAuthType(AuthenticationType.MD5);
                if (authenticatonTypes[4]) // Straight password
                    this.AddAuthType(AuthenticationType.Straight);
                if (authenticatonTypes[0]) // None
                    this.AddAuthType(AuthenticationType.None);

                // Get authentication support message field 2 and split
                byte[] authSupport2 = IpmiSharedFunc.ByteSplit(authSupTwo, new int[5] { 7, 5, 4, 3, 0 });
                bool kgStatus = false;
                bool perMessage = false;
                bool userAuth = false;

                // [0] one key login required
                // [1] two key login required
                if (authSupport2[1] == 0x01)
                {
                    kgStatus = true;
                }
                // Per Message Authentication
                if (authSupport2[2] == 0x01)
                {
                    perMessage = true;
                }
                // User Authentication
                if (authSupport2[3] == 0x01)
                {
                    userAuth = true;
                }

                // set authentication
                this.SetMessageAuth(userAuth, perMessage, kgStatus);

                // [2] - 1b = Non-null usernames enabled. (One or more users are enabled that have non-null usernames). 
                // [1] - 1b = Null usernames enabled (One or more users that have a null username, but non-null password, are presently enabled) 
                // [0] - 1b = Anonymous Login enabled (A user that has a null username and null password is presently enabled) 
                byte anonymous_Login = (byte)(authSupport2[4] & 0x01);
                byte null_User = (byte)(authSupport2[4] & 0x02);
                byte non_nullUsers = (byte)(authSupport2[4] & 0x04);

                this.SetUserId(non_nullUsers, null_User, anonymous_Login);

                // Get authentication support message field 3 and split
                // [7:2] reserved
                // [1] 1b = supports ipmi v2 connections
                // [0] 1b = supprots ipmi v1.5 connections
                byte[] authSupport3 = IpmiSharedFunc.ByteSplit(extCapabilities, new int[3] { 2, 1, 0 });
                if (authSupport3[1] == 0x01)
                {
                    this.AddChannelSupport(IpmiVersion.V20);
                }

                if (authSupport3[2] == 0x01)
                {
                    this.AddChannelSupport(IpmiVersion.V15);
                }

                // convert 3 byte oem id into integer using bitwise operation
                int oem = ((oemId[0] << 0) + (oemId[1] << 8) + (oemId[2] << 16));

                byte auxiliaryData = oemData;

                this.SetOemData(oem, auxiliaryData);
            }
        }

        /// <summary>
        /// 1b = IPMI v2.0+ extended capabilities available. See Extended 
        /// Capabilities field, below. 
        /// 0b = IPMI v1.5 support only.
        /// </summary>
        public IpmiVersion Authentication
        {
            get { return this._authentication; }
        }

        /// <summary>
        /// [5] -  OEM proprietary (per OEM identified by the IANA OEM ID in 
        ///        the RMCP Ping Response) 
        /// [4] -  straight password / key 
        /// [3] -  reserved 
        /// [2] -  MD5 
        /// [1] -  MD2 
        /// [0] -  none  
        /// </summary>
        public List<AuthenticationType> AuthTypes
        {
            get { return this._authTypes; }
        }

        /// <summary>
        ///  false = KgAllZero
        ///  true  = KgNoneZero
        /// </summary>
        public bool KGStatus
        {
            get { return this._kGStatus; }
        }

        /// <summary>
        /// Per Message Authentication
        /// </summary>
        public bool MessageAuthentication
        {
            get { return this._messageAuthentication; }
        }

        /// <summary>
        /// User Level Authentication
        /// </summary>
        public bool UserLevelAuthentication
        {
            get { return this._userLevelAuthentication; }
        }

        /// <summary>
        /// Non Null User Id Enabled.
        /// </summary>
        public bool NonNullUserId
        {
            get { return this._nonNullUserId; }
        }

        /// <summary>
        /// Null User Id
        /// </summary>
        public bool NullUserId
        {
            get { return this._nullUserId; }
        }

        /// <summary>
        /// Anonymous Login
        /// </summary>
        public bool AnonymousLogOn
        {
            get { return this._anonymousLogon; }
        }

        /// <summary>
        /// Ipmi Channel Support
        /// </summary>
        public List<IpmiVersion> ChannelSupport
        {
            get { return this._ChannelSupport; }
        }

        /// <summary>
        /// OEM Id
        /// </summary>
        public int OemId
        {
            get { return this._oemId; }
        }

        /// <summary>
        /// Auxiliary Data
        /// </summary>
        public byte AuxiliaryData
        {
            get { return this._auxiliaryData; }
        }

        /// <summary>
        /// Returns the Current Channel Number
        /// </summary>
        public byte ChannelNumber
        {
            get { return this._channelNumber; }
        }

    }

    /// <summary>
    /// Response to Get Channel Info Command
    /// </summary>
    public class ChannelInfo : ResponseBase
    {
        /// <summary>
        /// Channel number.
        /// </summary>
        private byte _channelNumber;

        /// <summary>
        /// Channel Medium.
        /// </summary>
        private byte _channelMedium;

        /// <summary>
        /// Channel Protocol
        /// </summary>
        private byte _channelProtocol;

        /// <summary>
        /// Number of Sessions Supported
        /// </summary>
        private byte _numberOfSessions;


        /// <summary>
        /// Channel Session Support
        ///     00b = channel is session-less
        ///     01b = channel is single-session
        ///     10b = channel is multi-session
        ///     11b = channel is session-based
        /// </summary>
        private byte _channelSessionSupport;

        public ChannelInfo(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            GetChannelInfoResponse response =
            (GetChannelInfoResponse)IpmiSharedFunc.ConvertResponse(data, typeof(GetChannelInfoResponse));

            if (response.CompletionCode == 0)
            {
                this.SetParamaters(
                                    response.ChannelNumber,
                                    response.ChannelMedium,
                                    response.ChannelProtocol,
                                    response.ChannelSessionSupport,
                                    response.NumberOfSessions);
            }

        }

        internal void SetParamaters(byte channelNumber, byte channelMedium, 
            byte channelProtocol, byte channelSession, byte sessions)
        {
            this.ChannelNumber = channelNumber;
            this.ChannelMedium = channelMedium;
            this.ChannelProtocol = channelProtocol;
            this.ChannelSessionSupport = channelSession;
            this.NumberOfSessions = sessions;
        }

        /// <summary>
        /// Gets and sets the Actual Channel number.
        /// </summary>
        /// <value>Channel number.</value>
        public byte ChannelNumber
        {
            get { return this._channelNumber; }
            private set { this._channelNumber = value; }
        }

        /// <summary>
        /// Channel Medium.
        /// </summary>
        public byte ChannelMedium
        {
            get { return this._channelMedium; }
            private set { this._channelMedium = value; }
        }

        /// <summary>
        /// Channel Protocol
        /// </summary>
        public byte ChannelProtocol
        {
            get { return this._channelProtocol; }
            private set { this._channelProtocol = value; }
        }

        /// <summary>
        /// Channel Session Support
        ///     00b = channel is session-less
        ///     01b = channel is single-session
        ///     10b = channel is multi-session
        ///     11b = channel is session-based
        /// </summary>
        public byte ChannelSessionSupport
        {
            get { return (byte)(this._channelSessionSupport); }
            private set { this._channelSessionSupport = value; }
        }

        /// <summary>
        /// Number of sessions
        /// </summary>
        public byte NumberOfSessions
        {
            get { return (byte)(this._numberOfSessions); }
            private set { this._numberOfSessions = value; }
        }

    }

    /// <summary>
    /// Response to Get SEL Time Command
    /// </summary>
    public class GetEventLogTime : ResponseBase
    {
        /// <summary>
        /// SEL Time
        /// </summary>
        private DateTime _time;

        public GetEventLogTime(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            SelTimeResponse response =
            (SelTimeResponse)IpmiSharedFunc.ConvertResponse(data, typeof(SelTimeResponse));

            if (response.CompletionCode == 0)
                this.SetParamaters(response.Time);
        }

        internal void SetParamaters(DateTime time)
        {
            this.EventLogTime = time;
        }

        /// <summary>
        /// Gets and sets the SEL Time.
        /// </summary>
        public DateTime EventLogTime
        {
            get { return this._time; }
            private set { this._time = value; }
        }
    }

    /// <summary>
    /// Class that supports the Get Sdr Repository Info command.
    /// </summary>
    public class SdrRepositoryInfo : ResponseBase
    {
        private string _version;

        private int _entries;

        private int _freeSpace;

        private DateTime _lastUpdate = DateTime.Now;

        private DateTime _lastCleared = DateTime.Now;

        /// <summary>
        /// Initialize class
        /// </summary>
        public SdrRepositoryInfo(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            GetSdrRepositoryInfoResponse response =
            (GetSdrRepositoryInfoResponse)IpmiSharedFunc.ConvertResponse(data, 
            typeof(GetSdrRepositoryInfoResponse));

            if (response.CompletionCode == 0)
                this.SetParamaters(response.SdrVersion, response.MSByte, response.LSByte, 
                    response.SdrFeeSpace, response.LastAdded, response.LastRemoved);
        
        }

        internal void SetParamaters(byte sdrVersion, byte msByte, byte lsByte, byte[] sdrFreeSpace, byte[] lastAdded, byte[] lastRemoved )
        {
            if (base.CompletionCode == 0x00)
            {
                // Sel Version ByteArray
                byte[] SdrVersionArray = IpmiSharedFunc.ByteSplit(sdrVersion, new int[2] { 4, 0 });
                // LS Version Bit [7:4]
                byte VersionLs = SdrVersionArray[0];
                // MS Version Bit [3:0]
                byte VersionMs = SdrVersionArray[1];

                // Sdr Version Number
                this.Version = ((int)VersionMs + "." + (int)VersionLs);

                // Number of Events in Sdr
                this.Entries = IpmiSharedFunc.GetShort(lsByte, msByte);

                // Default free space in Bytes
                int freeSpace = BitConverter.ToUInt16(sdrFreeSpace, 0);

                // add free space to class object
                this.FreeSpace = freeSpace;

                // Convert byte[] to int using Shift operation
                int lastAddedSeconds = lastAdded[0] + (lastAdded[1] << 8) + (lastAdded[2] << 16) + (lastAdded[3] << 24);

                // calculate last entry added date
                this.LastUpdate = IpmiSharedFunc.SecondsOffSet(lastAddedSeconds);

                // Convert byte[] to int using Shift operation
                int lastRemovedSeconds = lastRemoved[0] + (lastRemoved[1] << 8) + (lastRemoved[2] << 16) + (lastRemoved[3] << 24);

                // calculate last entry removed date
                this.LastCleared = IpmiSharedFunc.SecondsOffSet(lastRemovedSeconds);
            }
        }

        /// <summary>
        /// Sdr Version Number
        /// </summary>
        public string Version
        {
            get { return this._version; }
            private set { this._version = value; }
        }

        /// <summary>
        /// Number of Sdr entries;
        /// </summary>
        public int Entries
        {
            get { return this._entries; }
            private set { this._entries = value; }
        }

        /// <summary>
        /// Sdr free space in KB;
        /// </summary>
        public int FreeSpace
        {
            get { return this._freeSpace; }
            private set { this._freeSpace = value; }
        }

        /// <summary>
        /// Date and time the Sdr was last updated
        /// </summary>
        public DateTime LastUpdate
        {
            get { return this._lastUpdate; }
            private set { this._lastUpdate = value; }
        }

        /// <summary>
        /// Date and time the Sdr was last cleared
        /// </summary>
        public DateTime LastCleared
        {
            get { return this._lastCleared; }
            private set { this._lastCleared = value; }
        }
    }

    /// <summary>
    /// Class that supports the Get Sdr Repository Info command.
    /// </summary>
    public class FruInventoryArea : ResponseBase
    {
        internal ushort fruSize;

        internal bool accessedByBytes = false;

        /// <summary>
        /// Initialize class
        /// </summary>
        public FruInventoryArea(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] param)
        { }

        /// <summary>
        /// Fru Size
        /// </summary>
        public ushort FruSize
        {
            get { return this.fruSize; }
        }

        /// <summary>
        /// Accessed By Bytes
        /// If false, access is by WORD
        /// </summary>
        public bool AccessedByBytes
        {
            get { return this.accessedByBytes; }
        }
    }

    #region OEM Commands

    public class EnergyStorage : ResponseBase
    { 
        private byte presence;

        private EnergyStorageState state;

        private byte percentageCharge;

        private ushort holdTimeInSeconds;

        private DateTime timestamp;

        /// <summary>
        /// Initialize class
        /// </summary>
        public EnergyStorage(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal void SetParamaters(byte presence, byte state, byte charge, ushort holdTime, DateTime timestamp)
        {
            this.presence = presence;
            this.state = (EnergyStorageState)state;
            this.percentageCharge = charge;
            this.holdTimeInSeconds = holdTime;
            this.timestamp = timestamp;
        }

        /// <summary>
        /// Set response class parmaters given raw payload data
        /// </summary>
        /// <param name="param"></param>
        internal override void SetParamaters(byte[] param)
        {
            SetParamaters(
                            (byte)(param[0] & 0x03),         // presence
                            (byte)(param[0] & 0x1C),         // state
                            param[1],                        // percent charge
                            BitConverter.ToUInt16(param, 2), // hold time
                            IpmiSharedFunc.SecondsOffSet(BitConverter.ToUInt32(param, 4)) // timestamp
                            );

        }

        /// <summary>
        /// Energy Stroage Presence
        /// </summary>
        public ushort Presence
        {
            get { return this.presence; }
        }

        /// <summary>
        /// Energy Storage State
        /// </summary>
        public EnergyStorageState State
        {
            get { return this.state; }
        }

        /// <summary>
        /// Energy Storage Percentage Charge
        /// </summary>
        public byte PercentCharge
        {
            get { return this.percentageCharge; }
        }

        /// <summary>
        /// Energy Storage Hold Time in seconds.
        /// </summary>
        public ushort HoldTime
        {
            get { return this.holdTimeInSeconds; }
        }

        /// <summary>
        /// Energy Storage last updated timestamp.
        /// </summary>
        public DateTime Timestamp
        {
            get { return this.timestamp; }
        }
   
    }


    #endregion

    #region Bridge Classes

    /// <summary>
    /// Class that supports the Send Message / Get Message command.
    /// </summary>
    public class BridgeMessage : ResponseBase
    {
        /// <summary>
        /// Response message payload.
        /// </summary>
        private byte[] messageData;

        /// <summary>
        /// Initialize class
        /// </summary>
        public BridgeMessage(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] param)
        {
            this.messageData = param;
        }

        /// <summary>
        /// Response message payload.
        /// </summary>
        public byte[] MessageData
        {
            get { return this.messageData; }
            set { this.messageData = value; }
        }
    }

    /// <summary>
    /// Class that supports the Get Message Flags command.
    /// </summary>
    public class MessageFlags : ResponseBase
    {

        /// <summary>
        /// Response message payload.
        /// </summary>
        private byte messageAvail;

        /// <summary>
        /// Receive Buffer full
        /// </summary>
        private byte bufferFull;

        /// <summary>
        /// Watch Dog pre-timeout interrupt
        /// </summary>
        private byte watchDogTimeout;

        /// <summary>
        /// OEM 1 Data Available
        /// </summary>
        private byte oem1;

        /// <summary>
        /// OEM 2 Data Available
        /// </summary>
        private byte oem2;

        /// <summary>
        /// OEM 3 Data Available
        /// </summary>
        private byte oem3;

        /// <summary>
        /// Initialize class
        /// </summary>
        public MessageFlags(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        /// <summary>
        /// Set class properties
        /// </summary>
        internal void SetParamaters(byte messageAvail, byte bufferFull, byte watchDog, byte oem1, byte oem2, byte oem3)
        {
            this.messageAvail = messageAvail;
            this.bufferFull = bufferFull;
            this.watchDogTimeout = watchDog;
            this.oem1 = oem1;
            this.oem2 = oem2;
            this.oem3 = oem3;
        }

        /// <summary>
        /// Set class properties
        /// </summary>
        internal override void SetParamaters(byte[] param)
        {
            this.messageAvail =  (byte)(param[0] & 0x01);

            this.bufferFull =  (byte)((param[0] & 0x02) >> 1);

            this.watchDogTimeout = (byte)((param[0] & 0x08) >> 3);

            this.oem1 =  (byte)((param[0] & 0x20) >> 5);

            this.oem2 =  (byte)((param[0] & 0x40) >> 6);

            this.oem3 =  (byte)((param[0] & 0x80) >> 7);
        }

        /// <summary>
        /// Receive Message Available
        /// </summary>
        public byte MessageAvailable
        {
            get { return this.messageAvail; }
        }

        /// <summary>
        /// Receive Buffer full
        /// </summary>
        public byte BufferFull
        {
            get { return this.bufferFull; }
        }

        /// <summary>
        /// Watch Dog pre-timeout interrupt
        /// </summary>
        public byte WatchDogTimeout
        {
            get { return this.watchDogTimeout; }
        }

        /// <summary>
        /// OEM 1 Data Available
        /// </summary>
        public byte OEM1
        {
            get { return this.oem1; }
        }

        /// <summary>
        /// OEM 2 Data Available
        /// </summary>
        public byte OEM2
        {
            get { return this.oem2; }
        }

        /// <summary>
        /// OEM 3 Data Available
        /// </summary>
        public byte OEM3
        {
            get { return this.oem3; }
        }
    }

    /// <summary>
    /// Class that supports the Send Message / Get Message command.
    /// </summary>
    public class BridgeChannelReceive : ResponseBase
    {
        /// <summary>
        /// Channel to send the message.
        /// </summary>
        private byte channel;

        /// <summary>
        /// Channel Enable/Disable State.
        /// </summary>
        private byte channelState;

        /// <summary>
        /// Initialize class
        /// </summary>
        public BridgeChannelReceive(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal void SetParamaters(byte chennel, byte channelsate)
        {
            this.channel = chennel;
            this.channelState = channelsate;
        }

        internal override void SetParamaters(byte[] param)
        {
            this.channel = param[0];
            this.channelState = param[1];
        }

        /// <summary>
        /// Channel to send the request message.
        /// </summary>
        [IpmiMessageData(0)]
        public byte Channel
        {
            get { return this.channel; }
            set { this.channel = (byte)(value & 0x0f); }
        }

        /// <summary>
        /// Channel State
        /// </summary>
        [IpmiMessageData(1)]
        public byte ChannelState
        {
            get { return this.channelState; }
            set { this.channelState = (byte)(value & 0x01); }
        }
    }

    #endregion

    #region SEL Classes

    /// <summary>
    /// Class that supports the Get SEL Info command.
    /// </summary>
    public class SystemEventLogInfo : ResponseBase
    {
        private string _version;

        private int _entries;

        private int _space;

        private DateTime _lastUpdate;

        private DateTime _lastCleared;

        /// <summary>
        /// Initialize class
        /// </summary>
        public SystemEventLogInfo(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            SelInfoResponse response =
                (SelInfoResponse)IpmiSharedFunc.ConvertResponse(data, typeof(SelInfoResponse));

            if (base.CompletionCode == 0x00)
            {
                this.SetParamaters(response.SELVersion,
                                                    response.MSByte,
                                                    response.LSByte,
                                                    response.SelFeeSpace,
                                                    response.LastAdded,
                                                    response.LastRemoved);
            }
        }

        internal void SetParamaters(byte version, byte msByte, byte lsByte, byte[] spaceFree, byte[] lastAdded, byte[] lastRemoved)
        {
            if (base.CompletionCode == 0x00)
            {
                // Sel Version ByteArray
                byte[] SelVersionArray = IpmiSharedFunc.ByteSplit(version, new int[2] { 4, 0 });
                // LS Version Bit [7:4]
                byte VersionLs = SelVersionArray[0];
                // MS Version Bit [3:0]
                byte VersionMs = SelVersionArray[1];

                // SEL Version Number
                this._version = ((int)VersionMs + "." + (int)VersionLs);

                // Number of Events in SEL
                this._entries = (msByte << 8) + lsByte;

                // Default free space in Bytes
                int freeSpace = 65536;

                // Get Real Free Space
                byte[] defaultfreeSpace = new byte[2] { 0xFF, 0xFF };

                if (spaceFree[0] != defaultfreeSpace[0] && spaceFree[1] != defaultfreeSpace[1])
                {
                    // FreeSpace LS byte First
                    byte[] FreeSpaceBytes = spaceFree;
                    freeSpace = (FreeSpaceBytes[1] << 8) + FreeSpaceBytes[0];
                }

                // add free space to class object
                this._space = freeSpace;

                // Convert byte[] to int using Shift operation
                int lastAddedSeconds = lastAdded[0] + (lastAdded[1] << 8) + (lastAdded[2] << 16) + (lastAdded[3] << 24);

                // calculate last entry added date
                this._lastUpdate = IpmiSharedFunc.SecondsOffSet(lastAddedSeconds);

                // Convert byte[] to int using Shift operation
                int lastRemovedSeconds = lastRemoved[0] + (lastRemoved[1] << 8) + (lastRemoved[2] << 16) + (lastRemoved[3] << 24);

                // calculate last entry removed date
                this._lastCleared = IpmiSharedFunc.SecondsOffSet(lastRemovedSeconds);
            }

        }

        /// <summary>
        /// SEL number
        /// </summary>
        public string Version
        {
            get { return this._version; }
        }

        /// <summary>
        /// Number of SEL record entries;
        /// </summary>
        public int Entries
        {
            get { return this._entries; }
        }

        /// <summary>
        /// SEL free space in KB;
        /// </summary>
        public int FreeSpace
        {
            get { return this._space; }
        }

        /// <summary>
        /// Date and time the SEL was last updated
        /// </summary>
        public DateTime LastUpdate
        {
            get { return this._lastUpdate; }
        }

        /// <summary>
        /// Date and time the SEL was last cleared
        /// </summary>
        public DateTime LastCleared
        {
            get { return this._lastCleared; }
        }

    }

    /// <summary>
    /// Collection of SEL records (string formatted).
    /// </summary>
    public class SystemEventLog: ResponseBase
    {
        public SystemEventLog(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] param)
        { }

        private Collection<SystemEventLogMessage> _eventLog = new Collection<SystemEventLogMessage>();

        public Collection<SystemEventLogMessage> EventLog
        { get { return this._eventLog; } }

    }

    /// <summary>
    /// System Event Log (SEL) message class.
    /// </summary>
    public class SystemEventLogMessage : ResponseBase
    {
        // sel event format
        private EventMessageFormat _eventFormat;

        // event date/time
        private DateTime _eventDate;

        // event message format version
        private MsgVersion _eventVersion;

        // sensor type (voltage, temp, processor, etc)
        private SensorType _sensorType;

        // unique sensor number
        private byte _sensorNumber;

        // event direction (assertion/desertion)
        private EventDir _eventDir;

        // ipmi event type
        private byte _eventType;

        // raw sensor Type
        private byte _rawSensorType;

        // event message data
        private EventData _eventMessage;

        // raw event payload as a hex string
        private string _eventPayload = string.Empty;

        /// <summary>
        /// Raw unconverted ipmi Payload bytes
        /// </summary>
        private byte[] _rawPayload = new byte[3];

        public SystemEventLogMessage(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        { }

        /// <summary>
        /// Raw ipmi message payload. 3 byte event
        /// payload.
        /// </summary>
        internal byte[] RawPayload
        {
            get { return this._rawPayload; }
            set { this._rawPayload = value; }
        }

        /// <summary>
        /// Raw ipmi message payload event type.
        /// </summary>
        internal byte EventTypeCode
        {
            get { return this._eventType; }
            set { this._eventType = value; }
        }

        /// <summary>
        /// Raw ipmi message payload event type.
        /// </summary>
        internal byte RawSensorType
        {
            get { return this._rawSensorType; }
            set { this._rawSensorType = value; }
        }

        /// <summary>
        /// Byte split event message.
        /// </summary>
        internal EventData EventMessage
        {
            get { return this._eventMessage; }
            set { this._eventMessage = value; }
        }

        #region public properties

        // sel event type
        public EventMessageFormat EventFormat
        {
            get { return this._eventFormat; }
            internal set { this._eventFormat = value; }
        }

        // event date/time
        public DateTime EventDate
        {
            get { return this._eventDate; }
            internal set { this._eventDate = value; }
        }

        // event message format
        public MsgVersion EventVersion
        {
            get { return this._eventVersion; }
            internal set { this._eventVersion = value; }
        }

        // sensor type (voltage, temp, processor, etc)
        public SensorType SensorType
        {
            get { return this._sensorType; }
            internal set { this._sensorType = value; }
        }

        // unique sensor number
        public byte SensorNumber
        {
            get { return this._sensorNumber; }
            internal set { this._sensorNumber = value; }
        }

        // event direction (assertion/desertion)
        public EventDir EventDir
        {
            get { return this._eventDir; }
            internal set { this._eventDir = value; }
        }

        public string EventPayload
        {
            get { return this._eventPayload; }
            internal set { this._eventPayload = value; }
        }

        #endregion

    }

    #endregion

}
