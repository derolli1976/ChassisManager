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
    /// Represents the IPMI 'Clear SEL' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.SelClear, 6)]
    internal class SelLogClearRequest : IpmiRequest
    {
        /// <summary>
        /// Reservation Id.
        /// </summary> 
        private byte[] reservationId;

        /// <summary>
        /// IPMI Specification defined value 'C'
        /// </summary> 
        private byte val1 = 0x43;

        /// <summary>
        /// IPMI Specification defined value 'L'
        /// </summary> 
        private byte val2 = 0x4c;

        /// <summary>
        /// IPMI Specification defined value 'R'
        /// </summary> 
        private byte val3 = 0x52;

        /// <summary>
        /// SEL operation.
        /// </summary> 
        private byte operation;

        /// <summary>
        /// Sel Erase operation.
        /// </summary> 
        internal const byte InitiateErase = 0xAA;

        /// <summary>
        /// Sel Erase Status operation.
        /// </summary> 
        internal const byte GetErasureStatus = 0x00;


        internal SelLogClearRequest(byte[] reservationId, byte operation)
        {
            this.reservationId = reservationId;
            this.operation = operation;
        }


        /// <summary>
        /// Gets reservation Id
        /// </summary>       
        [IpmiMessageData(0,2)]
        public byte[] ReservationId
        {
            get { return this.reservationId; }

        }


        /// <summary>
        /// Gets Val1
        /// </summary>       
        [IpmiMessageData(2)]
        public byte Val1
        {
            get { return this.val1; }

        }

        /// <summary>
        /// Gets Val2
        /// </summary>       
        [IpmiMessageData(3)]
        public byte Val2
        {
            get { return this.val2; }

        }

        /// <summary>
        /// Gets Val3
        /// </summary>       
        [IpmiMessageData(4)]
        public byte Val3
        {
            get { return this.val3; }

        }

        /// <summary>
        /// Gets Val3
        /// </summary>       
        [IpmiMessageData(5)]
        public byte Operation
        {
            get { return this.operation; }
            set { this.operation = value; }

        }

    }
 }
