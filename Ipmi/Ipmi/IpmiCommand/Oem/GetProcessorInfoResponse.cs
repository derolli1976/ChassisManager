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
    /// Represents the IPMI 'Get Processor Info Command' request message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.OemGroup, IpmiCommand.GetProcessorInfo)]
    class GetProcessorInfoResponse : IpmiResponse
    {
        /// <summary>
        /// Processor Type
        /// </summary>
        private byte _type;

        /// <summary>
        /// Processor Frequency
        /// </summary>
        private ushort _frequency;

        /// <summary>
        /// Processor State
        /// </summary>
        private byte _state;

        /// <summary>
        /// Processor Type
        /// </summary>       
        [IpmiMessageData(0)]
        public byte ProcessorType
        {
            get { return this._type; }
            set { this._type = value; }
        }

        /// <summary>
        /// Processor Frequency 
        /// </summary>       
        [IpmiMessageData(1)]
        public ushort Frequency
        {
            get { return this._frequency; }
            set { this._frequency = value; }
        }

        /// <summary>
        /// Processor State
        /// </summary>       
        [IpmiMessageData(3)]
        public byte ProcessorState
        {
            get { return this._state; }
            set { this._state = value; }
        }
    }
}
