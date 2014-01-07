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
    /// Represents the IPMI 'Get Sensor Reading Factors' response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Sensor, IpmiCommand.SensorFactors, 8)]
    internal class ReadingFactorsResponse : IpmiResponse
    {
        /// <summary>
        /// Next known Reading byte.
        /// </summary> 
        private byte nextReading;

        /// <summary>
        /// Reading Factors.
        /// </summary> 
        private byte[] factors;

        /// <summary>
        /// Next known Reading byte
        /// </summary>       
        [IpmiMessageData(0)]
        public byte NextReading
        {
            get { return this.nextReading; }
            set { this.nextReading = value; }
        }

        /// <summary> 
        /// bytes [1-7] Sensor Reading factores
        /// </summary>     
        [IpmiMessageData(1)]
        public byte[] Factors
        {
            get { return this.factors; }
            set { this.factors = value; }
        }
    }
}
