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
    /// Represents the IPMI 'Reserve SEL' response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.SelReserve)]
    internal class SelReserveResponse : IpmiResponse
    {

        /// <summary>
        /// Reservation Id LS (Least Significant Byte).
        /// </summary>  
        private byte reservationIdLS;


        /// <summary>
        /// Reservervation Id MS (Most Significant byte).
        /// </summary>  
        private byte reservationIdMS;


        /// <summary>
        /// Gets reservationId LS byte (Least Significant byte).
        /// </summary>       
        [IpmiMessageData(0)]
        public byte ReservationIdLS
        {
            get { return this.reservationIdLS; }
            set { this.reservationIdLS = value; }
        }

        /// <summary>
        /// Gets reservationId MS byte (Most Significant byte).
        /// </summary>       
        [IpmiMessageData(1)]
        public byte ReservationIdMS
        {
            get { return this.reservationIdMS; }
            set { this.reservationIdMS = value; }
           
        }
    }
}
