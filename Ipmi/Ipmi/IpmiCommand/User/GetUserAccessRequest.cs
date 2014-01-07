/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*   							                        *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    /// <summary>
    /// Represents the IPMI 'Get User Access ' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetUserAccess)]
    internal class GetUserAccessRequest : IpmiRequest
    {
        /// <summary>
        // channel number
        /// <summary>
        private byte channelNumber;

        /// <summary>
        /// User Id
        /// <summary>
        private byte userId;
       
        /// <summary>
        /// Initializes a new instance of the GetUserAccessRequest class.
        /// </summary>
        public GetUserAccessRequest(byte channelNum, byte userId)
        {
            this.channelNumber = channelNum;
            this.userId = userId;
        }

        /// <summary>
        /// Gets the Channel number.
        /// </summary>
        /// <value>Channel number.</value>
        [IpmiMessageData(0)]
        public byte ChannelNumber
        {
            get { return this.channelNumber; }
        }

        /// <summary>
        /// Sets the reserved value.
        /// </summary>
        /// <value>reserved</value>
        [IpmiMessageData(1)]
        public byte UserId
        {
            get { return this.userId; }
        }


        
    }
}
