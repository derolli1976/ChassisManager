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
    /// Represents the IPMI 'Get Processor Info Command' OEM request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.OemGroup, IpmiCommand.GetProcessorInfo)]
    internal class GetProcessorInfoRequest : IpmiRequest
    {

        /// <summary>
        /// Processor device index.  Default = 0x00
        /// Get Processor Information changed to zero based 
        /// indexing in firmware V3.05
        /// </summary>
        private readonly byte _processor;

        /// <summary>
        /// Get Processor Info Request
        /// </summary>
        internal GetProcessorInfoRequest(byte processor)
        { this._processor = processor; }


        /// <summary>
        /// Processor Number
        /// </summary>       
        [IpmiMessageData(0)]
        public byte Processor
        {
            get { return this._processor; }

        }
    }
}
