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
    /// Represents the IPMI 'Set Power Restore Policy Command' response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Chassis, IpmiCommand.SetPowerRestore)]
    internal class SetPowerRestoreResponse : IpmiResponse
    {
        /// <summary>
        /// Current power state.
        /// </summary>
        private byte _restorePolicy;

        /// <summary>
        /// Gets and sets the current power restore policy.
        /// </summary>
        /// <value>Current power state.</value>
        [IpmiMessageData(0)]
        public byte PowerRestorePolicy
        {
            get { return this._restorePolicy; }
            set { this._restorePolicy = value; }
        }

    }
}
