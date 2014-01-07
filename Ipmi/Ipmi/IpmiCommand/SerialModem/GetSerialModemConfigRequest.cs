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
    /// Represents the IPMI 'Set Serial Modem Configuration' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Transport, IpmiCommand.GetSerialModemConfiguration)]
    class GetSerialModemConfigRequest<T> : IpmiRequest where T : SerialConfig.SerialConfigBase
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
        /// Set Selector
        /// </summary>
        byte setSelector;

        /// <summary>
        /// Block selector
        /// </summary> 
        byte block;

        public GetSerialModemConfigRequest(byte channel, T paramater)
        {
            this.channel = channel;
            this.selector = paramater.Selector;
            this.setSelector = 0x00;
            this.block = 0x00;
        }

        public GetSerialModemConfigRequest(byte channel, byte selector, byte block, T paramater)
        {
            this.channel = channel;
            this.selector = paramater.Selector;
            this.setSelector = selector;
            this.block = block;
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
        public byte SetSelector
        {
            get { return this.setSelector; }
        }

        /// <summary>
        /// Sets the selection paramater.
        /// </summary>
        [IpmiMessageData(3)]
        public byte BlockSelector
        {
            get { return this.block; }
        }
    }
}
