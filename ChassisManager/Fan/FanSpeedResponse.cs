/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Author:  Bryankel@Microsoft.com                     *
*   		 srsankar@microsoft.com					    *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager
{
    using System;


    /// <summary>
    /// Represents the Fan 'Get Reading' application response message.
    /// </summary>
    [ChassisMessageResponse(FunctionCode.GetFanSpeed)]
    internal class FanSpeedResponse : ChassisResponse
    {
        // fan rpm
        private ushort _rpm;

        /// <summary>
        /// Fan RPM
        /// </summary>
        [ChassisMessageData(0)]
        public ushort RPM
        {
            get { return this._rpm; }
            set { this._rpm = value; }
        }
    }

    
}
