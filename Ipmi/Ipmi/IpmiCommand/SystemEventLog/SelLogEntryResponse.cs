/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*                                       	            *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    /// <summary>
    /// Represents the IPMI 'Get SEL Entry' response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.GetSelEntry)]
    internal class SelEntryResponse : IpmiResponse
    {
        /// <summary>
        /// Next SEL RecordID
        /// </summary> 
        private byte[] nextRecordId;

        /// <summary>
        /// SEL Entry Offste.
        /// </summary> 
        private byte[] selentry;
         
        /// <summary>
        /// Next SEL RecordID 
        /// </summary>       
        [IpmiMessageData(0,2)]
        public byte[] NextRecordId
        {
            get { return this.nextRecordId; }
            set { this.nextRecordId = value; }

        }
        
        /// <summary>
        /// SEL Record
        /// </summary>       
        [IpmiMessageData(2,16)]
        public byte[] SelEntry
        {
            get { return this.selentry; }
            set { this.selentry = value; }

        }
    }
}
