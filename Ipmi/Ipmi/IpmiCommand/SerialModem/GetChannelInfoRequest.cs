using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    /// <summary>
    /// Represents the IPMI 'Get Channel Info' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetChannelInfo, 1)]
    internal class GetChannelInfoRequest : IpmiRequest
    {

        /// <summary>
        /// Channel Number  
        /// 0x0E = Channel the request is being sent over.
        /// </summary>
        private readonly byte channel;

        /// <summary>
        /// Initializes a new instance of the GetChannelInfoRequest class.
        /// </summary>
        internal GetChannelInfoRequest(byte channel)
        {
            this.channel = channel;
        }

        /// <summary>
        /// Initializes a new instance of the GetChannelInfoRequest class.
        /// Based on the Channel the request is being sent over.
        /// </summary>
        internal GetChannelInfoRequest()
        {
            this.channel = 0x0E;
        }

        /// <summary>
        /// Channel Number  
        /// 0x0E = Channel the request is being sent over.
        /// </summary>
        [IpmiMessageData(0)]
        public byte Channel
        {
            get { return this.channel; }
        }



    }
}
