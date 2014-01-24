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
    /// Represents the IPMI 'Set Serial/Modem Mux Command' application response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Application, IpmiCommand.SetSerialModelMux)]
    internal class SetSerialMuxResponse : IpmiResponse
    {

        internal void GetMux()
        {
            //[7] -  	0b = requests to switch mux to system are allowed 
            //          1b = requests to switch mux to system are blocked 
            if ((byte)(muxSetting & 0x0) == 0x80)
                muxSwitchAllowed = true;
            //[6] -  	0b = requests to switch mux to BMC are allowed 
            //          1b = requests to switch mux to BMC are blocked 
            if ((byte)(muxSetting & 0x0) == 0x40)
                requestToBmcAllowed = true;
            //[3] -  	0b = no alert presently in progress 
            //          1b = alert in progress on channel 
            if ((byte)(muxSetting & 0x0) == 0x08)
                alertInProgress = true;
            //[2] -  	0b = no IPMI or OEM messaging presently active on channel 
            //          1b = IPMI or OEM messaging session active on channel 
            if ((byte)(muxSetting & 0x0) == 0x04)
                messagingActive = true;
            //[1] -  	0b = request was rejected 
            //          1b = request was accepted (see note, below) or switch was forced 
            //          present mux setting 
            if ((byte)(muxSetting & 0x0) == 0x02)
                requestAccepted = true;
            //[0] -  	0b = mux is set to system (system can transmit and receive) 
            //          1b = mux is set to BMC  (BMC can transmit. System can neither 
            //          transmit nor receive) 
            if ((byte)(muxSetting & 0x0) == 0x01)
                muxSetToSystem = true;
        }

        /// <summary>
        /// [7] -  	0b = requests to switch mux to system are allowed 
        ///         1b = requests to switch mux to system are blocked 
        /// </summary>
        private bool muxSwitchAllowed = false;

        /// <summary>
        /// [6] -  	0b = requests to switch mux to BMC are allowed 
        ///         1b = requests to switch mux to BMC are blocked 
        /// </summary>
        private bool requestToBmcAllowed = false;

        /// <summary>
        /// [3] -  	0b = no alert presently in progress 
        ///         1b = alert in progress on channel 
        /// </summary>
        private bool alertInProgress = false;

        /// <summary>
        /// [2] -  	0b = no IPMI or OEM messaging presently active on channel 
        ///         1b = IPMI or OEM messaging session active on channel 
        /// </summary>
        private bool messagingActive = false;

        /// <summary>
        /// [1] -  	0b = request was rejected 
        ///         1b = request was accepted (see note, below) or switch was forced 
        /// </summary>
        private bool requestAccepted = false;

        /// <summary>
        /// [0] -  	0b = mux is set to system (system can transmit and receive) 
        ///         1b = mux is set to BMC  (BMC can transmit. System can neither 
        ///         transmit nor receive) 
        /// </summary>
        private bool muxSetToSystem = false;

        /// <summary>
        /// Mux Setting
        /// </summary>
        private byte muxSetting;

        /// <summary>
        /// Mux Setting
        /// </summary>
        [IpmiMessageData(0)]
        public byte MuxSetting
        {
            get { return (byte)this.muxSetting; }
            set { this.muxSetting = value; }
        }

        /// <summary>
        /// [7] -  	0b = requests to switch mux to system are allowed 
        ///         1b = requests to switch mux to system are blocked 
        /// </summary>
        internal bool MuxSwitchAllowed
        {
            get { return this.muxSwitchAllowed; }   
        }

        /// <summary>
        /// [6] -  	0b = requests to switch mux to BMC are allowed 
        ///         1b = requests to switch mux to BMC are blocked 
        /// </summary>
        internal bool RequestToBmcAllowed
        {
            get { return this.requestToBmcAllowed; }   
        }

        /// <summary>
        /// [3] -  	0b = no alert presently in progress 
        ///         1b = alert in progress on channel 
        /// </summary>
        internal bool AlertInProgress
        {
            get { return this.alertInProgress; }   
        }

        /// <summary>
        /// [2] -  	0b = no IPMI or OEM messaging presently active on channel 
        ///         1b = IPMI or OEM messaging session active on channel 
        /// </summary>
        internal bool MessagingActive
        {
            get { return this.messagingActive; }   
        }

        /// <summary>
        /// [1] -  	0b = request was rejected 
        ///         1b = request was accepted (see note, below) or switch was forced 
        /// </summary>
        internal bool RequestAccepted
        {
            get { return this.requestAccepted; }   
        }

        /// <summary>
        /// [0] -  	0b = mux is set to system (system can transmit and receive) 
        ///         1b = mux is set to BMC  (BMC can transmit. System can neither 
        ///         transmit nor receive) 
        /// </summary>
        internal bool MuxSetToSystem
        {
            get { return this.muxSetToSystem; }   
        }

    }
}
