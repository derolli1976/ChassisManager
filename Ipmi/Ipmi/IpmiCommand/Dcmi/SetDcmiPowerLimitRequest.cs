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
    using System;
    /// <summary>
    /// Represents the DCMI 'Set Power Limit' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Dcgrp, IpmiCommand.DcmiSetLimit)]
    internal class SetDcmiPowerLimitRequest : IpmiRequest
    {
        /// <summary>
        /// Group Extension byte.  Always 0xDC
        /// </summary> 
        private byte groupextension = 0xDC;

        /// <summary>
        /// reserved
        /// </summary> 
        private byte[] reserved = { 0x00, 0x00, 0x00 };

        /// <summary>
        /// Exception Actions: 0 = none, 1 = reboot, 2-10 = OEM
        /// </summary>
        private byte exceptionActions;

        /// <summary>
        /// 2 byte prower limit in Watts.
        /// </summary>
        private byte[] powerLimit;

        /// <summary>
        /// 4 byte correction time in ms.
        /// </summary>
        private byte[] correctionTime;

        /// <summary>
        /// 2 bytes reserved for future use
        /// </summary>
        private byte[] reservation = { 0x00, 0x00 };

        /// <summary>
        /// 2 bytes sampling period in seconds
        /// </summary>
        private byte[] samplingPeriod;

        /// <summary>
        /// Initializes a new instance of the SetDcmiPowerLimitRequest class.
        /// </summary>
        internal SetDcmiPowerLimitRequest(short watts, int correctionTime, byte action, short samplingPeriod)
        {
            this.powerLimit = BitConverter.GetBytes(watts);
            this.correctionTime = BitConverter.GetBytes(correctionTime);
            this.samplingPeriod = BitConverter.GetBytes(samplingPeriod);
            this.exceptionActions = action;
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
        /// Reserved
        /// </summary>       
        [IpmiMessageData(1,3)]
        public byte[] Reserved
        {
            get { return this.reserved; }

        }

        /// <summary>
        /// Exception Action 
        /// </summary>       
        [IpmiMessageData(4)]
        public byte ExceptionAction
        {
            get { return this.exceptionActions; }
        }

        /// <summary>
        /// Power limit in watts
        /// </summary>       
        [IpmiMessageData(5,2)]
        public byte[] PowerLimit
        {
            get { return this.powerLimit; }
        }

        /// <summary>
        /// Correction time is the maximum time taken to limit the power, 
        /// otherwise exception action will be taken as configured.
        /// </summary>
        /// <value>CorrectionTime in ms.</value>
        [IpmiMessageData(7, 4)]
        public byte[] CorrectionTime
        {
            get { return this.correctionTime; }
        }

        /// <summary>
        /// Correction time is the maximum time taken to limit the power, 
        /// otherwise exception action will be taken as configured.
        /// </summary>
        /// <value>CorrectionTime in ms.</value>
        [IpmiMessageData(11, 2)]
        public byte[] Reservation
        {
            get { return this.reservation; }
        }

        /// <summary>
        /// Sampling period in seconds.
        /// </summary>
        [IpmiMessageData(13, 2)]
        public byte[] SamplingPeriod
        {
            get { return this.samplingPeriod; }
        }

    }
}
