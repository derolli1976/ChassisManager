/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*            srsankar@microsoft.com                    	*
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager
{
    using System;
    using System.Text;
    using System.Timers;
    using System.Threading;
    using System.Collections;
    using System.Collections.Generic;
    
    using System.Globalization;

    public abstract class FanBase : ChassisSendReceive
    {
        /// <summary>
        ///  Define Device Type
        /// </summary>
        protected DeviceType _type;

        /// <summary>
        /// Device Id for the Sled
        /// </summary>
        protected byte _deviceId;

        public FanBase(byte deviceId)
        {
            this._deviceId = deviceId;
        }
    }
}
