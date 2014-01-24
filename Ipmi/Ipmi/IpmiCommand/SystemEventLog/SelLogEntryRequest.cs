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
