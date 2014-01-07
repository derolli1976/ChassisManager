/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*                                       	            *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.GetSelInfo)]
    internal class SelInfoRequest : IpmiRequest
    {
    }
}
