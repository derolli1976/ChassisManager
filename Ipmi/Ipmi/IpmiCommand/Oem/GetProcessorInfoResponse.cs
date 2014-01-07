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
    /// Represents the IPMI 'Get Processor Info Command' request message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.OemGroup, IpmiCommand.GetProcessorInfo)]
    class GetProcessorInfoResponse : IpmiResponse
    {
        /// <summary>
        /// Processor Type
        /// </summary>
        private byte _type;

        /// <summary>
        /// Processor Frequency
        /// </summary>
        private ushort _frequency;

        /// <summary>
        /// Processor State
        /// </summary>
        private byte _state;

        /// <summary>
        /// Processor Type
        /// </summary>       
        [IpmiMessageData(0)]
        public byte ProcessorType
        {
            get { return this._type; }
            set { this._type = value; }
        }

        /// <summary>
        /// Processor Frequency 
        /// </summary>       
        [IpmiMessageData(1)]
        public ushort Frequency
        {
            get { return this._frequency; }
            set { this._frequency = value; }
        }

        /// <summary>
        /// Processor State
        /// </summary>       
        [IpmiMessageData(3)]
        public byte ProcessorState
        {
            get { return this._state; }
            set { this._state = value; }
        }
    }
}
