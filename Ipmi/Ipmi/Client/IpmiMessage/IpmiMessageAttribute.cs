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

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    using System;

    /// <summary>
    /// Defines a class as an IPMI message.
    /// </summary>
    internal abstract class IpmiMessageAttribute : Attribute
    {
        /// <summary>
        /// IPMI message function.
        /// </summary>
        private readonly IpmiFunctions function;

        /// <summary>
        /// IPMI message command within the current function.
        /// </summary>
        private readonly IpmiCommand command;

        /// <summary>
        /// IPMI message lenght within the current function.
        /// </summary>
        private readonly int dataLength;

        /// <summary>
        /// Initializes a new instance of the IpmiMessageAttribute class.
        /// </summary>
        /// <param name="function">IPMI message function.</param>
        /// <param name="command">IPMI message command.</param>
        protected IpmiMessageAttribute(IpmiFunctions function, IpmiCommand command)
        {
            this.function = function;
            this.command = command;
        }

        /// <summary>
        /// Initializes a new instance of the IpmiMessageAttribute class.
        /// </summary>
        /// <param name="function">IPMI message function.</param>
        /// <param name="command">IPMI message command.</param>
        /// <param name="dataLength">IPMI message data length.</param>
        protected IpmiMessageAttribute(IpmiFunctions function, IpmiCommand command, int dataLength)
        {
            this.function = function;
            this.command = command;
            this.dataLength = dataLength;
        }

        /// <summary>
        /// Gets the IPMI message function.
        /// </summary>
        internal IpmiFunctions IpmiFunctions
        {
            get { return this.function; }
        }

        /// <summary>
        /// Gets the IPMI message command.
        /// </summary>
        internal IpmiCommand IpmiCommand
        {
            get { return this.command; }
        }
    }
}
