/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*                                       	            *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    using System;
    /// <summary>
    /// Represents the IPMI 'Set Sel Time' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.SetSelTime)]
    internal class SetSelTimeRequest : IpmiRequest
    {
        /// <summary>
        /// Seconds.
        /// </summary> 
        private int offset;

        internal SetSelTimeRequest(DateTime date)
        {
            offset = Convert.ToInt32(IpmiSharedFunc.SecondsFromUnixOffset(date));
        }


        /// <summary>
        /// Unix Offset in Seconds
        /// </summary>       
        [IpmiMessageData(0)]
        public int OffSet
        {
            get { return this.offset; }

        }
    }
 }
