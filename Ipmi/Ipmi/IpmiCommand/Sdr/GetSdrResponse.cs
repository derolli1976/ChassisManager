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
    /// Represents the IPMI 'Get SDR' response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.GetSdr)]
    internal class GetSdrResponse : IpmiResponse
    {
        /// <summary>
        /// Next Record Id LS byte.
        /// </summary> 
        private byte recordIdLsByte;

        /// <summary>
        /// Next Record Id MS Byte.
        /// </summary> 
        private byte recordIdMsByte;

        /// <summary>
        /// Record ID.
        /// </summary> 
        private byte[] recordId;

        /// <summary>
        /// SDR Verions.
        /// </summary> 
        private byte sdrVersion;

        /// <summary>
        /// Record Type.
        /// </summary> 
        private byte recordType;

        /// <summary>
        /// Record Length.
        /// </summary> 
        private byte recordLength;

        /// <summary>
        /// Record Data.
        /// </summary> 
        private byte[] recordData;
         
        /// <summary>
        /// Next Record ID LS byte
        /// </summary>       
        [IpmiMessageData(0)]
        public byte RecordIdLsByte
        {
            get { return this.recordIdLsByte; }
            set { this.recordIdLsByte = value; }
        }

        /// <summary>
        /// Next Record ID MS byte
        /// </summary>       
        [IpmiMessageData(1)]
        public byte RecordIdMsByte
        {
            get { return this.recordIdMsByte; }
            set { this.recordIdMsByte = value; }
        }
        
        /// <summary>
        /// SDR Record Id
        /// </summary>       
        [IpmiMessageData(2,2)]
        public byte[] RecordId
        {
            get { return this.recordId; }
            set { this.recordId = value; }
        }

        /// <summary>
        /// Record Version
        /// </summary>       
        [IpmiMessageData(4)]
        public byte SdrVersion
        {
            get { return this.sdrVersion; }
            set { this.sdrVersion = value; }
        }
        
        /// <summary>
        /// Record Type
        /// </summary>       
        [IpmiMessageData(5)]
        public byte RecordType
        {
            get { return this.recordType; }
            set { this.recordType = value; }
        }
        
        /// <summary>
        /// Record Lenght
        /// </summary>       
        [IpmiMessageData(6)]
        public byte RecordLenght
        {
            get { return this.recordLength; }
            set { this.recordLength = value; }
        }
   
        /// <summary>
        /// Record Data
        /// </summary>       
        [IpmiMessageData(7)]
        public byte[] RecordData
        {
            get { return this.recordData; }
            set { this.recordData = value; }
        }
    }
}
