/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*   							                        *
*   							                        *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{

    /// <summary>
    /// Represents the IPMI 'Get Disk Info Command for WCS JBOD' OEM request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Oem, IpmiCommand.GetDiskInfo)]
    internal class GetDiskInfoRequest : IpmiRequest
    {

        /// <summary>
        /// JBOD Expander Channel.  Default = 0x00
        /// </summary>
        private readonly byte _channel = 0x00;

        /// <summary>
        /// JBOD Disk Number.  Default = 0x00,
        /// which indicates individual disks are 
        /// not supported, JBOD information is
        /// returned instead.
        /// </summary>
        private readonly byte _disk = 0x00;

        /// <summary>
        /// Get Disk Info Request
        /// </summary>
        internal GetDiskInfoRequest()
        { }

        /// <summary>
        /// Initialize Get Disk Info Request
        /// </summary>
        /// <param name="channel">JBOD Channel Number</param>
        /// <param name="disk">Disk Number</param>
        internal GetDiskInfoRequest(byte channel, byte disk)
        {
            this._channel = channel;
            this._disk = disk;
        }

        /// <summary>
        /// Channel Byte
        /// </summary>       
        [IpmiMessageData(0)]
        public byte Channel
        {
            get { return this._channel; }

        }

        /// <summary>
        /// Disk Byte
        /// </summary>       
        [IpmiMessageData(1)]
        public byte Disk
        {
            get { return this._disk; }

        }

    }
}
