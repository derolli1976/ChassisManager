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
    /// Represents the IPMI 'Set Serial/Modem Mux Command' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Transport, IpmiCommand.SetSerialModelMux)]
    class SetSerialMuxRequest : IpmiRequest
    {

        /// <summary>
        /// Channel Number
        /// [7:4] Reserved
        /// [3:0] Channel Number
        /// </summary>    
        byte channel;

        /// <summary>
        /// [7:4] Reserved
        /// [3:0] Channel Number
        /// Mux Setting
        /// </summary> 
        byte muxSetting;

        /// <summary>
        /// Set Serial/Modem Mux Command
        /// </summary>
        public SetSerialMuxRequest(byte channel, MuxSwtich mux)
        {
            // [7:4] Reserved
            // [3:0] Channel Number
            this.channel = (byte)(channel & 0x0F);
            this.muxSetting = (byte)mux;                           
        }

        /// <summary>
        /// Set Serial/Modem Mux Command
        /// </summary>
        public SetSerialMuxRequest(MuxSwtich mux)
        {
            // Channel number (0x0E == current channel this request was issued on).
            this.channel = 0x0E;
            this.muxSetting = (byte)mux;
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
        /// Mux Setting.
        /// </summary>
        [IpmiMessageData(1)]
        public byte MuxSetting
        {
            get { return this.muxSetting; }
        }
    }
}
