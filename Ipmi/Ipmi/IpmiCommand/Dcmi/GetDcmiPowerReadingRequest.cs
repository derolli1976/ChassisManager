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
    /// Represents the DCMI 'Get Power Reading' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Dcgrp, IpmiCommand.DcmiPowerReading)]
    internal class GetDcmiPowerReadingRequest : IpmiRequest
    {
        /// <summary>
        /// Group Extension byte.  Always 0xDC
        /// </summary> 
        private byte groupextension = 0xDC;

        /// <summary>
        /// Mode byte.
        /// </summary> 
        private byte readingMode;

        /// <summary>
        /// Rolling Average byte.
        /// </summary> 
        private byte rollingAverage;

        /// <summary>
        /// byte 3 is currently reserved.
        /// </summary> 
        private byte reserved = 0x00;

        /// <summary>
        /// Initializes a new instance of the GetDcmiPowerReadingRequest class.
        /// </summary>
        internal GetDcmiPowerReadingRequest(byte readingMode, byte rollingAverage)
        {
            this.rollingAverage = rollingAverage;
            this.readingMode = readingMode;
        }

        /// <summary>
        /// Group Extension byte
        /// </summary>       
        [IpmiMessageData(0)]
        public byte GroupExtension
        {
            get { return this.groupextension; }

        }

        /// <summary>
        /// Mode byte
        /// </summary>       
        [IpmiMessageData(1)]
        public byte ReadingMode
        {
            get { return this.readingMode; }

        }

        /// <summary>
        /// Sets Rolling Average
        /// </summary>       
        [IpmiMessageData(2)]
        public byte RollingAverage
        {
            get { return this.rollingAverage; }

        }

        /// <summary>
        /// Reserved
        /// </summary>       
        [IpmiMessageData(3)]
        public byte Reserved
        {
            get { return this.reserved; }

        }
    }
}
