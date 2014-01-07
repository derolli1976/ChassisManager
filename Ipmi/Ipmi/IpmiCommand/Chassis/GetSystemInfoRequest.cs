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
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetSystemInfoParameters)]
    internal class GetSystemInfoRequest : IpmiRequest
    {

        /// <summary>
        /// Pramater Revision [Request Byte 1].
        /// </summary>
        private byte _getParamater = 0x00;

        /// <summary>
        /// Paramater Selector
        /// </summary>
        private byte _selector;

        /// <summary>
        /// Set Selector
        /// </summary>
        private byte _setSelector;

        /// <summary>
        /// Block Selector
        /// </summary>
        private byte _blockSelector;


        /// <summary>
        /// Initializes a new instance of the GetSystemInfoRequest class.
        /// </summary>
        /// <param name="operation">Operation to perform.</param>
        internal GetSystemInfoRequest(byte selector, byte setSelector = 0x00, byte blockSelector = 0x00)
        {

            this._selector = selector;
            
            this._setSelector = setSelector;

            this._blockSelector = blockSelector;
        }

        /// <summary>
        /// Gets the operation to perform.
        /// </summary>
        [IpmiMessageData(0)]
        public byte GetParameter
        {
            get { return this._getParamater; }
        }

        /// <summary>
        /// Gets the operation to perform.
        /// </summary>
        [IpmiMessageData(1)]
        public byte Selector
        {
            get { return this._selector; }
        }

        /// <summary>
        /// Gets the operation to perform.
        /// </summary>
        [IpmiMessageData(2)]
        public byte SetSelector
        {
            get { return this._setSelector; }
        }

        /// <summary>
        /// Gets the operation to perform.
        /// </summary>
        [IpmiMessageData(3)]
        public byte BlockSelector
        {
            get { return this._blockSelector; }
        }
    }
}