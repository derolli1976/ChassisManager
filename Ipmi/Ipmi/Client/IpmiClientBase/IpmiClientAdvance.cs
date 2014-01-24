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
    using System.Globalization;
    using System.Collections.Generic;

    internal abstract class IpmiClientAdvance : IpmiClientBasic
    {

        #region Sensor Reading

        /// <summary>
        ///  Get Sensor Data Repository. Returns SDR Info.
        /// </summary>
        public virtual SdrCollection GetSdr()
        {
            // Default Record Off Set
            int offSet = 0;

            // Number of Bytes to Read. 0xFF for entire record.
            byte bytesToRead = 0xFF;

            // SDR RecordId (0000h for entry point)
            ushort recordId = 0;

            // Last SDR RecordId (aborts event log Loop)
            ushort lastRecordId = 65535;

            // security mech to step out of loop.
            int pass = 0;

            // create sdr record collection for raw SDR records.
            IpmiSdrCollection records = new IpmiSdrCollection();

            // reserve the SDR for partial reads
            ReserveSdrResponse reserve = (ReserveSdrResponse)this.IpmiSendReceive(
            new ReserveSdrRequest(), typeof(ReserveSdrResponse));

            if (reserve.CompletionCode == 0)
            {

                // reserved LS byte
                byte reserveLs = reserve.ReservationLS;

                // reserved MS byte
                byte reserveMs = reserve.ReservationMS;

                // retrieve all records while connected by recursively calling the SDR entry command 
                while (recordId != lastRecordId || pass > 1000)
                {
                    // create SDR record
                    SdrRecord sdr = new SdrRecord();
                    {
                        // get the SEL record
                        GetSdrPartialResponse response = (GetSdrPartialResponse)this.IpmiSendReceive(
                        new GetSdrPartialRequest(reserveLs, reserveMs, recordId, offSet, bytesToRead), typeof(GetSdrPartialResponse));

                        if (response.CompletionCode == 0)
                        {
                            sdr.completionCode = response.CompletionCode;

                            // set record id
                            sdr.RecordId = new byte[2] { response.RecordData[1], response.RecordData[0] };

                            // set the record version
                            sdr.RecordVersion = response.RecordData[2];

                            // set record type
                            sdr.RecordType = response.RecordData[3];

                            // set record lenght
                            sdr.RecordLenght = response.RecordData[4];

                            // set the record data to record data
                            sdr.RecordData = response.RecordData;

                            // update the record Id (signals loop exit)
                            recordId = BitConverter.ToUInt16(new byte[2] { response.RecordIdMsByte, response.RecordIdLsByte }, 0);
                        }
                        else
                        {
                            sdr.completionCode = response.CompletionCode;
                            break;
                        }
                    }

                    pass++;

                    // add the record to the collection
                    records.Add(sdr);
                }
            }

            // return collection
            SdrCollection sdrMessages = new SdrCollection();

            // check response collection holds values
            if (records.Count > 0)
            {
                // sdr version array
                byte[] verarr = new byte[2];

                // record id
                short id;

                foreach (SdrRecord record in records)
                {
                    if (record.completionCode == 0)
                    {
                        // set the sdr collection completion code to indicate a failure occurred
                        sdrMessages.completionCode = record.completionCode;

                        // record Id
                        id = BitConverter.ToInt16(record.RecordId, 0);

                        // populdate version array
                        Buffer.BlockCopy(IpmiSharedFunc.ByteSplit(record.RecordVersion, new int[2] { 4, 0 }), 0, verarr, 0, 2);

                        string sVersion = Convert.ToUInt16(verarr[1]).ToString(CultureInfo.InvariantCulture) + "." + Convert.ToInt16(verarr[0]).ToString(CultureInfo.InvariantCulture);

                        // set version
                        Decimal version = 0;
                        // sdr record version number
                        if (!decimal.TryParse(sVersion, out version)) { version = 0; }

                        GetSdrMetatData(id, version, record.RecordType, record, ref sdrMessages);

                    }
                    // set the sdr completion code to indicate a failure occurred
                    sdrMessages.completionCode = record.completionCode;
                }
            }

            return sdrMessages;



        }

        /// <summary>
        ///  Get Sensor Data Repository Information Incrementally. Returns SDR Info.
        /// </summary>
        public virtual SdrCollection GetSdrIncrement()
        {
            // Default Record Off Set
            int offSet = 0;

            // Number of Bytes to Read. 0xFF for entire record.
            int bytesToRead = 0;

            // SDR RecordId (0000h for entry point)
            ushort recordId = 0;

            // Last SDR RecordId (aborts event log Loop)
            ushort lastRecordId = 65535;

            // security mech to step out of loop.
            int pass = 0;

            // create sdr record collection for raw SDR records.
            IpmiSdrCollection records = new IpmiSdrCollection();

            // reserve the SDR for partial reads
            ReserveSdrResponse reserve = (ReserveSdrResponse)this.IpmiSendReceive(
            new ReserveSdrRequest(), typeof(ReserveSdrResponse));

            if (reserve.CompletionCode == 0)
            {

                // reserved LS byte
                byte reserveLs = reserve.ReservationLS;

                // reserved MS byte
                byte reserveMs = reserve.ReservationMS;

                // lenght of the SDR record
                int recordLenght = 0;

                // sdr record index
                int index = 0;

                // retrieve all records while connected by recursively calling the SDR entry command 
                while (recordId != lastRecordId || pass > 1000)
                {
                    // create SDR record
                    SdrRecord sdr = new SdrRecord();
                    {
                        // initialize offset to zero for new record
                        offSet = 0;

                        // initialize to the minimum read size of 5 bytes. (extended to 14 on incremental read)
                        bytesToRead = 5;

                        // get the SEL record
                        GetSdrPartialResponse response = (GetSdrPartialResponse)this.IpmiSendReceive(
                        new GetSdrPartialRequest(reserveLs, reserveMs, recordId, offSet, bytesToRead), typeof(GetSdrPartialResponse));

                        // set the sdr completion code.
                        sdr.completionCode = response.CompletionCode;

                        if (response.CompletionCode == 0)
                        {
                            // set record id
                            sdr.RecordId = new byte[2] { response.RecordData[1], response.RecordData[0] };

                            // set the record version
                            sdr.RecordVersion = response.RecordData[2];

                            // set record type
                            sdr.RecordType = response.RecordData[3];

                            // set record lenght
                            sdr.RecordLenght = response.RecordData[4];

                            // convert record lenght to int & add the initial 5 bytes.
                            recordLenght = (Convert.ToInt32(response.RecordData[4])) + 5;

                            // create SDR byte array to hold record data.
                            byte[] sdrDataRecord = new byte[recordLenght];

                            // initilize sdr array index to zero
                            index = 0;

                            // copy SDR data to the sdr data record array and increase the index
                            Buffer.BlockCopy(response.RecordData, 0, sdrDataRecord, index, bytesToRead);
                            index += bytesToRead;

                            // increase the offset by bytes already read
                            offSet += bytesToRead;

                            int offsetPass = 0;

                            // recursively get partial sdr record until the offset reaches the recordlenght
                            while (offSet < recordLenght || offsetPass > 100)
                            {
                                // get maximum read chunk (14 bytes or less).
                                bytesToRead = (recordLenght - offSet);
                                // the size to 14 byte increments
                                bytesToRead = (bytesToRead > 14) ? 14 : bytesToRead;

                                // get partial SDR record
                                GetSdrPartialResponse partialResponse = (GetSdrPartialResponse)this.IpmiSendReceive(
                                new GetSdrPartialRequest(reserve.ReservationLS, reserve.ReservationMS, recordId, offSet, bytesToRead), typeof(GetSdrPartialResponse));

                                if (partialResponse.CompletionCode == 0)
                                {
                                    // copy SDR data to the sdr data array and increase the index size by bytes read.
                                    Buffer.BlockCopy(partialResponse.RecordData, 0, sdrDataRecord, index, bytesToRead);
                                    index += bytesToRead;

                                    // increase the offset by the bytes read
                                    offSet += bytesToRead;
                                }
                                else
                                {
                                    // set the sdr completion code.
                                    sdr.completionCode = partialResponse.CompletionCode;
                                    break;
                                }

                                offsetPass++;
                            }

                            // set the record data to the fully populdated sdrDataRecord array
                            sdr.RecordData = sdrDataRecord;

                            // update the record Id (signals loop exit)
                            recordId = BitConverter.ToUInt16(new byte[2] { response.RecordIdMsByte, response.RecordIdLsByte }, 0);
                        }
                        else
                        {
                            break;
                        }
                    }

                    pass++;

                    // add the record to the collection
                    records.Add(sdr);
                }
            }

            // return collection
            SdrCollection sdrMessages = new SdrCollection();
            sdrMessages.completionCode = reserve.CompletionCode;

            if (reserve.CompletionCode != 0)
                sdrMessages.command = typeof(ReserveSdrResponse).ToString();

            // check response collection holds values
            if (records.Count > 0)
            {
                // sdr version array
                byte[] verarr = new byte[2];

                // record id
                short id;

                foreach (SdrRecord record in records)
                {
                    if (record.completionCode == 0)
                    {
                        // record Id
                        id = BitConverter.ToInt16(record.RecordId, 0);

                        // populdate version array
                        Buffer.BlockCopy(IpmiSharedFunc.ByteSplit(record.RecordVersion, new int[2] { 4, 0 }), 0, verarr, 0, 2);

                        string sVersion = Convert.ToUInt16(verarr[1]).ToString(CultureInfo.InvariantCulture) + "." + Convert.ToInt16(verarr[0]).ToString(CultureInfo.InvariantCulture);

                        // set version
                        Decimal version = 0;
                        // sdr record version number
                        if (!decimal.TryParse(sVersion, out version)) { version = 0; }

                        GetSdrMetatData(id, version, record.RecordType, record, ref sdrMessages);
                    }
                    else
                    {
                        sdrMessages.command += " : " + sdrMessages.completionCode + ". RecordId:" + record.RecordId.ToString();
                        sdrMessages.completionCode = record.completionCode;
                    }
                }
            }

            return sdrMessages;
        }

        /// <summary>
        ///  Get Sensor Data Record Information. Returns Sdr Info.
        /// </summary>
        public virtual SdrRepositoryInfo GetSdrInfo()
        {
            GetSdrRepositoryInfoResponse response = (GetSdrRepositoryInfoResponse)this.IpmiSendReceive(
            new GetSdrRepositoryInfoRequest(), typeof(GetSdrRepositoryInfoResponse));

            // create new SelInfo class
            SdrRepositoryInfo info = new SdrRepositoryInfo(response.CompletionCode);

            if (response.CompletionCode == 0x00)
            {
                if (response.CompletionCode == 0)
                    info.SetParamaters(response.SdrVersion, response.MSByte, response.LSByte,
                        response.SdrFeeSpace, response.LastAdded, response.LastRemoved);
            }

            return info;
        }

        /// <summary>
        /// Gets Sensor Reading Factors
        /// </summary>
        internal ReadingFactorsResponse GetSensorFactors(byte SensorNumber, byte SensorReading)
        {
            ReadingFactorsResponse response = (ReadingFactorsResponse)this.IpmiSendReceive(
            new ReadingFactorsRequest(SensorNumber, SensorReading), typeof(ReadingFactorsResponse));

            return response;
        }

        /// <summary>
        /// Gets Sensor Reading
        /// </summary>
        public virtual SensorReading GetSensorReading(byte SensorNumber, byte SensorType)
        {
            SensorReadingResponse response = (SensorReadingResponse)this.IpmiSendReceive(
            new SensorReadingRequest(SensorNumber), typeof(SensorReadingResponse));

            SensorReading respObj = new SensorReading(response.CompletionCode);
            respObj.SensorNumber = SensorNumber;
            respObj.SensorType = SensorType;

            if (response.CompletionCode == 0x00)
                respObj.SetParamaters(response.SensorReading,
                    response.SensorStatus, response.StateOffset,
                    response.OptionalOffset);

            return respObj;
        }

        /// <summary>
        /// Gets Sensor Reading
        /// </summary>
        public virtual SensorTypeCode GetSensorType(byte SensorNumber)
        {
            SensorTypeResponse response = (SensorTypeResponse)this.IpmiSendReceive(
            new SensorTypeRequest(SensorNumber), typeof(SensorTypeResponse));

            SensorTypeCode respObj = new SensorTypeCode(response.CompletionCode);

            if (response.CompletionCode == 0x00)
                respObj.SetParamaters(response.SensorType,
                    response.EventTypeCode);

            return respObj;
        }

        #endregion

        #region Command Support: Sdr

        /// <summary>
        /// Format Sensor Data Records
        /// </summary>
        internal void GetSdrMetatData(short id, decimal version, byte recordType, SdrRecord record, ref SdrCollection sdrMessages)
        {
            SdrRecordType sdrRecordType;
            int sensorType = (int)recordType;
            // convert message type to to enum or return unspecified
            if (Enum.IsDefined(typeof(SdrRecordType), sensorType))
            {
                sdrRecordType = (SdrRecordType)sensorType;
            }
            else
            {
                sdrRecordType = SdrRecordType.Unspecified;
            }

            switch (sdrRecordType)
            {
                case SdrRecordType.Full: // Full Sensor
                    {
                        #region full Record
                        if (record.RecordData != null)
                        {
                            // Create sensor data record.
                            FullSensorRecord str;

                            // IPMI Spec [Table  43-1] Byte 24 Linearization 
                            // [7] -  reserved 
                            // [6:0] -  enum (linear, ln, log10, log2, e, exp10, exp2, 1/x, sqr(x), cube(x), sqrt(x), 
                            //                cube-1 (x) )
                            //                70h = non-linear. 71h-7Fh = non-linear, OEM defined. 
                            byte linearByte = (byte)(record.RecordData[23] & 0x7F);

                            if (linearByte > 0x0B && linearByte <= 0x7F)
                            {
                                // get current sensor reading using get sensor reading ipmi command
                                SensorReadingResponse reading = (SensorReadingResponse)this.IpmiSendReceive(
                                new SensorReadingRequest(record.RecordData[7]), typeof(SensorReadingResponse));

                                if (reading.CompletionCode == 0)
                                {
                                    // non linear sensors require the get sensor reading factores command to retrieve reading
                                    // factors.  ReadingFactorsRequest(Sensornumber, SensorReading)
                                    ReadingFactorsResponse factors = (ReadingFactorsResponse)this.IpmiSendReceive(
                                    new ReadingFactorsRequest(record.RecordData[7], reading.SensorReading), typeof(ReadingFactorsResponse));

                                    // Initialize the sdr message class with reading & factors
                                    str = new FullSensorRecord(id,
                                                            version,
                                                            sdrRecordType,
                                                            record,
                                                            factors.Factors,
                                                            factors.CompletionCode
                                                            );
                                }
                                else
                                {
                                    // Initialize the sdr message class with reading & factors
                                    str = new FullSensorRecord(id,
                                                            version,
                                                            sdrRecordType,
                                                            record,
                                                            new byte[6], // initialize with blank conversation factors.
                                                            reading.CompletionCode
                                                            );
                                }
                            }
                            else
                            {
                                // Initialize the sdr message class
                                str = new FullSensorRecord(id,
                                                        version,
                                                        sdrRecordType,
                                                        record,
                                                        record.completionCode
                                                        );
                            }

                            // append message to collection
                            sdrMessages.Add(str);
                        }
                        #endregion
                    }
                    break;
                case SdrRecordType.Compact: // Compact Sensor
                    {
                        #region Compact Sensor

                        // Initialize the sdr message class
                        CompactSensorRecord sdr = new CompactSensorRecord(id,
                                                                    version,
                                                                    sdrRecordType,
                                                                    record
                                                                    );
                        //SdrRecordCompact(ref str, record);
                        sdrMessages.Add(sdr);

                        #endregion
                    }
                    break;
                case SdrRecordType.EventOnly: // Event-Only
                    {
                        #region Event Only

                        SdrEventOnlyRecord sdr = new SdrEventOnlyRecord(id,
                                                                        version,
                                                                        sdrRecordType,
                                                                        record
                                                                        );
                        //SdrRecordCompact(ref str, record);
                        sdrMessages.Add(sdr);

                        #endregion
                    }
                    break;
                case SdrRecordType.Association: // Entity Association
                case SdrRecordType.DeviceRelative: // Device-relative Entity
                case SdrRecordType.GenericDevice: // Generic Device Locator 
                case SdrRecordType.FRUDevice: // FRU Device Locator
                case SdrRecordType.ManagementDevice: // Management Controller Device
                case SdrRecordType.ManagementController: // Management Controller Confirmation
                case SdrRecordType.BmcMessageChannel: // BMC Message Channel Info
                case SdrRecordType.Oem: // OEM Record
                default:   // 0x0A = 0x0F reserved
                    break;
            }
        }

        #endregion

        #region System Event Log

        /// <summary>
        /// Reset SEL Log
        /// </summary>
        public virtual bool ClearSel()
        {
            // reserve sel to prevent new entries.
            SelReserveResponse response =
                (SelReserveResponse)this.IpmiSendReceive(
                      new SelReserveRequest(),
                        typeof(SelReserveResponse));

            if (response.CompletionCode == 0x00)
            {
                // Send Clear Sel Request
                SelLogClearResponse response2 =
                    (SelLogClearResponse)this.IpmiSendReceive(
                          new SelLogClearRequest(new Byte[] { response.ReservationIdLS, response.ReservationIdMS }, SelLogClearRequest.InitiateErase),
                            typeof(SelLogClearResponse));

                if (response2.CompletionCode == 0x00)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///  Get System Event Log Information. Returns SEL Info.
        /// </summary>
        public virtual SystemEventLogInfo GetSelInfo()
        {

            SelInfoResponse response = (SelInfoResponse)this.IpmiSendReceive(
            new SelInfoRequest(), typeof(SelInfoResponse));

            // create new SelInfo class
            SystemEventLogInfo info = new SystemEventLogInfo(response.CompletionCode);

            if (response.CompletionCode == 0x00)
            {
                info.SetParamaters(response.SELVersion,
                                    response.MSByte,
                                    response.LSByte,
                                    response.SelFeeSpace,
                                    response.LastAdded,
                                    response.LastRemoved);

            }

            return info;
        }

        /// <summary>
        /// Recursively retrieves System Event Log entries.
        /// </summary>
        public virtual SystemEventLog GetSel()
        {
            // return value. create string collection to hold system event strings. 
            SystemEventLog messageCollection = new SystemEventLog(0x00);

            // Default Record Off Set
            byte offSet = 0x00;

            // limit number of records to retrive
            ushort RecordCount = 0;

            // limit number of records to retrive
            ushort RecordLimit = 350;

            // system event log entry point
            ushort recordId = 0;

            // system event log record reserve (used for partial retrieve)
            ushort reserveId = 0;

            // signal last system event record (aborts event log Loop)
            ushort lastRecordId = 65535;

            // system event log record Id and raw payload collection
            IpmiSelCollection responseCollection = new IpmiSelCollection();

            // retrieve all records while connected by recursively calling the Sel entry command 
            while (recordId != lastRecordId || RecordCount >= RecordLimit)
            {
                // get the SEL record
                SelEntryResponse response = (SelEntryResponse)this.IpmiSendReceive(
                new SelEntryRequest(reserveId, recordId, offSet), typeof(SelEntryResponse));

                // reset the main class error code.
                messageCollection.CompletionCode = response.CompletionCode;

                if (response.CompletionCode == 0x00)
                {
                    // add the record to the collection
                    responseCollection.Add(response);

                    // update the record Id (signals loop exit)
                    recordId = BitConverter.ToUInt16(response.NextRecordId, 0);
                }
                else
                {
                    // add the errored record to the collection
                    responseCollection.Add(response);
                    // break the loop on error.
                    break;
                }

                RecordCount++;
            }

            // check for valid sel event messages before processing
            if (responseCollection.Count > 0)
            {
                foreach (SelEntryResponse response in responseCollection)
                {
                    SystemEventLogMessage message = new SystemEventLogMessage(response.CompletionCode);

                    // if the response was not an error cast it.
                    if (response.CompletionCode == 0x00)
                    {
                        // bytes 0-2 = Record Id
                        ushort recordNo = BitConverter.ToUInt16(response.SelEntry, 0);

                        // sel record type
                        byte recordType = response.SelEntry[2];

                        // byte array for message data
                        byte[] messageData = new byte[13];

                        // copy message data in message data array
                        Buffer.BlockCopy(response.SelEntry, 3, messageData, 0, (response.SelEntry.Length - 3));

                        // IPMI SPEC: 31.6.1  SEL Record Type Ranges
                        if (recordType >= 0x00 && recordType <= 0xBF)
                        {
                            // Standard Range. 0x02 "System Event". (Spec declares 0x02.  TODO: Monitor the range here)
                            message.EventFormat = EventMessageFormat.SystemEvent;

                            // Format Standard SEL record
                            SelSupport.StandardSelFormat(ref message, messageData);

                            message.EventMessage = ExtractSystemEventRecordMessage(message.RawSensorType, 
                                message.EventTypeCode, message.RawPayload);
                        }
                        else if ((recordType >= 0xC0) && (recordType <= 0xDF))
                        {
                            // Time Stamped OEM. (override type to reduce string repetition in resource file)
                            message.EventFormat = EventMessageFormat.OemTimeStamped;

                            // Format "TimeStamped OEM" SEL record
                            SelSupport.TimeStampedOEMSelFormat(ref message, messageData);

                            message.EventMessage = ExtractOemTimestampedEventMessage(message.RawPayload);
                        }
                        else if ((recordType >= 0xE0) && (recordType <= 0xFF))
                        {
                            // Non TimeStamped OEM. (override type to reduce string repetition in resource file)
                            message.EventFormat = EventMessageFormat.OemNonTimeStamped;

                            // Format "Non TimeStamped OEM" SEL record
                            SelSupport.NonTimestampedOemSelFormat(ref message, messageData);

                            message.EventMessage = ExtractOemNonTimestampedEventMessage(message.RawPayload);
                        }

                        // add message to the collection
                        messageCollection.EventLog.Add(message);
                    }
                }

            }

            return messageCollection;
        }

        /// <summary>
        /// Gets the System Event Log Time
        /// </summary>
        public virtual GetEventLogTime GetSelTime()
        {
            SelTimeResponse response = (SelTimeResponse)this.IpmiSendReceive(
            new SelTimeRequest(), typeof(SelTimeResponse));

            GetEventLogTime respObj = new GetEventLogTime(response.CompletionCode);

            if (response.CompletionCode == 0)
                respObj.SetParamaters(response.Time);

            return respObj;
        }

        /// <summary>
        /// Set System Event Log Time
        /// </summary>
        public virtual bool SetSelTime(DateTime date)
        {
            SetSelTimeResponse response = (SetSelTimeResponse)this.IpmiSendReceive(
            new SetSelTimeRequest(date), typeof(SetSelTimeResponse));

            if (response.CompletionCode == 0)
                return true;
            else
                return false;
        }

        #region SEL Support

        private EventData ExtractSystemEventRecordMessage(byte sensorType, byte eventTypeCode, byte[] payload)
        {
            // [IPMI 2.0 Tbl: 42.1] Event Type Code determines the
            // system event log event message type.
            if (eventTypeCode == 0x01) // Threshold
            {
                ThresholdEvent eventDataField =
                    new ThresholdEvent(EventLogMsgType.Threshold, sensorType, eventTypeCode, payload);

                return eventDataField;
            }
            else if ((eventTypeCode >= 0x02) && (eventTypeCode <= 0x0C)) // Generic
            {
                DiscreteEvent eventDataField =
                        new DiscreteEvent(EventLogMsgType.Discrete, sensorType, eventTypeCode, payload);

                return eventDataField;
            }
            else if (eventTypeCode == 0x6f) // Sensor - Specific
            {
                DiscreteEvent eventDataField =
                    new DiscreteEvent(EventLogMsgType.SensorSpecific, sensorType, eventTypeCode, payload);

                return eventDataField;
            }
            else if ((eventTypeCode >= 0x70) && (eventTypeCode <= 0x7F)) // OEM
            {
                OemEvent eventDataField =
                    new OemEvent(EventLogMsgType.Oem, sensorType, eventTypeCode, payload);

                return eventDataField;
            }
            else // Unknown Event Type Code
            {
                UnknownEvent eventDataField =
                    new UnknownEvent(EventLogMsgType.Unspecified, sensorType, eventTypeCode, payload);

                return eventDataField;
            }
        }

        private EventData ExtractOemTimestampedEventMessage(byte[] payload)
        {
            OemTimeStampedEvent eventDataField = new OemTimeStampedEvent(payload);
            return eventDataField;
        }

        private EventData ExtractOemNonTimestampedEventMessage(byte[] payload)
        {
            OemNonTimeStampedEvent eventDataField = new OemNonTimeStampedEvent(payload);
            return eventDataField;
        }

        #endregion

        #endregion

        #region FRU

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
        public virtual WriteFruDevice WriteFruDevice(int deviceId, ushort offset, byte[] payload)
        {
            byte loOffset;
            byte hiOffset;

            // split address into hi and lo bytes.
            IpmiSharedFunc.SplitWord(offset, out loOffset, out hiOffset);

            WriteFruDataResponse fruResponse =
                this.IpmiSendReceive<WriteFruDataResponse>(new WriteFruDataRequest(loOffset, hiOffset, payload) { DeviceId = (byte)deviceId });

            WriteFruDevice response = new WriteFruDevice(fruResponse.CompletionCode);

            if (fruResponse.CompletionCode == 0)
                response.SetParamaters(new byte[1] { fruResponse.CountWritten });

            return response;
        }

        /// <summary>
        /// Reads raw fru data, and returns a byte array.
        /// </summary>
        public virtual byte[] ReadFruDevice(int deviceId, ushort offset, byte readCount)
        {
            byte loOffset;
            byte hiOffset;

            // split address into hi and lo bytes.
            IpmiSharedFunc.SplitWord(offset, out loOffset, out hiOffset);

            GetFruDataResponse fruResponse =
                this.IpmiSendReceive<GetFruDataResponse>(new GetFruDataRequest(loOffset, hiOffset, readCount) { DeviceId = (byte)deviceId });

            return fruResponse.DataReturned;

        }

        /// <summary>
        /// Write Fru Data to Baseboard containing BMC FRU.
        /// </summary>
        public virtual WriteFruDevice WriteFruDevice(ushort address, byte[] payload)
        {
            return WriteFruDevice(0, address, payload);
        }

        /// <summary>
        /// Get Fru Inventory Area
        /// </summary>
        public virtual FruInventoryArea GetFruInventoryArea(byte deviceId = 0)
        {
            GetFruInventoryAreaInfoResponse response = (GetFruInventoryAreaInfoResponse)this.IpmiSendReceive(
            new GetFruInventoryAreaInfoRequest(deviceId), typeof(GetFruInventoryAreaInfoResponse));

            FruInventoryArea fruArea = new FruInventoryArea(response.CompletionCode);

            if (response.CompletionCode == 0)
            {
                fruArea.fruSize = IpmiSharedFunc.GetShort(response.OffSetLS, response.OffSetMS);
                if ((byte)(response.AccessType & 0x01) == 0x01)
                    fruArea.accessedByBytes = false;
            }

            return fruArea;
        }

        /// <summary>
        /// Get Fru Device
        /// </summary>
        public virtual FruDevice GetFruDeviceInfo(int deviceId, bool maxLenght = false)
        {
            return ReadFru(deviceId, maxLenght);
        }

        /// <summary>
        /// Get Fru Device
        /// </summary>
        public virtual FruDevice GetFruDeviceInfo(bool maxLenght = false)
        {
            return ReadFru(0, maxLenght);
        }

        #region Command Support: FRU

        internal FruDevice ReadFru(int deviceId, bool maxLenght = false)
        {
            byte countToRead = 8; //FRU common header size

            GetFruDataResponse fruResponse =
                IpmiSendReceive<GetFruDataResponse>(new GetFruDataRequest(0, 0, countToRead) { DeviceId = (byte)deviceId });

            if (fruResponse.CompletionCode == 0x00)
            {

                FruCommonHeader commonHeader = new FruCommonHeader(fruResponse.DataReturned);

                ushort areaOffset;

                byte[] chassisInfo = null;
                byte[] boardInfo = null;
                byte[] productInfo = null;
                byte completionCode = fruResponse.CompletionCode;

                areaOffset = commonHeader.ChassisInfoStartingOffset;
                if (areaOffset != 0)
                {
                    chassisInfo = ReadFruAreaBytes(deviceId, areaOffset, maxLenght, out completionCode);
                }

                areaOffset = commonHeader.BoardAreaStartingOffset;
                if (areaOffset != 0)
                {
                    boardInfo = ReadFruAreaBytes(deviceId, areaOffset, maxLenght, out completionCode);
                }

                areaOffset = commonHeader.ProductAreaStartingOffset;
                if (areaOffset != 0)
                {
                    productInfo = ReadFruAreaBytes(deviceId, areaOffset, maxLenght, out completionCode);
                }


                return new FruDevice(deviceId,
                                        commonHeader,
                                        chassisInfo,
                                        boardInfo,
                                        productInfo, completionCode);
            }
            else
            {
                return new FruDevice(fruResponse.CompletionCode);
            }
        }

        /// <summary>
        /// Returns a byte array with all bytes from a specific area of the fru: Chassis, Baseboard, Product
        /// </summary>
        private byte[] ReadFruAreaBytes(int deviceId, ushort offset, bool maxLenght, out byte completionCode)
        {
            byte countToRead = 0x10;
            byte loOffset;
            byte hiOffset;

            List<byte> areaBytes = new List<byte>();

            IpmiSharedFunc.SplitWord(offset, out loOffset, out hiOffset);

            ushort totalDataRead = countToRead;
            GetFruDataRequest fruRequest =
                new GetFruDataRequest(loOffset, hiOffset, countToRead) { DeviceId = (byte)deviceId };

            GetFruDataResponse fruResponse = IpmiSendReceive<GetFruDataResponse>(fruRequest);

            completionCode = fruResponse.CompletionCode;

            if (completionCode == 0x00)
            {
                ushort dataSize = FruArea.AreaLength(fruResponse.DataReturned);
                totalDataRead = Math.Min(countToRead, dataSize);
                IpmiSharedFunc.AppendArrayToList(fruResponse.DataReturned, areaBytes, totalDataRead);
                offset += totalDataRead;
                int pass = 0;

                while (dataSize > totalDataRead || pass > 12)
                {
                    IpmiSharedFunc.SplitWord(offset, out loOffset, out hiOffset);

                    if (!maxLenght)
                        countToRead = (byte)Math.Min(countToRead, dataSize - totalDataRead);
                    else
                        countToRead = (byte)Math.Min(byte.MaxValue, dataSize - totalDataRead);

                    fruRequest = new GetFruDataRequest(loOffset, hiOffset, countToRead) { DeviceId = (byte)deviceId };
                    // send request for more data
                    fruResponse = IpmiSendReceive<GetFruDataResponse>(fruRequest);
                    totalDataRead += countToRead;
                    offset += countToRead;

                    completionCode = fruResponse.CompletionCode;

                    if (completionCode == 0x00)
                    {
                        IpmiSharedFunc.AppendArrayToList(fruResponse.DataReturned, areaBytes, countToRead);
                    }
                    else
                    {
                        break;
                    }

                    pass++;
                }

                if (pass > 12)
                {
                    completionCode = 0xEF;
                }
            }

            return areaBytes.ToArray();
        }

        #endregion

        #endregion
    }
}
