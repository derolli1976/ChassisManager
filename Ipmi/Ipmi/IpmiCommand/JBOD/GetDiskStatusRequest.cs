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
    [IpmiMessageRequest(IpmiFunctions.Oem, IpmiCommand.GetDiskStatus)]
    internal class GetDiskStatusRequest : IpmiRequest
    {

    }
}
