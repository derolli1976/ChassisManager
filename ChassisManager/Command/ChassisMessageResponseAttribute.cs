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
