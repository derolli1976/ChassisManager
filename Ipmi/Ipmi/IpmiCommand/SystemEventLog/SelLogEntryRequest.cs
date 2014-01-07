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
    /// Represents the IPMI 'Get SEL Entry' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.GetSelEntry, 6)]
    internal class SelEntryRequest : IpmiRequest
    {
        /// <summary>
        /// Reservation Id.
        /// </summary> 
        private ushort reservationId;

        /// <summary>
        /// SEL RecordID
        /// </summary> 
        private ushort recordId;

        /// <summary>
        /// SEL Entry Offste.
        /// </summary> 
        private byte offset;

        /// <summary>
        /// Number of bytes to read. 0xFF for entire record.
        /// </summary> 
        private byte readbytes = 0xFF;

        /// <summary>
        /// Initializes a new instance of the SelEntryRequest class.
        /// </summary>
        internal SelEntryRequest(ushort reserveId, ushort record, byte offset)
        {
            this.reservationId = reserveId;
            this.recordId = record;
            this.offset = offset;
        }

        /// <summary>
        /// Gets reservation Id
        /// </summary>       
        [IpmiMessageData(0)]
        public ushort ReservationId
        {
            get { return this.reservationId; }

        }

        /// <summary>
        /// SEL RecordID
        /// </summary>       
        [IpmiMessageData(2)]
        public ushort RecordId
        {
            get { return this.recordId; }

        }

        /// <summary>
        /// SEL Offset
        /// </summary>       
        [IpmiMessageData(4)]
        public byte OffSet
        {
            get { return this.offset; }

        }

        /// <summary>
        /// Number of bytes to read.
        /// </summary>       
        [IpmiMessageData(5)]
        public byte ReadBytes
        {
            get { return this.readbytes; }

        }
    }
}
