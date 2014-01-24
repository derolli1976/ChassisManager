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
    /// Represents the IPMI 'Send Master Write-Read' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.MasterReadWrite)]
    internal class MasterWriteReadRequest : IpmiRequest
    {

        /// <summary>
        /// Channel to send the message.
        /// </summary>
        private readonly byte channel;

        /// <summary>
        /// Slave Id to send the message.
        /// </summary>
        private readonly byte slaveId;

        /// <summary>
        /// Read Count.  1 based. 0 = no bytes to read.  
        /// The maximum read count should be at least 34 bytes
        /// </summary>
        private readonly byte readCount;

        /// <summary>
        /// Data to write.
        /// </summary>
        private byte[] writeData;

        /// <summary>
        /// Initializes a new instance of the MasterWriteReadRequest class.
        /// </summary>
        internal MasterWriteReadRequest(byte channel, byte slaveId, byte readCount, byte[] writeData)
        {
            this.channel = (byte)((channel << 4) & 0xf0);
            this.slaveId = slaveId;
            this.readCount = readCount;
            this.writeData = writeData;
        }

        /// <summary>
        /// Channel to send the request message.
        /// </summary>
        [IpmiMessageData(0)]
        public byte Channel
        {
            get { return this.channel; }
        }

        /// <summary>
        /// Slave Id to send the message.
        /// </summary>
        [IpmiMessageData(1)]
        public byte SlaveId
        {
            get { return this.slaveId; }
        }

        /// <summary>
        /// Read Count.  1 based. 0 = no bytes to read.  
        /// The maximum read count should be at least 34 bytes
        /// </summary>
        [IpmiMessageData(2)]
        public byte ReadCount
        {
            get { return this.readCount; }
        }

        /// <summary>
        /// Data to write.
        /// </summary>
        [IpmiMessageData(3)]
        public byte[] WriteData
        {
            get { return this.writeData; }
        }

    }
}
