/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*   							                        *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{

    /// <summary>
    /// Represents the IPMI 'Chassis Control' chassis request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Chassis, IpmiCommand.ChassisControl, 1)]
    internal class ChassisControlRequest : IpmiRequest
    {
        /// <summary>
        /// Power down.
        /// </summary>
        internal const byte OperationPowerDown = 0;

        /// <summary>
        /// Power up.
        /// </summary>
        internal const byte OperationPowerUp = 1;

        /// <summary>
        /// Power cycle.
        /// </summary>
        internal const byte OperationPowerCycle = 2;

        /// <summary>
        /// Hard reset.
        /// </summary>
        internal const byte OperationHardReset = 3;

        /// <summary>
        /// Diagnostic interrupt.
        /// </summary>
        internal const byte OperationDiagnosticInterrupt = 4;

        /// <summary>
        /// Soft shutdown.
        /// </summary>
        internal const byte OperationSoftShutdown = 5;

        /// <summary>
        /// Operation to perform.
        /// </summary>
        private readonly byte operation;

        /// <summary>
        /// Initializes a new instance of the ChassisControlRequest class.
        /// </summary>
        /// <param name="operation">Operation to perform.</param>
        internal ChassisControlRequest(byte operation)
        {
            this.operation = operation;
        }

        /// <summary>
        /// Gets the operation to perform.
        /// </summary>
        [IpmiMessageData(0)]
        public byte Operation
        {
            get { return this.operation; }
        }
    }
}