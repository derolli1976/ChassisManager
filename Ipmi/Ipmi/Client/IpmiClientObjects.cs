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
    using System.Text;
    using System.Collections;

    /// <summary>
    /// Power Reading Return Type
    /// </summary>
    internal class PowerReadingSupport : ResponseBase
    {
        private byte[] readings;

        internal PowerReadingSupport(byte completionCode)
        {
            base.CompletionCode = completionCode;
        }

        internal override void SetParamaters(byte[] data)
        {
            readings = data;
        }

        public byte[] Readings
        {
            get { return this.readings; }
        }
    }

    /// <summary>
    /// FRU Inventory Return Type
    /// </summary>
    internal class ReadFruInventorySupport
    {
        internal ArrayList fruInventory;
        internal byte completionCode;

        internal ReadFruInventorySupport(byte completionCode)
        {
            this.completionCode = completionCode;
        }

        internal ArrayList FruInventory
        {
            get { return this.fruInventory; }
        }

        /// <summary>
        /// Completion Code
        /// </summary>
        internal byte CompletionCode
        {
            get { return this.completionCode; }
        }


    }

    #region SDR Classes
        
        /// <summary>
        /// Record Base Class
        /// </summary>
        public abstract class SensorMetadataBase
        {

            internal readonly SdrRecord _record;

            internal SensorMetadataBase(SdrRecord record)
            {
                this._record = record;
            }

            protected void SetDescription(int index)
            {
                // sensor id. [7:6] 00 = Unicode, 01 = BCD plus, 10 = 6-bit ASCII, 11 = 8-bit ASCII + Latin 1.
                // [5] reserved. [4:0] = length of following data.
                byte[] sensorId = IpmiSharedFunc.ByteSplit(_record.RecordData[index], new int[3] { 6, 5, 0 });

                int len = sensorId[2];

                if (len > 16)
                    len = 16;

                // temp byte array to hold string data
                byte[] strData = new byte[len];

                // forward index byte 1.
                index = checked(index + 1);

                // populate temp type array
                if (_record.RecordData.Length >= index + len)
                    Buffer.BlockCopy(_record.RecordData, index, strData, 0, len);

                // sensor description.
                _description = string.Empty;

                if (sensorId[2] > 0)
                {
                    switch (sensorId[0])
                    {
                        case 0x00:
                            _description = UnicodeEncoding.Unicode.GetString(strData);
                            break;
                        case 0x01:
                            _description = IpmiSharedFunc.DecodeBcdPlus(strData);
                            break;
                        case 0x02:
                            _description = IpmiSharedFunc.DecodePacked6bitAscii(strData);
                            break;
                        case 0x03:
                            _description = Encoding.ASCII.GetString(strData);
                            break;
                        default:
                            break;
                    }
                }
            }

            private byte _completionCode;

            /// <summary>
            /// Ipmi entity type signals whether entity is
            /// a physical component or logical group of
            /// physical components
            /// </summary>
            private IpmiEntityType _entityType;

            /// <summary>
            /// entity type (processor, baseboard etc)
            /// </summary>
            private IpmiEntity _entity;

            /// <summary>
            /// entity instance #
            /// </summary>
            private int _entityInstance;

            /// <summary>
            /// sensor number
            /// </summary>
            private byte _sensorNumber;

            /// <summary>
            /// sensor type (voltage, temp, processor, etc)
            /// </summary>
            private SensorType _sensorType = SensorType.Unknown;

            /// <summary>
            /// sensor type in byte format (voltage, temp, processor, etc)
            /// </summary>
            private byte _rawSensorType;
            
            /// <summary>
            /// event type code
            /// </summary>
            private EventLogMsgType _eventType;

            /// <summary>
            /// event reading type code
            /// </summary>
            private byte _eventCode;

            /// <summary>
            /// event offSet
            /// </summary>
            private int _eventOffSet;

            /// <summary>
            /// sensor capabilities (internal)
            /// </summary>
            private byte[] _capabilities;

            /// <summary>
            /// sensor initialization
            /// </summary>
            private byte[] _initialization;

            /// <summary>
            /// sensor description
            /// </summary>
            private string _description;

            #region Internal Functions/Methods

            /// <summary>
            /// Splits the event reading code byte
            /// into an int array of valid values.
            /// </summary>
            private static byte[] SdrEventReading(byte eventCode)
            {

                byte[] tempArray = new byte[1];
                tempArray[0] = eventCode;

                BitArray bits = new BitArray(tempArray);


                // get count of true values
                int index = 0;
                for (int i = 0; i < bits.Count; i++)
                {
                    if (bits[i])
                        index++;
                }

                byte[] arr = new byte[index];

                index = 0;
                for (int i = 0; i < bits.Count; i++)
                {
                    if (bits[i])
                    {
                        arr[index] = (byte)i;
                        index++;
                    }

                }

                return arr;
            }

            /// <summary>
            /// Returns the ipmi/dcmi SDR sensor reading event offset.
            /// depending on the sensor, this event represents a
            /// descrete or sensor specific event code.
            /// </summary>
            protected int SdrEventOffset(byte eventCode)
            {
                // default is to return zero
                int sensorEvent = 0;

                // split sensor event code into bits
                byte[] offsetArr = SdrEventReading(eventCode);

                // if 1 or more bits are set, select the highest
                if (offsetArr.Length >= 1)
                {
                    // set the event offset
                    byte eventOffset = offsetArr[0];

                    for (int i = 1; i < offsetArr.Length; i++)
                    {
                        if (offsetArr[i] > eventOffset)
                            eventOffset = offsetArr[i];
                    }

                    // calculate sensor event offset using bitwise shifting
                    sensorEvent = ((eventOffset) + (this._rawSensorType << 8) + (0x00 << 16) + (0x00 << 24));
                }

                return sensorEvent;
            }

            /// <summary>
            /// Converts entity byte into entity enum value
            /// </summary>
            /// <param name="entity">entity byte</param>
            /// <returns>entity enum</returns>
            protected void SetEntity(byte entity)
            {
                // Convert ranges to single enum value

                // 0x38 - 0x3f = reserved
                if (entity >= 0x38 && entity <= 0x3F)
                {
                    // 0x38 = reserved
                    entity = 0x38;
                }
                // 0x90 - 0xAF = Chassis-specific
                else if (entity >= 0x90 && entity <= 0xAF)
                {
                    // 0x90 = Chassis-specific
                    entity = 0x90;
                }
                // 0xB0 - 0xCF = board-set specific
                else if (entity >= 0xB0 && entity <= 0xCF)
                {
                    // 0xB0 = board-set specific
                    entity = 0xB0;
                }
                // 0xD0 - 0xFF = OEM System Integrator define
                else if (entity >= 0xD0 && entity <= 0xFF)
                {
                    // 0xB0 = OEM
                    entity = 0xD0;
                }

                // convert to enum or return unknown
                if (Enum.IsDefined(typeof(IpmiEntity), entity))
                {
                    this._entity =  (IpmiEntity)entity;
                }
                else
                {
                    this._entity = IpmiEntity.Unknown;
                }
            }

            /// <summary>
            /// Converts entity type Int into entity type enum value
            /// </summary>
            /// <param name="entityType">entity type number</param>
            /// <returns>entity type enum</returns>
            protected void SetEntityType(int entityType)
            {
                // convert to enum or return unknown
                if (Enum.IsDefined(typeof(IpmiEntityType), entityType))
                {
                    this._entityType = (IpmiEntityType)entityType;
                }
                else
                {
                    this._entityType = IpmiEntityType.Unknown;
                }
            }

            /// <summary>
            /// SdrMessage properties common in full & compact sensor records
            /// </summary>
            /// <param name="record">raw sdr data record</param>
            internal void SdrCommonHeader<T>(SdrRecord record) where T : SensorMetadataBase
            {

                // record[7] = Sensor Number
                this._sensorNumber = record.RecordData[7];

                // get the record type
                Type recordType = typeof(T);
                
                if((recordType == typeof(FullSensorRecord)) ||
                    (recordType == typeof(CompactSensorRecord)) ||
                    (recordType == typeof(SdrEventOnlyRecord))
                    )
                {
                    // record[8] = Entity Id, 
                    this.SetEntity(record.RecordData[8]);

                    // bit [7] = physical/group 
                    this.SetEntityType((record.RecordData[9] & 0x80));

                    // bit [6-0]  = Entity Instance
                    this._entityInstance = (record.RecordData[9] & 0x7F);

                    // if the record Type is full or compact, get the initialization and capabilities.
                    if ((recordType == typeof(FullSensorRecord)) ||
                        (recordType == typeof(CompactSensorRecord)))
                    {
                        // record[10] = Sensor Initialization
                        this._initialization = IpmiSharedFunc.ByteSplit(record.RecordData[10], new int[8] { 7, 6, 5, 4, 3, 2, 1, 0 });

                        // sensor capabilities. [0] = ignore sensor if entity disabled. [1] = sensor re-arm
                        // [2] = hysteresis support.  [3]  = threshold support.  [4] = event message control
                        this._capabilities = IpmiSharedFunc.ByteSplit(record.RecordData[11], new int[5] { 7, 6, 4, 2, 0 });

                        // raw sensor type
                        this._rawSensorType = record.RecordData[12];

                        // sensor type
                        if (Enum.IsDefined(typeof(SensorType), this._rawSensorType))
                        {
                            this._sensorType = (SensorType)this._rawSensorType;
                        }
                        else
                        {
                            this._sensorType = SensorType.Unknown;
                        }

                        // event Reading Type code
                        this._eventCode = record.RecordData[13];

                        // record[13] = Event/Reading Type Code Classification
                        if (record.RecordData[13] == 0x01)
                        {
                            // Threshold
                            this._eventType = EventLogMsgType.Threshold;
                        }
                        else if ((record.RecordData[13] >= 0x02) && (record.RecordData[13] <= 0x0C))
                        {
                            // Generic Discrete
                            this._eventType = EventLogMsgType.Discrete;
                        }
                        else if (record.RecordData[13] == 0x6f)
                        {
                            // Specific discrete
                            this._eventType = EventLogMsgType.SensorSpecific;
                        }
                        else if ((record.RecordData[13] >= 0x70) && (record.RecordData[13] <= 0x7F))
                        {
                            // OEM
                            this._eventType = EventLogMsgType.Oem;
                        }
                        else
                        {
                            // Unknown
                            this._eventType = EventLogMsgType.Unspecified;
                        }
                    }
                    else if (recordType == typeof(SdrEventOnlyRecord))
                    {
                        // Raw sensor type byte 11 for Event Only Sensors.
                        // IPMI 2.0 specification Table 43-3, Event-Only Sensor Record
                        this._rawSensorType = record.RecordData[10];

                        // sensor type
                        if (Enum.IsDefined(typeof(SensorType), this._rawSensorType))
                        {
                            this._sensorType = (SensorType)this._rawSensorType;
                        }
                        else
                        {
                            this._sensorType = SensorType.Unknown;
                        }

                        // raw event code
                        this._eventCode = record.RecordData[11];

                        // record[13] = Event/Reading Type Code
                        if (record.RecordData[11] == 0x01)
                        {
                            // Threshold
                            this._eventType = EventLogMsgType.Threshold;
                        }
                        else if ((record.RecordData[11] >= 0x02) && (record.RecordData[11] <= 0x0C))
                        {
                            // Generic Discrete
                            this._eventType = EventLogMsgType.Discrete;
                        }
                        else if (record.RecordData[11] == 0x6f)
                        {
                            // Specific discrete
                            this._eventType = EventLogMsgType.SensorSpecific;
                        }
                        else if ((record.RecordData[11] >= 0x70) && (record.RecordData[11] <= 0x7F))
                        {
                            // OEM
                            this._eventType = EventLogMsgType.Oem;
                        }
                        else
                        {
                            // Unknown
                            this._eventType = EventLogMsgType.Unspecified;
                        }
                    }

                }
            }

            internal byte[] capabilities
            {
                get { return this._capabilities; }
            }

            internal byte[] initialization
            {
                get { return this._initialization; }
            }

            #endregion

            #region Public Variables

            /// <summary>
            /// sensor type (voltage, temp, processor, etc)
            /// </summary>
            public SensorType SensorType
            {
                get { return this._sensorType; }
            }

            /// <summary>
            /// sensor type in byte format (voltage, temp, processor, etc)
            /// </summary>
            public byte RawSensorType
            {
                get { return this._rawSensorType; }
            }    

            /// <summary>
            /// sensor number
            /// </summary>
            public byte SensorNumber
            {
                get { return this._sensorNumber; }
                protected set { this._sensorNumber = value; }
            }

            /// <summary>
            /// event reading type code
            /// </summary>
            public EventLogMsgType EventType
            {
                get { return this._eventType; }
            }

            /// <summary>
            /// event reading type code
            /// </summary>
            public byte EventReadingCode
            {
                get { return this._eventCode; }
            }

            /// <summary>
            /// event offSet
            /// </summary>
            public int EventOffset
            {
                get { return this._eventOffSet; }
                protected set { this._eventOffSet = value; }
            }

            /// <summary>
            /// entity type (processor, baseboard etc)
            /// </summary>
            public IpmiEntity Entity
            {
                get { return this._entity; }
            }

            /// <summary>
            /// entity instance #
            /// </summary>
            public int EntityInstance
            {
                get { return this._entityInstance; }
            }

            /// <summary>
            /// Entity type (physical, logical group etc)
            /// </summary>
            public IpmiEntityType EntityType
            {
                get { return this._entityType; }
            }

            /// <summary>
            /// sensor description
            /// </summary>
            public string Description
            {
                get { return this._description; }
            }

            public byte CompletionCode
            {
                get { return this._completionCode; }
                protected set {this._completionCode = value;}
            }

            #endregion

        }

        /// <summary>
        /// Full Sensor Data Record.
        /// </summary>
        public class FullSensorRecord : SensorMetadataBase
        {
            /// <summary>
            /// initializes instance of the class
            /// </summary>
            /// <param name="recordId">sdr record id</param>
            /// <param name="version">sdr version </param>
            /// <param name="sdrType">sdr record type</param>
            internal FullSensorRecord(short recordId, Decimal version, SdrRecordType sdrType, SdrRecord record, byte completionCode)
                : base(record)
            {
                this._recordId = recordId;
                this._version = version;
                this._sdrType = sdrType;
                base.CompletionCode = completionCode;
                this.Initialize();
            }

           /// <summary>
            /// initializes instance of the class
            /// </summary>
            /// <param name="recordId">sdr record id</param>
            /// <param name="version">sdr version </param>
            /// <param name="sdrType">sdr record type</param>
            internal FullSensorRecord(short recordId, Decimal version, SdrRecordType sdrType, SdrRecord record, byte[] readingFactors, byte completionCode)
                : base(record)
            {
                this._recordId = recordId;
                this._version = version;
                this._sdrType = sdrType;
                base.CompletionCode = completionCode;
                Buffer.BlockCopy(readingFactors, 0, _rawfactors,0, 6);
                this._setFactors = true;
                this.Initialize();
            }

            /// <summary>
            /// Sensor Reading Status sets the sensor state.
            /// </summary>
            /// <param name="sensorStates"></param>
            internal void SetState(byte sensorStates, byte stateOffset)
            {
                #region sensor state
                // split 
                byte[] status = IpmiSharedFunc.ByteSplit(sensorStates, new int[4] { 7, 6, 5, 0 });

                // sensor state unavailable
                if (status[2] == 0x01)
                {
                    // set state to unavailable
                    _sensorState = SdrState.Unavailable;

                    // set scanning to unavilable
                    _scanning = SdrState.Unavailable;

                }
                // sensor state available
                else
                {
                    // convert sensor state to enum or return unspecified
                    if (Enum.IsDefined(typeof(SdrState), status[0]))
                    {
                        _sensorState = (SdrState)status[0];
                    }
                    else
                    {
                        _sensorState = SdrState.Unavailable;
                    }

                    // convert sensor state to enum or return unspecified
                    if (Enum.IsDefined(typeof(SdrState), status[1]))
                    {
                        _scanning = (SdrState)status[1];
                    }
                    else
                    {
                        _scanning = SdrState.Unavailable;
                    }

                }
                #endregion

                // convert event offset(s) to byte array
                EventOffset = base.SdrEventOffset(stateOffset);
            }

            #region private Variables

            /// <summary>
            /// sensor state
            /// </summary>
            private SdrState _sensorState = SdrState.Unavailable;

            /// <summary>
            /// sensor scanning state
            /// </summary>
            private SdrState _scanning = SdrState.Unavailable;

            // returns analog value.
            private bool _isNumeric = false;

            // default linerization state.
            private Linearization _linearization = Linearization.Linear;

            // byte array for sensor reading factors
            private byte[] _rawfactors = new byte[6];

            // signals factors were provided.
            private bool _setFactors = false;

            // sensor reading factors
            private SdrConversionFactors _factors;

            /// <summary>
            /// Reading factors complement
            /// </summary>
            private byte _complement;

            /// <summary>
            /// Indicates how Threasholds are accessed
            /// IPMI 2.0 Table 43-1: Byte[12]: Sensor Capabilities
            ///     Sensor Threshold Access Support
            ///     [3:2]   - 00b = no thresholds
            ///             - 01b = thresholds are readable per reading mask
            ///             - 10b = thresholds are readable & seattable per reading mask
            ///             - 11b = fixed, unreadable thresholds.  Hard-coded in sensor
            /// </summary>
            private byte _readableThresholds;

            /// <summary>
            /// defines sdr record type
            /// </summary>
            private readonly SdrRecordType _sdrType;

            /// <summary>
            /// sensor data record Id
            /// </summary>
            private readonly short _recordId;

            /// <summary>
            /// sensor data record version number (1.5)
            /// </summary>
            private readonly Decimal _version;

            /// <summary>
            /// sensor base unit
            /// </summary>
            private SensorUnitTypeCode _baseUnit;

            /// <summary>
            /// sensor unit rate
            /// </summary>
            private SdrUnitRate _unitRate;

            /// <summary>
            /// sensor modifier unit
            /// </summary>
            private SensorUnitTypeCode _modifierUnit;

            /// <summary>
            /// indicates whether the sensor provides
            /// a nominal reading or not.
            /// </summary>
            private bool _hasNominal;

            /// <summary>
            /// nominal reading
            /// </summary>
            private Double _nominalReading;

            /// <summary>
            /// normal maximum reading
            /// </summary>
            private Double _nomnalMaximum;

            /// <summary>
            /// normal minimum reading
            /// </summary>
            private Double _nomnalMinimum;

            /// <summary>
            /// maximum sensor reading
            /// </summary>
            private Double _sensorMaximum;

            /// <summary>
            /// minimum sensor reading
            /// </summary>
            private Double _sensorMinimum;

            /// <summary>
            /// indicates whether threshold values are provided by the sensor
            /// </summary>
            private bool _hasThresholds;

            /// <summary>
            /// upper non-recoverable threshold value
            /// </summary>
            private Double _thresholdUpperNonRecoverable;

            /// <summary>
            /// upper critical threshold value
            /// </summary>
            private Double _thresholdUpperCritical;

            /// <summary>
            /// upper non-critical threshold value
            /// </summary>
            private Double _thresholdUpperNonCritical;

            /// <summary>
            /// lower non-recoverable threshold value
            /// </summary>
            private Double _thresholdLowerNonRecoverable;

            /// <summary>
            /// lower critical threshold value
            /// </summary>
            private Double _thresholdLowerCritical;

            /// <summary>
            /// lower non-critical threshold value
            /// </summary>
            private Double _thresholdLowerNonCritical;

            #endregion

            /// <summary>
            /// Gets full Sensor Record
            /// </summary>  
            private void Initialize()
            {
                // full & compact sensors have the same
                // properties wrapped in a shared method
                SdrCommonHeader<FullSensorRecord>(this._record);

                // SdrCommonHeader: [3]  = threshold support.
                this.ThresholdReadable = capabilities[3];

                #region Units

                SetUnit(21);

                #endregion

                #region Analog data format

                SetAnalogDataFormat(20);

                #endregion

                #region linearization & factors

                // IPMI Spec [Table  43-1] Byte 24 Linearization 
                SetFactors(23);

                #endregion

                #region Analog characteristic flags.

                SetAnalogFlags(30);

                #endregion

                #region Sensor Max/Min

                // sensor maximum reading
                SetSensorMaximum(34);

                // sensor minimum reading
                SetSensorMinimum(35);

                #endregion

                #region Threshold Values

                SetSensorThresholds(10, 18, 36);

                #endregion

                #region Reverse Upper/Lower readings for 1x

                // reverse all upper lower readings.
                if (Linearization == Linearization.OneX)
                {
                    ReverseThresholds();
                }
                #endregion

                #region Description

                base.SetDescription(47);

                #endregion

            }

            /// <summary>
            /// Set the Sensor Data Record unit type
            /// </summary>
            private void SetUnit(int index)
            {
                // reacord[21] =  Sensor Units Base Unit
                // convert base unit to enum or return unspecified
                if (Enum.IsDefined(typeof(SensorUnitTypeCode), base._record.RecordData[index]))
                {
                    _baseUnit = (SensorUnitTypeCode)base._record.RecordData[index];
                }
                else
                {
                    _baseUnit = SensorUnitTypeCode.Unspecified;
                }

                // reacord[22] =  Sensor Modifer Unit
                if (Enum.IsDefined(typeof(SensorUnitTypeCode), base._record.RecordData[index+1]))
                {
                    _modifierUnit = (SensorUnitTypeCode)base._record.RecordData[index+1];
                }
                else
                {
                    _modifierUnit = SensorUnitTypeCode.Unspecified;
                }
            
            }

            /// <summary>
            /// Set the Sensor Data Record analog data format
            /// </summary>
            private void SetAnalogDataFormat(int index)
            {

                // IPMI Spec [Table  43-1] ** Specifies threshold and ‘analog’ reading, if ‘analog’ reading provided. If neither 
                // thresholds nor analog reading are provided, this field should be written as 00h. 
                // sensor units 1. [7:6] analog data format, [5:3] rate unit, [2:1] modifier unit, [0] percentage 
                byte[] sensorUnits = IpmiSharedFunc.ByteSplit(base._record.RecordData[index], new int[4] { 6, 3, 1, 0 });

                // set the complement.
                this._complement = sensorUnits[0];

                // sensor units 1. [7:6] analog data format. Spec Byte [21], 
                // 00b = unsigned 
                // 01b = 1’s complement (signed) 
                // 10b = 2’s complement (signed) 
                // 11b = Does not return analog (numeric) reading
                // analog data format. 0x03 indicates no numeric reading is returned
                if (this._complement < 0x03)
                {
                    _isNumeric = true;
                }

                // convert base unit to enum or return unspecified
                if (Enum.IsDefined(typeof(SdrUnitRate), sensorUnits[1]))
                {
                    _unitRate = (SdrUnitRate)sensorUnits[1];
                }
                else
                {
                    _unitRate = SdrUnitRate.none;
                }
            
            }

            /// <summary>
            /// Set the Sensor Data Record conversion factors
            /// </summary>
            private void SetFactors(int index)
            {

                // IPMI Spec [Table  43-1] Byte 24 Linearization 
                // [7] -  reserved 
                // [6:0] -  enum (linear, ln, log10, log2, e, exp10, exp2, 1/x, sqr(x), cube(x), sqrt(x), 
                //                cube-1 (x) )
                //                70h = non-linear. 71h-7Fh = non-linear, OEM defined. 
                byte linearByte = (byte)(base._record.RecordData[index] & 0x7F);

                // if linearization < 11, sensor reading is solved with
                // standard linear funciton and factors
                if (linearByte <= 0x0B)
                {
                    if (Enum.IsDefined(typeof(Linearization), linearByte))
                    {
                        Linearization = (Linearization)linearByte;
                    }

                    this._setFactors = true;

                    // copy raw factors from recordData array to rawfactors array
                    Buffer.BlockCopy(base._record.RecordData, (index+1), _rawfactors, 0, 6);

                }
                // sensor must be non-linear and requires sensor reading factors
                else if (linearByte > 0x0B && linearByte <= 0x7F)
                {
                    // non linear reading
                    Linearization = Linearization.Nonlinear;

                    // non linear sensors require the get sensor reading factores command to retrieve reading
                    // factors.  ReadingFactorsRequest(Sensornumber, SensorReading)
                    // this should be provided during initialization.
                    // TODO Add checking.
                    if (!_setFactors)
                    { }
                }

                // extract raw conversion factors by passing rawfactors array and complement type
                _factors = new SdrConversionFactors(_rawfactors, this._complement);
            
            }

            /// <summary>
            /// Set the Sensor Data Record analog reading flags
            /// </summary>
            private void SetAnalogFlags(int index)
            {
                // analog characteristic flags
                bool normalMin = (base._record.RecordData[index] & 0x04) == 4 ? true : false;
                bool normalMax = (base._record.RecordData[index] & 0x02) == 2 ? true : false;
                bool nominal = (base._record.RecordData[index] & 0x01) == 1 ? true : false;

                if (nominal)
                {
                    // update sdr message to indicate sensor provides nominal values
                    HasNominal = true;

                    // formatted nominal value
                    NominalReading = ConvertReading(base._record.RecordData[(index+1)]);
                }
                if (normalMax)
                {
                    // formatted normal maximum
                    NomnalMaximum = ConvertReading(base._record.RecordData[(index + 2)]);
                }
                if (normalMin)
                {
                    // formatted normal minimum
                    NomnalMinimum = ConvertReading(base._record.RecordData[(index + 3)]);
                }
            }

            /// <summary>
            /// Set the Sensor Data Record maximum values
            /// </summary>
            private void SetSensorMaximum(int index)
            {
                // sensor maximum reading
                SensorMaximum = ConvertReading(base._record.RecordData[index]);
            }

            /// <summary>
            /// Set the Sensor Data Record minimum values
            /// </summary>
            private void SetSensorMinimum(int index)
            {
                // sensor minimum reading
                SensorMinimum = ConvertReading(base._record.RecordData[index]);
            }

            /// <summary>
            /// Set the Sensor Data Record Threshold values
            /// </summary>
            private void SetSensorThresholds(int initialization, int mask, int index)
            {
                // Use of this field is based on Settable Threshold Mask. If the corresponding bit is 
                // set in the mask byte and the ‘Init Sensor Thresholds’ bit is also set, then this 
                // value will be used for initializing the sensor threshold. Otherwise, this value 
                // should be ignored.

                // Sensor Initialization = RecordData[10]
                // bit 4 = Init Sensor Thresholds.
                if ((base._record.RecordData[initialization] & 0x10) == 16)
                {
                    // update sdr message to indicate sensor provides threshold values
                    HasThresholds = true;

                    // settable threshold mask bit 5 = upper non-recoverable threshold is settable 
                    if ((base._record.RecordData[mask] & 0x20) == 32)
                    {
                        // upper non-recoverable threshold
                        ThresholdUpperNonRecoverable = ConvertReading(base._record.RecordData[index]);
                    }

                    // settable threshold mask bit 4 = upper critical threshold is settable 
                    if ((base._record.RecordData[mask] & 0x10) == 16)
                    {
                        // upper critical threshold
                        ThresholdUpperCritical = ConvertReading(base._record.RecordData[(index + 1)]);
                    }

                    // settable threshold mask bit 3 = upper non-critical threshold is settable 
                    if ((base._record.RecordData[mask] & 0x08) == 8)
                    {
                        // upper non-critical threshold
                        ThresholdUpperNonCritical = ConvertReading(base._record.RecordData[(index + 2)]);
                    }

                    // settable threshold mask bit 2 = lower non-recoverable threshold
                    if ((base._record.RecordData[mask] & 0x04) == 4)
                    {
                        // lower non-recoverable threshold
                        ThresholdLowerNonRecoverable = ConvertReading(base._record.RecordData[(index + 3)]);
                    }

                    // settable threshold mask bit 1 = lower critical threshold 
                    if ((base._record.RecordData[mask] & 0x02) == 2)
                    {
                        // lower critical threshold 
                        ThresholdLowerCritical = ConvertReading(base._record.RecordData[(index + 4)]);
                    }

                    // settable threshold mask bit 0 = lower non-critical threshold 
                    if ((base._record.RecordData[mask] & 0x01) == 1)
                    {
                        // lower non-critical threshold 
                        ThresholdLowerNonCritical = ConvertReading(base._record.RecordData[(index + 5)]);
                    }
                }
            
            }

            /// <summary>
            /// Some linear sensor types require reverse thresholds
            /// </summary>
            private void ReverseThresholds()
            {
                // holds upper value, during reverse
                Double upper;

                // holds lower value, during reverse
                Double lower;

                if (NomnalMaximum != 0)
                {
                    // set temp upper an lower values
                    upper = NomnalMaximum;
                    lower = NomnalMinimum;

                    // swap/reverse record values
                    NomnalMaximum = lower;
                    NomnalMinimum = upper;
                }

                if (_sensorMaximum != 0)
                {
                    // set temp upper an lower values
                    upper = SensorMaximum;
                    lower = SensorMinimum;

                    // swap/reverse record values
                    SensorMaximum = lower;
                    SensorMinimum = upper;
                }

                if (ThresholdUpperCritical != 0)
                {
                    // set temp upper an lower values
                    upper = ThresholdUpperCritical;
                    lower = ThresholdLowerCritical;

                    // swap/reverse record values
                    ThresholdUpperCritical = lower;
                    ThresholdLowerCritical = upper;
                }

                if (ThresholdUpperNonCritical != 0)
                {
                    // set temp upper an lower values
                    upper = ThresholdUpperNonCritical;
                    lower = ThresholdLowerNonCritical;

                    // swap/reverse record values
                    ThresholdUpperNonCritical = lower;
                    ThresholdLowerNonCritical = upper;
                }

                if (ThresholdUpperNonRecoverable != 0)
                {
                    // set temp upper an lower values
                    upper = ThresholdUpperNonRecoverable;
                    lower = ThresholdLowerNonRecoverable;

                    // swap/reverse record values
                    ThresholdUpperNonRecoverable = lower;
                    ThresholdLowerNonRecoverable = upper;
                }
            
            }

            /// <summary>
            /// Convert Sensor Reading
            /// </summary>
            public double ConvertReading(byte rawReading, byte[] rawfactors)
            {
                 // extract raw conversion factors by passing rawfactors array and complement type
                this._factors = new SdrConversionFactors(_rawfactors, this._complement);
                
                return ConvertReading(rawReading);
            }

            /// <summary>
            /// Convert Sensor Reading
            /// </summary>
            /// <param name="rawReading"></param>
            /// <returns></returns>
            public double ConvertReading(byte rawReading)
            {
                // switch raw sensor reading between unsigned, 1's complement signed
                // or 2's complement signed
                double value = _factors.FormatReading(rawReading);

                // reading formatted in units. add formatted current reading to the sdr message collection
                return IpmiSharedFunc.Linearize(_linearization, (_factors.ConvertReading(value)));
            }

            #region Public Properties

            /// <summary>
            /// Sensor state
            /// </summary>
            public SdrState SensorState
            {
                get { return this._sensorState; }
                private set { this._sensorState = value; }
            }

            /// <summary>
            /// Sensor scanning state
            /// </summary>
            public SdrState Scanning
            {
                get { return this._scanning; }
                private set { this._scanning = value; }
            }

            /// <summary>
            /// Indicates the sensor returns a numeric value
            /// </summary>
            public bool IsNumeric
            {
                get { return this._isNumeric; }
            }

            // Sdr record Type
            public SdrRecordType SdrType
            {
                get { return this._sdrType; }
            }

            /// <summary>
            /// sensor data record Id
            /// </summary>
            public short RecordId
            {
                get { return this._recordId; }
            }

            /// <summary>
            /// sensor data record version number (1.5)
            /// </summary>
            public Decimal Version
            {
                get { return this._version; }
            }

            /// <summary>
            /// sensor base unit
            /// </summary>
            public SensorUnitTypeCode BaseUnit
            {
                get { return this._baseUnit; }
                private set { this._baseUnit = value; }
            }

            /// <summary>
            /// sensor unit rate
            /// </summary>
            public SdrUnitRate UnitRate
            {
                get { return this._unitRate; }
                private set { this._unitRate = value; }
            }

            /// <summary>
            /// sensor modifier unit
            /// </summary>
            public SensorUnitTypeCode ModifierUnit
            {
                get { return this._modifierUnit; }
                private set { this._modifierUnit = value; }
            }

            /// <summary>
            /// indicates whether the sensor provides
            /// a nominal reading or not.
            /// </summary>
            public bool HasNominal
            {
                get { return this._hasNominal; }
                private set { this._hasNominal = value; }
            }

            /// <summary>
            /// nominal reading
            /// </summary>
            public Double NominalReading
            {
                get { return this._nominalReading; }
                private set { this._nominalReading = value; }
            }

            /// <summary>
            /// normal maximum reading
            /// </summary>
            public Double NomnalMaximum
            {
                get { return this._nomnalMaximum; }
                private set { this._nomnalMaximum = value; }
            }

            /// <summary>
            /// normal minimum reading
            /// </summary>
            public Double NomnalMinimum
            {
                get { return this._nomnalMinimum; }
                private set { this._nomnalMinimum = value; }
            }

            /// <summary>
            /// maximum sensor reading
            /// </summary>
            public Double SensorMaximum
            {
                get { return this._sensorMaximum; }
                private set { this._sensorMaximum = value; }
            }

            /// <summary>
            /// minimum sensor reading
            /// </summary>
            public Double SensorMinimum
            {
                get { return this._sensorMinimum; }
                private set { this._sensorMinimum = value; }
            }

            /// <summary>
            /// Indicates whether threshold values are provided by the sensor
            /// </summary>
            public bool HasThresholds
            {
                get { return this._hasThresholds; }
                private set { this._hasThresholds = value; }
            }

            /// <summary>
            /// Sensor Linearization
            /// </summary>
            public Linearization Linearization
            {
                get { return this._linearization; }
                private set { this._linearization = value; }
            }

            /// <summary>
            /// Indicates how Threasholds are accessed
            /// IPMI 2.0 Table 43-1: Byte[12]: Sensor Capabilities
            /// Sensor Threshold Access Support
            /// [3:2]   - 00b = no thresholds
            ///         - 01b = thresholds are readable per reading mask
            ///         - 10b = thresholds are readable & seattable per reading mask
            ///         - 11b = fixed, unreadable thresholds.  Hard-coded in sensor
            /// </summary>
            public byte ThresholdReadable
            {
                get { return this._readableThresholds; }
                private set { this._readableThresholds = value; }
            }

            /// <summary>
            /// upper non-recoverable threshold value
            /// </summary>
            public Double ThresholdUpperNonRecoverable
            {
                get { return this._thresholdUpperNonRecoverable; }
                private set { this._thresholdUpperNonRecoverable = value; }
            }

            /// <summary>
            /// upper critical threshold value
            /// </summary>
            public Double ThresholdUpperCritical
            {
                get { return this._thresholdUpperCritical; }
                private set { this._thresholdUpperCritical = value; }
            }

            /// <summary>
            /// upper non-critical threshold value
            /// </summary>
            public Double ThresholdUpperNonCritical
            {
                get { return this._thresholdUpperNonCritical; }
                private set { this._thresholdUpperNonCritical = value; }
            }

            /// <summary>
            /// lower non-recoverable threshold value
            /// </summary>
            public Double ThresholdLowerNonRecoverable
            {
                get { return this._thresholdLowerNonRecoverable; }
                private set { this._thresholdLowerNonRecoverable = value; }
            }

            /// <summary>
            /// lower critical threshold value
            /// </summary>
            public Double ThresholdLowerCritical
            {
                get { return this._thresholdLowerCritical; }
                private set { this._thresholdLowerCritical = value; }
            }

            /// <summary>
            /// lower non-critical threshold value
            /// </summary>
            public Double ThresholdLowerNonCritical
            {
                get { return this._thresholdLowerNonCritical; }
                private set { this._thresholdLowerNonCritical = value; }
            }

            #endregion

        }

        /// <summary>
        /// Compact Sensor Data Record.
        /// </summary>
        internal class CompactSensorRecord : SensorMetadataBase
        {
            /// <summary>
            /// Initialize the sensor meta data
            /// </summary>
            private void Initialize()
            {
                // full & compact sensors have the same
                // properties wrapped in a shared method
                SdrCommonHeader<CompactSensorRecord>(this._record);

                // initialization[6] = 1b event generation enabled 
                if (initialization[6] == 0x01)
                {
                    SetState(_rawStatusByte, _rawStatusOffset);
                }
                else
                {
                    // set state to unavailable
                    _sensorState = SdrState.Unavailable;

                    // set scanning to unavilable
                    _scanning = SdrState.Unavailable;

                    // convert event offset(s) to byte array
                    base.EventOffset = base.SdrEventOffset(0x00);
                }

                // set the description
                base.SetDescription(31);
            }

            /// <summary>
            /// Set the Sensor State after reading.
            /// </summary>
            /// <param name="sensorStatus"></param>
            /// <param name="stateOffset"></param>
            internal void SetState(byte sensorStatus, byte stateOffset)
            {
                // split 
                byte[] status = IpmiSharedFunc.ByteSplit(sensorStatus, new int[4] { 7, 6, 5, 0 });

                // sensor state unavailable
                if (status[2] == 0x01)
                {
                    // set state to unavailable
                    _sensorState = SdrState.Unavailable;

                    // set scanning to unavilable
                    _scanning = SdrState.Unavailable;

                }
                // sensor state available
                else
                {

                    // convert sensor state to enum or return unspecified
                    if (Enum.IsDefined(typeof(SdrState), status[0]))
                    {
                        _sensorState = (SdrState)status[0];
                    }
                    else
                    {
                        _sensorState = SdrState.Unavailable;
                    }

                    // convert sensor state to enum or return unspecified
                    if (Enum.IsDefined(typeof(SdrState), status[1]))
                    {
                        _scanning = (SdrState)status[1];
                    }
                    else
                    {
                        _scanning = SdrState.Unavailable;
                    }

                }

                // convert event offset(s) to byte array
                base.EventOffset = base.SdrEventOffset(stateOffset);

            }

            /// <summary>
            /// initializes instance of the class
            /// </summary>
            /// <param name="recordId">sdr record id</param>
            /// <param name="version">sdr version </param>
            /// <param name="sdrType">sdr record type</param>
            internal CompactSensorRecord(short recordId, Decimal version, SdrRecordType sdrType, SdrRecord record) : base(record)
            {
                this._recordId = recordId;
                this._version = version;
                this._sdrType = sdrType;

                this.Initialize();
            }

            #region Private Variables

                /// <summary>
                /// Sensor Reading State Byte
                /// </summary>
                private byte _rawStatusByte = 0x00;

                /// <summary>
                /// Sensor Reading Event Offset
                /// </summary>
                private byte _rawStatusOffset = 0x00;

                /// <summary>
                /// defines sdr record type
                /// </summary>
                private readonly SdrRecordType _sdrType;

                /// <summary>
                /// sensor data record Id
                /// </summary>
                private readonly short _recordId;

                /// <summary>
                /// sensor data record version number (1.5)
                /// </summary>
                private readonly Decimal _version;

                /// <summary>
                /// sensor state
                /// </summary>
                private SdrState _sensorState;

                /// <summary>
                /// sensor scanning state
                /// </summary>
                private SdrState _scanning;

            #endregion

            /// <summary>
            /// sensor capabilities (internal)
            /// </summary>
            public SdrRecordType SdrType
            {
                get { return this._sdrType; }
            }

            /// <summary>
            /// sensor state
            /// </summary>
            public SdrState SensorState
            {
                get { return this._sensorState; }
            }

            /// <summary>
            /// sensor scanning state
            /// </summary>
            public SdrState Scanning
            {
                get { return this._scanning; }
            }

        }

        /// <summary>
        /// Event Only Sensor Data Record.
        /// </summary>
        internal class SdrEventOnlyRecord : SensorMetadataBase
        {

            /// <summary>
            /// initializes instance of the class
            /// </summary>
            internal SdrEventOnlyRecord(short recordId, Decimal version, SdrRecordType sdrType, SdrRecord record)
                : base(record)
            {
                SdrCommonHeader<CompactSensorRecord>(this._record);

                this._recordId = recordId;
                this._version = version;
                this._sdrType = sdrType;

                this.Initialize();
            }

            /// <summary>
            /// Event Only Sensor Data Record.
            /// </summary>  
            private void Initialize()
            {
                // record[7] = Sensor Number
                base.SensorNumber = base._record.RecordData[7];

                // set the description
                base.SetDescription(16);
            }

            #region Private Variables

            /// <summary>
            /// defines sdr record type
            /// </summary>
            private readonly SdrRecordType _sdrType;

            /// <summary>
            /// sensor data record Id
            /// </summary>
            private readonly short _recordId;

            /// <summary>
            /// sensor data record version number (1.5)
            /// </summary>
            private readonly Decimal _version;

            #endregion

            public SdrRecordType SdrType
            { get { return this._sdrType; } }

            public short RecordId
            { get { return this._recordId; } }

            public Decimal Version
            { get { return this._version; } }

        }

    #endregion

    /// <summary>
    /// System Event Log Entry Response. (Called by GetSel method).
    /// Contains raw SEL Data
    /// </summary>
    internal class IpmiSelCollection : CollectionBase
    {
        internal void Add(SelEntryResponse response)
        {
            this.List.Add(response);
        }

        internal void Remove(SelEntryResponse response)
        {
            this.List.Remove(response);
        }

        internal SelEntryResponse this[int index]
        {
            get { return this.List[index] as SelEntryResponse; }
            set { this.List[index] = value; }
        }
    }

    /// <summary>
    /// sdr record collection. (used by Getsdr method).
    /// </summary>
    internal class IpmiSdrCollection : CollectionBase
    {
        public void Add(SdrRecord response)
        {
            this.List.Add(response);
        }

        public void Remove(SdrRecord response)
        {
            this.List.Remove(response);
        }

        public SdrRecord this[int index]
        {
            get { return this.List[index] as SdrRecord; }
            set { this.List[index] = value; }
        }
    }

    /// <summary>
    /// collection of sensor data records (string formatted).
    /// </summary>
    public class SdrCollection : CollectionBase
    {
        // completion code
        internal byte completionCode;

        // command trace.
        internal string command = string.Empty;

        /// <summary>
        /// Completion Code
        /// </summary>
        public byte CompletionCode
        {
            get { return this.completionCode; }
            set { this.completionCode = value; }
        }

        /// <summary>
        /// collection of sensor data records (string formatted).
        /// </summary>
        public SdrCollection()
        { }

        /// <summary>
        /// collection of sensor data records (string formatted).
        /// </summary>
        public SdrCollection(byte completioncode)
        {
            this.completionCode = completioncode;
        }

        /// <summary>
        /// Command Trace for Code
        /// </summary>
        public string Command
        {
            get { return this.command; }
            set { this.command = value; }
        }

        public void Add(SensorMetadataBase response)
        {
            this.List.Add(response);
        }

        public void Remove(SensorMetadataBase response)
        {
            this.List.Remove(response);
        }

        public SensorMetadataBase this[int index]
        {
            get { return this.List[index] as SensorMetadataBase; }
            set { this.List[index] = value; }
        }
    }

    /// <summary>
    /// Sensor Reading Factors collection
    /// </summary>
    internal class SensorFactorCollection : CollectionBase
    {
        internal void Add(SensorFactor response)
        {
            this.List.Add(response);
        }

        internal void Remove(SensorFactor response)
        {
            this.List.Remove(response);
        }

        internal SensorFactor this[int index]
        {
            get { return this.List[index] as SensorFactor; }
            set { this.List[index] = value; }
        }
    }

    /// <summary>
    /// serialized sensor data record
    /// </summary>
    internal class SdrRecord
    {
        /// <summary>
        /// Ipmi CompletionCode
        /// </summary>
        public byte completionCode;

        /// <summary>
        /// sdr record id
        /// </summary>
        public byte[] RecordId;

        /// <summary>
        /// sdr type (full, compact, etc)
        /// </summary>
        public byte RecordType;

        /// <summary>
        /// sdr data lenght
        /// </summary>
        public byte RecordLenght;

        /// <summary>
        /// sdr version
        /// </summary>
        public byte RecordVersion;

        /// <summary>
        /// sdr data
        /// </summary>
        public byte[] RecordData;
    }

    /// <summary>
    /// Sensor Reading Factors
    /// </summary>
    internal class SensorFactor
    {
        internal byte[] NextReading = null;
        internal byte[] MLsBits = null;
        internal byte[] MMsBits = null;
        internal byte[] Tolerance = null;
        internal byte[] BLsBits = null;
        internal byte[] BMsBits = null;
        internal byte[] AccuracyLs = null;
        internal byte[] AccuracyMs = null;
        internal byte[] Rexponent = null;
        internal byte[] Bexponent = null;
    }
}
