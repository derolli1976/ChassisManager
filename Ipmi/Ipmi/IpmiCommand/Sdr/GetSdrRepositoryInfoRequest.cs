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
    /// Represents the IPMI 'Get Sdr Repository Info' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.GetSdrRepositoryInfo)]
    internal class GetSdrRepositoryInfoRequest : IpmiRequest
    {                           
        /// <summary>
        /// Initializes a new instance of the GetSdrRepositoryInfo class.
        /// </summary>
        internal GetSdrRepositoryInfoRequest()
        {
        }

    }
}
