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
