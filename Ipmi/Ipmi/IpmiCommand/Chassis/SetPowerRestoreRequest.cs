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
    /// Represents the IPMI 'Set Power Restore Policy Command' chassis request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Chassis, IpmiCommand.SetPowerRestore, 1)]
    internal class SetPowerRestoreRequest : IpmiRequest
    {
        /// <summary>
        /// Chassis Restore Policy Option
        /// </summary>
        private byte _policyOption;

        /// <summary>
        /// Initializes a new instance of the Set Power Restore Policy Command class.
        /// </summary>
        /// <param name="operation">Operation to perform.</param>
        internal SetPowerRestoreRequest(PowerRestoreOption option)
        {
            this._policyOption = (byte)option;
        }

        /// <summary>
        /// Gets the operation to perform.
        /// </summary>
        [IpmiMessageData(0)]
        public byte PolicyOption
        {
            get { return this._policyOption; }
        }
    }
}