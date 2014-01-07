/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*                                       	            *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    /// <summary>
    /// Represents the IPMI 'Get SEL Time' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.GetSelTime)]
    internal class SelTimeRequest : IpmiRequest
    {
        /// <summary>
        /// Represents the IPMI 'Get SEL Time' request message.
        /// </summary>
        internal SelTimeRequest()
        {
        }

    }
 }
