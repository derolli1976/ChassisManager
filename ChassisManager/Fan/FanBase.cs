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
