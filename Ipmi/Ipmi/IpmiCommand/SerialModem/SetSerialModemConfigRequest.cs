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
