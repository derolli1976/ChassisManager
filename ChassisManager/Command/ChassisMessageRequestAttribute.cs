/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*                                       	            *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager
{
    using System;

    /// <summary>
    /// Defines a class as an OMC request message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ChassisMessageRequestAttribute : ChassisMessageAttribute
    {
        /// <summary>
        /// Initializes a new instance of the ChassisMessageRequestAttribute class.
        /// </summary>
        /// <param name="function">OMC message function.</param>
        /// <param name="command">OMC message command.</param>
        public ChassisMessageRequestAttribute(FunctionCode command)
            : base(command, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ChassisMessageRequestAttribute class.
        /// </summary>
        /// <param name="function">OMC message function.</param>
        /// <param name="command">OMC message command.</param>
        /// <param name="dataLength">OMC message data length.</param>
        public ChassisMessageRequestAttribute(FunctionCode command, Int32 dataLength)
            : base(command, dataLength)
        {
        }
    }
}
