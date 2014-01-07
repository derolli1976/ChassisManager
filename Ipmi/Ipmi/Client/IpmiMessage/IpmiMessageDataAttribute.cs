/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*                                       	            *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    using System;

    /// <summary>
    /// Defines data with an IPMI message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal sealed class IpmiMessageDataAttribute : Attribute
    {
        /// <summary>
        /// Byte offset into the IPMI message data stream this property begins at.
        /// </summary>
        private readonly int offset;

        /// <summary>
        /// Length of the data item or 0 to based on the property's type.
        /// </summary>
        private readonly int length;

        /// <summary>
        /// Initializes a new instance of the IpmiMessageDataAttribute class.
        /// </summary>
        /// <param name="offset">Byte offset into the IPMI message data stream.</param>
        public IpmiMessageDataAttribute(int offset)
            : this(offset, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the IpmiMessageDataAttribute class.
        /// </summary>
        /// <param name="offset">Byte offset into the IPMI message data stream.</param>
        /// <param name="length">Length of the data item or 0 to based on the property's type.</param>
        public IpmiMessageDataAttribute(int offset, int length)
        {
            this.offset = offset;
            this.length = length;
        }

        /// <summary>
        /// Gets the byte offset into the IPMI message data stream.
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
