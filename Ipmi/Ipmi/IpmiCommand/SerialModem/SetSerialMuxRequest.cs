/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*                                       	            *
********************************************************/

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
