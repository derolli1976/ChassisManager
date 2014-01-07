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
    /// Defines a class as an IPMI request message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    sealed internal class IpmiMessageRequestAttribute : IpmiMessageAttribute
    {
        /// <summary>
        /// Initializes a new instance of the IpmiMessageRequestAttribute class.
        /// </summary>
        /// <param name="function">IPMI message function.</param>
        /// <param name="command">IPMI message command.</param>
        public IpmiMessageRequestAttribute(IpmiFunctions function, IpmiCommand command)
            : base(function, command, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the IpmiMessageRequestAttribute class.
        /// </summary>
        /// <param name="function">IPMI message function.</param>
        /// <param name="command">IPMI message command.</param>
        /// <param name="dataLength">IPMI message data length.</param>
        public IpmiMessageRequestAttribute(IpmiFunctions function, IpmiCommand command, int dataLength)
            : base(function, command, dataLength)
        {
        }
    }
}
