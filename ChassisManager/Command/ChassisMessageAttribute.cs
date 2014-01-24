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
    /// Defines a class as an OMC message.
    /// </summary>
    public abstract class ChassisMessageAttribute : Attribute
    {
        /// <summary>
        /// OMC message command within the current function.
        /// </summary>
        private readonly FunctionCode _command;

        /// <summary>
        /// Initializes a new instance of the ChassisMessageAttribute class.
        /// </summary>
        /// <param name="command">OMC message command.</param>
        protected ChassisMessageAttribute(FunctionCode command)
        {
            this._command = command;
        }

        /// <summary>
        /// Initializes a new instance of the ChassisMessageAttribute class.
        /// </summary>
        /// <param name="command">OMC message command.</param>
        /// <param name="dataLength">OMC message data length.</param>
        protected ChassisMessageAttribute(FunctionCode command, Int32 dataLength)
        {
            this._command = command;
        }

        /// <summary>
        /// Gets the Sled message command.
        /// </summary>
        public FunctionCode Command
        {
            get { return this._command; }
        }
    }
}
