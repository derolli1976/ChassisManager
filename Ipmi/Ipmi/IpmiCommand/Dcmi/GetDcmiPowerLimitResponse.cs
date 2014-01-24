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
    /// Represents the DCMI 'Get Power Reading' response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Dcgrp, IpmiCommand.DcmiGetLimit)]
    internal class GetDcmiPowerLimitResponse : IpmiResponse
    {
        /// <summary>
        /// Group Extension (0xDC).
        /// </summary>
        private byte groupExtension;

        /// <summary>
        /// Reserved for future use. (0x0000)
        /// </summary>
        private byte[] reserved;

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
        private byte[] reservation;

        /// <summary>
        /// 2 bytes sampling period in seconds
        /// </summary>
        private byte[] samplingPeriod;

        /// <summary>
        /// Group Extension
        /// </summary>
        /// <value>0xDC</value>
        [IpmiMessageData(0)]
        public byte GroupExtension
        {
            get { return this.groupExtension; }
            set { this.groupExtension = value; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Reserved.</value>
        [IpmiMessageData(1,2)]
        public byte[] Reserved
        {
            get { return this.reserved; }
            set { this.reserved = value; }
        }

        /// <summary>
        /// Exception Actions: 0 = none, 1 = reboot, 2-10 = OEM
        /// </summary>
        /// <value>Exception Action.</value>
        [IpmiMessageData(3)]
        public byte ExceptionActions
        {
            get { return this.exceptionActions; }
            set { this.exceptionActions = value; }
        }

        /// <summary>
        /// Power Limit in watts.
        /// </summary>
        /// <value>Maximum Power.</value>
        [IpmiMessageData(4,2)]
        public byte[] PowerLimit
        {
            get { return this.powerLimit; }
            set { this.powerLimit = value; }
        }

        /// <summary>
        /// Correction time is the maximum time taken to limit the power, 
        /// otherwise exception action will be taken as configured.
        /// </summary>
        /// <value>CorrectionTime in ms.</value>
        [IpmiMessageData(6,4)]
        public byte[] CorrectionTime
        {
            get { return this.correctionTime; }
            set { this.correctionTime = value; }
        }

        /// <summary>
        /// Reserved in the spec for future use.
        /// </summary>
        [IpmiMessageData(10, 2)]
        public byte[] Reservation
        {
            get { return this.reservation; }
            set { this.reservation = value; }
        }

        /// <summary>
        /// Sampling period in seconds.
        /// </summary>
        [IpmiMessageData(12, 2)]
        public byte[] SamplingPeriod
        {
            get { return this.samplingPeriod; }
            set { this.samplingPeriod = value; }
        }
    }
}
