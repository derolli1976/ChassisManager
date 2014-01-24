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

    /// <summary>
    /// Defines a class as an OMC response message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ChassisMessageResponseAttribute : ChassisMessageAttribute
    {
        /// <summary>
        /// Initializes a new instance of the ChassisMessageResponseAttribute class.
        /// </summary>
        /// <param name="function">OMC message function.</param>
        /// <param name="command">OMC message command.</param>
        public ChassisMessageResponseAttribute(FunctionCode command)
            : base(command, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ChassisMessageResponseAttribute class.
        /// </summary>
        /// <param name="command">OMC message command.</param>
        /// <param name="dataLength">OMC message data length.</param>
        public ChassisMessageResponseAttribute(FunctionCode command, Int32 dataLength)
            : base(command, dataLength)
        {
        }
    }
}
