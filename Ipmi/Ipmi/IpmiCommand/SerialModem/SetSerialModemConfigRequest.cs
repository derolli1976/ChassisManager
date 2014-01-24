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
    /// Represents the IPMI 'Set Serial Modem Configuration' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Transport, IpmiCommand.SetSerialModemConfiguration)]
    class SetSerialModemConfigRequest<T> : IpmiRequest where T : SerialConfig.SerialConfigBase
    {

        /// <summary>
        /// Channel Number
        /// [7:4] Reserved
        /// [3:0] Channel Number
        /// </summary>    
        byte channel;

        /// <summary>
        /// Paramater selector
        /// </summary> 
        byte selector;

        /// <summary>
        /// Paramater payload
        /// </summary>
        byte[] paramaters;

        public SetSerialModemConfigRequest(byte channel, T paramater)
        {
            this.channel = channel;
            this.selector = paramater.Selector;
            this.paramaters = paramater.Payload;
        }

        /// <summary>
        /// Sets the Channel Number.
        /// </summary>
        [IpmiMessageData(0)]
        public byte Channel
        {
            get { return this.channel; }
        }


        /// <summary>
        /// Sets the selection paramater.
        /// </summary>
        [IpmiMessageData(1)]
        public byte ParamaterSelector
        {
            get { return this.selector; }
        }

        /// <summary>
        /// Sets the selection paramater.
        /// </summary>
        [IpmiMessageData(2)]
        public byte[] Paramaters
        {
            get { return this.paramaters; }
        }




    }
}
