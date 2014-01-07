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
    /// Represents the IPMI 'Set User Access ' application request message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Application, IpmiCommand.SetUserAccess)]
    class SetUserAccessResponse : IpmiResponse
    {

    }
}
