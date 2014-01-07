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
    /// Represents the IPMI 'Get Sensor Reading Factors' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Sensor, IpmiCommand.SensorFactors, 2)]
    internal class ReadingFactorsRequest : IpmiRequest
    {
        /// <summary>
        /// Sensor Number.
        /// </summary> 
        private byte sensornumber;

        /// <summary>
        /// Reading byte.
        /// </summary> 
        private byte readingbyte;

        /// <summary>
        /// Initializes a new instance of the ReadingFactorsRequest class.
        /// </summary>
        internal ReadingFactorsRequest(byte SensorNumber, byte ReadingByte)
        {
            this.sensornumber = SensorNumber;
            this.readingbyte = ReadingByte;
        }

        /// <summary>
        /// Sensor Number byte
        /// </summary>       
        [IpmiMessageData(0)]
        public byte SensorNumber
        {
            get { return this.sensornumber; }

        }
        /// <summary>
        /// Sensor Reading byte
        /// </summary>     
        [IpmiMessageData(1)]
        public byte Readingbyte
        {
            get { return this.readingbyte; }

        }
    }
}
