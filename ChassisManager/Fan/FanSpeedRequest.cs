/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*   		 srsankar@microsoft.com                     *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager
{
 
    /// <summary>
    /// Represents the Fan 'Get Fan Speed' request message.
    /// </summary>
    [ChassisMessageRequest(FunctionCode.GetFanSpeed)]
    internal class FanSpeedRequest : ChassisRequest
    {
    }

    /// <summary>
    /// Represents the Fan 'Get Fan Speed' application response message.
    /// </summary>
    [ChassisMessageResponse(FunctionCode.GetFanSpeed)]
    internal class FanSpeedResponse : ChassisResponse
    {
        // fan rpm
        private ushort rpm;

        /// <summary>
        /// Fan RPM
        /// </summary>
        [ChassisMessageData(0)]
        public ushort Rpm
        {
            get { return this.rpm; }
            set { this.rpm = value; }
        }
    }

}
