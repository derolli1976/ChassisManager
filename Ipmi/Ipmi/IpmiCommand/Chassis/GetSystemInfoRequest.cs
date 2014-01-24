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
    /// Represents the IPMI 'Chassis Control' chassis request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetSystemInfoParameters)]
    internal class GetSystemInfoRequest : IpmiRequest
    {

        /// <summary>
        /// Pramater Revision [Request Byte 1].
        /// </summary>
        private byte _getParamater = 0x00;

        /// <summary>
        /// Paramater Selector
        /// </summary>
        private byte _selector;

        /// <summary>
        /// Set Selector
        /// </summary>
        private byte _setSelector;

        /// <summary>
        /// Block Selector
        /// </summary>
        private byte _blockSelector;


        /// <summary>
        /// Initializes a new instance of the GetSystemInfoRequest class.
        /// </summary>
        /// <param name="operation">Operation to perform.</param>
        internal GetSystemInfoRequest(byte selector, byte setSelector = 0x00, byte blockSelector = 0x00)
        {

            this._selector = selector;
            
            this._setSelector = setSelector;

            this._blockSelector = blockSelector;
        }

        /// <summary>
        /// Gets the operation to perform.
        /// </summary>
        [IpmiMessageData(0)]
        public byte GetParameter
        {
            get { return this._getParamater; }
        }

        /// <summary>
        /// Gets the operation to perform.
        /// </summary>
        [IpmiMessageData(1)]
        public byte Selector
        {
            get { return this._selector; }
        }

        /// <summary>
        /// Gets the operation to perform.
        /// </summary>
        [IpmiMessageData(2)]
        public byte SetSelector
        {
            get { return this._setSelector; }
        }

        /// <summary>
        /// Gets the operation to perform.
        /// </summary>
        [IpmiMessageData(3)]
        public byte BlockSelector
        {
            get { return this._blockSelector; }
        }
    }
}
