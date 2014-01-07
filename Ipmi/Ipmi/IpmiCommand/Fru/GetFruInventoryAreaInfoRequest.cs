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
    /// Represents the IPMI 'Get FRU Inventory Info' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.GetFruInventoryAreaInfo, 1)]
    internal class GetFruInventoryAreaInfoRequest : IpmiRequest
    {
        private byte deviceId;

        
        /// <summary>
        /// Initializes a new instance of the GetFruInventoryAreaInfoRequest class.
        /// </summary>
        /// <param name="deviceId">byte value for device Id</param>
        internal GetFruInventoryAreaInfoRequest(byte deviceId)
        {
            this.deviceId = deviceId;
        }

        /// <summary>
        /// Gets FRU Inventory Area Info.
        /// </summary>       
        [IpmiMessageData(0)]
        public byte DeviceId
        {
            get { return this.deviceId; }
        
        }
    }
}
