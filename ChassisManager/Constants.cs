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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Project wide constants are defined in this class.
    /// </summary>
    internal class Constants
    {
        /// <summary>
        /// The default field name.
        /// </summary>
        public const string NotApplicable = "Not Applicable";

        /// <summary>
        /// WebOperationContext constant for cache control
        /// </summary>
        public const string CacheControl = "Cache-Control";

        /// <summary>
        /// WebOperationContext constant for no caching
        /// </summary>
        public const string NoCache = "no-cache";
        /// <summary>
        /// Minimum SerialDevice Timeout In Msecs - not exposed to user. 
        /// We do not want users to set a very low timeout value for the device send/receive
        /// </summary>
        internal const int MinimumSerialDeviceTimeoutInMsecs = 100;
    } 
}
