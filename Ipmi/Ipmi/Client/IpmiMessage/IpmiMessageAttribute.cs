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
