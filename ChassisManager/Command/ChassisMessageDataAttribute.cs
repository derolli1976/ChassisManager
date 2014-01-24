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
    /// Defines data with an OMC message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ChassisMessageDataAttribute : Attribute
    {
        /// <summary>
        /// Byte offset into the OMC message data stream this property begins at.
        /// </summary>
        private readonly int offset;

        /// <summary>
        /// Length of the data item or 0 to based on the property's type.
        /// </summary>
        private readonly int length;

        /// <summary>
        /// Initializes a new instance of the ChassisMessageDataAttribute class.
        /// </summary>
        /// <param name="offset">Byte offset into the OMC message data stream.</param>
        public ChassisMessageDataAttribute(int offset)
            : this(offset, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ChassisMessageDataAttribute class.
        /// </summary>
        /// <param name="offset">Byte offset into the OMC message data stream.</param>
        /// <param name="length">Length of the data item or 0 to based on the property's type.</param>
        public ChassisMessageDataAttribute(int offset, int length)
        {
            this.offset = offset;
            this.length = length;
        }

        /// <summary>
        /// Gets the byte offset into the OMC message data stream.
        /// </summary>
        public int Offset
        {
            get { return this.offset; }
        }

        /// <summary>
        /// Gets the length of the data or 0 to based on the property's type.
        /// </summary>
        public int Length
        {
            get { return this.length; }
        }

    }
}
