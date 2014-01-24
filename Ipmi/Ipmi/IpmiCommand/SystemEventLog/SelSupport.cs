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

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    static class SelSupport
    {
        #region Command Support: SEL

        /// <summary>
        /// Formats System Event (Standard Range) SEL Records
        /// </summary>
        /// <param name="EventMessageData">Event Message Data</param>
        /// <param name="RecordCount">SEL Record Type</param>
        internal static void StandardSelFormat(ref SystemEventLogMessage message, byte[] messageData)
        {

            // convert data bytes from messageData byte[] to int using Shift operation
            int totalSeconds = TimeStampFromBytes(messageData);

            // calculate Recorded date
            message.EventDate = IpmiSharedFunc.SecondsOffSet(totalSeconds);

            // SEL Event Message
            if (Enum.IsDefined(typeof(MsgVersion), messageData[6]))
            {
                message.EventVersion = (MsgVersion)messageData[6];
            }
            else
            {
                message.EventVersion = MsgVersion.Unknown;
            }

            // Sensor Type
            byte sensorType = messageData[7];

            // add sensor type to attribute class
            if (Enum.IsDefined(typeof(SensorType), sensorType))
            {
                message.SensorType = (SensorType)sensorType;
            }
            else
            {
                message.SensorType = SensorType.Unknown;
            }

            // add sensor type to message
            message.RawSensorType = messageData[7];

            // add sensor number to the message class
            message.SensorNumber = messageData[8];

            // Event Data Byte
            byte[] eventDataByte = IpmiSharedFunc.ByteSplit(messageData[9], new int[2] { 7, 0 });
            // Event Dir. Asersion/Deserstion Bit 7
            byte eventDir = eventDataByte[0];
            // EventType [6:0]
            byte eventTypeCode = eventDataByte[1];

            message.EventTypeCode = eventTypeCode;

            // Event Dir
            if (Enum.IsDefined(typeof(EventDir), eventDir))
            {
                message.EventDir = (EventDir)eventDir;
            }
            else
            {
                message.EventDir = EventDir.Unknown;
            }

            // copy event message payload to the response raw payload array
            Buffer.BlockCopy(messageData, 10, message.RawPayload, 0, 3);

            // Add the raw payload as hex string to the user return class.
            message.EventPayload = IpmiSharedFunc.ByteArrayToHexString(message.RawPayload);
        }

        /// <summary>
        ///  returns SEL time stamp in Seconds from message paylaod
        /// </summary>
        internal static int TimeStampFromBytes(byte[] timedata)
        {
            // convert data bytes from messageData byte[] to int using Shift operation
            return timedata[0] + (timedata[1] << 8) + (timedata[2] << 16) + (timedata[3] << 24);
        }

        /// <summary>
        /// Formats Time Stamped OEM SEL Records
        /// </summary>
        /// <param name="EventMessageData">Event Message Data</param>
        /// <param name="RecordCount">SEL Record Type</param>
        internal static void TimeStampedOEMSelFormat(ref SystemEventLogMessage message, byte[] messageData)
        {
            // convert byte[] to int using Shift operation
            int TotalSeconds = messageData[0] + (messageData[1] << 8) + (messageData[2] << 16) + (messageData[3] << 24);

            // calculate Recorded date
            message.EventDate = IpmiSharedFunc.SecondsOffSet(TotalSeconds);

            message.EventVersion = MsgVersion.OEM;

            // SensorType, RawSensorType and EventTypeCode are not used for OEM SEL entries
            message.SensorType = SensorType.Reserved;
            message.RawSensorType = (byte)SensorType.Reserved;
            message.EventTypeCode = 0xFF;

            message.SensorNumber = 0;

            message.EventDir = EventDir.Assertion;

            if (messageData.Length >= 13)
            {
                // Allocate larger array to store OEM Timestamped payload
                message.RawPayload = new byte[9];

                // Copy Manufacturer ID and OEM Defined payload to the response raw payload array. Format shown in IPMI 2.0 Spec Table 32-2.
                Buffer.BlockCopy(messageData, 4, message.RawPayload, 0, 9);

                // Add the raw payload as hex string to the user return class.
                message.EventPayload = IpmiSharedFunc.ByteArrayToHexString(message.RawPayload);
            }
        }

        /// <summary>
        /// Formats Non Time Stamped OEM SEL Records
        /// </summary>
        /// <param name="EventMessageData">Event Message Data</param>
        /// <param name="RecordCount">SEL Record Type</param>
        internal static void NonTimestampedOemSelFormat(ref SystemEventLogMessage message, byte[] messageData)
        {
            // calculate Recorded date
            message.EventDate = new DateTime(0000, 0, 0);

            message.EventVersion = MsgVersion.OEM;

            // SensorType, RawSensorType and EventTypeCode are not used for OEM SEL entries
            message.SensorType = SensorType.Reserved;
            message.RawSensorType = (byte)SensorType.Reserved;
            message.EventTypeCode = 0xFF;

            message.SensorNumber = 0;

            message.EventDir = EventDir.Assertion;

            if (messageData.Length >= 13)
            {
                // Allocate larger array to store OEM Non-timestamped payload
                message.RawPayload = new byte[13];

                // Copy OEM Defined payload to the response raw payload array. Format shown in IPMI 2.0 Spec Table 32-3.
                Buffer.BlockCopy(messageData, 0, message.RawPayload, 0, 13);

                // Add the raw payload as hex string to the user return class.
                message.EventPayload = IpmiSharedFunc.ByteArrayToHexString(message.RawPayload);
            }
        }

        #endregion
    }

    /// <summary>
    /// Base class for Ipmi System Event Log 
    /// Event Data Field Formats.  IPMI Spec[2.0 tbl 29-6]
    /// </summary>
    internal abstract class EventData
    {
        private EventLogMsgType messageType;
        private byte[] eventPayload;
        private int sensorType;
        private int eventType;
        private int evtData1Part1;
        private int evtData1Part2;
        private int offset;

        protected EventData(EventLogMsgType MsgType, byte sensorType, byte eventTypeCode, byte[] payload)
        {
            this.messageType = MsgType;
            this.eventType = Convert.ToInt32(eventTypeCode);
            this.eventPayload = payload;
            this.sensorType = Convert.ToInt32(sensorType);

            byte[] evtData1 = IpmiSharedFunc.ByteSplit(eventPayload[0], new int[3] { 6, 4, 0 });

            this.evtData1Part1 = Convert.ToInt32(evtData1[0]);
            this.evtData1Part2 = Convert.ToInt32(evtData1[1]);
            this.offset = Convert.ToInt32(evtData1[2]);
        }

        /// <summary>
        /// Event Data Message Type:  Thresdhold/Discrete/OEM
        /// </summary>
        internal EventLogMsgType MessageType
        {
            get { return this.messageType; }
        }

        /// <summary>
        /// Event Data Byte 1 part 1 [7:0] message type indicator
        /// </summary>
        internal int EvtData1Part1
        {
            get { return this.evtData1Part1; }
        }

        /// <summary>
        /// Event Data Byte 1 part 2 [5:4] type indicator
        /// </summary>
        internal int EvtData1Part2
        {
            get { return this.evtData1Part2; }
        }

        /// <summary>
        /// Event Data Byte 1 Offset from Event Reading
        /// No present on OEM Record Type
        /// </summary>
        protected int Offset
        {
            get { return this.offset; }
        }

        internal int EventTypeCode
        {
            get { return this.eventType; }
        }

        internal int SensorType
        {
            get { return this.sensorType; }
        }

        internal byte[] EventPayload
        {
            get { return this.eventPayload; }
        }
    }

    /// <summary>
    /// Threshold Event class for Ipmi System Event Log 
    /// Event Data Field Formats.  IPMI Spec[2.0 tbl 29-6]
    /// </summary>
    internal class ThresholdEvent : EventData
    {
        private byte triggerReading;
        private byte triggerThreshold;

        internal ThresholdEvent(EventLogMsgType MsgType, byte sensorType, byte eventType, byte[] eventPayload)
            : base(MsgType, sensorType, eventType, eventPayload)
        {
            triggerReading = eventPayload[1];
            triggerThreshold = eventPayload[2];
        }

        /// <summary>
        /// Byte 1 [3:0] Offset from event Reading
        /// </summary>
        internal int ReadingOffset
        {
            get { return base.Offset; }
        }

        /// <summary>
        /// Event Data Byte 2
        /// </summary>
        internal byte TriggerReading
        {
            get { return this.triggerReading; }
        }

        /// <summary>
        /// Event Data Byte 2
        /// </summary>
        internal byte TriggerThreshold
        {
            get { return this.triggerThreshold; }
        }
    }

    /// <summary>
    /// Discrete Event class for Ipmi System Event Log 
    /// Event Data Field Formats.  IPMI Spec[2.0 tbl 29-6]
    /// </summary>
    internal class DiscreteEvent : EventData
    {
        // Optional Byte 2 Severity Event Reading
        private byte evtByte2Severity;
        // Optional Byte 2 Event Reading
        private byte evtByte2Reading;

        internal DiscreteEvent(EventLogMsgType MsgType, byte sensorType, byte eventType, byte[] eventPayload)
            : base(MsgType, sensorType, eventType, eventPayload)
        {
            if (MsgType == EventLogMsgType.SensorSpecific
                && sensorType == 42) // add exception for user Id
            {
                byte[] evtData2 = IpmiSharedFunc.ByteSplit(eventPayload[1], new int[2] { 6, 0 });
                evtByte2Severity = evtData2[0];
                evtByte2Reading = evtData2[1];
            }
            else if (MsgType == EventLogMsgType.SensorSpecific
                && sensorType == 43) // add exception for Version Change Type
            {
                evtByte2Reading = eventPayload[1];
            }
            else if (MsgType == EventLogMsgType.SensorSpecific
            && sensorType == 40 && base.Offset == 5)  // add exception for FRU device Id
            {
                // swap event payload with payload 2.
                byte temp = eventPayload[1];
                eventPayload[1] = eventPayload[2];
                eventPayload[2] = temp;
            }
            else
            {
                byte[] evtData2 = IpmiSharedFunc.ByteSplit(eventPayload[1], new int[2] { 4, 0 });
                evtByte2Severity = evtData2[0];
                evtByte2Reading = evtData2[1];
            }
        }

        /// <summary>
        /// Optional Byte 2 [7:4] Severity Event Reading
        /// </summary>
        internal byte EvtByte2Severity
        {
            get { return this.evtByte2Severity; }
        }

        /// <summary>
        /// Optional Byte 2 [3:0] Event Reading
        /// </summary>
        internal byte EvtByte2Reading
        {
            get { return this.evtByte2Reading; }
        }

        /// <summary>
        /// Byte 1 [3:0] Offset from event Reading
        /// </summary>
        internal int ReadingOffset
        {
            get { return base.Offset; }
        }

    }

    /// <summary>
    /// OEM Event class for Ipmi System Event Log 
    /// Event Data Field Formats.  IPMI Spec[2.0 tbl 29-6]
    /// </summary>
    internal class OemEvent : EventData
    {
        // Optional Byte 2 Severity Event Reading
        private byte evtByte2Severity;
        // Optional Byte 2 Event Reading
        private byte evtByte2Reading;

        internal OemEvent(EventLogMsgType MsgType, byte sensorType, byte eventType, byte[] eventPayload)
            : base(MsgType, sensorType, eventType, eventPayload)
        {
            byte[] evtData2 = IpmiSharedFunc.ByteSplit(eventPayload[1], new int[2] { 4, 3 });
            evtByte2Severity = evtData2[0];
            evtByte2Reading = evtData2[1];
        }

        /// <summary>
        /// Optional Byte 2 Severity Event Reading
        /// </summary>
        internal byte EvtByte2Severity
        {
            get { return this.evtByte2Severity; }
        }

        /// <summary>
        /// Optional Byte 2 Event Reading
        /// </summary>
        internal byte EvtByte2Reading
        {
            get { return this.evtByte2Reading; }
        }

    }

    /// <summary>
    /// OEM Timestamped SEL Record class for Ipmi System Event Log 
    /// Event Data Field Formats. IPMI Spec 2.0 Table 32-2
    /// </summary>
    internal class OemTimeStampedEvent : EventData
    {
        private int _manufacturerID;
        private byte[] _oemDefined = new byte[6];

        internal OemTimeStampedEvent(byte[] eventPayload)
            : base(EventLogMsgType.OemTimestamped, (byte)Microsoft.GFS.WCS.ChassisManager.Ipmi.SensorType.Reserved, 0xFF, eventPayload)
        {
            if (eventPayload.Length >= 9)
            {
                // Convert 3 byte manufacturer ID into integer using bitwise operation
                this._manufacturerID = ((eventPayload[0] << 0) + (eventPayload[1] << 8) + (eventPayload[2] << 16));

                Buffer.BlockCopy(eventPayload, 3, _oemDefined, 0, 6);
            }
        }

        /// <summary>
        /// Gets the manufacturer identifier.
        /// </summary>
        /// <value>
        /// The manufacturer identifier.
        /// </value>
        internal int ManufacturerID
        {
            get { return this._manufacturerID; }
        }

        /// <summary>
        /// Gets the oem defined payload.
        /// </summary>
        /// <value>
        /// The oem defined payload.
        /// </value>
        internal byte[] OemDefined
        {
            get { return this._oemDefined; }
        }
    }

    /// <summary>
    /// OEM Non-Timestamped SEL Record class for Ipmi System Event Log 
    /// Event Data Field Formats. IPMI Spec 2.0 Table 32-3
    /// </summary>
    internal class OemNonTimeStampedEvent : EventData
    {
        private byte[] _oemDefined = new byte[13];
        
        internal OemNonTimeStampedEvent(byte[] eventPayload)
            : base(EventLogMsgType.OemNonTimeStamped, (byte)Microsoft.GFS.WCS.ChassisManager.Ipmi.SensorType.Reserved, 0xFF, eventPayload)
        {
            if (eventPayload.Length >= 13)
            {
                Buffer.BlockCopy(eventPayload, 0, _oemDefined, 0, 13);
            }
        }

        /// <summary>
        /// Gets the oem defined payload.
        /// </summary>
        /// <value>
        /// The oem defined payload.
        /// </value>
        internal byte[] OemDefined
        {
            get { return this._oemDefined; }
        }
    }

    internal class UnknownEvent : EventData
    {
        internal UnknownEvent(EventLogMsgType MsgType, byte sensorType, byte eventType, byte[] eventPayload)
            : base(MsgType, sensorType, eventType, eventPayload)
        {
        }
    }

}
