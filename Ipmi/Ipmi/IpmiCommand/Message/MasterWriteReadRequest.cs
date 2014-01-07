/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*   							                        *
********************************************************/

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
