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
    /// Represents the IPMI 'Get Disk Status Command for WCS JBOD' OEM request message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Oem, IpmiCommand.GetDiskStatus)]
    class GetDiskStatusResponse : IpmiResponse
    {
        private byte channel;

        private byte diskcount;

        private byte[] statusData;

        /// <summary>
        /// Disk Controller Channel
        /// </summary>       
        [IpmiMessageData(0)]
        public byte Channel
        {
            get { return this.channel; }
            set { this.channel = value; }
        }

        /// <summary>
        /// Disk Count on Controller
        /// </summary>       
        [IpmiMessageData(1)]
        public byte DiskCount
        {
            get { return this.diskcount; }
            set { this.diskcount = value; }
        }

        /// <summary>
        /// Disk Status Data
        /// Each byte = [7-6]:  Disk Status (0 = Normal, 1 = Failed, 2 = Error)
        ///             [5-0]:  Disk #: Number/Location Id
        /// </summary>       
        [IpmiMessageData(2)]
        public byte[] StatusData
        {
            get { return this.statusData; }
            set { this.statusData = value; }
        }
    }
}
