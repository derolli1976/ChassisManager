/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  chong@Microsoft.com                        *
*   							                        *
*   							                        *
********************************************************/


namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    /// <summary>
    /// Represents the IPMI 'BMC Debug' application response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.OemGroup, IpmiCommand.BmcDebug)]
    internal class BmcDebugResponse : IpmiResponse
    {
    }
}