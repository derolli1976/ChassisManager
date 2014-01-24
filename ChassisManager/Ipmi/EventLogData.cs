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
    using System.Collections.Generic;
    using Microsoft.GFS.WCS.ChassisManager.Ipmi;

    public enum EvenLogClass
    {
        Unknown = 0,
        Discrete = 1,
        SensorSpecific = 2,
        OEM = 3
    }

    /// <summary>
    /// IPMI System Event Log string class
    /// </summary>
    public class EventLogData
    {
        private int number;
        private int offset;
        private EventLogMsgType strType;
        private string message = string.Empty;
        private string description = string.Empty;
        private Dictionary<int, string> extension = new Dictionary<int, string>();

        public EventLogData(int number, int offset, EventLogMsgType eventLogType, string message, string description)
        {
            this.number = number;
            this.offset = offset;
            this.strType = eventLogType;
            this.message = message;
            this.description = description;
        }

        public EventLogData()
        {
        }    

        /// <summary>
        /// Add Extension string value to dictionary object
        /// </summary>
        internal void AddExtension(int Id, string detail)
        {
            if (!extension.ContainsKey(Id))
                extension.Add(Id, detail);
        }

        /// <summary>
        /// Event Message String Number
        /// </summary>
        public int Number
        {
            get { return this.number; }
            internal set { this.number = value; }
        }

        /// <summary>
        /// Event Message String Offset
        /// </summary>
        public int OffSet
        {
            get { return this.offset; }
            internal set { this.offset = value; }
        }

        /// <summary>
        /// Event Message Classification
        /// </summary>
        public EventLogMsgType MessageClass
        {
            get { return this.strType; }
            internal set { this.strType = value; }
        }

        /// <summary>
        /// Event Message String
        /// </summary>
        public string EventMessage
        {
            get { return this.message; }
            internal set { this.message = value; }
        }

        /// <summary>
        /// Event Message Description
        /// </summary>
        public string Description
        {
            get { return this.description; }
            internal set { this.description = value; }
        }

        /// <summary>
        /// Event Message Extension
        /// </summary>
        public string GetExtension(int Id)
        {
            if (extension.ContainsKey(Id))
                return extension[Id];
            else
                return string.Empty;
        }

    }
}
