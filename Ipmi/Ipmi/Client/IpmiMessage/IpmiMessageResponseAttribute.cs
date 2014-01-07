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
    /// Defines a class as an IPMI response message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class IpmiMessageResponseAttribute : IpmiMessageAttribute
    {
        /// <summary>
        /// Initializes a new instance of the IpmiMessageResponseAttribute class.
        /// </summary>
        /// <param name="function">IPMI message function.</param>
        /// <param name="command">IPMI message command.</param>
        public IpmiMessageResponseAttribute(IpmiFunctions function, IpmiCommand command)
            : base(function, command, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the IpmiMessageResponseAttribute class.
        /// </summary>
        /// <param name="function">IPMI message function.</param>
        /// <param name="command">IPMI message command.</param>
        /// <param name="dataLength">IPMI message data length.</param>
        public IpmiMessageResponseAttribute(IpmiFunctions function, IpmiCommand command, int dataLength)
            : base(function, command, dataLength)
        {
        }
    }
}
