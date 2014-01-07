//-----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Microsoft">
//     Author:  v-shmada
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------- 
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
