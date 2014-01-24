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
    [IpmiMessageRequest(IpmiFunctions.Dcgrp, IpmiCommand.DcmiPowerReading)]
    internal class GetDcmiPowerReadingResponse : IpmiResponse
    {
        /// <summary>
        /// Group Extension.
        /// </summary>
        private byte groupExtension;

        /// <summary>
        /// Current Power in Watts.
        /// </summary>
        private byte[] currentpower;

        /// <summary>
        /// Minimum Power in Watts over requested time period.
        /// </summary>
        private byte[] minimumpower;

        /// <summary>
        /// Maximum Power in Watts over requested time period.
        /// </summary>
        private byte[] maximumpower;

        /// <summary>
        /// Maximum Power in Watts over requested time period.
        /// </summary>
        private byte[] averagepower;

        /// <summary>
        /// Ipmi based time stamp (4 bytes).
        /// </summary>
        private byte[] timeStamp;

        /// <summary>
        /// Timeframe in milliseconds, over which the controller collects statistics (4 bytes).
        /// </summary>
        private byte[] statistics;

        /// <summary>
        /// Power reading state.
        /// </summary>
        private byte powerState;

        /// <summary>
        /// Gets and sets the Group Extension.
        /// </summary>
        /// <value>Group Extension.</value>
        [IpmiMessageData(0)]
        public byte GroupExtension
        {
            get { return this.groupExtension; }
            set { this.groupExtension = value; }
        }

        /// <summary>
        /// Gets and sets the Current Power.
        /// </summary>
        /// <value>Minimum Power.</value>
        [IpmiMessageData(1,2)]
        public byte[] CurrentPower
        {
            get { return this.currentpower; }
            set { this.currentpower = value; }
        }

        /// <summary>
        /// Gets and sets the Minimum Power.
        /// </summary>
        /// <value>Minimum Power.</value>
        [IpmiMessageData(3,2)]
        public byte[] MinimumPower
        {
            get { return this.minimumpower; }
            set { this.minimumpower = value; }
        }

        /// <summary>
        /// Gets and sets the Maximum Power.
        /// </summary>
        /// <value>Maximum Power.</value>
        [IpmiMessageData(5,2)]
        public byte[] MaximumPower
        {
            get { return this.maximumpower; }
            set { this.maximumpower = value; }
        }

        /// <summary>
        /// Gets and sets the Average Power.
        /// </summary>
        /// <value>Average Power.</value>
        [IpmiMessageData(7,2)]
        public byte[] AveragePower
        {
            get { return this.averagepower; }
            set { this.averagepower = value; }
        }

        /// <summary>
        /// IPMI Specification based Time Stamp.
        /// </summary>
        /// <value>Time Stamp.</value>
        [IpmiMessageData(9, 4)]
        public byte[] TimeStamp
        {
            get { return this.timeStamp; }
            set { this.timeStamp = value; }
        }

        /// <summary>
        /// Statistics reporting time period (milliseconds).
        /// </summary>
        /// <value>Statistics reporting time period.</value>
        [IpmiMessageData(13, 4)]
        public byte[] Statistics
        {
            get { return this.statistics; }
            set { this.statistics = value; }
        }

        /// <summary>
        /// Power Reading State.
        /// </summary>
        /// <value>Statistics reporting time period.</value>
        [IpmiMessageData(17)]
        public byte PowerState
        {
            get { return this.powerState; }
            set { this.powerState = value; }
        }
    }
}
