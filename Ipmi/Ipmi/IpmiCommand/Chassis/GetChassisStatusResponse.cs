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
    /// Represents the IPMI 'Get Chassis Status' response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Chassis, IpmiCommand.GetChassisStatus)]
    internal class GetChassisStatusResponse : IpmiResponse
    {
        /// <summary>
        /// Current power state.
        /// </summary>
        private byte currentPowerState;

        /// <summary>
        /// Last power event.
        /// </summary>
        private byte lastPowerEvent;

        /// <summary>
        /// Miscellaneous chassis state.
        /// </summary>
        private byte miscellaneousChassisState;

        /// <summary>
        /// Front panel button capabilities and disable/enable status (optional).
        /// </summary>
        private byte frontPanelButton;

        /// <summary>
        /// Gets and sets the Current power state.
        /// </summary>
        /// <value>Current power state.</value>
        [IpmiMessageData(0)]
        public byte CurrentPowerState
        {
            get { return this.currentPowerState; }
            set { this.currentPowerState = value; }
        }

        /// <summary>
        /// Gets and sets the Last power event.
        /// </summary>
        /// <value>Last power event.</value>
        [IpmiMessageData(1)]
        public byte LastPowerEvent
        {
            get { return this.lastPowerEvent; }
            set { this.lastPowerEvent = value; }
        }

        /// <summary>
        /// Gets and sets the Miscellaneous chassis state.
        /// </summary>
        /// <value>Miscellaneous chassis state.</value>
        [IpmiMessageData(2)]
        public byte MiscellaneousChassisState
        {
            get { return this.miscellaneousChassisState; }
            set { this.miscellaneousChassisState = value; }
        }

        /// <summary>
        /// Gets and sets the Front panel button capabilities and disable/enable status (optional).
        /// </summary>
        /// <value>Front panel button capabilities and disable/enable status (optional).</value>
        [IpmiMessageData(3)]
        public byte FrontPanelButton
        {
            get { return this.frontPanelButton; }
            set { this.frontPanelButton = value; }
        }
    }
}
