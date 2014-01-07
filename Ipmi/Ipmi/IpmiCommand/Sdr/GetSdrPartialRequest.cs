/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*   							                        *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    using System;
    /// <summary>
    /// Represents the IPMI 'Get SDR Partial read' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.GetSdr, 6)]
    internal class GetSdrPartialRequest : IpmiRequest
    {
        /// <summary>
        /// Reservation Id LS byte.
        /// Only required for partial reads
        /// Default = 0x00,  
        /// </summary> 
        private byte reservationLsByte;

        /// <summary>
        /// Reservation Id MS Byte.
        /// Only required for partial reads
        /// Default = 0x00,  
        /// </summary> 
        private byte reservationMsByte;

        /// <summary>
        /// Record Id LS Byte.
        /// </summary> 
        private byte recordIdLsByte;

        /// <summary>
        /// Record Id MS Byte.
        /// </summary> 
        private byte recordIdMsByte;

        /// <summary>
        /// SDR offset.
        /// </summary> 
        private byte offset;
        
        /// <summary>
        /// Number of bytes to read. 
        /// 0xFF for entire record.
        /// </summary> 
        private byte readbytes;
                           
        /// <summary>
        /// Initializes a new instance of the GetSdrRequest class.
        /// </summary>
        internal GetSdrPartialRequest(byte reservationLs, byte reservationMs, ushort recordId, int offset, int bytesToRead)
        {
            this.reservationLsByte = reservationLs;
            this.reservationMsByte = reservationMs;
            byte[] recordid = BitConverter.GetBytes(recordId);
            this.recordIdLsByte = recordid[1];
            this.recordIdMsByte = recordid[0];
            this.offset = Convert.ToByte(offset);
            this.readbytes = Convert.ToByte(bytesToRead);
        }

        /// <summary>
        /// Sets reservation LS byte
        /// </summary>       
        [IpmiMessageData(0)]
        public byte ReservationLsByte
        {
            get { return this.reservationLsByte; }

        }

        /// <summary>
        /// Sets reservation MS byte
        /// </summary>       
        [IpmiMessageData(1)]
        public byte ReservationMsByte
        {
            get { return this.reservationMsByte; }

        }

        /// <summary>
        /// Sets Record LS Byte
        /// </summary>       
        [IpmiMessageData(2)]
        public byte RecordIdLsByte
        {
            get { return this.recordIdLsByte; }

        }

        /// <summary>
        /// Sets Record MS Byte
        /// </summary>       
        [IpmiMessageData(3)]
        public byte RecordIdMsByte
        {
            get { return this.recordIdMsByte; }

        }

        /// <summary>
        /// Sets Offset
        /// </summary>       
        [IpmiMessageData(4)]
        public byte OffSet
        {
            get { return this.offset; }

        }

        /// <summary>
        /// Number of Bytes to read.
        /// </summary>       
        [IpmiMessageData(5)]
        public byte ReadBytes
        {
            get { return this.readbytes; }

        }
    }
}
