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
    /// Represents the IPMI 'Reserve SDR Repository' response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.ReserveSdrRepository)]
    internal class ReserveSdrResponse : IpmiResponse
    {
        /// <summary>
        /// Reservation LS Byte.
        /// </summary> 
        private byte reservationLs;

        /// <summary>
        /// Reservation MS Byte.
        /// </summary> 
        private byte reservationMs;
         
        /// <summary>
        /// Reservation LS Byte
        /// </summary>       
        [IpmiMessageData(0)]
        public byte ReservationLS
        {
            get { return this.reservationLs; }
            set { this.reservationLs = value; }
        }

        /// <summary>
        /// Reservation MS Byte
        /// </summary>       
        [IpmiMessageData(1)]
        public byte ReservationMS
        {
            get { return this.reservationMs; }
            set { this.reservationMs = value; }
        }
  
    }
}
